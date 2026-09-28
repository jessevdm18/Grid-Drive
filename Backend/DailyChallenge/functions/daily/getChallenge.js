'use strict';

const {
  resolveChallengeForNow,
  getAttempt,
  attemptToClient,
  challengeToClient,
} = require('./challenge');
const { requireAuth } = require('./validation');

async function getDailyChallengeHandler(request) {
  const uid = requireAuth(request);
  // Server clock only — ignore any client-supplied dayId.
  const serverNow = new Date();
  const challenge = await resolveChallengeForNow(serverNow);
  const attempt = await getAttempt(challenge.dayId, uid);

  return {
    success: true,
    challenge: challengeToClient(challenge),
    attempt: attemptToClient(attempt),
    serverUtc: serverNow.toISOString(),
  };
}

module.exports = { getDailyChallengeHandler };
