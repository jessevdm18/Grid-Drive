'use strict';

/**
 * Official Daily simulated entries.
 *
 * This is a direct port of Unity SimulatedDailyLeaderboardProvider,
 * SimulatedDailyLeaderboardNames, and DailyChallengeDeterministicRng.
 * Settlement and the Global leaderboard both read the stored documents.
 * Clients never submit these scores.
 *
 * Already-settled days are left untouched. Entries are written once for an
 * unsettled day and then frozen, so a later generator change cannot rewrite them.
 */

const admin = require('firebase-admin');
const { getLevels, getScoringConfig } = require('./config');
const { stableHash32, selectLevelIndexForDay } = require('./hash');
const { calculateScoreV1 } = require('./scoring');

const SIMULATED_ENTRY_COUNT = 30;
const RANKING_VERSION = 2;
const SEED_SALT = 'RushOut.DailyLeaderboard.Simulated.v1';
const CROSS_BRACKET_CHANCE = Math.fround(0.15);

const FIRST_NAMES = [
  'Alex', 'Maya', 'Luca', 'Emma', 'Noah', 'Sofia', 'Leo', 'Mila', 'Sam', 'Nora',
  'Kai', 'Lina', 'Max', 'Aya', 'Finn', 'Zoe', 'Theo', 'Sara', 'Eli', 'Mia',
  'Owen', 'Iris', 'Jonas', 'Hana', 'Felix', 'Clara', 'Hugo', 'Yuna', 'Nico', 'Lara',
  'Omar', 'Ines', 'Ravi', 'Noor', 'Elias', 'Amira', 'Jasper', 'Leila', 'Mateo', 'Anya',
  'Soren', 'Tara', 'Ivo', 'Vera', 'Remy', 'Nia', 'Arlo', 'Sena', 'Pavel', 'Kira',
];

const SURNAME_INITIALS = [
  'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K',
  'L', 'M', 'N', 'P', 'R', 'S', 'T', 'V', 'W', 'Y',
];

function db() {
  return admin.firestore();
}

function dayRef(dayId) {
  return db().collection('dailyChallenges').doc(dayId);
}

/**
 * C# uint xorshift32. The BigInt form is the reference; xorShift32 must match it.
 * JS `<<` is a signed int32 shift, so the result is always coerced with `>>> 0`.
 */
function xorShift32(state) {
  let x = state >>> 0;
  x = (x ^ (x << 13)) >>> 0;
  x = (x ^ (x >>> 17)) >>> 0;
  x = (x ^ (x << 5)) >>> 0;
  return x >>> 0;
}

function createRng(seed) {
  const normalized = seed >>> 0;
  let state = normalized === 0 ? 0xa341316c : normalized;

  function nextUint() {
    state = xorShift32(state);
    return state;
  }

  return {
    nextInt(minInclusive, maxExclusive) {
      if (maxExclusive <= minInclusive) {
        return minInclusive;
      }
      const span = (maxExclusive - minInclusive) >>> 0;
      return minInclusive + (nextUint() % span);
    },
    nextFloat01() {
      return (nextUint() & 0x00ffffff) / 16777216;
    },
  };
}

function buildDisplayName(rng, index) {
  const first = rng.nextInt(0, FIRST_NAMES.length);
  const initial =
    (rng.nextInt(0, SURNAME_INITIALS.length) + index) % SURNAME_INITIALS.length;
  return FIRST_NAMES[first] + ' ' + SURNAME_INITIALS[initial] + '.';
}

function pickUniqueName(rng, index, used) {
  for (let attempt = 0; attempt < 64; attempt++) {
    const name = buildDisplayName(rng, index + attempt * 17);
    if (!used.has(name)) {
      used.add(name);
      return name;
    }
  }
  const fallback = 'Player ' + (index + 1);
  used.add(fallback);
  return fallback;
}

/** 10% +0, 25% +1–2, 35% +3–5, 20% +6–9, 10% +10–15. */
function sampleExtraMoves(rng) {
  const roll = rng.nextInt(0, 100);
  if (roll < 10) {
    return 0;
  }
  if (roll < 35) {
    return rng.nextInt(1, 3);
  }
  if (roll < 70) {
    return rng.nextInt(3, 6);
  }
  if (roll < 90) {
    return rng.nextInt(6, 10);
  }
  return rng.nextInt(10, 16);
}

