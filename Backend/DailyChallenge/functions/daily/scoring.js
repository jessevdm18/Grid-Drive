'use strict';

/**
 * Daily Challenge Score V1 — must match Unity DailyChallengeScoreCalculator exactly.
 *
 * Unity uses single-precision floats for the time-bonus path and Mathf.RoundToInt
 * (banker's rounding / half-to-even).
 */

function f32(n) {
  return Math.fround(n);
}

function roundHalfToEven(value) {
  if (!Number.isFinite(value)) {
    return 0;
  }
  // Operate on the float32-rounded magnitude Unity would see after cast-to-int path.
  const x = f32(value);
  const floor = Math.floor(x);
  const frac = x - floor;
  // Compare with float tolerance around 0.5
  if (frac > 0.5) {
    return floor + 1;
  }
  if (frac < 0.5) {
    return floor;
  }
  return floor % 2 === 0 ? floor : floor + 1;
}

function calculateScoreV1(scoring, moves, completionTimeMs, minimumMoves) {
  const baseMove = scoring.baseMoveScore | 0;
  const penalty = scoring.movePenaltyPerExtraMove | 0;
  const minMoveScore = scoring.minimumMoveScore | 0;
  const timeBonusMax = scoring.timeBonusMax | 0;
  const timeRef = f32(scoring.timeReferenceSeconds);

  let referenceMoves = minimumMoves | 0;
  if (referenceMoves <= 0) {
    referenceMoves = Math.max(1, moves | 0);
  }

  const safeMoves = Math.max(0, moves | 0);
  const safeMs = completionTimeMs < 0 ? 0 : completionTimeMs;

  const moveOverPar = Math.max(0, safeMoves - referenceMoves);
  const moveScore = Math.max(minMoveScore, baseMove - moveOverPar * penalty);

  // Mirror: float timeSeconds = safeMs / 1000f;
  const timeSeconds = f32(safeMs / 1000);
  const denom = f32(timeRef + timeSeconds);
  let timeBonus = 0;
  if (denom > 0.0001 && timeBonusMax > 0 && timeRef > 0) {
    // Mathf.RoundToInt(timeBonusMax * (timeRef / denom))
    const ratio = f32(timeRef / denom);
    const raw = f32(timeBonusMax * ratio);
    timeBonus = roundHalfToEven(raw);
    timeBonus = Math.max(0, Math.min(timeBonusMax, timeBonus));
  }

  const totalScore = Math.max(0, moveScore + timeBonus);
  return {
    referenceMoves,
    moves: safeMoves,
    completionTimeMs: safeMs,
    moveOverPar,
    moveScore,
    timeBonus,
    totalScore,
  };
}

module.exports = {
  roundHalfToEven,
  calculateScoreV1,
  f32,
};
