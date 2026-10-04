'use strict';

const {
  TOPIC_PRODUCTION,
  TOPIC_DEV,
  NOTIFICATION_TITLE,
  NOTIFICATION_BODY,
  NOTIFICATION_VERSION,
} = require('./notifications');

let failed = 0;
function assert(cond, msg) {
  if (!cond) {
    failed += 1;
    console.error('FAIL: ' + msg);
  } else {
    console.log('PASS: ' + msg);
  }
}

assert(TOPIC_PRODUCTION === 'daily_challenge_new', 'production topic');
assert(TOPIC_DEV === 'daily_challenge_dev', 'dev topic');
assert(TOPIC_PRODUCTION !== TOPIC_DEV, 'dev topic distinct from production');
assert(NOTIFICATION_TITLE === 'NEW DAILY CHALLENGE', 'title');
assert(
  NOTIFICATION_BODY === "Today's challenge is ready. You only get one attempt!",
  'body'
);
assert(NOTIFICATION_VERSION === 1, 'notification version');

if (failed > 0) {
  process.exit(1);
}
console.log('All notification constant tests passed.');
process.exit(0);
