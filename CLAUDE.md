# CLAUDE.md

Kingdom Ov Animals: turn-based multiplayer card duel game, Unity 2021.3.10f1 + FishNet. No CLI build/lint/test —
everything runs through the Editor's custom **KOA** menu (`Board State Setup` validates scene refs; `Testing →
Rebuild SceneIds + Build + Run` is the standard multiplayer test flow: builds `Build/KingdomOvAnimals.exe` as
client, Editor Play Mode as host). Editor and Build log to separate files (`Docs/log_editor.log` /
`Docs/log_build.log`) — check both when debugging netcode. No `.asmdef` under `Assets/Scripts`, so there's no
automated test runner; `Tests/DeckLoaderTest.cs` is a manual Play Mode script, not NUnit.

## Architecture

Story 030/036 fully revamped board/state architecture (`Docs/scrum/stories/story-030-*.md`). One-directional
layers: `KOA.Model` (`BoardState`/`CardState` — pure data, single source of truth, hand-written FishNet
serializers in-file) → `KOA.Network` (`NetworkBoardState`: server-authoritative `SyncVar<BoardState>`, one
`[ServerRpc] Cmd*` per action re-validating turn/mana/target, broadcasts via `[ObserversRpc] Rpc*`) →
`KOA.View`/`KOA.Controllers` (dumb rendering + input→`Cmd*` translation; `BoardView` owns visual perspective,
never the model). `KOA.Data` holds ScriptableObject templates; `KOA.Abilities` behaviors are auto-discovered by
reflection via `AbilityBehaviorRegistry`.

**Invariants:** players/slots are ints, never strings; only the server mutates state (inside `Cmd*`, always
re-validated); Unity 2021 API (`FindObjectOfType`, not `FindFirstObjectByType`); new scene `[SerializeField]`s
need a check wired into `BoardStateSetupWindow.cs`. There are two independent turn-tracking systems
(`NetworkGameManager` vs. `NetworkBoardState`) — see `Docs/networking/turn-synchronization.md` before touching
turn logic.

## Further reading

*vdate = date content was checked against source.*

- [Docs/architecture.md](Docs/architecture.md) — vdate 2026-08-12, full layer breakdown
- [Docs/networking/README.md](Docs/networking/README.md) — vdate 2026-08-12, FishNet components + reconnection status
- [Docs/controllers/README.md](Docs/controllers/README.md) — vdate 2026-08-12, current View/Input classes
- [Docs/game-design/](Docs/game-design/README.md) — vdate 2026-08-12; adventure-mode.md and most of
  abilities.md are design vision, flagged inline as not implemented