function sampleCompletionTimeMs(rng, extraMoves) {
  let minSec;
  let maxSec;
  if (extraMoves <= 0) {
    minSec = 28;
    maxSec = 55;
  } else if (extraMoves <= 2) {
    minSec = 32;
    maxSec = 75;
  } else if (extraMoves <= 5) {
    minSec = 50;
    maxSec = 140;
  } else if (extraMoves <= 9) {
    minSec = 90;
    maxSec = 220;
  } else {
    minSec = 140;
    maxSec = 300;
  }

  if (rng.nextFloat01() < CROSS_BRACKET_CHANCE) {
    minSec = Math.max(22, minSec - 12);
    maxSec = Math.min(320, maxSec + 25);
  }

  const seconds = rng.nextInt(minSec, maxSec + 1);
  const centiseconds = rng.nextInt(0, 100);
  return seconds * 1000 + centiseconds * 10;
}

function entryIdFor(dayId, index) {
  const suffix = String(index).padStart(3, '0');
  return 'sim_' + dayId + '_' + suffix;
}

/**
 * Full official bot list for one day. Same inputs always return the same rows.
 * `levelId` is the Unity asset name (Daily_00N). `minimumMoves` is the day's par.
 */
function generateSimulatedEntries(options) {
  const dayId = options && options.dayId ? String(options.dayId) : '';
  const levelId = options && options.levelId ? String(options.levelId) : '';
  const minimumMoves = Number(options && options.minimumMoves);
  const count =
    options && options.count != null
      ? Math.max(0, options.count | 0)
      : SIMULATED_ENTRY_COUNT;
  const scoring = getScoringConfig();
  const seed = stableHash32(dayId + '|' + levelId + '|' + SEED_SALT);
  const rng = createRng(seed);
  const used = new Set();
  const entries = [];

  for (let i = 0; i < count; i++) {
    const displayName = pickUniqueName(rng, i, used);
    const extraMoves = sampleExtraMoves(rng);
    const moves = Math.max(1, minimumMoves | 0) + extraMoves;
    const completionTimeMs = sampleCompletionTimeMs(rng, extraMoves);
    const scored = calculateScoreV1(
      scoring,
      moves,
      completionTimeMs,
      minimumMoves | 0
    );
    const entryId = entryIdFor(dayId, i);
    entries.push({
      entryId,
      tieBreakId: entryId,
      displayName,
      score: scored.totalScore,
      moves,
      completionTimeMs,
      isSimulated: true,
      rankingVersion: RANKING_VERSION,
    });
  }

  return entries;
}

function isDaySettled(dayData) {
  if (!dayData || typeof dayData !== 'object') {
    return false;
  }
  if (dayData.rewardSettlementVersion != null) {
    return true;
  }
  return !!dayData.rewardSettledAt;
}

function levelInputs(dayId, dayData) {
  const levels = getLevels();
  const idx = selectLevelIndexForDay(dayId, levels.length);
  const selected = idx >= 0 ? levels[idx] : null;
  const levelId =
    dayData && typeof dayData.levelId === 'string' && dayData.levelId
      ? dayData.levelId
      : selected && selected.levelId
        ? selected.levelId
        : '';
  let minimumMoves = 0;
  const storedMoves = Number(dayData && dayData.minimumMoves);
  if (Number.isInteger(storedMoves) && storedMoves > 0) {
    minimumMoves = storedMoves;
  } else if (selected && Number.isInteger(Number(selected.minimumMoves))) {
    minimumMoves = Number(selected.minimumMoves);
  }
  if (!levelId || !Number.isInteger(minimumMoves) || minimumMoves <= 0) {
    return null;
  }
  return { levelId, minimumMoves };
}

function isFrozen(dayData) {
  return (
    !!dayData &&
    dayData.simulatedEntriesFrozen === true &&
    Number(dayData.rankingVersion) === RANKING_VERSION
  );
}

/**
 * Write the day's official bots once. No-op when the day is already settled
 * or the list is already frozen. Does not stamp reward settlement.
 */
