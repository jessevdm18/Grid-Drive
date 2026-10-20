'use strict';

/**
 * Daily prize settlement + claim.
 *
 * Reward settlement version 1 (historical, already settled):
 *   real Completed + scoreTrusted attempts only. Rank #1 gets 1000 coins.
 *   Those days are never re-ranked with bots.
 *
 * Reward settlement version 2 (new / still unsettled days):
 *   the same real attempts plus the day's frozen simulated entries.
 *   Prize positions are #1 / #2 / #3 on that full hybrid list.
 *   A bot occupies the position and receives nothing. The next real player
 *   is not promoted into that slot.
 *
 * Hybrid order:
 *   1. score DESC
 *   2. moves ASC
 *   3. completionTimeMs ASC
 *   4. tieBreakId ASC (real UID, or the stable simulated entry id)
 *
 * Friends ranking does not use this hybrid list and never creates rewards.
 *
 * TRUST BOUNDARY: real moves and completionTimeMs remain client-reported.
 * Simulated scores are server-generated and stored. Client bot positions
 * are ignored.
 */

const admin = require('firebase-admin');
const {
  queueDailyResultNotifications,
  resultNotificationRef,
} = require('./resultNotifications');
const {
  ensureOfficialSimulatedEntries,
  loadStoredSimulatedEntries,
} = require('./simulatedEntries');
const { HttpsError } = require('firebase-functions/v2/https');
const { defineBoolean } = require('firebase-functions/params');
const { requireAuth, utcDayId } = require('./validation');

/** Historical real-only settlement. Do not rewrite those days. */
const LEGACY_REWARD_SETTLEMENT_VERSION = 1;
/** Hybrid real + official simulated settlement. New unsettled days only. */
const REWARD_SETTLEMENT_VERSION = 2;
const FIRST_PLACE_RANK = 1;
const REWARD_TYPE_COINS = 'coins';
const REWARD_AMOUNT_COINS = 1000;
const PRIZE_AMOUNT_BY_RANK = {
  1: 1000,
  2: 500,
  3: 250,
};
const REWARD_SOURCE = 'daily_first_place';
const REWARD_SOURCE_BY_RANK = {
  1: 'daily_first_place',
  2: 'daily_second_place',
  3: 'daily_third_place',
};
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
 * Hybrid order. tieBreakId is the real UID or the stable simulated entry id.
 * Friends ranking keeps compareRankRows and never calls this.
 */
function compareHybridRows(a, b) {
  if (b.score !== a.score) {
    return b.score - a.score;
  }
  if (a.moves !== b.moves) {
    return a.moves - b.moves;
  }
  if (a.completionTimeMs !== b.completionTimeMs) {
    return a.completionTimeMs - b.completionTimeMs;
  }
  const at = a.tieBreakId || a.uid || '';
  const bt = b.tieBreakId || b.uid || '';
  if (at < bt) {
    return -1;
  }
  if (at > bt) {
    return 1;
  }
  return 0;
}

function rankHybrid(rows) {
  const copy = (Array.isArray(rows) ? rows : []).map((row) => ({ ...row }));
  copy.sort(compareHybridRows);
  for (let i = 0; i < copy.length; i++) {
    copy[i].rank = i + 1;
  }
  return copy;
}

function prizeAmountForRank(rank) {
  return Object.prototype.hasOwnProperty.call(PRIZE_AMOUNT_BY_RANK, rank)
    ? PRIZE_AMOUNT_BY_RANK[rank]
    : 0;
}

/**
 * Prize slots on an already-sorted hybrid list.
 * A bot consumes the slot and creates no reward. Nobody is promoted.
 */
function prizesForRanking(ranking) {
  const prizes = [];
  for (let rank = 1; rank <= 3; rank++) {
    const row = ranking && ranking[rank - 1];
    if (!row || row.isSimulated || !row.uid) {
      prizes.push({
        rank,
        rewarded: false,
        amount: 0,
        uid: null,
      });
      continue;
    }
    prizes.push({
      rank,
      rewarded: true,
      amount: prizeAmountForRank(rank),
      uid: row.uid,
      score: row.score,
      moves: row.moves,
      completionTimeMs: row.completionTimeMs,
      source: REWARD_SOURCE_BY_RANK[rank] || REWARD_SOURCE,
    });
  }
  return prizes;
}

