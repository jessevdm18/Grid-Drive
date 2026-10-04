# Rush Out Daily Challenge Backend (Phase 4.1B / 4.3)

Authoritative Daily Challenge via **Firebase Cloud Functions + Firestore Admin SDK**.

See **[DEPLOY.md](./DEPLOY.md)** for exact deployment steps.  
See **[REWARDS.md](./REWARDS.md)** for Phase 4.3 Daily #1 winner rewards.  
See **[NOTIFICATIONS.md](./NOTIFICATIONS.md)** for Phase 4.4 Daily push notifications.

## Layout

```
Backend/DailyChallenge/
  firebase.json
  firestore.rules          # client writes denied
  firestore.indexes.json
  DEPLOY.md
  config/
    daily-levels.json      # repo copy of eligible catalog
    score-vectors-v1.json
  functions/
    package.json
    index.js               # exports callables
    config/                # deployed catalog + vectors
    daily/
      challenge.js
      getChallenge.js
      startAttempt.js
      submitResult.js
      abandonAttempt.js
      rewards.js            # Phase 4.3 settlement + claim
      notifications.js      # Phase 4.4 publish + FCM topic
      scoring.js
      validation.js
      hash.js
      config.js
      runScoreParity.js
      runRewardRankingTest.js
  REWARDS.md
  NOTIFICATIONS.md
```

## Callables (region: europe-west1 · Firestore: eur3)

| Function | Purpose |
|----------|---------|
| `getDailyChallenge` | Server UTC day + challenge + attempt |
| `startDailyAttempt` | Atomic Started claim |
| `submitDailyResult` | Server score + Completed |
| `abandonDailyAttempt` | Started → Abandoned |
| `getPendingDailyRewards` | Unclaimed #1 rewards for auth UID |
| `claimDailyReward` | Claim one day reward (idempotent) |
| `createDevDailyReward` | DEV only — smoke reward (`ALLOW_DEV_DAILY_REWARDS`) |
| `publishDailyChallengeAndNotify` | Scheduled 00:01 UTC — ensure today + FCM topic |
| `settleDailyChallengeWinners` | Scheduled 00:05 UTC — settle previous day #1 |
| `sendDailyChallengeDevNotification` | SAFE test push to `daily_challenge_dev` only |

## Unity

- `DailyChallengeBackendMode.Online` on `DailyChallengeConfig`
- `DailyChallengeFirebaseSettings` — projectId, apiKey, functionsRegion
- Mutations: `FirebaseOnlineDailyChallengeAuthority` → Callable REST
- Leaderboard reads: Firestore query (Completed only)
