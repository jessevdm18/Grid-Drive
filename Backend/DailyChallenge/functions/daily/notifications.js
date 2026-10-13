'use strict';

/**
 * Phase 4.4 — Daily Challenge new-day FCM topic notification.
 *
 * Uses the SAME challenge selection helper as getDailyChallenge
 * (resolveChallengeForNow). Never notifies if challenge creation fails.
 *
 * Topics:
 *   daily_challenge_new  — production (scheduled)
 *   daily_challenge_dev  — safe test callable only
 *
 * notificationSentAt is written only after FCM accepts the send.
 * A claim without sentAt does not count as delivered, so a later run can still send.
 * A crash after FCM accept but before sentAt can still duplicate once on retry.
 */

const admin = require('firebase-admin');
const { resolveChallengeForNow } = require('./challenge');
const { requireAuth, utcDayId } = require('./validation');

const NOTIFICATION_VERSION = 1;
const CLAIM_IN_PROGRESS_MS = 75 * 1000;
const TOPIC_PRODUCTION = 'daily_challenge_new';
const TOPIC_DEV = 'daily_challenge_dev';
const NOTIFICATION_TITLE = 'NEW DAILY CHALLENGE';
const NOTIFICATION_BODY =
  "Today's challenge is ready. You only get one attempt!";

function db() {
  return admin.firestore();
}

function dayRef(dayId) {
  return db().collection('dailyChallenges').doc(dayId);
}

function isNotificationAlreadySent(dayData) {
  if (!dayData || typeof dayData !== 'object') {
    return false;
  }
  if (dayData.notificationSentAt) {
    return true;
  }
  if (
    dayData.notificationVersion != null &&
    dayData.notificationTopic === TOPIC_PRODUCTION
  ) {
    // Version stamped without sentAt is treated as in-progress/prior send marker.
    // Prefer notificationSentAt; keep this as secondary guard.
    return dayData.notificationSentAt != null;
  }
  return false;
}

function buildMessage(topic, dayId) {
  return {
    topic,
    notification: {
      title: NOTIFICATION_TITLE,
      body: NOTIFICATION_BODY,
    },
    data: {
      type: 'daily_challenge',
      dayId: String(dayId),
    },
    android: {
      priority: 'high',
      notification: {
        channelId: 'daily_challenge',
        // Tap opens the app; no deep-link activity override in V1.
      },
    },
    apns: {
      payload: {
        aps: {
          sound: 'default',
        },
      },
    },
  };
}

/**
 * Ensure today's challenge exists, then notify production topic once.
 */
