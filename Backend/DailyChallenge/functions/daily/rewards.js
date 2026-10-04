'use strict';

/**
 * Phase 4.3 — Daily #1 winner reward settlement + claim.
 *
 * Settlement reads ONLY Firestore attempt documents under
 * dailyChallenges/{dayId}/attempts. Simulated / hybrid client leaderboard
 * rows are never consulted and can never receive rewards.
 *
 * Ranking (strict sequential #1, no shared first place):
 *   1. score DESC
 *   2. moves ASC
 *   3. completionTimeMs ASC
 *   4. Firebase UID ASC
 *
 * TRUST BOUNDARY: moves and completionTimeMs remain client-reported.
 */

const admin = require('firebase-admin');
const { HttpsError } = require('firebase-functions/v2/https');
const { defineBoolean } = require('firebase-functions/params');
const { requireAuth, utcDayId } = require('./validation');

const REWARD_SETTLEMENT_VERSION = 1;
const FIRST_PLACE_RANK = 1;
const REWARD_TYPE_COINS = 'coins';
const REWARD_AMOUNT_COINS = 1000;
const REWARD_SOURCE = 'daily_first_place';
/** Fake dayId for Editor/dev claim smoke tests — never a real UTC day. */
const DEV_REWARD_DAY_ID = 'dev-reward-smoke-test';
const REWARD_SOURCE_DEV = 'daily_first_place_dev';

/**
 * Firebase Functions v2 parameterized config (recommended).
 * Default FALSE — createDevDailyReward stays closed unless explicitly enabled
 * via functions/.env (or .env.<projectId>) and redeploy.
 *
 * Plain process.env alone is unreliable for 2nd Gen unless the CLI injected
 * dotenv at deploy time; defineBoolean makes the flag deploy-time explicit.
 */
const allowDevDailyRewardsParam = defineBoolean('ALLOW_DEV_DAILY_REWARDS', {
  default: false,
  description:
    'Enable createDevDailyReward smoke-test callable (DEV only; default false).',
});

function db() {
  return admin.firestore();
}

/**
 * Server-side gate for createDevDailyReward.
 * Default OFF. Emulator always allows. Deployed requires param/env === true.
 */
function isDevDailyRewardCreationEnabled() {
  if (process.env.FUNCTIONS_EMULATOR === 'true') {
    return true;
  }

  let paramEnabled = false;
  try {
    paramEnabled = allowDevDailyRewardsParam.value() === true;
  } catch (_) {
    paramEnabled = false;
  }

  // Fallback if dotenv was injected as a classic env var without params plumbing.
  const envEnabled = process.env.ALLOW_DEV_DAILY_REWARDS === 'true';
  return paramEnabled || envEnabled;
}

/** Safe gate diagnostics — never logs secret/env values. */
function logDevRewardGateDiagnostics(enabled) {
  const emulator = process.env.FUNCTIONS_EMULATOR === 'true';
  const rawPresent = typeof process.env.ALLOW_DEV_DAILY_REWARDS !== 'undefined';
  console.log('[DailyRewardDev] gate raw present=' + rawPresent);
  console.log('[DailyRewardDev] gate enabled=' + !!enabled);
  console.log('[DailyRewardDev] emulator=' + emulator);
}

function isValidClaimDayId(dayId) {
  if (typeof dayId !== 'string' || !dayId) {
    return false;
  }
  if (/^\d{4}-\d{2}-\d{2}$/.test(dayId)) {
    return true;
  }
  return dayId === DEV_REWARD_DAY_ID;
}

function previousUtcDayId(serverNow = new Date()) {
  return utcDayId(new Date(serverNow.getTime() - 24 * 60 * 60 * 1000));
}

function dayRef(dayId) {
  return db().collection('dailyChallenges').doc(dayId);
}

function rewardRef(dayId, uid) {
  return dayRef(dayId).collection('rewards').doc(uid);
}

function attemptsCol(dayId) {
  return dayRef(dayId).collection('attempts');
}

/**
 * Eligible for rewards: Completed + scoreTrusted only.
 * Never simulated; never Started/Abandoned/malformed.
 */
function isEligibleAttempt(data, uid) {
  if (!data || typeof data !== 'object') {
    return false;
  }
  if (!uid || typeof uid !== 'string' || uid.length === 0) {
    return false;
  }
  if (data.state !== 'Completed') {
    return false;
  }
  if (data.scoreTrusted !== true) {
    return false;
  }
  const score = Number(data.score);
  const moves = Number(data.moves);
  const completionTimeMs = Number(data.completionTimeMs);
  if (!Number.isFinite(score) || !Number.isInteger(score)) {
    return false;
  }
  if (!Number.isFinite(moves) || !Number.isInteger(moves) || moves < 0) {
    return false;
  }
  if (
    !Number.isFinite(completionTimeMs) ||
    !Number.isInteger(completionTimeMs) ||
    completionTimeMs < 0
  ) {
    return false;
  }
  return true;
}

