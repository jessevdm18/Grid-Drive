'use strict';

/**
 * Offline ranking / eligibility unit checks for Phase 4.3 rewards.
 * Run: npm run test:rewards
 */

const {
  isEligibleAttempt,
  compareRankRows,
  findPlayerRank,
  REWARD_AMOUNT_COINS,
} = require('./rewards');

let failed = 0;

function assert(cond, msg) {
  if (!cond) {
    failed += 1;
    console.error('FAIL: ' + msg);
  } else {
    console.log('PASS: ' + msg);
  }
}

function sortRows(rows) {
  return rows.slice().sort(compareRankRows);
}

// Eligibility
assert(
  isEligibleAttempt(
    { state: 'Completed', scoreTrusted: true, score: 10, moves: 5, completionTimeMs: 1000 },
    'uidA'
  ),
  'eligible completed+trusted'
);
assert(
  !isEligibleAttempt(
    { state: 'Completed', scoreTrusted: false, score: 10, moves: 5, completionTimeMs: 1000 },
    'uidA'
  ),
  'reject untrusted'
);
assert(
  !isEligibleAttempt(
    { state: 'Started', scoreTrusted: true, score: 10, moves: 5, completionTimeMs: 1000 },
    'uidA'
  ),
  'reject Started'
);
assert(
  !isEligibleAttempt(
    { state: 'Abandoned', scoreTrusted: true, score: 10, moves: 5, completionTimeMs: 1000 },
    'uidA'
  ),
  'reject Abandoned'
);
assert(
  !isEligibleAttempt(
    { state: 'Completed', scoreTrusted: true, score: 10, moves: 5, completionTimeMs: 1000 },
    ''
  ),
  'reject empty uid'
);
assert(
  !isEligibleAttempt(
    { state: 'Completed', scoreTrusted: true, score: 'x', moves: 5, completionTimeMs: 1000 },
    'uidA'
  ),
  'reject malformed score'
);

// Ranking order
const ranked = sortRows([
  { uid: 'b', score: 100, moves: 10, completionTimeMs: 5000 },
  { uid: 'a', score: 200, moves: 20, completionTimeMs: 9000 },
  { uid: 'c', score: 100, moves: 8, completionTimeMs: 6000 },
  { uid: 'd', score: 100, moves: 10, completionTimeMs: 4000 },
  { uid: 'e', score: 100, moves: 10, completionTimeMs: 5000 },
]);

assert(ranked[0].uid === 'a', 'score DESC wins (#1 = a)');
assert(ranked[1].uid === 'c', 'moves ASC among same score (#2 = c)');
assert(ranked[2].uid === 'd', 'time ASC among same score+moves (#3 = d)');
assert(ranked[3].uid === 'b', 'uid ASC final tie-break (#4 = b before e)');
assert(ranked[4].uid === 'e', 'uid ASC final tie-break (#5 = e)');
assert(findPlayerRank(ranked, 'a') === 1, 'findPlayerRank #1');
assert(findPlayerRank(ranked, 'c') === 2, 'findPlayerRank #2');
assert(findPlayerRank(ranked, 'd') === 3, 'findPlayerRank #3');
assert(findPlayerRank(ranked, 'missing') === 0, 'findPlayerRank absent');
assert(findPlayerRank(ranked, '') === 0, 'findPlayerRank empty uid');

assert(REWARD_AMOUNT_COINS === 1000, 'V1 reward amount is 1000 coins');

if (failed > 0) {
  console.error('Reward ranking tests failed: ' + failed);
  process.exit(1);
}

console.log('All reward ranking tests passed.');
process.exit(0);