function daySettlementVersion(dayData) {
  if (!dayData || typeof dayData !== 'object') {
    return 0;
  }
  const version = Number(dayData.rewardSettlementVersion);
  if (version === REWARD_SETTLEMENT_VERSION) {
    return REWARD_SETTLEMENT_VERSION;
  }
  if (version === LEGACY_REWARD_SETTLEMENT_VERSION || dayData.rewardSettledAt) {
    return LEGACY_REWARD_SETTLEMENT_VERSION;
  }
  return 0;
}

/**
 * Real eligible attempts only. Friends and version-1 settlement use this.
 */
async function loadRealRanking(dayId) {
  const snap = await attemptsCol(dayId).where('state', '==', 'Completed').get();
  const rows = [];
  snap.forEach((doc) => {
    const data = doc.data() || {};
    const uid = doc.id;
    if (!isEligibleAttempt(data, uid)) {
      return;
    }
    const row = toRankRow(uid, data);
    rows.push({
      isSimulated: false,
      uid: row.uid,
      tieBreakId: row.uid,
      score: row.score,
      moves: row.moves,
      completionTimeMs: row.completionTimeMs,
    });
  });
  return rankHybrid(rows);
}

/** @deprecated name kept for callers. Real attempts only — not the hybrid board. */
async function loadEligibleRanking(dayId) {
  return loadRealRanking(dayId);
}

async function loadHybridRanking(dayId) {
  const reals = await loadRealRanking(dayId);
  const bots = await loadStoredSimulatedEntries(dayId);
  const rows = [];
  for (let i = 0; i < reals.length; i++) {
    rows.push(reals[i]);
  }
  for (let i = 0; i < bots.length; i++) {
    const bot = bots[i];
    rows.push({
      isSimulated: true,
      uid: '',
      entryId: bot.entryId,
      tieBreakId: bot.tieBreakId,
      displayName: bot.displayName,
      score: bot.score,
      moves: bot.moves,
      completionTimeMs: bot.completionTimeMs,
    });
  }
  return rankHybrid(rows);
}