function toRankRow(uid, data) {
  return {
    uid,
    score: Number(data.score),
    moves: Number(data.moves),
    completionTimeMs: Number(data.completionTimeMs),
  };
}

function compareRankRows(a, b) {
  if (b.score !== a.score) {
    return b.score - a.score;
  }
  if (a.moves !== b.moves) {
    return a.moves - b.moves;
  }
  if (a.completionTimeMs !== b.completionTimeMs) {
    return a.completionTimeMs - b.completionTimeMs;
  }
  if (a.uid < b.uid) {
    return -1;
  }
  if (a.uid > b.uid) {
    return 1;
  }
  return 0;
}

/**
 * Load eligible attempts for a day and return sorted ranking (best first).
 */
async function loadEligibleRanking(dayId) {
  const snap = await attemptsCol(dayId).where('state', '==', 'Completed').get();
  const rows = [];
  snap.forEach((doc) => {
    const data = doc.data() || {};
    const uid = doc.id;
    if (!isEligibleAttempt(data, uid)) {
      return;
    }
    rows.push(toRankRow(uid, data));
  });
  rows.sort(compareRankRows);
  return rows;
}

function isDaySettled(dayData) {
  if (!dayData || typeof dayData !== 'object') {
    return false;
  }
  if (dayData.rewardSettlementVersion != null) {
    return true;
  }
  if (dayData.rewardSettledAt) {
    return true;
  }
  return false;
}

/**
 * Idempotent settlement for a closed UTC day (typically yesterday).
 * Safe to invoke multiple times.
 */
async function settleDay(dayId, options = {}) {
  if (!dayId || !/^\d{4}-\d{2}-\d{2}$/.test(dayId)) {
    return {
      ok: false,
      errorCode: 'invalid_day',
      dayId: dayId || '',
    };
  }

  const dayDocumentRef = dayRef(dayId);
  const existingDay = await dayDocumentRef.get();
  if (existingDay.exists && isDaySettled(existingDay.data())) {
    const data = existingDay.data() || {};
    return {
      ok: true,
      dayId,
      alreadySettled: true,
      firstPlaceUid: data.firstPlaceUid || null,
      rewardCreated: false,
      eligibleCount: null,
    };
  }

  const ranking = await loadEligibleRanking(dayId);
  const winner = ranking.length > 0 ? ranking[0] : null;

  const outcome = await db().runTransaction(async (tx) => {
    // All reads before writes (Firestore transaction requirement).
    const daySnap = await tx.get(dayDocumentRef);
    const dayData = daySnap.exists ? daySnap.data() || {} : {};
    const rRef = winner ? rewardRef(dayId, winner.uid) : null;
    const rewardSnap = rRef ? await tx.get(rRef) : null;

    if (isDaySettled(dayData)) {
      return {
        alreadySettled: true,
        firstPlaceUid: dayData.firstPlaceUid || null,
        rewardCreated: false,
      };
    }

    const settlementPatch = {
      dayId,
      rewardSettlementVersion: REWARD_SETTLEMENT_VERSION,
      rewardSettledAt: admin.firestore.FieldValue.serverTimestamp(),
      firstPlaceUid: winner ? winner.uid : null,
      firstPlaceRank: winner ? FIRST_PLACE_RANK : null,
      firstPlaceRewardType: winner ? REWARD_TYPE_COINS : null,
      firstPlaceRewardAmount: winner ? REWARD_AMOUNT_COINS : null,
    };

    tx.set(dayDocumentRef, settlementPatch, { merge: true });

    if (!winner) {
      return {
        alreadySettled: false,
        firstPlaceUid: null,
        rewardCreated: false,
      };
    }

    if (rewardSnap && rewardSnap.exists) {
      // Day was not marked settled but reward exists — finish settlement, no duplicate.
      return {
        alreadySettled: false,
        firstPlaceUid: winner.uid,
        rewardCreated: false,
      };
    }

    tx.set(rRef, {
      dayId,
      uid: winner.uid,
      rank: FIRST_PLACE_RANK,
      rewardType: REWARD_TYPE_COINS,
      rewardAmount: REWARD_AMOUNT_COINS,
      createdAt: admin.firestore.FieldValue.serverTimestamp(),
      claimed: false,
      claimedAt: null,
      source: REWARD_SOURCE,
      score: winner.score,
      moves: winner.moves,
      completionTimeMilliseconds: winner.completionTimeMs,
    });

    return {
      alreadySettled: false,
      firstPlaceUid: winner.uid,
      rewardCreated: true,
    };
  });

  if (outcome.rewardCreated) {
    // Structured audit log for analytics pipelines (no UID).
    console.log(
      JSON.stringify({
        event: 'daily_reward_created',
        rank: FIRST_PLACE_RANK,
        reward_type: REWARD_TYPE_COINS,
        reward_amount: REWARD_AMOUNT_COINS,
        day_id: dayId,
        source: REWARD_SOURCE,
      })
    );
  }

  return {
    ok: true,
    dayId,
    alreadySettled: !!outcome.alreadySettled,
    firstPlaceUid: outcome.firstPlaceUid || null,
    rewardCreated: !!outcome.rewardCreated,
    eligibleCount: ranking.length,
    winnerPreview: winner
      ? {
          score: winner.score,
          moves: winner.moves,
          completionTimeMs: winner.completionTimeMs,
        }
      : null,
  };
}

