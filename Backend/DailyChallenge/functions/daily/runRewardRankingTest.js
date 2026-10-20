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

const {
  PRIZE_AMOUNT_BY_RANK,
  REWARD_SETTLEMENT_VERSION,
  LEGACY_REWARD_SETTLEMENT_VERSION,
  compareHybridRows,
  rankHybrid,
  prizesForRanking,
} = require('./rewards');
const {
  resultCopy,
  BODY_RANKED,
  TITLE,
  shouldDeliverQueuedResult,
} = require('./resultNotifications');
const {
  generateSimulatedEntries,
  xorShift32,
  SIMULATED_ENTRY_COUNT,
  FIRST_NAMES,
  SURNAME_INITIALS,
} = require('./simulatedEntries');
const { calculateScoreV1 } = require('./scoring');
const { getScoringConfig } = require('./config');
const fs = require('fs');
const path = require('path');

assert(LEGACY_REWARD_SETTLEMENT_VERSION === 1, 'legacy settlement stays version 1');
assert(REWARD_SETTLEMENT_VERSION === 2, 'hybrid settlement is version 2');
assert(PRIZE_AMOUNT_BY_RANK[1] === 1000, 'prize #1 is 1000');
assert(PRIZE_AMOUNT_BY_RANK[2] === 500, 'prize #2 is 500');
assert(PRIZE_AMOUNT_BY_RANK[3] === 250, 'prize #3 is 250');
assert(FIRST_NAMES.length === 50, 'simulated first-name list length');
assert(SURNAME_INITIALS.length === 20, 'simulated initial list length');

function xorShift32Reference(state) {
  let x = BigInt(state >>> 0);
  const mask = 0xffffffffn;
  x = (x ^ ((x << 13n) & mask)) & mask;
  x = (x ^ (x >> 17n)) & mask;
  x = (x ^ ((x << 5n) & mask)) & mask;
  return Number(x);
}

let rngState = 0xa341316c;
let rngOk = true;
for (let step = 0; step < 5000; step++) {
  const next = xorShift32(rngState);
  const ref = xorShift32Reference(rngState);
  if (next !== ref) {
    rngOk = false;
    break;
  }
  rngState = next;
}
assert(rngOk, 'xorshift32 matches uint32 reference');
assert(xorShift32(1) === 270369, 'xorshift32 seed 1 first step');

function row(uid, score, moves, time, simulated) {
  return {
    isSimulated: !!simulated,
    uid: simulated ? '' : uid,
    tieBreakId: uid,
    score,
    moves,
    completionTimeMs: time,
  };
}

const realOnly = rankHybrid([
  row('b', 100, 10, 5000, false),
  row('a', 200, 20, 9000, false),
  row('c', 100, 8, 6000, false),
  row('d', 100, 10, 4000, false),
  row('e', 100, 10, 5000, false),
]);
assert(
  realOnly.map((r) => r.uid).join(',') === 'a,c,d,b,e',
  'hybrid tie-break matches real UID order when no bots are present'
);
assert(
  compareHybridRows(realOnly[3], realOnly[4]) < 0,
  'equal score/moves/time orders by tieBreakId'
);

const caseA = rankHybrid([
  row('real1', 500, 8, 40000, false),
  row('sim_b', 400, 9, 40000, true),
  row('sim_c', 300, 10, 40000, true),
  row('real4', 200, 12, 40000, false),
]);
const prizesA = prizesForRanking(caseA);
assert(caseA[0].uid === 'real1' && caseA[0].rank === 1, 'A real is #1');
assert(caseA[1].isSimulated && caseA[2].isSimulated, 'A bots hold #2 and #3');
assert(caseA[3].uid === 'real4' && caseA[3].rank === 4, 'A second real stays #4');
assert(
  prizesA[0].rewarded && prizesA[0].amount === 1000 && prizesA[0].uid === 'real1',
  'A #1 real receives 1000'
);
assert(!prizesA[1].rewarded && !prizesA[2].rewarded, 'A bot slots create no rewards');
assert(
  prizesA.every((prize) => prize.uid !== 'real4'),
  'A #4 real is not promoted and receives nothing'
);

const caseB = rankHybrid([
  row('sim1', 500, 8, 30000, true),
  row('real2', 400, 9, 30000, false),
  row('sim3', 300, 10, 30000, true),
]);
const prizesB = prizesForRanking(caseB);
assert(caseB[1].uid === 'real2' && caseB[1].rank === 2, 'B real stays #2');
assert(!prizesB[0].rewarded, 'B #1 bot is unpaid');
assert(
  prizesB[1].rewarded && prizesB[1].amount === 500 && prizesB[1].uid === 'real2',
  'B #2 real receives only the #2 prize'
);
assert(!prizesB[2].rewarded, 'B #3 bot is unpaid');

const caseC = rankHybrid([
  row('sim1', 500, 8, 30000, true),
  row('sim2', 400, 9, 30000, true),
  row('real3', 300, 10, 30000, false),
]);
const prizesC = prizesForRanking(caseC);
assert(caseC[2].uid === 'real3' && caseC[2].rank === 3, 'C real stays #3');
assert(!prizesC[0].rewarded && !prizesC[1].rewarded, 'C top bots are unpaid');
assert(
  prizesC[2].rewarded && prizesC[2].amount === 250 && prizesC[2].uid === 'real3',
  'C #3 real receives only the #3 prize'
);

const caseD = rankHybrid([
  row('sim1', 500, 8, 30000, true),
  row('sim2', 400, 9, 30000, true),
  row('sim3', 300, 10, 30000, true),
]);
const prizesD = prizesForRanking(caseD);
assert(
  prizesD.every((prize) => !prize.rewarded),
  'D three bots create no reward documents'
);

