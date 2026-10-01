# Architecture Overview

*Verified against source: 2026-08-12 (vdate)*

Story 030/036 replaced the original `EncounterController`/`PlayerController`/`CardController`/`HandController`
design with a layered Model → Network → View architecture. This doc describes the **current** system.

## Layers

```
KOA.Model      Assets/Scripts/Model/     Pure data, no MonoBehaviours
KOA.Network    Assets/Scripts/Network/   Server-authoritative sync (FishNet)
KOA.View       Assets/Scripts/View/      Dumb rendering + input
KOA.Data       Assets/Scripts/Data/      ScriptableObject templates
KOA.Logic      Assets/Scripts/Logic/     Pure, stateless helpers
KOA.Abilities  Assets/Scripts/Abilities/ Ability effect implementations
KOA.Editor     Assets/Scripts/Editor/    Editor-only tooling
```

Dependencies flow one direction: Model → Network → View/Controllers. View never decides game outcomes.

## Model

- **`BoardState`** — the single source of truth for the whole match: `Players[2]` (`PlayerBoardState`),
  `CurrentTurnPlayerId`, `TurnNumber`, `IsGameActive`, `WinnerPlayerId`.
- **`PlayerBoardState`** — one player's `Health`/`MaxHealth`, `Mana`/`MaxMana`, `Board[3]` (slots), `Hand`
  (`List<CardState>`), `Deck` (card ID list). `OnTurnStart(turnNumber)` recalculates `MaxMana` from the shared
  turn counter (capped at 10) and untaps/clears sickness on board cards.
- **`CardState`** — one card instance: `InstanceId`, `CardDataId` (string ref into `CardLibrary`), `OwnerId`,
  `CurrentHealth`, and flags `IsTapped`/`HasSummoningSickness`/`IsFrozen`/`IsDefending`/`IsFlipped`.
  `CanAct` = valid, not tapped, no sickness, not frozen.
- Both `BoardState` and `CardState` (and `PlayerBoardState`) have hand-written FishNet `Writer`/`Reader`
  extension serializers in the same file — update these whenever fields are added.

## Network

- **`NetworkBoardState`** (singleton, `NetworkBoardState.Instance`) owns the one `SyncVar<BoardState> State` and
  is the only place game rules execute. Every mutating action is a `[ServerRpc(RequireOwnership = false)]`
  `Cmd*` method — `CmdPlayCard`, `CmdUseFlipAbility` (hand card ability), `CmdUseBoardAbility` (board card
  attack), `CmdAttackPlayer`, `CmdUseFlipAbilityOnPlayer`, `CmdUseSupportAbility`, `CmdHealPlayer`, `CmdEndTurn`
  — each re-validates turn/mana/slot/target server-side before mutating `State.Value`, then broadcasts via
  `[ObserversRpc]` `Rpc*` methods and C# events (`OnStateChanged`, `OnCardPlayed`, `OnCardDamaged`, `OnCardDied`,
  `OnPlayerDamaged`, `OnPlayerHealed`, `OnTurnChanged`, `OnGameEnded`, `OnSupportAbilityUsed`).
- **`NetworkGameManager`** (singleton) is a thin layer *above* NetworkBoardState: it owns turn/connection
  bookkeeping that isn't card data — `CurrentTurnObjectId`/`TurnNumber`/`GameStarted`/`ShuffleSeed` SyncVars,
  registers `NetworkPlayer` instances, starts the game once both are connected (loads a deck from
  `Resources/Decks` and calls `NetworkBoardState.InitializeGame`), and tracks `OpponentDisconnected` with a
  configurable grace-period timer (default 120s) that returns to `MainMenu` on expiry.
- **`NetworkPlayer`** is identity + basic stats only (`PlayerId`, `PlayerName`, `IsReady`, `CurrentHealth`,
  `MaxHealth`, `CurrentMana`, `MaxMana`). Card/ability/turn logic used to live here but has moved to
  `NetworkBoardState` — don't add gameplay RPCs to `NetworkPlayer`.
- **`ConnectionManager`** is a thin wrapper around FishNet's `NetworkManager` (`StartHost`/`StartServer`/
  `StartClient`/`StopConnection`). **`MainMenuController`** drives matchmaking: a client attempts to join for
  `connectionTimeout` seconds (default 3s), then falls back to becoming host.
- **`ReconnectionManager`** (static class) and **`DisconnectedPlayerState`** (capture struct) exist to support
  client reconnection, but as of this writing (see `git log`, "netcode wip") they are **not yet fully wired**
  end-to-end — their methods exist but have no call sites outside their own files. Don't assume reconnection
  works without checking current call sites first.

## View / Input

- **`BoardView`** renders `BoardState` onto scene transforms and owns visual perspective —
  `GetSlotTransform()` mirrors the opponent's slots so both players see their own side as "near". It creates/
  updates/destroys `CardView` instances in `RenderBoard()` in response to `NetworkBoardState.OnStateChanged`.
- **`CardView`** is a single prefab used for both hand and board rendering (`SetHandPosition`/
  `SetBoardPosition`, `SetFaceDown` for opponent's hand). Pure rendering — no rules.
- **`HandView`** lays out one player's hand (fan/arc/hover) and re-renders on `OnStateChanged`.
- **`InputController`** translates mouse input into `NetworkBoardState.Cmd*` calls. Current scheme:
  **left-click + drag** is the offensive/play action (hand card → empty slot = play; hand card → enemy card or
  opponent avatar = flip ability; board card → enemy card or opponent avatar = attack). **Right-click** on your
  own actionable board card enters support-targeting mode, and left-clicking a friendly board card uses its
  support ability (`CmdUseSupportAbility`). There is currently no path to use a support ability directly from a
  hand card.
- **`TargetingHelper`** (`KOA.Logic`) computes valid target lists from `BoardState` + `TargetType` — pure,
  stateless, shared by input highlighting.

## Data

ScriptableObject templates: `CardData` (`id`, `displayName`, `health`, `manaCost`, `artwork`,
`offensiveAbility`, `defensiveAbility`), `AbilityData` (`targetType`, `damage`, `healAmount`, `behaviorType`),
`DeckData`, `CardLibrary` (loads `CardData`/`AbilityData` from `Resources/Cards` and `Resources/Abilities`,
looked up by ID — `CardLibrary.Instance.GetCardById(id)`).

## Abilities

`AbilityBehavior` subclasses under `Abilities/Behaviors/` are auto-discovered by `AbilityBehaviorRegistry` via
reflection (no manual registration). Implemented today: `DamageAbility`, `HealAbility`, `BuffAttackAbility`,
`PoisonAbility`, `StunAbility`, `DrawCardAbility`, `ReturnToHandAbility`. Many effect types described in
[game-design/abilities.md](./game-design/abilities.md) (Freeze, Silence, Taunt, Lifesteal, Shield, Summon, etc.)
are design vision, not yet implemented — check `Abilities/Behaviors/` before assuming an effect exists.

## See also

[Networking](./networking/README.md) · [Controllers/View](./controllers/README.md) ·
[Game Design](./game-design/README.md)
