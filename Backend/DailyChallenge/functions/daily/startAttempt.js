'use strict';

const admin = require('firebase-admin');
const { SCORE_VERSION } = require('./config');
const {
  resolveChallengeForNow,
  getAttempt,
  attemptToClient,
  challengeToClient,
} = require('./challenge');
const { requireAuth } = require('./validation');

/**
 * Atomically claim today's attempt for request.auth.uid.
 * Ownership key is Firebase UID (provider-independent).
 */
async function startDailyAttemptHandler(request) {
  const uid = requireAuth(request);
  const serverNow = new Date();
  const challenge = await resolveChallengeForNow(serverNow);

  const attemptRef = admin
    .firestore()
    .collection('dailyChallenges')
    .doc(challenge.dayId)
    .collection('attempts')
    .doc(uid);

  const outcome = await admin.firestore().runTransaction(async (tx) => {
    const snap = await tx.get(attemptRef);
    if (snap.exists) {
      const data = snap.data() || {};
      return {
        claimed: false,
        attempt: {
          exists: true,
          dayId: challenge.dayId,
          userId: uid,
          state: data.state || 'None',
          levelId: data.levelId || '',
          levelVersion: data.levelVersion || '',
          scoreVersion: data.scoreVersion != null ? data.scoreVersion : SCORE_VERSION,
          moves: data.moves || 0,
          completionTimeMs: data.completionTimeMs || 0,
          score: data.score || 0,
          scoreTrusted: data.scoreTrusted === true,
          startedAt: data.startedAt,
          completedAt: data.completedAt,
        },
      };
    }

    const created = {
      userId: uid,
      state: 'Started',
      levelId: challenge.levelId,
      levelVersion: challenge.levelVersion,
      scoreVersion: SCORE_VERSION,
      minimumMoves: challenge.minimumMoves,
      moves: 0,
      completionTimeMs: 0,
      score: 0,
      scoreTrusted: false,
      startedAt: admin.firestore.FieldValue.serverTimestamp(),
    };
    tx.set(attemptRef, created);
    return {
      claimed: true,
      attempt: {
        exists: true,
        dayId: challenge.dayId,
        userId: uid,
        state: 'Started',
        levelId: challenge.levelId,
        levelVersion: challenge.levelVersion,
        scoreVersion: SCORE_VERSION,
        moves: 0,
        completionTimeMs: 0,
        score: 0,
        scoreTrusted: false,
        startedAt: serverNow.toISOString(),
        completedAt: '',
      },
    };
  });

  if (!outcome.claimed) {
    // Refresh attempt from disk for accurate timestamps after reject.
    const existing = await getAttempt(challenge.dayId, uid);
    return {
      success: false,
      errorCode: 'attempt_exists',
      errorMessage: 'Attempt already used.',
      challenge: challengeToClient(challenge),
      attempt: attemptToClient(existing),
    };
  }

  return {
    success: true,
    challenge: challengeToClient(challenge),
    attempt: attemptToClient(outcome.attempt),
  };
}

module.exports = { startDailyAttemptHandler };
