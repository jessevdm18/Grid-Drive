'use strict';

const fs = require('fs');
const path = require('path');

/** Firebase Functions region — keep in sync with Unity DailyChallengeFirebaseSettings. */
const FUNCTIONS_REGION = 'europe-west1';

const SCORE_VERSION = 1;

let cachedCatalog = null;

function loadCatalog() {
  if (cachedCatalog) {
    return cachedCatalog;
  }

  // Packaged under functions/config so deploy includes the catalog.
  const catalogPath = path.join(__dirname, '..', 'config', 'daily-levels.json');
  const raw = fs.readFileSync(catalogPath, 'utf8');
  cachedCatalog = JSON.parse(raw);
  if (!cachedCatalog.levels || cachedCatalog.levels.length === 0) {
    throw new Error('daily-levels.json has no levels');
  }
  return cachedCatalog;
}

function getScoringConfig() {
  const catalog = loadCatalog();
  return catalog.scoring || {
    baseMoveScore: 100000,
    movePenaltyPerExtraMove: 2500,
    minimumMoveScore: 10000,
    timeBonusMax: 20000,
    timeReferenceSeconds: 60,
  };
}

function getValidationConfig() {
  const catalog = loadCatalog();
  return catalog.validation || {
    minMoves: 1,
    maxMoves: 5000,
    minCompletionTimeMs: 500,
    maxCompletionTimeMs: 21600000,
  };
}

function getLevels() {
  return loadCatalog().levels;
}

function findLevel(levelId) {
  return getLevels().find((l) => l.levelId === levelId) || null;
}

module.exports = {
  FUNCTIONS_REGION,
  SCORE_VERSION,
  loadCatalog,
  getScoringConfig,
  getValidationConfig,
  getLevels,
  findLevel,
};