const caseE = rankHybrid([
  row('r1', 500, 8, 30000, false),
  row('r2', 400, 9, 30000, false),
  row('r3', 300, 10, 30000, false),
]);
const prizesE = prizesForRanking(caseE);
assert(prizesE[0].amount === 1000 && prizesE[0].uid === 'r1', 'E #1 prize');
assert(prizesE[1].amount === 500 && prizesE[1].uid === 'r2', 'E #2 prize');
assert(prizesE[2].amount === 250 && prizesE[2].uid === 'r3', 'E #3 prize');

const caseFRows = [];
for (let i = 0; i < 12; i++) {
  caseFRows.push(row('sim_' + i, 1000 - i, 8, 30000, true));
}
caseFRows.push(row('real13', 100, 20, 90000, false));
const caseF = rankHybrid(caseFRows);
const realF = caseF.find((entry) => entry.uid === 'real13');
assert(realF && realF.rank === 13, 'F real hybrid rank is 13');
assert(
  prizesForRanking(caseF).every((prize) => !prize.rewarded || prize.uid !== 'real13'),
  'F rank 13 receives no prize'
);

const tied = rankHybrid([
  row('b', 10, 1, 1, false),
  row('sim_a', 10, 1, 1, true),
  row('a', 10, 1, 1, false),
]);
assert(
  tied.map((entry) => entry.tieBreakId).join(',') === 'a,b,sim_a',
  'equal rows order by tieBreakId, real UID and bot id together'
);

const bots = generateSimulatedEntries({
  dayId: '2026-01-15',
  levelId: 'Daily_001',
  minimumMoves: 9,
});
const botsAgain = generateSimulatedEntries({
  dayId: '2026-01-15',
  levelId: 'Daily_001',
  minimumMoves: 9,
});
const otherDay = generateSimulatedEntries({
  dayId: '2026-01-16',
  levelId: 'Daily_001',
  minimumMoves: 9,
});
assert(bots.length === SIMULATED_ENTRY_COUNT, 'official bot count is 30');
assert(JSON.stringify(bots) === JSON.stringify(botsAgain), 'bot list is deterministic');
assert(JSON.stringify(bots) !== JSON.stringify(otherDay), 'a different day changes the bots');
assert(bots[0].entryId === 'sim_2026-01-15_000', 'bot id is stable');
assert(bots.every((bot) => bot.isSimulated === true && !bot.uid), 'bots are not player rewards');
const scoring = getScoringConfig();
let scoresMatch = true;
for (let i = 0; i < bots.length; i++) {
  const expected = calculateScoreV1(
    scoring,
    bots[i].moves,
    bots[i].completionTimeMs,
    9
  ).totalScore;
  if (expected !== bots[i].score) {
    scoresMatch = false;
    break;
  }
}
assert(scoresMatch, 'bot scores use the server score calculator');

const friendsSrc = fs.readFileSync(
  path.join(__dirname, 'friendsLeaderboard.js'),
  'utf8'
);
assert(
  friendsSrc.indexOf('simulatedEntries') < 0 &&
    friendsSrc.indexOf('loadHybridRanking') < 0 &&
    friendsSrc.indexOf('prizesForRanking') < 0,
  'friends leaderboard does not read bots or assign prizes'
);
assert(
  friendsSrc.indexOf('never creates rewards') >= 0,
  'friends ranking stays reward-free'
);

assert(resultCopy(1).title === TITLE, 'result title');
assert(
  resultCopy(1).body === 'You finished #1! Your reward is waiting.',
  'notify #1'
);
assert(
  resultCopy(2).body === 'You finished #2! Your reward is waiting.',
  'notify #2'
);
assert(
  resultCopy(3).body === 'You finished #3! Your reward is waiting.',
  'notify #3'
);
assert(resultCopy(4).body === BODY_RANKED, 'notify non-prize rank');
assert(resultCopy(13).body === BODY_RANKED, 'notify rank 13');

assert(
  shouldDeliverQueuedResult({ rank: 1 }, { claimed: false }) === true,
  'rank 1 unclaimed still sends reward copy'
);
assert(
  shouldDeliverQueuedResult({ rank: 1 }, { claimed: true }) === false,
  'rank 1 claimed before delivery sends nothing'
);
assert(
  shouldDeliverQueuedResult({ rank: 2 }, { claimed: true }) === false,
  'rank 2 claimed before delivery sends nothing'
);
assert(
  shouldDeliverQueuedResult({ rank: 3 }, { claimed: true }) === false,
  'rank 3 claimed before delivery sends nothing'
);
assert(
  shouldDeliverQueuedResult({ rank: 10 }, null) === true,
  'rank 10 still sends the generic result'
);
assert(
  shouldDeliverQueuedResult(
    { rank: 1, cancelledAt: { seconds: 1 } },
    { claimed: false }
  ) === false,
  'cancelled queue is not sent'
);
assert(
  shouldDeliverQueuedResult(
    { rank: 1, sentAt: { seconds: 1 } },
    { claimed: true }
  ) === false,
  'claimed reward is not sent again'
);
const rewardsSrc = fs.readFileSync(path.join(__dirname, 'rewards.js'), 'utf8');
const cancelFn = rewardsSrc.slice(
  rewardsSrc.indexOf('function cancelQueuedResultInTransaction'),
  rewardsSrc.indexOf('async function claimDailyRewardHandler')
);
assert(
  cancelFn.indexOf('cancelReason') >= 0 && cancelFn.indexOf('sentAt') < 0,
  'claim cancel marks the queue and does not rewrite sentAt'
);

if (failed > 0) {
  console.error('Reward ranking tests failed: ' + failed);
  process.exit(1);
}

console.log('All reward ranking tests passed.');
process.exit(0);
