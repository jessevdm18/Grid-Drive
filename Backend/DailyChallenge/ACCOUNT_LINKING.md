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

## Current configuration

`PlayGamesSettings.asset` is configured. Runtime scripts do not hardcode these IDs.

| Item | Value |
|------|--------|
| Play Games App ID | `410107420523` (same as Firebase `project_number`) |
| Web Client ID | `410107420523-lgj6m805i9819q3soa1doi5s2rjqi630.apps.googleusercontent.com` |
| Package | `com.raddergames.griddrive` |
| Firebase project | `grid-drive` |

Firebase Console → Authentication → Play Games must use that same Web Client ID. The Web client secret stays in Firebase / Google Cloud only. Never put it in Unity.

The Android OAuth client for `com.raddergames.griddrive` must include the certificate that actually signs the installed build. A build installed through Google Play is signed by the Play App Signing certificate, not only the upload keystore.

## Runtime flow (4.2B)

```
LINK GOOGLE PLAY (user tap)
  -> GooglePlayGamesAccountProvider.AuthenticateForLinkAsync
  -> PlayGamesPlatform.Activate()          // bind platform; no prior boot auth
  -> ManuallyAuthenticate()                // explicit UI (signIn)
  -> RequestServerSideAccess(true)         // server auth code via Web client
  -> ExternalAccountCredential (code + playgames.google.com)
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

## Play Games credential shape

`accounts:signInWithIdp` links the current anonymous user. The Play Games `postBody` must match `PlayGamesAuthProvider.GetCredential(serverAuthCode)`:

```
code=<URL-encoded server auth code>&providerId=playgames.google.com
```

`providerId` is exactly `playgames.google.com`. The server auth code is the `code` field, not `authCode`, `id_token`, or `access_token`.

The request also sends the current anonymous user's `idToken`, `requestUri=http://localhost`, `returnIdpCredential=true`, and `returnSecureToken=true`. On success the Firebase UID must stay the same.

`INVALID_CREDENTIAL_OR_PROVIDER_ID` means Firebase rejected this credential. It is a link failure, not proof that Play Games is unavailable, and it is not by itself a Firebase backend outage. After the `code` field is correct, the remaining checks are the Firebase Play Games provider Web Client ID and the Play App Signing SHA-1 on the Android OAuth client.

A previous device test rejected this link because the client sent `authCode=` instead of `code=`.

Client handling when Firebase still rejects the credential:

| Item | Behavior |
|------|----------|
| Result | `AccountLinkResult.UnknownError` (`INVALID_CREDENTIAL_OR_PROVIDER_ID`) |
| UI | "Could not link your Google Play account. Please try again." |
| Session | Anonymous Firebase UID + Daily Challenge data unchanged |
| Retry | None automatic — user may tap LINK GOOGLE PLAY again; each attempt requests a fresh server auth code |

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
