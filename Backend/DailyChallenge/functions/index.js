'use strict';

/**
 * Rush Out Daily Challenge — Cloud Functions
 *
 * Authoritative mutations use Admin SDK. Clients call these with Firebase ID token.
 * Ownership key is always request.auth.uid (provider-independent Firebase UID).
 *
 * Anonymous Auth is V1 only. Future Google Play / Apple linking that preserves
 * the same Firebase UID keeps all Daily attempts/results/rewards attached automatically.
 */

const admin = require('firebase-admin');
const {
  getFirestore,
  FieldValue,
  Timestamp,
} = require('firebase-admin/firestore');
const { getMessaging } = require('firebase-admin/messaging');
const { onCall, HttpsError } = require('firebase-functions/v2/https');
const { onSchedule } = require('firebase-functions/v2/scheduler');
const { setGlobalOptions } = require('firebase-functions/v2');
const { FUNCTIONS_REGION } = require('./daily/config');
const { getDailyChallengeHandler } = require('./daily/getChallenge');
const { startDailyAttemptHandler } = require('./daily/startAttempt');
const { submitDailyResultHandler } = require('./daily/submitResult');
const { abandonDailyAttemptHandler } = require('./daily/abandonAttempt');
const {
  settlePreviousUtcDay,
  getPendingDailyRewardsHandler,
  claimDailyRewardHandler,
  createDevDailyRewardHandler,
  getSettledDailyResultHandler,
} = require('./daily/rewards');
const {
  publishDailyChallengeAndNotify,
  sendDailyChallengeDevNotificationHandler,
} = require('./daily/notifications');

admin.initializeApp();

// firebase-admin 14 removed the namespaced admin.firestore() / admin.messaging()
// helpers. Existing call sites keep working through this shim.
if (typeof admin.firestore !== 'function') {
  const firestore = () => getFirestore();
  firestore.FieldValue = FieldValue;
  firestore.Timestamp = Timestamp;
  admin.firestore = firestore;
}
if (typeof admin.messaging !== 'function') {
  admin.messaging = () => getMessaging();
}

setGlobalOptions({ region: FUNCTIONS_REGION, maxInstances: 20 });

function wrap(handler) {
  return async (request) => {
    try {
      return await handler(request);
    } catch (err) {
      // Preserve intentional HttpsError codes (e.g. failed-precondition for DEV gate).
      if (err instanceof HttpsError) {
        throw err;
      }

      const code = err && err.code ? err.code : 'internal';
      const message = err && err.message ? err.message : 'Internal error.';
      if (code === 'unauthenticated') {
        throw new HttpsError('unauthenticated', message);
      }
      if (code === 'no_level' || code === 'failed-precondition') {
        throw new HttpsError('failed-precondition', message);
      }
      console.error('[DailyBackend] function error', code, message);
      throw new HttpsError('internal', message);
    }
  };
}

exports.getDailyChallenge = onCall(wrap(getDailyChallengeHandler));
exports.startDailyAttempt = onCall(wrap(startDailyAttemptHandler));
exports.submitDailyResult = onCall(wrap(submitDailyResultHandler));
exports.abandonDailyAttempt = onCall(wrap(abandonDailyAttemptHandler));

/** Phase 4.3 — pending #1 rewards for the authenticated Firebase UID. */
exports.getPendingDailyRewards = onCall(wrap(getPendingDailyRewardsHandler));

/**
 * Settled final placement for the authenticated player (any rank).
 * Does not grant coins. Rank #1 rewards stay on getPendingDailyRewards / claimDailyReward.
 */
exports.getSettledDailyResult = onCall(wrap(getSettledDailyResultHandler));

/** Phase 4.3 — claim one day reward; idempotent AlreadyClaimed (no second grant). */
exports.claimDailyReward = onCall(wrap(claimDailyRewardHandler));

/**
 * Phase 4.3 DEV smoke — create reward for auth.uid under dayId=dev-reward-smoke-test.
 * Gated by ALLOW_DEV_DAILY_REWARDS=true or Functions emulator. No production settlement.
 */
exports.createDevDailyReward = onCall(wrap(createDevDailyRewardHandler));

/**
 * Phase 4.4 — ensure today's challenge exists, then notify topic daily_challenge_new.
 * Runs at 00:01 UTC. Does not notify if challenge creation fails.
 */
exports.publishDailyChallengeAndNotify = onSchedule(
  {
    schedule: '1 0 * * *',
    timeZone: 'UTC',
    region: FUNCTIONS_REGION,
    retryCount: 4,
    minBackoffSeconds: 90,
    maxBackoffSeconds: 600,
    maxRetrySeconds: 1800,
  },
  async () => {
    await publishDailyChallengeAndNotify(new Date());
  }
);

/**
 * Phase 4.3 — settle previous UTC day's #1 winner shortly after midnight.
 * Idempotent. Simulated leaderboard entries are never eligible.
 */
exports.settleDailyChallengeWinners = onSchedule(
  {
    schedule: '5 0 * * *',
    timeZone: 'UTC',
    region: FUNCTIONS_REGION,
    retryCount: 4,
    minBackoffSeconds: 90,
    maxBackoffSeconds: 600,
    maxRetrySeconds: 1800,
  },
  async () => {
    await settlePreviousUtcDay(new Date());
  }
);

/**
 * Phase 4.4 SAFE test — FCM to daily_challenge_dev only (never production topic).
 */
exports.sendDailyChallengeDevNotification = onCall(
  wrap(sendDailyChallengeDevNotificationHandler)
);