async function ensureOfficialSimulatedEntries(dayId) {
  if (!dayId || !/^\d{4}-\d{2}-\d{2}$/.test(dayId)) {
    return { ok: false, errorCode: 'invalid_day' };
  }

  const ref = dayRef(dayId);
  const existing = await ref.get();
  const existingData = existing.exists ? existing.data() || {} : {};
  if (isDaySettled(existingData)) {
    return { ok: true, skipped: true, reason: 'settled' };
  }
  if (isFrozen(existingData)) {
    return {
      ok: true,
      skipped: true,
      reason: 'frozen',
      count: Number(existingData.simulatedEntryCount) || SIMULATED_ENTRY_COUNT,
    };
  }

  const inputs = levelInputs(dayId, existingData);
  if (!inputs) {
    return { ok: false, errorCode: 'no_level' };
  }

  const entries = generateSimulatedEntries({
    dayId,
    levelId: inputs.levelId,
    minimumMoves: inputs.minimumMoves,
    count: SIMULATED_ENTRY_COUNT,
  });

  await db().runTransaction(async (tx) => {
    const snap = await tx.get(ref);
    const current = snap.exists ? snap.data() || {} : {};
    if (isDaySettled(current) || isFrozen(current)) {
      return;
    }

    for (let i = 0; i < entries.length; i++) {
      const entry = entries[i];
      tx.set(ref.collection('simulatedEntries').doc(entry.entryId), {
        entryId: entry.entryId,
        tieBreakId: entry.tieBreakId,
        displayName: entry.displayName,
        score: entry.score,
        moves: entry.moves,
        completionTimeMs: entry.completionTimeMs,
        isSimulated: true,
        rankingVersion: RANKING_VERSION,
      });
    }

    const patch = {
      dayId,
      rankingVersion: RANKING_VERSION,
      simulatedEntriesFrozen: true,
      simulatedEntryCount: entries.length,
      simulatedLevelId: inputs.levelId,
      simulatedMinimumMoves: inputs.minimumMoves,
    };
    if (!current.levelId) {
      patch.levelId = inputs.levelId;
      patch.minimumMoves = inputs.minimumMoves;
    }
    tx.set(ref, patch, { merge: true });
  });

  return { ok: true, skipped: false, count: entries.length };
}

function isValidBot(data, docId) {
  if (!data || data.isSimulated !== true) {
    return false;
  }
  const score = Number(data.score);
  const moves = Number(data.moves);
  const completionTimeMs = Number(data.completionTimeMs);
  if (!Number.isInteger(score) || !Number.isInteger(moves) || moves < 0) {
    return false;
  }
  if (!Number.isInteger(completionTimeMs) || completionTimeMs < 0) {
    return false;
  }
  const entryId =
    typeof data.entryId === 'string' && data.entryId ? data.entryId : docId;
  if (!entryId) {
    return false;
  }
  return true;
}

async function loadStoredSimulatedEntries(dayId) {
  const snap = await dayRef(dayId).collection('simulatedEntries').get();
  const rows = [];
  snap.forEach((doc) => {
    const data = doc.data() || {};
    if (!isValidBot(data, doc.id)) {
      return;
    }
    const entryId =
      typeof data.entryId === 'string' && data.entryId ? data.entryId : doc.id;
    const tieBreakId =
      typeof data.tieBreakId === 'string' && data.tieBreakId
        ? data.tieBreakId
        : entryId;
    rows.push({
      isSimulated: true,
      uid: '',
      entryId,
      tieBreakId,
      displayName:
        typeof data.displayName === 'string' && data.displayName
          ? data.displayName
          : 'Player',
      score: Number(data.score),
      moves: Number(data.moves),
      completionTimeMs: Number(data.completionTimeMs),
    });
  });
  return rows;
}

module.exports = {
  SIMULATED_ENTRY_COUNT,
  RANKING_VERSION,
  SEED_SALT,
  FIRST_NAMES,
  SURNAME_INITIALS,
  xorShift32,
  generateSimulatedEntries,
  ensureOfficialSimulatedEntries,
  loadStoredSimulatedEntries,
};