async function loadSettlementRanking(dayId, version) {
  if (version === REWARD_SETTLEMENT_VERSION) {
    const bots = await loadStoredSimulatedEntries(dayId);
    if (!bots.length) {
      const err = new Error(
        'Hybrid settlement is missing official simulated entries for ' + dayId
      );
      err.code = 'sim_entries_missing';
      throw err;
    }
    return loadHybridRanking(dayId);
  }
  return loadRealRanking(dayId);
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
    const version = daySettlementVersion(data);
    if (data.finalRanksPublished !== true) {
      const existingRanking = await loadSettlementRanking(dayId, version);
      await publishFinalRanks(dayId, existingRanking, version);
      await queueDailyResultNotifications(dayId, existingRanking);
    } else {
      const existingRanking = await loadSettlementRanking(dayId, version);
      await queueDailyResultNotifications(dayId, existingRanking);
    }
    return {
      ok: true,
      dayId,
      alreadySettled: true,
      settlementVersion: version,
      firstPlaceUid: data.firstPlaceUid || null,
      rewardCreated: false,
      eligibleCount: null,
    };
  }

  const ensured = await ensureOfficialSimulatedEntries(dayId);
  if (!ensured.ok) {
    const err = new Error(
      'Official simulated entries were not frozen for ' + dayId
    );
    err.code = ensured.errorCode || 'sim_entries';
    throw err;
  }

  const ranking = await loadSettlementRanking(dayId, REWARD_SETTLEMENT_VERSION);
  const prizes = prizesForRanking(ranking);
  const rewarded = prizes.filter((prize) => prize.rewarded);
  const firstPrize = rewarded.find((prize) => prize.rank === FIRST_PLACE_RANK) || null;

  const outcome = await db().runTransaction(async (tx) => {
    // All reads before writes (Firestore transaction requirement).
    const daySnap = await tx.get(dayDocumentRef);
    const dayData = daySnap.exists ? daySnap.data() || {} : {};
    const rewardSnaps = [];
    for (let i = 0; i < rewarded.length; i++) {
      rewardSnaps.push({
        prize: rewarded[i],
        snap: await tx.get(rewardRef(dayId, rewarded[i].uid)),
      });
    }

    if (isDaySettled(dayData)) {
      return {
        alreadySettled: true,
        firstPlaceUid: dayData.firstPlaceUid || null,
        created: [],
      };
    }

    const slotUid = (rank) => {
      const prize = prizes[rank - 1];
      return prize && prize.rewarded ? prize.uid : null;
    };

    tx.set(
      dayDocumentRef,
      {
        dayId,
        rankingVersion: REWARD_SETTLEMENT_VERSION,
        rewardSettlementVersion: REWARD_SETTLEMENT_VERSION,
        rewardSettledAt: admin.firestore.FieldValue.serverTimestamp(),
        firstPlaceUid: slotUid(1),
        firstPlaceRank: slotUid(1) ? FIRST_PLACE_RANK : null,
        firstPlaceRewardType: slotUid(1) ? REWARD_TYPE_COINS : null,
        firstPlaceRewardAmount: slotUid(1) ? PRIZE_AMOUNT_BY_RANK[1] : null,
        secondPlaceUid: slotUid(2),
        secondPlaceRewardAmount: slotUid(2) ? PRIZE_AMOUNT_BY_RANK[2] : null,
        thirdPlaceUid: slotUid(3),
        thirdPlaceRewardAmount: slotUid(3) ? PRIZE_AMOUNT_BY_RANK[3] : null,
      },
      { merge: true }
    );

    const created = [];
    for (let i = 0; i < rewardSnaps.length; i++) {
      const item = rewardSnaps[i];
      if (item.snap.exists) {
        continue;
      }
      const prize = item.prize;
      tx.set(rewardRef(dayId, prize.uid), {
        dayId,
        uid: prize.uid,
        rank: prize.rank,
        finalRank: prize.rank,
        rewardType: REWARD_TYPE_COINS,
        rewardAmount: prize.amount,
        amount: prize.amount,
        createdAt: admin.firestore.FieldValue.serverTimestamp(),
        claimed: false,
        claimedAt: null,
        source: prize.source,
        score: prize.score,
        moves: prize.moves,
        completionTimeMilliseconds: prize.completionTimeMs,
        rewardSettlementVersion: REWARD_SETTLEMENT_VERSION,
      });
      created.push({
        rank: prize.rank,
        amount: prize.amount,
        source: prize.source,
      });
    }

    return {
      alreadySettled: false,
      firstPlaceUid: firstPrize ? firstPrize.uid : null,
      created,
    };
  });

  if (!outcome.alreadySettled) {
    await publishFinalRanks(dayId, ranking, REWARD_SETTLEMENT_VERSION);
    await queueDailyResultNotifications(dayId, ranking);
  }

  const created = Array.isArray(outcome.created) ? outcome.created : [];
  for (let i = 0; i < created.length; i++) {
    const prize = created[i];
    console.log(
      JSON.stringify({
        event: 'daily_reward_created',
        rank: prize.rank,
        reward_type: REWARD_TYPE_COINS,
        reward_amount: prize.amount,
        day_id: dayId,
        source: prize.source,
      })
    );
  }

  const realCount = ranking.filter((row) => row && !row.isSimulated && row.uid).length;
  return {
    ok: true,
    dayId,
    alreadySettled: !!outcome.alreadySettled,
    settlementVersion: REWARD_SETTLEMENT_VERSION,
    firstPlaceUid: outcome.firstPlaceUid || null,
    rewardCreated: created.length > 0,
    prizesCreated: created.length,
    eligibleCount: realCount,
    hybridCount: ranking.length,
    winnerPreview: firstPrize
      ? {
          score: firstPrize.score,
          moves: firstPrize.moves,
          completionTimeMs: firstPrize.completionTimeMs,
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
      ' prizesCreated=' +
      (result.prizesCreated != null ? result.prizesCreated : 0) +
      ' settlementVersion=' +
      (result.settlementVersion != null ? result.settlementVersion : '') +
      ' hasFirstPlaceReward=' +
      !!result.firstPlaceUid
  );
  return result;
}

const SETTLED_RESULT_LOOKBACK_DAYS = 14;
const FINAL_RANK_BATCH = 400;

/**
 * 1-based rank in an already-sorted hybrid or real-only list.
 * Simulated rows have no uid and are never returned.
 */
function findPlayerRank(ranking, uid) {
  if (!uid || !Array.isArray(ranking)) {
    return 0;
  }
  for (let i = 0; i < ranking.length; i++) {
    const row = ranking[i];
    if (!row || row.isSimulated || row.uid !== uid) {
      continue;
    }
    return row.rank != null ? Number(row.rank) : i + 1;
  }
  return 0;
}

function utcDayIdDaysAgo(serverNow, daysAgo) {
  const t = Date.UTC(
    serverNow.getUTCFullYear(),
    serverNow.getUTCMonth(),
    serverNow.getUTCDate() - daysAgo,
    0,
    0,
    0,
    0
  );
  return utcDayId(new Date(t));
}

