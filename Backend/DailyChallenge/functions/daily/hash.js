'use strict';

/**
 * FNV-1a 32-bit over UTF-16 code units — matches DailyChallengeHash.StableHash32 (C#).
 */
function stableHash32(value) {
  const FnvOffset = 2166136261;
  const FnvPrime = 16777619;
  if (!value) {
    return FnvOffset >>> 0;
  }
  let hash = FnvOffset >>> 0;
  for (let i = 0; i < value.length; i++) {
    hash ^= value.charCodeAt(i);
    hash = Math.imul(hash, FnvPrime) >>> 0;
  }
  return hash >>> 0;
}

function selectLevelIndexForDay(dayId, levelCount) {
  if (levelCount <= 0) {
    return -1;
  }
  const hash = stableHash32(dayId || '');
  return hash % levelCount;
}

module.exports = {
  stableHash32,
  selectLevelIndexForDay,
};
