'use strict';

const { getValidationConfig, SCORE_VERSION } = require('./config');

function requireAuth(request) {
  if (!request.auth || !request.auth.uid) {
    const err = new Error('Authentication required.');
    err.code = 'unauthenticated';
    throw err;
  }
  return request.auth.uid;
}

function validateSubmitInput(data) {
  const v = getValidationConfig();
  const levelId = data && typeof data.levelId === 'string' ? data.levelId.trim() : '';
  const moves = data && data.moves != null ? Number(data.moves) : NaN;
  const completionTimeMs =
    data && data.completionTimeMs != null ? Number(data.completionTimeMs) : NaN;
  const scoreVersion =
    data && data.scoreVersion != null ? Number(data.scoreVersion) : SCORE_VERSION;

  if (!levelId || levelId.length >= 128) {
    return { ok: false, code: 'invalid_level', message: 'Invalid levelId.' };
  }
  if (!Number.isInteger(moves) || moves < v.minMoves || moves > v.maxMoves) {
    return { ok: false, code: 'invalid_moves', message: 'Invalid moves.' };
  }
  if (
    !Number.isFinite(completionTimeMs) ||
    !Number.isInteger(completionTimeMs) ||
    completionTimeMs < v.minCompletionTimeMs ||
    completionTimeMs > v.maxCompletionTimeMs
  ) {
    return {
      ok: false,
      code: 'invalid_time',
      message: 'Invalid completionTimeMs.',
    };
  }
  if (scoreVersion !== SCORE_VERSION) {
    return {
      ok: false,
      code: 'score_version',
      message: 'Unsupported scoreVersion.',
    };
  }

  return {
    ok: true,
    levelId,
    moves,
    completionTimeMs,
    scoreVersion,
  };
}

function utcDayId(date = new Date()) {
  const y = date.getUTCFullYear();
  const m = String(date.getUTCMonth() + 1).padStart(2, '0');
  const d = String(date.getUTCDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

function nextUtcMidnightIso(date = new Date()) {
  const next = new Date(
    Date.UTC(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate() + 1, 0, 0, 0, 0)
  );
  return next.toISOString().replace(/\.\d{3}Z$/, 'Z');
}

function dayOpenIso(dayId) {
  return `${dayId}T00:00:00Z`;
}

module.exports = {
  requireAuth,
  validateSubmitInput,
  utcDayId,
  nextUtcMidnightIso,
  dayOpenIso,
};
