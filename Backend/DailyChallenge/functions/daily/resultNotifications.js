'use strict';

/**
 * Player-specific Daily Result Ready pushes.
 * Tokens are stored per Firebase UID. Settlement only queues.
 * A 15-minute dispatcher sends around 09:15 in the device UTC offset.
 * The global daily_challenge_new topic is not used.
 */

const crypto = require('crypto');
const admin = require('firebase-admin');
const { utcDayId } = require('./validation');

const TITLE = 'DAILY RESULT READY';
const BODY_RANKED = 'Your final rank is ready. See how you did!';
const BODY_WINNER = 'You finished #1! Your reward is waiting.';
const CHANNEL_ID = 'daily_challenge';
const DELIVERY_HOUR = 9;
const DELIVERY_START_MINUTE = 15;
const LOOKBACK_DAYS = 3;
const CLAIM_MS = 2 * 60 * 1000;

function db() {
  return admin.firestore();
}

function tokenDocId(token) {
  return crypto.createHash('sha256').update(String(token)).digest('hex').slice(0, 32);
}

function resultCopy(rank) {
  const place = Number(rank);
  if (place === 1 || place === 2 || place === 3) {
    return {
      title: TITLE,
      body: 'You finished #' + place + '! Your reward is waiting.',
    };
  }
  return {
    title: TITLE,
    body: BODY_RANKED,
  };
}

function isPrizeRank(rank) {
  const place = Number(rank);
  return place === 1 || place === 2 || place === 3;
}

/** Same path as rewards.js rewardRef. Kept local to avoid a require cycle. */
function rewardDocRef(dayId, uid) {
  return db().collection('dailyChallenges').doc(dayId).collection('rewards').doc(uid);
}

/**
 * Send a queued result only when it is still current.
 * A claimed prize cancels the whole notification, including generic copy.
 * Rank > 3 does not consult the reward doc.
 */
function shouldDeliverQueuedResult(queueData, rewardData) {
  const queue = queueData || {};
  if (queue.cancelledAt) {
    return false;
  }
  if (isPrizeRank(queue.rank) && rewardData && rewardData.claimed === true) {
    return false;
  }
  return true;
}

function resultCancelFields() {
  return {
    cancelledAt: admin.firestore.FieldValue.serverTimestamp(),
    cancelReason: 'reward_claimed',
  };
}

function clampOffsetMinutes(value) {
  const n = Number(value);
  if (!Number.isInteger(n)) {
    return null;
  }
  if (n < -14 * 60 || n > 14 * 60) {
    return null;
  }
  return n;
}

function localHourMinute(now, offsetMinutes) {
  const shifted = new Date(now.getTime() + offsetMinutes * 60 * 1000);
  return {
    hour: shifted.getUTCHours(),
    minute: shifted.getUTCMinutes(),
  };
}

function isLocalDeliveryWindow(now, offsetMinutes) {
  const parts = localHourMinute(now, offsetMinutes);
  return parts.hour === DELIVERY_HOUR && parts.minute >= DELIVERY_START_MINUTE;
}

function isInvalidTokenError(err) {
  const code = err && err.code ? String(err.code) : '';
  return (
    code.indexOf('registration-token-not-registered') >= 0 ||
    code.indexOf('invalid-registration-token') >= 0
  );
}

function usersRef(uid) {
  return db().collection('users').doc(uid);
}

function tokensCol(uid) {
  return usersRef(uid).collection('notificationTokens');
}

function resultRef(dayId, uid) {
  return db()
    .collection('dailyChallenges')
    .doc(dayId)
    .collection('resultNotifications')
    .doc(uid);
}