/**
 * Write finalRank onto real attempt docs only.
 * The rank is the position in the list that was settled (hybrid for version 2).
 * Bots have no attempt document.
 */
async function publishFinalRanks(dayId, ranking, version) {
  const stamp =
    version === REWARD_SETTLEMENT_VERSION
      ? REWARD_SETTLEMENT_VERSION
      : LEGACY_REWARD_SETTLEMENT_VERSION;
  const rows = (Array.isArray(ranking) ? ranking : []).filter(
    (row) => row && !row.isSimulated && row.uid && Number.isInteger(Number(row.rank)) && Number(row.rank) > 0
  );
  for (let i = 0; i < rows.length; i += FINAL_RANK_BATCH) {
    const batch = db().batch();
    const slice = rows.slice(i, i + FINAL_RANK_BATCH);
    let writes = 0;
    for (let j = 0; j < slice.length; j++) {
      const row = slice[j];
      batch.set(
        attemptsCol(dayId).doc(row.uid),
        {
          finalRank: Number(row.rank),
          finalRankSettlementVersion: stamp,
        },
        { merge: true }
      );
      writes += 1;
    }
    if (writes > 0) {
      await batch.commit();
    }
  }

  await dayRef(dayId).set(
    {
      finalRanksPublished: true,
    },
    { merge: true }
  );
}

function storedRewardRank(data) {
  const finalRank = Number(data && data.finalRank);
  if (Number.isInteger(finalRank) && finalRank > 0) {
    return finalRank;
  }
  const rank = Number(data && data.rank);
  if (Number.isInteger(rank) && rank > 0) {
    return rank;
  }
  return FIRST_PLACE_RANK;
}

function storedRewardAmount(data) {
  const amount = Number(data && data.amount);
  if (Number.isFinite(amount)) {
    return amount;
  }
  const rewardAmount = Number(data && data.rewardAmount);
  if (Number.isFinite(rewardAmount)) {
    return rewardAmount;
  }
  const ranked = prizeAmountForRank(storedRewardRank(data));
  return ranked > 0 ? ranked : REWARD_AMOUNT_COINS;
}

