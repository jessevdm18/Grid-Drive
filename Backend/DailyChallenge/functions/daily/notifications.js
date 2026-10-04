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
 * V1 duplicate limitation: crash after FCM accept but before Firestore
 * notificationSentAt can theoretically cause a duplicate on retry.
 */

const admin = require('firebase-admin');
const { resolveChallengeForNow } = require('./challenge');
const { requireAuth, utcDayId } = require('./validation');

const NOTIFICATION_VERSION = 1;
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
    return {
      ok: false,
      dayId,
      notified: false,
      errorCode: code,
    };
  }

  if (!challenge || !challenge.dayId || !challenge.levelId) {
    console.error(
      '[DailyNotify] Challenge missing levelId — NOT notifying. dayId=' + dayId
    );
    return {
      ok: false,
      dayId,
      notified: false,
      errorCode: 'no_level',
    };
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

  // Claim send slot transactionally before FCM to reduce duplicates.
  // Remaining V1 gap: crash after FCM succeeds but before we confirm sentAt
  // is already written — see REWARDS/NOTIFICATIONS docs.
  const claim = await db().runTransaction(async (tx) => {
    const snap = await tx.get(ref);
    const data = snap.exists ? snap.data() || {} : {};
    if (data.notificationSentAt) {
      return { proceed: false, alreadySent: true };
    }
    if (data.notificationSendClaimedAt && !data.notificationSentAt) {
      // Another invocation claimed recently — skip to avoid double send.
      // Allow retry if claim is stale (>10 minutes) for failed runs.
      const claimed = data.notificationSendClaimedAt;
      let claimedMs = 0;
      if (claimed && typeof claimed.toMillis === 'function') {
        claimedMs = claimed.toMillis();
      }
      if (claimedMs > 0 && Date.now() - claimedMs < 10 * 60 * 1000) {
        return { proceed: false, alreadySent: false, claimed: true };
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
    return {
      ok: true,
      dayId: challenge.dayId,
      levelId: challenge.levelId,
      notified: false,
      alreadySent: !!claim.alreadySent,
      claimed: !!claim.claimed,
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
    return {
      ok: false,
      dayId: challenge.dayId,
      levelId: challenge.levelId,
      notified: false,
      errorCode: 'fcm_send_failed',
      challengeCreated: !!challenge.created,
    };
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