async function registerDailyNotificationTokenHandler(request) {
  const { requireAuth } = require('./validation');
  const uid = requireAuth(request);
  const data = request.data || {};
  const enabled = data.enabled === true;
  const token = typeof data.token === 'string' ? data.token.trim() : '';
  const offset = clampOffsetMinutes(data.utcOffsetMinutes);
  const timeZoneId =
    typeof data.timeZoneId === 'string'
      ? data.timeZoneId.replace(/[^\w+\-/]/g, '').slice(0, 64)
      : '';

  console.log(
    '[DailyResult] register enabled=' +
      enabled +
      ' tokenPresent=' +
      (token.length > 0) +
      ' tokenLength=' +
      token.length +
      ' uidLength=' +
      uid.length +
      ' offset=' +
      (offset == null ? 'none' : String(offset))
  );

  if (!enabled) {
    if (token.length > 0) {
      await tokensCol(uid).doc(tokenDocId(token)).set(
        {
          dailyNotificationsEnabled: false,
          updatedAt: admin.firestore.FieldValue.serverTimestamp(),
        },
        { merge: true }
      );
    } else {
      const existing = await tokensCol(uid).get();
      const batch = db().batch();
      existing.forEach((doc) => {
        batch.set(
          doc.ref,
          {
            dailyNotificationsEnabled: false,
            updatedAt: admin.firestore.FieldValue.serverTimestamp(),
          },
          { merge: true }
        );
      });
      if (!existing.empty) {
        await batch.commit();
      }
    }
    await usersRef(uid).set(
      {
        dailyNotificationsEnabled: false,
        updatedAt: admin.firestore.FieldValue.serverTimestamp(),
      },
      { merge: true }
    );
    return { success: true, enabled: false };
  }

  if (token.length < 20 || token.length > 4096 || offset == null) {
    const err = new Error('Invalid notification registration.');
    err.code = 'failed-precondition';
    throw err;
  }

  await tokensCol(uid).doc(tokenDocId(token)).set(
    {
      token,
      dailyNotificationsEnabled: true,
      utcOffsetMinutes: offset,
      timeZoneId,
      updatedAt: admin.firestore.FieldValue.serverTimestamp(),
    },
    { merge: true }
  );
  await usersRef(uid).set(
    {
      dailyNotificationsEnabled: true,
      updatedAt: admin.firestore.FieldValue.serverTimestamp(),
    },
    { merge: true }
  );
  console.log('[DailyResult] register success uidLength=' + uid.length);
  return { success: true, enabled: true, tokenLength: token.length };
}

/**
 * Queue one result notification doc per real ranked player.
 * Bots are skipped. Does not send. Existing docs are left unchanged.
 */
async function queueDailyResultNotifications(dayId, ranking) {
  const rows = Array.isArray(ranking) ? ranking : [];
  let queued = 0;
  for (let i = 0; i < rows.length; i++) {
    const row = rows[i];
    if (!row || row.isSimulated || !row.uid) {
      continue;
    }
    const rank = Number(row.rank != null ? row.rank : i + 1);
    if (!Number.isInteger(rank) || rank < 1) {
      continue;
    }
    const ref = resultRef(dayId, row.uid);
    const snap = await ref.get();
    if (snap.exists) {
      continue;
    }
    await ref.set({
      dayId,
      uid: row.uid,
      rank,
      queuedAt: admin.firestore.FieldValue.serverTimestamp(),
    });
    queued += 1;
  }
  if (queued > 0 || rows.length > 0) {
    console.log(
      '[DailyResult] queued dayId=' +
        dayId +
        ' rankedRows=' +
        rows.length +
        ' newDocs=' +
        queued
    );
  }
  return queued;
}

function buildDirectMessage(token, dayId, rank) {
  const copy = resultCopy(rank);
  return {
    token,
    notification: {
      title: copy.title,
      body: copy.body,
    },
    data: {
      type: 'daily_result',
      dayId: String(dayId),
    },
    android: {
      priority: 'high',
      notification: {
        channelId: CHANNEL_ID,
      },
    },
  };
}