function rewardToClient(data) {
  const rank = storedRewardRank(data);
  const amount = storedRewardAmount(data);
  return {
    dayId: data.dayId || '',
    rank,
    finalRank: rank,
    rewardType: data.rewardType || REWARD_TYPE_COINS,
    rewardAmount: amount,
    amount,
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
 * Claim proves the result panel was shown. Cancel the unsent result push.
 * Does not modify delivery sentAt records.
 */
function cancelQueuedResultInTransaction(tx, noteRef, note, dayId, uid) {
  if (note && note.cancelledAt) {
    return;
  }
  const patch = {
    cancelledAt: admin.firestore.FieldValue.serverTimestamp(),
    cancelReason: 'reward_claimed',
  };
  if (!note) {
    patch.dayId = dayId;
    patch.uid = uid;
  }
  tx.set(noteRef, patch, { merge: true });
}

/**
 * Callable: claim one reward for request.auth.uid.
 * Idempotent: AlreadyClaimed does not return a second spendable grant.
 * A successful or already-claimed reward cancels that day's result notification.
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
  const noteRef = resultNotificationRef(dayId, uid);
  const outcome = await db().runTransaction(async (tx) => {
    const snap = await tx.get(rRef);
    const noteSnap = await tx.get(noteRef);
    if (!snap.exists) {
      return { status: 'not_found' };
    }

    const reward = snap.data() || {};
    if (reward.uid && reward.uid !== uid) {
      return { status: 'denied' };
    }

    const note = noteSnap.exists ? noteSnap.data() || {} : null;
    if (reward.claimed === true) {
      cancelQueuedResultInTransaction(tx, noteRef, note, dayId, uid);
      return { status: 'already_claimed', reward };
    }

    tx.update(rRef, {
      claimed: true,
      claimedAt: admin.firestore.FieldValue.serverTimestamp(),
    });
    cancelQueuedResultInTransaction(tx, noteRef, note, dayId, uid);

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

function parseAcknowledgedDayIds(data) {
  const out = new Set();
  const raw = data && data.acknowledgedDayIds;
  if (!Array.isArray(raw)) {
    return out;
  }
  const limit = Math.min(raw.length, 30);
  for (let i = 0; i < limit; i++) {
    const id = raw[i];
    if (typeof id === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(id)) {
      out.add(id);
    }
  }
  return out;
}

/**
 * Server rank for one real eligible attempt.
 * A stored version 1 or version 2 rank is trusted and is not recomputed.
 * A missing rank is filled with the algorithm that settled that day.
 * Version 1 stays real-only. Version 2 uses the frozen hybrid board.
 */
async function resolveFinalRank(dayId, uid, attemptData, dayData) {
  const stored = Number(attemptData && attemptData.finalRank);
  const storedVersion = Number(attemptData && attemptData.finalRankSettlementVersion);
  if (
    Number.isInteger(stored) &&
    stored > 0 &&
    (storedVersion === LEGACY_REWARD_SETTLEMENT_VERSION ||
      storedVersion === REWARD_SETTLEMENT_VERSION)
  ) {
    return stored;
  }

  const version = daySettlementVersion(dayData);
  const ranking = await loadSettlementRanking(
    dayId,
    version === REWARD_SETTLEMENT_VERSION
      ? REWARD_SETTLEMENT_VERSION
      : LEGACY_REWARD_SETTLEMENT_VERSION
  );
  const rank = findPlayerRank(ranking, uid);
  if (rank > 0) {
    await attemptsCol(dayId).doc(uid).set(
      {
        finalRank: rank,
        finalRankSettlementVersion:
          version === REWARD_SETTLEMENT_VERSION
            ? REWARD_SETTLEMENT_VERSION
            : LEGACY_REWARD_SETTLEMENT_VERSION,
      },
      { merge: true }
    );
  }
  return rank;
}

/**
 * Newest settled Daily the caller completed and has not acknowledged.
 * Today is never returned (rank is not final until settlement).
 * Does not create rewards. Rank #1 coins stay on the existing reward doc.
 */
async function getSettledDailyResultHandler(request) {
  const uid = requireAuth(request);
  const acknowledged = parseAcknowledgedDayIds(request.data || {});
  const serverNow = new Date();

  for (let daysAgo = 1; daysAgo <= SETTLED_RESULT_LOOKBACK_DAYS; daysAgo++) {
    const dayId = utcDayIdDaysAgo(serverNow, daysAgo);
    if (acknowledged.has(dayId)) {
      continue;
    }

    const daySnap = await dayRef(dayId).get();
    if (!daySnap.exists || !isDaySettled(daySnap.data())) {
      continue;
    }

    const attemptSnap = await attemptsCol(dayId).doc(uid).get();
    if (!attemptSnap.exists) {
      continue;
    }

    const attempt = attemptSnap.data() || {};
    if (!isEligibleAttempt(attempt, uid)) {
      continue;
    }

    const rank = await resolveFinalRank(dayId, uid, attempt, daySnap.data());
    if (rank <= 0) {
      continue;
    }

    let hasUnclaimedReward = false;
    let rewardAmount = 0;
    if (rank >= 1 && rank <= 3) {
      const rewardSnap = await rewardRef(dayId, uid).get();
      if (rewardSnap.exists) {
        const reward = rewardSnap.data() || {};
        hasUnclaimedReward = reward.claimed !== true;
        rewardAmount = storedRewardAmount(reward);
      }
    }

    return {
      success: true,
      found: true,
      settled: true,
      dayId,
      rank,
      finalRank: rank,
      score: Number(attempt.score),
      moves: Number(attempt.moves),
      completionTimeMs: Number(attempt.completionTimeMs),
      hasUnclaimedReward,
      rewardAmount,
    };
  }

  return {
    success: true,
    found: false,
    settled: false,
  };
}

module.exports = {
  LEGACY_REWARD_SETTLEMENT_VERSION,
  REWARD_SETTLEMENT_VERSION,
  FIRST_PLACE_RANK,
  REWARD_TYPE_COINS,
  REWARD_AMOUNT_COINS,
  PRIZE_AMOUNT_BY_RANK,
  REWARD_SOURCE,
  DEV_REWARD_DAY_ID,
  REWARD_SOURCE_DEV,
  previousUtcDayId,
  isEligibleAttempt,
  compareRankRows,
  compareHybridRows,
  rankHybrid,
  prizesForRanking,
  findPlayerRank,
  loadEligibleRanking,
  settleDay,
  settlePreviousUtcDay,
  getPendingDailyRewardsHandler,
  claimDailyRewardHandler,
  createDevDailyRewardHandler,
  getSettledDailyResultHandler,
  isDevDailyRewardCreationEnabled,
};
