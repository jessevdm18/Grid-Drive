'use strict';

/**
 * Friends Daily comparison.
 * Scores come only from real eligible attempt docs.
 * This ranking never creates rewards and never reads simulated rows.
 */

const crypto = require('crypto');
const admin = require('firebase-admin');
const { getAuth } = require('firebase-admin/auth');
const { isEligibleAttempt, compareRankRows } = require('./rewards');

const PLAY_PROVIDER = 'playgames.google.com';
const MAX_FRIEND_IDS = 200;
function isSafePlayerId(value) {
  if (typeof value !== 'string' || value.length < 3 || value.length > 128) {
    return false;
  }
  for (let i = 0; i < value.length; i++) {
    const code = value.charCodeAt(i);
    if (code < 33 || code > 126) {
      return false;
    }
    const ch = value.charAt(i);
    if (ch === '"' || ch === '\\' || ch === '{' || ch === '}') {
      return false;
    }
  }
  return true;
}
const DAY_ID_PATTERN = /^\d{4}-\d{2}-\d{2}$/;

function db() {
  return admin.firestore();
}

function playerDocId(playerId) {
  return crypto.createHash('sha256').update(String(playerId)).digest('hex').slice(0, 32);
}

function playerRef(playerId) {
  return db().collection('googlePlayPlayers').doc(playerDocId(playerId));
}

function cleanDisplayName(value) {
  if (typeof value !== 'string') {
    return '';
  }
  return value.replace(/[\u0000-\u001F]/g, '').trim().slice(0, 32);
}

function fallbackDisplayName(uid) {
  const hex = (stableHash32(uid) & 0xffff).toString(16).toUpperCase().padStart(4, '0');
  return 'Player ' + hex;
}

function stableHash32(value) {
  let hash = 2166136261;
  const text = value || '';
  for (let i = 0; i < text.length; i++) {
    hash ^= text.charCodeAt(i);
    hash = Math.imul(hash, 16777619);
  }
  return hash >>> 0;
}

function assertDayId(dayId) {
  if (typeof dayId !== 'string' || !DAY_ID_PATTERN.test(dayId)) {
    const err = new Error('Invalid day.');
    err.code = 'failed-precondition';
    throw err;
  }
  const parts = dayId.split('-');
  const year = Number(parts[0]);
  const month = Number(parts[1]);
  const day = Number(parts[2]);
  const utc = new Date(Date.UTC(year, month - 1, day));
  if (
    utc.getUTCFullYear() !== year ||
    utc.getUTCMonth() !== month - 1 ||
    utc.getUTCDate() !== day
  ) {
    const err = new Error('Invalid day.');
    err.code = 'failed-precondition';
    throw err;
  }
}

function collectFriendPlayerIds(raw) {
  const ids = [];
  const seen = new Set();
  const list = Array.isArray(raw) ? raw : [];
  for (let i = 0; i < list.length && ids.length < MAX_FRIEND_IDS; i++) {
    if (typeof list[i] !== 'string') {
      continue;
    }
    const id = list[i].trim();
    if (!isSafePlayerId(id) || seen.has(id)) {
      continue;
    }
    seen.add(id);
    ids.push(id);
  }
  return ids;
}

async function readPlayIdentity(uid) {
  const user = await getAuth().getUser(uid);
  const providers = user && user.providerData ? user.providerData : [];
  for (let i = 0; i < providers.length; i++) {
    const provider = providers[i];
    if (!provider || provider.providerId !== PLAY_PROVIDER) {
      continue;
    }
    const playerId = typeof provider.uid === 'string' ? provider.uid.trim() : '';
    if (!isSafePlayerId(playerId)) {
      return null;
    }
    return {
      playerId,
      displayName: cleanDisplayName(provider.displayName),
    };
  }
  return null;
}

