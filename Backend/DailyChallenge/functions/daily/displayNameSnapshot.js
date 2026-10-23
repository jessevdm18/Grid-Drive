'use strict';

/**
 * Frozen public name for one Daily attempt.
 * Written once per dayId + uid. Later publicProfiles edits do not change it.
 */

const admin = require('firebase-admin');
const { fallbackDisplayName } = require('./friendsLeaderboard');

const DAY_ID_PATTERN = /^\d{4}-\d{2}-\d{2}$/;
const UID_PATTERN = /^[A-Za-z0-9]{6,128}$/;
const MAX_UIDS = 100;

function db() {
  return admin.firestore();
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

function cleanSnapshotName(value) {
  if (typeof value !== 'string') {
    return '';
  }
  let cleaned = '';
  for (let i = 0; i < value.length && cleaned.length < 24; i++) {
    const ch = value.charAt(i);
    const code = value.charCodeAt(i);
    if (code < 32 || ch === '@' || ch === '"' || ch === '\\') {
      if (ch === '@') {
        return '';
      }
      continue;
    }
    cleaned += ch;
  }
  return cleaned.trim();
}

function hasDisplayNameSnapshot(data) {
  return cleanSnapshotName(data && data.displayNameSnapshot).length > 0;
}

function profileRef(uid) {
  return db().collection('publicProfiles').doc(uid);
}

function attemptRef(dayId, uid) {
  return db().collection('dailyChallenges').doc(dayId).collection('attempts').doc(uid);
}

function snapshotFromProfileData(uid, data) {
  const publicName = cleanSnapshotName(data && data.displayName);
  return publicName || fallbackDisplayName(uid);
}

/**
 * Reads inside an existing transaction. Call only when the attempt has no snapshot yet.
 * Does not write.
 */
function nameFromProfileSnap(uid, profileSnap) {
  const data = profileSnap && profileSnap.exists ? profileSnap.data() : null;
  return snapshotFromProfileData(uid, data);
}

/**
 * Freezes displayNameSnapshot when it is missing. No-op when a valid name is already stored.
 * Returns the frozen name, or '' when the attempt does not exist.
 */
async function freezeDisplayNameSnapshot(dayId, uid) {
  const ref = attemptRef(dayId, uid);
  const profiles = profileRef(uid);
  return db().runTransaction(async (tx) => {
    const snap = await tx.get(ref);
    if (!snap.exists) {
      return { name: '', wrote: false, readProfile: false };
    }
    const existing = cleanSnapshotName((snap.data() || {}).displayNameSnapshot);
    if (existing) {
      return { name: existing, wrote: false, readProfile: false };
    }
    const profileSnap = await tx.get(profiles);
    const name = nameFromProfileSnap(uid, profileSnap);
    tx.update(ref, { displayNameSnapshot: name });
    return { name, wrote: true, readProfile: true };
  });
}

async function freezeDailyDisplayNameSnapshotsHandler(request) {
  const { requireAuth } = require('./validation');
  const uid = requireAuth(request);
  const data = request.data || {};
  const dayId = typeof data.dayId === 'string' ? data.dayId.trim() : '';
  assertDayId(dayId);
  const raw = Array.isArray(data.uids) ? data.uids : [];
  const seen = new Set();
  const uids = [];
  for (let i = 0; i < raw.length && uids.length < MAX_UIDS; i++) {
    if (typeof raw[i] !== 'string' || !UID_PATTERN.test(raw[i]) || seen.has(raw[i])) {
      continue;
    }
    seen.add(raw[i]);
    uids.push(raw[i]);
  }

  const names = [];
  let wrote = 0;
  let profileReads = 0;
  for (let i = 0; i < uids.length; i++) {
    const result = await freezeDisplayNameSnapshot(dayId, uids[i]);
    if (!result.name) {
      continue;
    }
    if (result.wrote) {
      wrote++;
    }
    if (result.readProfile) {
      profileReads++;
    }
    names.push({ uid: uids[i], displayName: result.name });
  }

  console.log(
    '[DailyNames] freeze dayId=' +
      dayId +
      ' callerLength=' +
      uid.length +
      ' requested=' +
      uids.length +
      ' wrote=' +
      wrote +
      ' profileReads=' +
      profileReads
  );
  return { success: true, dayId, names };
}

module.exports = {
  cleanSnapshotName,
  hasDisplayNameSnapshot,
  profileRef,
  nameFromProfileSnap,
  freezeDisplayNameSnapshot,
  freezeDailyDisplayNameSnapshotsHandler,
};
