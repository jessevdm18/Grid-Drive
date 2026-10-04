# Phase 4.4 — Daily Challenge Push Notifications

## Goal

Notify **opted-in** players when a new UTC Daily Challenge is ready.

| Field | Value |
|-------|--------|
| Title | `NEW DAILY CHALLENGE` |
| Body | `Today's challenge is ready. You only get one attempt!` |
| Topic | `daily_challenge_new` |
| Schedule | `publishDailyChallengeAndNotify` at **00:01 UTC** (`1 0 * * *`) |
| Region | `europe-west1` |
| Reward settle | `settleDailyChallengeWinners` remains **00:05 UTC** |

Push does **not** require Google Play account linking. Anonymous Firebase users can opt in.

## Firebase Messaging SDK

| Item | Status |
|------|--------|
| Installed Firebase Unity SDK | **13.15.0** |
| Packages | App, Analytics, Crashlytics, **Messaging** |
| Android BoM messaging | `firebase-messaging` / `firebase-messaging-unity:13.15.0` |

Client uses strongly typed **13.15.0** APIs (do not use 13.16+ Registration APIs):

- `Firebase.Messaging.FirebaseMessaging`
- `TokenRegistrationOnInitEnabled` (opt-in auto token registration)
- `GetTokenAsync()` (explicit registration on enable; token never logged)
- `SubscribeAsync` / `UnsubscribeAsync`
- `MessageReceived` (`MessageReceivedEventArgs`)
- `TokenReceived` (`TokenReceivedEventArgs`) — token value never logged

Not used (13.16.0+ only): `RegistrationOnInitEnabled`, `RegisterAsync`.

## Opt-in / privacy

- Default: **OFF**
- **No** notification permission prompt on first launch / startup
- Settings: **DAILY NOTIFICATIONS** ON/OFF
- Helper: “Get notified when a new Daily Challenge is ready.”
- On enable (Android 13+): request `POST_NOTIFICATIONS`; if denied → toggle stays OFF
- On enable success: `TokenRegistrationOnInitEnabled = true` → `GetTokenAsync` → subscribe `daily_challenge_new`
- On disable: unsubscribe; game remains fully functional

### Auto-init behavior (SDK 13.15.0)

Uses **`FirebaseMessaging.TokenRegistrationOnInitEnabled`**.

Until the user opts in, token registration-on-init stays **disabled**, and Android
manifest `firebase_messaging_auto_init_enabled=false` reinforces that default.

On enable: set `TokenRegistrationOnInitEnabled=true` → `GetTokenAsync()` →
`SubscribeAsync("daily_challenge_new")`.

## Server flow

1. `resolveChallengeForNow` — **same** helper as `getDailyChallenge` (no duplicated selection)
2. If challenge ensure fails → **do not notify**; log; allow scheduler retry
3. Claim send on day doc (`notificationSendClaimedAt`)
4. `admin.messaging().send` to topic `daily_challenge_new`
5. Write `notificationSentAt`, `notificationVersion`, `notificationTopic`

### Payload

```
notification.title = NEW DAILY CHALLENGE
notification.body  = Today's challenge is ready. You only get one attempt!
data.type  = daily_challenge
data.dayId = <UTC dayId>
```

No UID, score, attempt state, or auth data.

### Duplicate-send limitation (V1)

A crash **after** FCM accepts the send but **before** Firestore records `notificationSentAt` can theoretically cause a duplicate on retry. V1 does not build exactly-once messaging.

## Client architecture

| Type | Role |
|------|------|
| `DailyNotificationService` | Opt-in persistence, permission, subscribe/unsubscribe |
| `FirebaseMessagingBridge` | Reflection integration boundary (no compile-time Messaging refs) |
| `AndroidNotificationPermission` | Android 13+ `POST_NOTIFICATIONS` |
| `DailyNotificationSettingsUI` | Authored Settings toggle |
| `DailyNotificationTopics` | `daily_challenge_new` / `daily_challenge_dev` |

Foreground: log + optional Daily sync if `dayId` differs — **no** custom popup.  
Tap: opens app (platform default). If Messaging delivers the message on resume, sync Daily — no brittle deep-link hacks.

## Safe testing

| Path | Behavior |
|------|----------|
| Callable `sendDailyChallengeDevNotification` | FCM to **`daily_challenge_dev` only** |
| Editor menu | `Rush Out → Testing → Daily Challenge → Notifications → Send DEV Topic Push` |
| Production topic | **Never** targeted from Editor send button |

Also valid: Firebase Console → Messaging → test message to a device/topic `daily_challenge_dev`.

## Future Apple

Not implemented in 4.4. When adding iOS:

- APNs authentication key in Firebase Console
- Push Notifications capability
- Remote notifications background mode
- Firebase APNs configuration / `GoogleService-Info.plist`
- Prefer `FirebaseMessaging.RequestPermissionAsync` for Apple permission

Shared scripts keep Android types behind `#if UNITY_ANDROID` guards.

## Analytics

| Event | Params |
|-------|--------|
| `daily_notification_opt_in` | `source` |
| `daily_notification_opt_out` | `source` |
| `daily_notification_received` | `source`, `day_id` |
| `daily_notification_opened` | `source`, `day_id` |

Never log FCM token, FID, or UID.

## Deploy

```bash
cd Backend/DailyChallenge/functions
npm install
npm run test:scoring
npm run test:rewards
cd ..
firebase deploy --only functions --project grid-drive
```

Unity: import Firebase Messaging 13.15.0, then  
`Rush Out → UI → Create Daily Notifications Section Under Settings`.
