# Phase 4.2A / 4.2B — Optional Google Play Account Linking

## Product rule

First launch uses **Firebase Anonymous Auth** automatically.  
**No Google Play popup on startup.**

Later, optional Settings action:

**Link Google Play** → Play Games `ManuallyAuthenticate` → server auth code → Firebase REST link → **same Firebase UID**.

Daily ownership remains Firebase UID only.

## Plugin status

| Item | Value |
|------|--------|
| Plugin | Google Play Games for Unity under `Assets/GooglePlayGames` |
| Runtime version | PluginVersion **2.1.0** (`GooglePlayGamesPlugin_v2.2.1`) |
| Assembly | `Google.Play.Games` (Android + Editor) |
| Package name | `com.raddergames.griddrive` |

## OAuth client roles (do not confuse)

| Client | Role |
|--------|------|
| **Android OAuth client** | Package name + SHA-1. Authorizes the Android app. |
| **Web / Game Server OAuth client** | Used for **server auth code**. Must match Firebase Console → Authentication → Play Games provider. Has a client secret — **never put the secret in Unity**. |

Unity stores only:

- Play Games **App ID** (numeric)
- Web / Game Server **Client ID** (public)

via the plugin asset:

`Assets/GooglePlayGames/Resources/PlayGamesSettings.asset`

## Unity setup (required before device link works)

`PlayGamesSettings.asset` currently has empty `mAppId` / `mWebClientId`.

1. Unity menu: **Window → Google Play Games → Setup → Android setup**
2. Enter:
   - Application ID (Play Games Services numeric App ID)
   - Web App Client ID (Game Server / Web client used in Firebase Play Games provider)
3. Complete setup so `PlayGamesSettings.asset` and `GooglePlayGamesManifest.androidlib` regenerate APP_ID metadata.
4. Resolve Android dependencies (EDM4U) if prompted.
5. Rebuild the Android player.

Do **not** hardcode IDs in runtime scripts.

## Runtime flow (4.2B)

```
LINK GOOGLE PLAY (user tap)
  -> GooglePlayGamesAccountProvider.AuthenticateForLinkAsync
  -> PlayGamesPlatform.Activate()          // bind platform; no prior boot auth
  -> ManuallyAuthenticate()                // explicit UI (signIn)
  -> RequestServerSideAccess(true)         // server auth code via Web client
  -> ExternalAccountCredential (authCode + playgames.google.com)
  -> DailyChallengeIdentityService.LinkGooglePlayAsync
  -> FirebaseRestClient.TryLinkIdpAsync    // accounts:signInWithIdp
  -> UID continuity check
  -> RefreshAccountStateAsync
```

Editor / iOS: `UnavailableGooglePlayAccountProvider` → `ProviderUnavailable`.

## Conflict

Google credential already linked to another Firebase UID:

→ `AlreadyLinkedToAnotherAccount`  
→ current anonymous UID unchanged  
→ no merge / no account switch

## Tester requirements (manual)

Depending on Play Games publishing state:

- Add tester Google accounts to Play Games Services testers if the game is not production-published
- Install from an eligible Play track if required by your PGS / license testing setup
- Signing certificate SHA-1 must match the Android OAuth client
- Firebase Play Games provider enabled with the **same** Web client ID

## Security

Never log or persist:

- server auth code
- Web client secret
- Firebase ID / refresh tokens (beyond existing intentional session storage)
- Google Play player ID