async function publishDailyChallengeAndNotify(serverNow = new Date()) {
  const dayId = utcDayId(serverNow);
  console.log('[DailyNotify] publishDailyChallengeAndNotify dayId=' + dayId);

  let challenge;
  try {
    challenge = await resolveChallengeForNow(serverNow);
  } catch (err) {
    const code = err && err.code ? err.code : 'challenge_failed';
    console.error(
      '[DailyNotify] Challenge ensure failed — NOT notifying. code=' +
        code +
        ' message=' +
        (err && err.message ? err.message : String(err))
    );
    const wrapped = new Error(
      'Daily challenge was not ready for notification. code=' + code
    );
    wrapped.code = code;
    throw wrapped;
  }

  if (!challenge || !challenge.dayId || !challenge.levelId) {
    console.error(
      '[DailyNotify] Challenge missing levelId — NOT notifying. dayId=' + dayId
    );
    const missing = new Error('Daily challenge has no levelId.');
    missing.code = 'no_level';
    throw missing;
  }

  const ref = dayRef(challenge.dayId);
  const pre = await ref.get();
  const preData = pre.exists ? pre.data() || {} : {};
  if (preData.notificationSentAt) {
    console.log(
      '[DailyNotify] Already sent for dayId=' +
        challenge.dayId +
        ' — skip FCM'
    );
    return {
      ok: true,
      dayId: challenge.dayId,
      levelId: challenge.levelId,
      notified: false,
      alreadySent: true,
      challengeCreated: !!challenge.created,
    };
  }

  // sentAt is the only proof of delivery. A fresh claim means another run is
  // still sending; throw so the scheduler retries instead of reporting success.
  // A stale claim means the previous run died before FCM success, so send.
  const claim = await db().runTransaction(async (tx) => {
    const snap = await tx.get(ref);
    const data = snap.exists ? snap.data() || {} : {};
    if (data.notificationSentAt) {
      return { proceed: false, alreadySent: true };
    }
    if (data.notificationSendClaimedAt) {
      const claimed = data.notificationSendClaimedAt;
      let claimedMs = 0;
      if (claimed && typeof claimed.toMillis === 'function') {
        claimedMs = claimed.toMillis();
      }
      if (claimedMs > 0 && Date.now() - claimedMs < CLAIM_IN_PROGRESS_MS) {
        return { proceed: false, alreadySent: false, inProgress: true };
      }
    }

    tx.set(
      ref,
      {
        notificationVersion: NOTIFICATION_VERSION,
        notificationTopic: TOPIC_PRODUCTION,
        notificationSendClaimedAt: admin.firestore.FieldValue.serverTimestamp(),
      },
      { merge: true }
    );
    return { proceed: true, alreadySent: false };
  });

  if (!claim.proceed) {
    if (claim.inProgress) {
      const busy = new Error(
        'Daily notification send already in progress for dayId=' + challenge.dayId
      );
      busy.code = 'notification_in_progress';
      throw busy;
    }
    return {
      ok: true,
      dayId: challenge.dayId,
      levelId: challenge.levelId,
      notified: false,
      alreadySent: !!claim.alreadySent,
      challengeCreated: !!challenge.created,
    };
  }

  let messageId = '';
  try {
    messageId = await admin.messaging().send(
      buildMessage(TOPIC_PRODUCTION, challenge.dayId)
    );
  } catch (err) {
    console.error(
      '[DailyNotify] FCM send failed dayId=' +
        challenge.dayId +
        ' err=' +
        (err && err.message ? err.message : String(err))
    );
    // Clear claim so scheduler retry can try again.
    await ref.set(
      {
        notificationSendClaimedAt: admin.firestore.FieldValue.delete(),
      },
      { merge: true }
    );
    const failed = new Error(
      'FCM send failed for dayId=' + challenge.dayId
    );
    failed.code = 'fcm_send_failed';
    throw failed;
  }

  await ref.set(
    {
      notificationVersion: NOTIFICATION_VERSION,
      notificationTopic: TOPIC_PRODUCTION,
      notificationSentAt: admin.firestore.FieldValue.serverTimestamp(),
      notificationMessageId: messageId || '',
    },
    { merge: true }
  );

  console.log(
    '[DailyNotify] Sent topic=' +
      TOPIC_PRODUCTION +
      ' dayId=' +
      challenge.dayId +
      ' levelId=' +
      challenge.levelId +
      ' messageIdLen=' +
      (messageId ? String(messageId).length : 0)
  );

  return {
    ok: true,
    dayId: challenge.dayId,
    levelId: challenge.levelId,
    notified: true,
    alreadySent: false,
    challengeCreated: !!challenge.created,
  };
}

/**
 * SAFE test: send to daily_challenge_dev only. Never production topic.
 * Authenticated callable — for device/dev testing after opting into the test topic.
 */
async function sendDailyChallengeDevNotificationHandler(request) {
  requireAuth(request);
  const serverNow = new Date();
  let challenge;
  try {
    challenge = await resolveChallengeForNow(serverNow);
  } catch (err) {
    return {
      success: false,
      errorCode: err && err.code ? err.code : 'challenge_failed',
      errorMessage: 'Challenge unavailable — not sending test push.',
      topic: TOPIC_DEV,
    };
  }

  if (!challenge || !challenge.levelId) {
    return {
      success: false,
      errorCode: 'no_level',
      errorMessage: 'No playable challenge — not sending test push.',
      topic: TOPIC_DEV,
    };
  }

  const messageId = await admin.messaging().send(
    buildMessage(TOPIC_DEV, challenge.dayId)
  );

  console.log(
    '[DailyNotify] DEV push sent topic=' +
      TOPIC_DEV +
      ' dayId=' +
      challenge.dayId +
      ' (production topic untouched)'
  );

  return {
    success: true,
    topic: TOPIC_DEV,
    dayId: challenge.dayId,
    levelId: challenge.levelId,
    messageIdLength: messageId ? String(messageId).length : 0,
  };
}

module.exports = {
  NOTIFICATION_VERSION,
  TOPIC_PRODUCTION,
  TOPIC_DEV,
  NOTIFICATION_TITLE,
  NOTIFICATION_BODY,
  publishDailyChallengeAndNotify,
  sendDailyChallengeDevNotificationHandler,
  isNotificationAlreadySent,
};
