'use strict';

const admin = require('firebase-admin');
const { getScoringConfig, SCORE_VERSION, findLevel } = require('./config');
const { calculateScoreV1 } = require('./scoring');
const {
  resolveChallengeForNow,
  getAttempt,
  attemptToClient,
  challengeToClient,
} = require('./challenge');
const { requireAuth, validateSubmitInput, utcDayId } = require('./validation');

/**
 * Submit raw run metrics. Server computes score. Idempotent if already Completed.
 *
 * TRUST BOUNDARY: moves and completionTimeMs are still client-reported.
 *
 * Optional data.dayId is a HINT only (locate Started attempt near UTC midnight).
 * Server never trusts client dayId as the challenge authority without verifying
 * an existing Started attempt for that uid/day.
 */
async function submitDailyResultHandler(request) {
  const uid = requireAuth(request);
  const data = request.data || {};
  const validated = validateSubmitInput(data);
  if (!validated.ok) {
    return {
      success: false,
      errorCode: validated.code,
      errorMessage: validated.message,
    };
  }

  const serverNow = new Date();
  const todayId = utcDayId(serverNow);
  const dayHint =
    typeof data.dayId === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(data.dayId)
      ? data.dayId
      : '';

  const candidateDays = [];
  candidateDays.push(todayId);
  if (dayHint && dayHint !== todayId) {
    const yesterday = utcDayId(new Date(serverNow.getTime() - 24 * 60 * 60 * 1000));
    if (dayHint === yesterday) {
      candidateDays.push(dayHint);
    }
  }

  let dayId = null;
  let attemptSnap = null;
  let attemptRef = null;
  let challengeDoc = null;

  for (const candidate of candidateDays) {
    const ref = admin
      .firestore()
      .collection('dailyChallenges')
      .doc(candidate)
      .collection('attempts')
      .doc(uid);
    const snap = await ref.get();
    if (!snap.exists) {
      continue;
    }
    const state = (snap.data() || {}).state;
    if (state === 'Started' || state === 'Completed') {
      dayId = candidate;
      attemptSnap = snap;
      attemptRef = ref;
      const challengeRef = admin.firestore().collection('dailyChallenges').doc(candidate);
      const cSnap = await challengeRef.get();
      challengeDoc = cSnap.exists ? cSnap.data() : null;
      break;
    }
  }

  if (!dayId || !attemptSnap) {
    // Ensure today challenge exists for consistent error payload.
    const todayChallenge = await resolveChallengeForNow(serverNow);
    return {
      success: false,
      errorCode: 'no_attempt',
      errorMessage: 'No started attempt.',
      challenge: challengeToClient(todayChallenge),
    };
  }

  const challenge = {
    dayId,
    levelId: (challengeDoc && challengeDoc.levelId) || validated.levelId,
    levelVersion:
      (challengeDoc && challengeDoc.levelVersion) ||
      (challengeDoc && challengeDoc.levelId) ||
      validated.levelId,
    scoreVersion:
      challengeDoc && challengeDoc.scoreVersion != null
        ? challengeDoc.scoreVersion
        : SCORE_VERSION,
    opensAtUtc: (challengeDoc && challengeDoc.opensAtUtc) || `${dayId}T00:00:00Z`,
    closesAtUtc: (challengeDoc && challengeDoc.closesAtUtc) || '',
    minimumMoves:
      challengeDoc && challengeDoc.minimumMoves != null
        ? challengeDoc.minimumMoves
        : null,
  };

  if (validated.levelId !== challenge.levelId) {
    return {
      success: false,
      errorCode: 'level_mismatch',
      errorMessage: 'Level does not match challenge.',
      challenge: challengeToClient(challenge),
      attempt: attemptToClient(await getAttempt(dayId, uid)),
    };
  }

  const catalogLevel = findLevel(challenge.levelId);
  const minimumMoves =
    challenge.minimumMoves != null
      ? challenge.minimumMoves
      : catalogLevel
        ? catalogLevel.minimumMoves
        : 0;

  if (minimumMoves <= 0) {
    return {
      success: false,
      errorCode: 'no_level',
      errorMessage: 'Challenge minimumMoves missing.',
    };
  }

  const scoring = getScoringConfig();
  const computed = calculateScoreV1(
    scoring,
    validated.moves,
    validated.completionTimeMs,
    minimumMoves
  );

  const outcome = await admin.firestore().runTransaction(async (tx) => {
    const snap = await tx.get(attemptRef);
    if (!snap.exists) {
      return { status: 'no_attempt' };
    }

    const existing = snap.data() || {};
    if (existing.state === 'Completed') {
      return { status: 'already_completed', data: existing };
    }

    if (existing.state === 'Abandoned') {
      return { status: 'abandoned', data: existing };
    }

    if (existing.state !== 'Started') {
      return { status: 'bad_state', data: existing };
    }

    if (existing.levelId && existing.levelId !== validated.levelId) {
      return { status: 'level_mismatch', data: existing };
    }

    if (existing.scoreVersion != null && existing.scoreVersion !== SCORE_VERSION) {
      return { status: 'score_version', data: existing };
    }

    const patch = {
      state: 'Completed',
      moves: validated.moves,
      completionTimeMs: validated.completionTimeMs,
      score: computed.totalScore,
      moveScore: computed.moveScore,
      timeBonus: computed.timeBonus,
      scoreVersion: SCORE_VERSION,
      scoreTrusted: true,
      levelId: challenge.levelId,
      levelVersion: challenge.levelVersion,
      minimumMoves,
      completedAt: admin.firestore.FieldValue.serverTimestamp(),
    };
    tx.update(attemptRef, patch);
    return { status: 'completed', data: { ...existing, ...patch } };
  });

  if (outcome.status === 'abandoned') {
    return {
      success: false,
      errorCode: 'abandoned',
      errorMessage: 'Attempt was abandoned.',
      challenge: challengeToClient(challenge),
      attempt: attemptToClient(await getAttempt(dayId, uid)),
    };
  }

  if (
    outcome.status === 'bad_state' ||
    outcome.status === 'level_mismatch' ||
    outcome.status === 'score_version' ||
    outcome.status === 'no_attempt'
  ) {
    return {
      success: false,
      errorCode: outcome.status,
      errorMessage: 'Submit rejected.',
      challenge: challengeToClient(challenge),
      attempt: attemptToClient(await getAttempt(dayId, uid)),
    };
  }

  const attempt = await getAttempt(dayId, uid);
  return {
    success: true,
    idempotent: outcome.status === 'already_completed',
    challenge: challengeToClient(challenge),
    attempt: attemptToClient(attempt),
    score: attempt.score,
    scoreTrusted: true,
  };
}

module.exports = { submitDailyResultHandler };
