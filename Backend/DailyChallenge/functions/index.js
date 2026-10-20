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
const {
  registerDailyNotificationTokenHandler,
  dispatchDueResultNotifications,
} = require('./daily/resultNotifications');
const {
  registerGooglePlayIdentityHandler,
  getDailyFriendsLeaderboardHandler,
} = require('./daily/friendsLeaderboard');

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
 * Does not grant coins. Prize rewards stay on getPendingDailyRewards / claimDailyReward.
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
 * Settle the previous UTC day shortly after midnight.
 * Version 2 ranks real players and the day's frozen simulated entries together.
 * Bots can occupy prize positions but never receive coins.
 * Already-settled version 1 days stay real-only.
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

/**
 * Stores this device's FCM token for the signed-in Firebase UID.
 * Used for Daily Result Ready only. Does not subscribe or send.
 */
exports.registerDailyNotificationToken = onCall(
  wrap(registerDailyNotificationTokenHandler)
);

/**
 * Maps the Firebase Auth Play Games provider id onto this UID.
 * The client does not supply the player id.
 */
exports.registerGooglePlayIdentity = onCall(wrap(registerGooglePlayIdentityHandler));

/**
 * Real Daily rows for the caller and Play Games friends who linked Grid Drive.
 * Comparison only. Does not affect reward settlement.
 */
exports.getDailyFriendsLeaderboard = onCall(wrap(getDailyFriendsLeaderboardHandler));

/**
 * Sends queued Daily Result Ready pushes when the device offset is 09:15–09:59.
 * Does not run at settlement (00:05 UTC).
 */
exports.dispatchDailyResultNotifications = onSchedule(
  {
    schedule: '*/15 * * * *',
    timeZone: 'UTC',
    region: FUNCTIONS_REGION,
    retryCount: 2,
    minBackoffSeconds: 30,
    maxBackoffSeconds: 120,
    maxRetrySeconds: 600,
  },
  async () => {
    await dispatchDueResultNotifications(new Date());
  }
);
