# Rush Out Daily Challenge Backend (Phase 4.1B)

Authoritative Daily Challenge via **Firebase Cloud Functions + Firestore Admin SDK**.

See **[DEPLOY.md](./DEPLOY.md)** for exact deployment steps.

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
      scoring.js
      validation.js
      hash.js
      config.js
      runScoreParity.js
```

## Callables (region: europe-west1 · Firestore: eur3)

| Function | Purpose |
|----------|---------|
| `getDailyChallenge` | Server UTC day + challenge + attempt |
| `startDailyAttempt` | Atomic Started claim |
| `submitDailyResult` | Server score + Completed |
| `abandonDailyAttempt` | Started → Abandoned |

## Unity

- `DailyChallengeBackendMode.Online` on `DailyChallengeConfig`
- `DailyChallengeFirebaseSettings` — projectId, apiKey, functionsRegion
- Mutations: `FirebaseOnlineDailyChallengeAuthority` → Callable REST
- Leaderboard reads: Firestore query (Completed only)
