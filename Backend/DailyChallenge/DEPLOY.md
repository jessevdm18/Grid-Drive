# Daily Challenge Phase 4.1B — Deployment

Project: **grid-drive**

| Resource | Location / region |
|----------|-------------------|
| Cloud Firestore | **eur3** (Europe) |
| Cloud Functions | **europe-west1** |

Unity `DailyChallengeFirebaseSettings.functionsRegion` must be **europe-west1** (Firebase recommends this as the nearest Functions region for an eur3 Firestore database).

## Architecture after 4.1B

```
Unity (ID token)
  -> Callable Cloud Functions in europe-west1 (Admin SDK writes)
       getDailyChallenge
       startDailyAttempt
       submitDailyResult
       abandonDailyAttempt
  -> Firestore (eur3)

Unity may READ Firestore for Completed leaderboard rows.
Unity must NOT write challenge/attempt documents (rules enforce write:false).
```

Ownership key: **Firebase UID** (`request.auth.uid`). Provider-independent.
Anonymous Auth is V1 only; future Google Play / Apple linking that preserves the same UID keeps Daily data attached.

## Billing / plan check

Cloud Functions deployment may require the Blaze (pay-as-you-go) plan depending on current Firebase requirements.
**Verify in Firebase Console before deploying. Do not enable billing automatically.**

## Steps

### 1. Firebase CLI

```bash
npm install -g firebase-tools
firebase login
firebase use grid-drive
```

Run commands from:

`Backend/DailyChallenge/`

### 2. Enable Anonymous Authentication

Firebase Console → Authentication → Sign-in method → **Anonymous** → Enable.

### 3. Firestore database

Production Firestore already exists in **eur3**.
Rules in this folder replace client write permissions (deploy rules; do not recreate the database).

### 4. Install function dependencies

```bash
cd functions
npm install
npm run test:scoring
cd ..
```

`npm run test:scoring` must exit 0 (exact integer score parity).

### 5. Export / verify Daily level catalog

In Unity Editor:

`Rush Out → Testing → Daily Challenge → Backend → Export Backend Daily Level Catalog`

Then confirm both files match:

- `config/daily-levels.json`
- `functions/config/daily-levels.json`

### 6. Deploy rules + indexes + functions

```bash
firebase deploy --only firestore:rules,firestore:indexes,functions --project grid-drive
```

Or stepwise:

```bash
firebase deploy --only firestore:rules --project grid-drive
firebase deploy --only firestore:indexes --project grid-drive
firebase deploy --only functions --project grid-drive
```

### 7. Verify region

Deployed callables should be in **europe-west1**:

- `getDailyChallenge`
- `startDailyAttempt`
- `submitDailyResult`
- `abandonDailyAttempt`

Callable base URL shape:

`https://europe-west1-grid-drive.cloudfunctions.net/<functionName>`

Unity settings: `Assets/Resources/DailyChallengeFirebaseSettings.asset` → Functions Region = `europe-west1`.

### 8. Switch Unity to Online

1. Select `Assets/Resources/DailyChallengeConfig.asset`
2. Backend Mode = **Online**
3. Play Mode → MainMenu → Daily card should sync (`CONNECT TO PLAY` if offline)

### 9. Safe smoke test

Editor menus:

- Backend → Log Identity State
- Backend → Fetch Server Challenge
- Backend → Compare Local vs Server Score Test Vectors (all PASS)
- Start Daily once → claim Started
- Complete → server score + Completed
- Backend → Refresh Online Leaderboard

**Do not** add client tools that delete/reset production attempts.

## Emulator (optional)

```bash
cd Backend/DailyChallenge
firebase emulators:start --only functions,firestore
```

In Unity (Editor / Development Build only):

- DailyChallengeFirebaseSettings → Use Functions Emulator = true
- Host `127.0.0.1`, Port `5001`

Release player builds ignore emulator flags.

## Security summary

**Secured by 4.1B**

- Final score calculated server-side (`scoreTrusted: true`)
- No client create/update/delete of challenges or attempts
- Atomic one-attempt claim (transaction)
- Server UTC day for challenge/start
- Server timestamps via Admin `FieldValue.serverTimestamp()`
- Idempotent Completed submit

**Not fully secured (known trust boundary)**

- Client-reported `moves`
- Client-reported `completionTimeMs`

**Not implemented**

- Google Play Games / Sign in with Apple
- Friends / rewards / push
- Replay verification
- App Check / Play Integrity / App Attest

## Identity continuity (future)

Anonymous Firebase UID `X` → later link Google Play / Apple **to the same user** → UID remains `X` → Daily attempts/results remain under `X`. Do not key ownership by provider IDs.