/**
 * Scheduled entry: settle the previous UTC day.
 */
async function settlePreviousUtcDay(serverNow = new Date()) {
  const dayId = previousUtcDayId(serverNow);
  console.log('[DailyReward] settleDailyChallengeWinners dayId=' + dayId);
  const result = await settleDay(dayId);
  console.log(
    '[DailyReward] settlement result dayId=' +
      dayId +
      ' alreadySettled=' +
      result.alreadySettled +
      ' eligibleCount=' +
      result.eligibleCount +
      ' rewardCreated=' +
      result.rewardCreated +
      ' hasWinner=' +
      !!result.firstPlaceUid
  );
  return result;
}

function rewardToClient(data) {
  return {
    dayId: data.dayId || '',
    rank: data.rank != null ? Number(data.rank) : FIRST_PLACE_RANK,
    rewardType: data.rewardType || REWARD_TYPE_COINS,
    rewardAmount:
      data.rewardAmount != null ? Number(data.rewardAmount) : REWARD_AMOUNT_COINS,
    claimed: data.claimed === true,
    source: data.source || REWARD_SOURCE,
    isDevelopmentTest: data.isDevelopmentTest === true,
    score: data.score != null ? Number(data.score) : 0,
    moves: data.moves != null ? Number(data.moves) : 0,
    completionTimeMilliseconds:
      data.completionTimeMilliseconds != null
        ? Number(data.completionTimeMilliseconds)
        : 0,
  };
}

/**
 * DEV/EDITOR smoke-test: create an unclaimed reward for request.auth.uid only.
 * Path: dailyChallenges/dev-reward-smoke-test/rewards/{uid}
 * Does not touch real UTC day docs or settlement metadata.
 * Does not emit production daily_reward_created analytics.
 */
async function createDevDailyRewardHandler(request) {
  const enabled = isDevDailyRewardCreationEnabled();
  logDevRewardGateDiagnostics(enabled);

  if (!enabled) {
    // Clear callable error (not generic INTERNAL).
    throw new HttpsError(
      'failed-precondition',
      'createDevDailyReward disabled. Set ALLOW_DEV_DAILY_REWARDS=true in functions/.env and redeploy, or use the Functions emulator.'
    );
  }

  const uid = requireAuth(request);
  const dayId = DEV_REWARD_DAY_ID;
  const rRef = rewardRef(dayId, uid);

  // Ensure parent day doc is a clear non-production marker only (no settlement fields).
  const dRef = dayRef(dayId);
  const existingReward = await rRef.get();
  if (existingReward.exists) {
    const data = existingReward.data() || {};
    console.log(
      '[DailyReward] createDevDailyReward idempotent hit uidLen=' +
        uid.length +
        ' claimed=' +
        (data.claimed === true)
    );
    return {
      success: true,
      created: false,
      alreadyExists: true,
      reward: rewardToClient({ ...data, dayId, uid }),
    };
  }

  const created = {
    dayId,
    uid,
    rank: FIRST_PLACE_RANK,
    rewardType: REWARD_TYPE_COINS,
    rewardAmount: REWARD_AMOUNT_COINS,
    createdAt: admin.firestore.FieldValue.serverTimestamp(),
    claimed: false,
    claimedAt: null,
    source: REWARD_SOURCE_DEV,
    isDevelopmentTest: true,
    score: 0,
    moves: 0,
    completionTimeMilliseconds: 0,
  };

  await db().runTransaction(async (tx) => {
    const snap = await tx.get(rRef);
    if (snap.exists) {
      return;
    }
    const daySnap = await tx.get(dRef);
    if (!daySnap.exists) {
      tx.set(dRef, {
        dayId,
        isDevelopmentTest: true,
        purpose: 'dev_reward_smoke_test',
        note: 'Not a real UTC Daily Challenge day. Settlement must ignore this id.',
      });
    }
    tx.set(rRef, created);
  });

  const after = await rRef.get();
  const data = after.exists ? after.data() || created : created;
  console.log(
    '[DailyReward] createDevDailyReward created dayId=' +
      dayId +
      ' source=' +
      REWARD_SOURCE_DEV +
      ' (no production analytics)'
  );

  return {
    success: true,
    created: true,
    alreadyExists: false,
    reward: rewardToClient({ ...data, dayId, uid }),
  };
}

