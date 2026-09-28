'use strict';

/**
 * Rush Out Daily Challenge — Phase 4.1B Cloud Functions
 *
 * Authoritative mutations use Admin SDK. Clients call these with Firebase ID token.
 * Ownership key is always request.auth.uid (provider-independent Firebase UID).
 *
 * Anonymous Auth is V1 only. Future Google Play / Apple linking that preserves
 * the same Firebase UID keeps all Daily attempts/results attached automatically.
 */

const admin = require('firebase-admin');
const { onCall, HttpsError } = require('firebase-functions/v2/https');
const { setGlobalOptions } = require('firebase-functions/v2');
const { FUNCTIONS_REGION } = require('./daily/config');
const { getDailyChallengeHandler } = require('./daily/getChallenge');
const { startDailyAttemptHandler } = require('./daily/startAttempt');
const { submitDailyResultHandler } = require('./daily/submitResult');
const { abandonDailyAttemptHandler } = require('./daily/abandonAttempt');

admin.initializeApp();
setGlobalOptions({ region: FUNCTIONS_REGION, maxInstances: 20 });

function wrap(handler) {
  return async (request) => {
    try {
      return await handler(request);
    } catch (err) {
      const code = err && err.code ? err.code : 'internal';
      const message = err && err.message ? err.message : 'Internal error.';
      if (code === 'unauthenticated') {
        throw new HttpsError('unauthenticated', message);
      }
      if (code === 'no_level') {
        throw new HttpsError('failed-precondition', message);
      }
      console.error('[DailyBackend] function error', code, message);
      throw new HttpsError('internal', message);
    }
  };
}

exports.getDailyChallenge = onCall(wrap(getDailyChallengeHandler));
exports.startDailyAttempt = onCall(wrap(startDailyAttemptHandler));
exports.submitDailyResult = onCall(wrap(submitDailyResultHandler));
exports.abandonDailyAttempt = onCall(wrap(abandonDailyAttemptHandler));
