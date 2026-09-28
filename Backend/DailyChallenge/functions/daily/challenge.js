'use strict';

const admin = require('firebase-admin');
const { getLevels, SCORE_VERSION } = require('./config');
const { selectLevelIndexForDay } = require('./hash');
const { utcDayId, dayOpenIso, nextUtcMidnightIso } = require('./validation');

function db() {
  return admin.firestore();
}

/**
 * Resolve authoritative challenge for current server UTC day.
 * Create-if-absent; never change level once the day document exists.
 */
async function resolveChallengeForNow(serverNow = new Date()) {
  const dayId = utcDayId(serverNow);
  const levels = getLevels();
  const idx = selectLevelIndexForDay(dayId, levels.length);
  if (idx < 0) {
    const err = new Error('No Daily levels in catalog.');
    err.code = 'no_level';
    throw err;
  }

  const selected = levels[idx];
  const ref = db().collection('dailyChallenges').doc(dayId);
  const opensAtUtc = dayOpenIso(dayId);
  const closesAtUtc = nextUtcMidnightIso(serverNow);

  const result = await db().runTransaction(async (tx) => {
    const snap = await tx.get(ref);
    if (snap.exists) {
      const data = snap.data() || {};
      return {
        dayId,
        levelId: data.levelId || selected.levelId,
        levelVersion: data.levelVersion || data.levelId || selected.levelVersion,
        scoreVersion: data.scoreVersion != null ? data.scoreVersion : SCORE_VERSION,
        opensAtUtc: data.opensAtUtc || opensAtUtc,
        closesAtUtc: data.closesAtUtc || closesAtUtc,
        minimumMoves:
          data.minimumMoves != null
            ? data.minimumMoves
            : selected.minimumMoves,
        created: false,
      };
    }

    const created = {
      dayId,
      levelId: selected.levelId,
      levelVersion: selected.levelVersion || selected.levelId,
      scoreVersion: SCORE_VERSION,
      opensAtUtc,
      closesAtUtc,
      minimumMoves: selected.minimumMoves,
    };
    tx.set(ref, created);
    return { ...created, created: true };
  });

  return result;
}

async function getAttempt(dayId, uid) {
  const ref = db()
    .collection('dailyChallenges')
    .doc(dayId)
    .collection('attempts')
    .doc(uid);
  const snap = await ref.get();
  if (!snap.exists) {
    return { exists: false, state: 'None', dayId, userId: uid };
  }
  const data = snap.data() || {};
  return {
    exists: true,
    dayId,
    userId: uid,
    state: data.state || 'None',
    levelId: data.levelId || '',
    levelVersion: data.levelVersion || '',
    scoreVersion: data.scoreVersion != null ? data.scoreVersion : SCORE_VERSION,
    moves: data.moves || 0,
    completionTimeMs: data.completionTimeMs || 0,
    score: data.score || 0,
    scoreTrusted: data.scoreTrusted === true,
    startedAt: serializeTs(data.startedAt),
    completedAt: serializeTs(data.completedAt),
  };
}

function serializeTs(value) {
  if (!value) {
    return '';
  }
  if (typeof value.toDate === 'function') {
    return value.toDate().toISOString();
  }
  if (value instanceof Date) {
    return value.toISOString();
  }
  return String(value);
}

function attemptToClient(attempt) {
  return {
    exists: !!attempt.exists,
    dayId: attempt.dayId || '',
    userId: attempt.userId || '',
    state: attempt.state || 'None',
    levelId: attempt.levelId || '',
    levelVersion: attempt.levelVersion || '',
    scoreVersion: attempt.scoreVersion != null ? attempt.scoreVersion : SCORE_VERSION,
    moves: attempt.moves || 0,
    completionTimeMs: attempt.completionTimeMs || 0,
    score: attempt.score || 0,
    scoreTrusted: attempt.scoreTrusted === true,
    startedAtServer: attempt.startedAt || '',
    completedAtServer: attempt.completedAt || '',
  };
}

function challengeToClient(challenge) {
  return {
    dayId: challenge.dayId,
    levelId: challenge.levelId,
    levelVersion: challenge.levelVersion,
    scoreVersion: challenge.scoreVersion,
    opensAtUtc: challenge.opensAtUtc,
    closesAtUtc: challenge.closesAtUtc,
    minimumMoves: challenge.minimumMoves,
  };
}

module.exports = {
  resolveChallengeForNow,
  getAttempt,
  attemptToClient,
  challengeToClient,
  serializeTs,
};
