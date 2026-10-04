# Phase 4.3 — Daily #1 Winner Reward

## Product

After a UTC day closes, the real #1 Daily Challenge player receives a **server-created** reward:

- **1000 coins**
- Rank **#1** only (no shared first place)
- **Simulated** leaderboard rows are never eligible
- **Anonymous** Firebase UIDs are eligible (Google Play link not required)
- Ownership key remains **Firebase UID**

## Settlement

| Item | Value |
|------|--------|
| Function | `settleDailyChallengeWinners` |
| Trigger | Cloud Scheduler `5 0 * * *` (00:05 UTC) |
| Region | `europe-west1` |
| Day settled | **Previous** UTC day |

### Eligible attempts (Firestore only)

```
dailyChallenges/{dayId}/attempts/{uid}
  state == "Completed"
  scoreTrusted == true
```

Never used:

- `SimulatedDailyLeaderboardProvider` / `HybridDailyLeaderboardProvider` rows
- Started / Abandoned / malformed attempts
- Client-local hybrid cache

### Ranking (strict sequential #1)

1. `score` DESC  
2. `moves` ASC  
3. `completionTimeMs` ASC  
4. Firebase UID ASC  

### Reward document

```
dailyChallenges/{dayId}/rewards/{uid}
```

Fields: `dayId`, `uid`, `rank`, `rewardType` (`coins`), `rewardAmount` (`1000`),
`createdAt`, `claimed`, `claimedAt`, `source` (`daily_first_place`),
`score`, `moves`, `completionTimeMilliseconds`.

Day settlement metadata (merge): `rewardSettlementVersion`, `rewardSettledAt`,
`firstPlaceUid`, `firstPlaceRank`, `firstPlaceRewardType`, `firstPlaceRewardAmount`.

Idempotent: re-running settlement does not create a second reward or change the winner.

## Claim API (callables)

| Function | Purpose |
|----------|---------|
| `getPendingDailyRewards` | Unclaimed rewards for `request.auth.uid` |
| `claimDailyReward` | `{ dayId }` → marks claimed; returns grant once |

- No client Firestore writes to rewards (rules deny).
- Duplicate claim → `alreadyClaimed` / `granted: false` (no second spendable grant).
- Wrong UID cannot claim another player’s reward (path + ownership check).

## Unity

- `DailyRewardService` — sync + claim
- `DailyRewardUI` — authored panel (`DAILY WINNER` / `#1` / `YOU WON 1000 COINS` / `CLAIM`)
- MainMenu Daily sync calls `DailyRewardService.SyncPendingAsync`
- On CLAIM: server claim first → only if `granted` → `CoinManager.AddCoins` + reward SFX

Authoring: **Rush Out → UI → Create Daily Reward UI (MainMenu)**  
Safe tests: **Rush Out → Testing → Daily Challenge → Rewards → …**

## DEV smoke-test (claim path)

Safe path to exercise Unity CLAIM without waiting for UTC settlement:

| Item | Value |
|------|--------|
| Callable | `createDevDailyReward` |
| Gate | `ALLOW_DEV_DAILY_REWARDS=true` **or** Functions emulator |
| dayId | `dev-reward-smoke-test` (not a real UTC day) |
| Path | `dailyChallenges/dev-reward-smoke-test/rewards/{uid}` |
| source | `daily_first_place_dev` |
| Flags | `isDevelopmentTest: true` |

Settlement (`settleDailyChallengeWinners`) only settles `YYYY-MM-DD` previous UTC days — this fake dayId is never settled as a real winner day.

Editor menus (no CoinManager shortcut):

1. **Create Dev Reward For Current User** → calls `createDevDailyReward` → `getPendingDailyRewards` sync
2. **Refresh Pending Rewards** → production pending sync only

CLAIM still uses production `claimDailyReward` → coins only when `granted=true`.

### Enable / disable the DEV gate (2nd Gen)

Uses Firebase parameterized config `defineBoolean('ALLOW_DEV_DAILY_REWARDS')`
(default **false**). Local `.env` alone does nothing until you **redeploy**.

**Enable (smoke test):**

```bash
cd Backend/DailyChallenge/functions
# ensure .env (and/or .env.grid-drive) contains:
# ALLOW_DEV_DAILY_REWARDS=true
cd ..
firebase deploy --only functions:createDevDailyReward --project grid-drive
```

Deploy log should mention loading environment variables / params from `.env`.
Cloud logs on invoke should show:

```
[DailyRewardDev] gate raw present=true|false
[DailyRewardDev] gate enabled=true
[DailyRewardDev] emulator=false
```

**Disable after smoke test:**

```bash
# Set ALLOW_DEV_DAILY_REWARDS=false in functions/.env
firebase deploy --only functions:createDevDailyReward --project grid-drive
```

When disabled, callable returns **`failed-precondition`** (not generic INTERNAL).

Emulator: gate always open (`FUNCTIONS_EMULATOR=true`).

No delete/reset tools. To re-smoke after claim: fresh anonymous UID, or manually remove that one Firestore reward doc in Console.

## Analytics

| Event | Params |
|-------|--------|
| `daily_reward_created` | `rank`, `reward_type`, `reward_amount` (no UID) |
| `daily_reward_claimed` | `rank`, `reward_type`, `reward_amount` (no UID) |

## Known V1 limitation (local coins)

Coins live in **PlayerPrefs** via `CoinManager`.

If the server claim succeeds and the app crashes **before** `AddCoins` persists, the player may lose that grant. V1 does **not** build a distributed transaction for local prefs.

- Do **not** silently grant again on `AlreadyClaimed`
- Keep the claim path simple

## Trust boundary

`moves` and `completionTimeMilliseconds` / `completionTimeMs` remain client-reported.
Settlement does not pretend full anti-cheat.

## Deploy

```bash
cd Backend/DailyChallenge/functions
npm install
npm run test:scoring
npm run test:rewards
cd ..
firebase deploy --only firestore:rules,firestore:indexes,functions --project grid-drive
```

Indexes: existing attempts `(state, score)`; new rewards collection-group `(uid, claimed)`.