/**
 * Callable: unclaimed rewards for request.auth.uid only.
 * Anonymous UIDs are eligible — Google Play link is NOT required.
 */
async function getPendingDailyRewardsHandler(request) {
  const uid = requireAuth(request);
  const snap = await db()
    .collectionGroup('rewards')
    .where('uid', '==', uid)
    .where('claimed', '==', false)
    .get();

  const rewards = [];
  snap.forEach((doc) => {
    const data = doc.data() || {};
    if (data.uid !== uid) {
      return;
    }
    if (data.claimed === true) {
      return;
    }
    rewards.push(rewardToClient(data));
  });

  rewards.sort((a, b) => {
    if (a.dayId < b.dayId) {
      return 1;
    }
    if (a.dayId > b.dayId) {
      return -1;
    }
    return 0;
  });

  return {
    success: true,
    rewards,
  };
}

/**
 * Callable: claim one reward for request.auth.uid.
 * Idempotent: AlreadyClaimed does not return a second spendable grant.
 */
async function claimDailyRewardHandler(request) {
  const uid = requireAuth(request);
  const data = request.data || {};
  const rawDayId = typeof data.dayId === 'string' ? data.dayId.trim() : '';
  const dayId = isValidClaimDayId(rawDayId) ? rawDayId : '';

  if (!dayId) {
    return {
      success: false,
      errorCode: 'invalid_day',
      errorMessage: 'Invalid dayId.',
      granted: false,
    };
  }

  const rRef = rewardRef(dayId, uid);
  const outcome = await db().runTransaction(async (tx) => {
    const snap = await tx.get(rRef);
    if (!snap.exists) {
      return { status: 'not_found' };
    }

    const reward = snap.data() || {};
    if (reward.uid && reward.uid !== uid) {
      return { status: 'denied' };
    }

    if (reward.claimed === true) {
      return { status: 'already_claimed', reward };
    }

    tx.update(rRef, {
      claimed: true,
      claimedAt: admin.firestore.FieldValue.serverTimestamp(),
    });

    return { status: 'claimed', reward };
  });

  if (outcome.status === 'not_found') {
    return {
      success: false,
      errorCode: 'not_found',
      errorMessage: 'No reward for this day.',
      granted: false,
      dayId,
    };
  }

  if (outcome.status === 'denied') {
    return {
      success: false,
      errorCode: 'denied',
      errorMessage: 'Reward ownership mismatch.',
      granted: false,
      dayId,
    };
  }

  const client = rewardToClient(outcome.reward || {});
  const granted = outcome.status === 'claimed';
  const isDevTest =
    client.isDevelopmentTest === true || dayId === DEV_REWARD_DAY_ID;

  // Production analytics only — suppress for isDevelopmentTest smoke rewards.
  if (granted && !isDevTest) {
    console.log(
      JSON.stringify({
        event: 'daily_reward_claimed',
        rank: client.rank,
        reward_type: client.rewardType,
        reward_amount: client.rewardAmount,
        day_id: dayId,
      })
    );
  }

  return {
    success: true,
    alreadyClaimed: outcome.status === 'already_claimed',
    granted,
    dayId,
    rank: client.rank,
    rewardType: client.rewardType,
    rewardAmount: client.rewardAmount,
    isDevelopmentTest: isDevTest,
    errorCode: outcome.status === 'already_claimed' ? 'already_claimed' : '',
  };
}

module.exports = {
  REWARD_SETTLEMENT_VERSION,
  FIRST_PLACE_RANK,
  REWARD_TYPE_COINS,
  REWARD_AMOUNT_COINS,
  REWARD_SOURCE,
  DEV_REWARD_DAY_ID,
  REWARD_SOURCE_DEV,
  previousUtcDayId,
  isEligibleAttempt,
  compareRankRows,
  loadEligibleRanking,
  settleDay,
  settlePreviousUtcDay,
  getPendingDailyRewardsHandler,
  claimDailyRewardHandler,
  createDevDailyRewardHandler,
  isDevDailyRewardCreationEnabled,
};