async function sendOneToken(dayId, uid, rank, tokenDoc, now) {
  const data = tokenDoc.data() || {};
  if (data.dailyNotificationsEnabled !== true || typeof data.token !== 'string') {
    return 'skipped';
  }
  const offset = clampOffsetMinutes(data.utcOffsetMinutes);
  if (offset == null || !isLocalDeliveryWindow(now, offset)) {
    return 'waiting';
  }

  const queueRef = resultRef(dayId, uid);
  const deliveryRef = queueRef.collection('deliveries').doc(tokenDoc.id);
  const prize = isPrizeRank(rank);
  const rewardRef = prize ? rewardDocRef(dayId, uid) : null;
  const claim = await db().runTransaction(async (tx) => {
    const queueSnap = await tx.get(queueRef);
    const queue = queueSnap.exists ? queueSnap.data() || {} : {};
    const rewardSnap = rewardRef ? await tx.get(rewardRef) : null;
    const reward = rewardSnap && rewardSnap.exists ? rewardSnap.data() || {} : null;
    const snap = await tx.get(deliveryRef);
    const existing = snap.exists ? snap.data() || {} : {};

    if (!shouldDeliverQueuedResult(Object.assign({ rank }, queue), reward)) {
      if (prize && reward && reward.claimed === true && !queue.cancelledAt) {
        tx.set(queueRef, resultCancelFields(), { merge: true });
      }
      return { proceed: false };
    }
    if (existing.sentAt) {
      return { proceed: false };
    }
    if (existing.claimedAt && typeof existing.claimedAt.toMillis === 'function') {
      if (Date.now() - existing.claimedAt.toMillis() < CLAIM_MS) {
        return { proceed: false };
      }
    }
    tx.set(
      deliveryRef,
      { claimedAt: admin.firestore.FieldValue.serverTimestamp() },
      { merge: true }
    );
    return { proceed: true };
  });
  if (!claim.proceed) {
    return 'already';
  }

  if (prize) {
    const stillDue = await db().runTransaction(async (tx) => {
      const queueSnap = await tx.get(queueRef);
      const queue = queueSnap.exists ? queueSnap.data() || {} : {};
      const rewardSnap = await tx.get(rewardRef);
      const reward = rewardSnap.exists ? rewardSnap.data() || {} : null;
      const deliverySnap = await tx.get(deliveryRef);
      const delivery = deliverySnap.exists ? deliverySnap.data() || {} : {};
      if (shouldDeliverQueuedResult(Object.assign({ rank }, queue), reward)) {
        return true;
      }
      if (reward && reward.claimed === true && !queue.cancelledAt) {
        tx.set(queueRef, resultCancelFields(), { merge: true });
      }
      if (deliverySnap.exists && !delivery.sentAt) {
        tx.delete(deliveryRef);
      }
      return false;
    });
    if (!stillDue) {
      return 'cancelled';
    }
  }

  try {
    const messageId = await admin.messaging().send(
      buildDirectMessage(data.token, dayId, rank)
    );
    await deliveryRef.set(
      {
        sentAt: admin.firestore.FieldValue.serverTimestamp(),
        messageId: messageId || '',
      },
      { merge: true }
    );
    console.log(
      '[DailyResult] sent dayId=' +
        dayId +
        ' rank=' +
        rank +
        ' uidLength=' +
        uid.length +
        ' tokenLength=' +
        data.token.length
    );
    return 'sent';
  } catch (err) {
    await deliveryRef.delete().catch(() => {});
    if (isInvalidTokenError(err)) {
      await tokenDoc.ref.delete().catch(() => {});
      console.log(
        '[DailyResult] removed invalid token uidLength=' +
          uid.length +
          ' tokenLength=' +
          data.token.length
      );
      return 'invalid';
    }
    console.error(
      '[DailyResult] send failed dayId=' +
        dayId +
        ' uidLength=' +
        uid.length +
        ' err=' +
        (err && err.message ? err.message : String(err))
    );
    return 'failed';
  }
}

async function dispatchDueResultNotifications(serverNow = new Date()) {
  let sent = 0;
  for (let daysAgo = 0; daysAgo <= LOOKBACK_DAYS; daysAgo++) {
    const dayId = utcDayId(
      new Date(
        Date.UTC(
          serverNow.getUTCFullYear(),
          serverNow.getUTCMonth(),
          serverNow.getUTCDate() - daysAgo
        )
      )
    );
    const queued = await db()
      .collection('dailyChallenges')
      .doc(dayId)
      .collection('resultNotifications')
      .get();
    for (const doc of queued.docs) {
      const row = doc.data() || {};
      if (row.cancelledAt) {
        continue;
      }
      const uid = doc.id;
      const rank = Number(row.rank);
      if (!Number.isInteger(rank) || rank < 1) {
        continue;
      }
      const tokens = await tokensCol(uid).get();
      for (const tokenDoc of tokens.docs) {
        const outcome = await sendOneToken(dayId, uid, rank, tokenDoc, serverNow);
        if (outcome === 'sent') {
          sent += 1;
        }
      }
    }
  }
  console.log('[DailyResult] dispatch sent=' + sent);
  return { sent };
}

module.exports = {
  TITLE,
  BODY_RANKED,
  BODY_WINNER,
  tokenDocId,
  resultCopy,
  isPrizeRank,
  shouldDeliverQueuedResult,
  resultNotificationRef: resultRef,
  clampOffsetMinutes,
  isLocalDeliveryWindow,
  isInvalidTokenError,
  registerDailyNotificationTokenHandler,
  queueDailyResultNotifications,
  dispatchDueResultNotifications,
};
