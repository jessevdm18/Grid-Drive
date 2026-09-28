'use strict';

/**
 * Node-side score parity runner for score-vectors-v1.json.
 * Usage: npm run test:scoring
 */

const fs = require('fs');
const path = require('path');
const { calculateScoreV1 } = require('./scoring');

const vectorsPath = path.join(__dirname, '..', 'config', 'score-vectors-v1.json');
const file = JSON.parse(fs.readFileSync(vectorsPath, 'utf8'));
const scoring = file.scoring;
let failed = 0;

console.log('[DailyBackend] Score V1 parity (server calculator vs expected vectors)');
for (const v of file.vectors) {
  const result = calculateScoreV1(scoring, v.moves, v.timeMs, v.minimumMoves);
  const ok = result.totalScore === v.expectedScore;
  console.log(
    (ok ? 'PASS' : 'FAIL') +
      ` ${v.name}: moves=${v.moves} ms=${v.timeMs} min=${v.minimumMoves}` +
      ` got=${result.totalScore} expected=${v.expectedScore}` +
      ` (moveScore=${result.moveScore} timeBonus=${result.timeBonus})`
  );
  if (!ok) {
    failed++;
  }
}

if (failed > 0) {
  console.error(`[DailyBackend] ${failed} vector(s) failed.`);
  process.exit(1);
}
console.log('[DailyBackend] All score vectors matched.');