async function registerGooglePlayIdentityHandler(request) {
  const { requireAuth } = require('./validation');
  const uid = requireAuth(request);
  const identity = await readPlayIdentity(uid);
  if (!identity) {
    console.log('[Friends] register skipped uidLength=' + uid.length);
    return { success: true, mapped: false };
  }

  const ref = playerRef(identity.playerId);
  const existing = await ref.get();
  if (existing.exists) {
    const owner = existing.data() && existing.data().uid;
    if (owner && owner !== uid) {
      console.log('[Friends] register conflict uidLength=' + uid.length);
      return { success: false, mapped: false, reason: 'conflict' };
    }
  }

  await ref.set(
    {
      uid,
      displayName: identity.displayName,
      linkedAt: admin.firestore.FieldValue.serverTimestamp(),
    },
    { merge: true }
  );
  await db().collection('users').doc(uid).set(
    {
      googlePlayPlayerIdHash: ref.id,
      googlePlayLinkedAt: admin.firestore.FieldValue.serverTimestamp(),
    },
    { merge: true }
  );
  console.log('[Friends] register mapped uidLength=' + uid.length);
  return { success: true, mapped: true };
}

async function mapPlayerIdsToUsers(playerIds) {
  const byUid = new Map();
  const refs = playerIds.map((playerId) => playerRef(playerId));
  for (let i = 0; i < refs.length; i += 50) {
    const chunk = refs.slice(i, i + 50);
    const snaps = await db().getAll(...chunk);
    snaps.forEach((snap) => {
      if (!snap.exists) {
        return;
      }
      const data = snap.data() || {};
      if (typeof data.uid !== 'string' || data.uid.length === 0) {
        return;
      }
      byUid.set(data.uid, cleanDisplayName(data.displayName));
    });
  }
  return byUid;
}

async function getDailyFriendsLeaderboardHandler(request) {
  const { requireAuth } = require('./validation');
  const uid = requireAuth(request);
  const data = request.data || {};
  const dayId = typeof data.dayId === 'string' ? data.dayId.trim() : '';
  assertDayId(dayId);
  const friendIds = collectFriendPlayerIds(data.friendPlayerIds);
  const namesByUid = await mapPlayerIdsToUsers(friendIds);
  namesByUid.set(uid, namesByUid.has(uid) ? namesByUid.get(uid) : '');

  const attemptRefs = [];
  namesByUid.forEach((_, mappedUid) => {
    attemptRefs.push(
      db().collection('dailyChallenges').doc(dayId).collection('attempts').doc(mappedUid)
    );
  });

  const rows = [];
  for (let i = 0; i < attemptRefs.length; i += 50) {
    const snaps = await db().getAll(...attemptRefs.slice(i, i + 50));
    snaps.forEach((snap) => {
      const attempt = snap.data();
      if (!isEligibleAttempt(attempt, snap.id)) {
        return;
      }
      rows.push({
        uid: snap.id,
        score: Number(attempt.score),
        moves: Number(attempt.moves),
        completionTimeMs: Number(attempt.completionTimeMs),
      });
    });
  }

  rows.sort(compareRankRows);
  const visible = rows.map((row, index) => {
    const storedName = namesByUid.get(row.uid) || '';
    const isCurrentPlayer = row.uid === uid;
    return {
      rank: index + 1,
      displayName: isCurrentPlayer ? 'YOU' : storedName || fallbackDisplayName(row.uid),
      score: row.score,
      moves: row.moves,
      completionTimeMs: row.completionTimeMs,
      isCurrentPlayer,
    };
  });

  console.log(
    '[Friends] leaderboard dayId=' +
      dayId +
      ' requested=' +
      friendIds.length +
      ' matched=' +
      Math.max(0, namesByUid.size - 1) +
      ' rows=' +
      visible.length
  );
  return {
    success: true,
    dayId,
    rows: visible,
  };
}

module.exports = {
  playerDocId,
  fallbackDisplayName,
  collectFriendPlayerIds,
  registerGooglePlayIdentityHandler,
  getDailyFriendsLeaderboardHandler,
};
