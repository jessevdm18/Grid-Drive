'use strict';

const admin = require('firebase-admin');
const {
  resolveChallengeForNow,
  getAttempt,
  attemptToClient,
  challengeToClient,
} = require('./challenge');
const { requireAuth } = require('./validation');

async function abandonDailyAttemptHandler(request) {
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
    if (!snap.exists) {
      return { status: 'none' };
    }
    const data = snap.data() || {};
    if (data.state === 'Completed') {
      return { status: 'completed', data };
    }
    if (data.state === 'Abandoned') {
      return { status: 'already_abandoned', data };
    }
    if (data.state !== 'Started') {
      return { status: 'bad_state', data };
    }
    tx.update(attemptRef, {
      state: 'Abandoned',
      abandonedAt: admin.firestore.FieldValue.serverTimestamp(),
    });
    return { status: 'abandoned' };
  });

  const attempt = await getAttempt(challenge.dayId, uid);
  if (outcome.status === 'none') {
    return {
      success: true,
      challenge: challengeToClient(challenge),
      attempt: attemptToClient(attempt),
      note: 'no_attempt',
    };
  }

  if (outcome.status === 'completed') {
    return {
      success: false,
      errorCode: 'already_completed',
      errorMessage: 'Cannot abandon a completed attempt.',
      challenge: challengeToClient(challenge),
      attempt: attemptToClient(attempt),
    };
  }

  return {
    success: true,
    challenge: challengeToClient(challenge),
    attempt: attemptToClient(attempt),
  };
}

module.exports = { abandonDailyAttemptHandler };
