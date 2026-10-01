# Troubleshooting Guide

This folder contains dated incident logs from past debugging sessions. **They describe the codebase as it was
on the date listed in each entry** — several reference `PlayerConnectionHandler` and `EncounterController`,
classes that were later removed in the Story 030/036 architecture revamp (see [architecture.md](../architecture.md)
for current class names). Read them for the failure *patterns* (singleton destruction races, duplicate
turn-start actions), not as a guide to current code structure.

## Index

### FishNet Networking
- [Reconnection Debugging](./reconnection-debugging.md) - Object lifecycle, logging issues
- [Turn Sync Issues](./turn-sync-issues.md) - Duplicate actions, wrong player execution

### Unity Lifecycle
- [DontDestroyOnLoad Issues](./dont-destroy-on-load.md) - Persistence and destruction problems

## How to Use This Guide

When encountering a bug:
1. Check if a similar issue is documented here
2. If not, document the issue with:
   - **Symptoms**: What you observed
   - **Root Cause**: Why it happened
   - **Bad Code**: The problematic code
   - **Fixed Code**: The solution
   - **Key Learnings**: Takeaways to prevent future occurrences

## Quick Reference

### Common FishNet Pitfalls

| Issue | Symptom | Solution |
|-------|---------|----------|
| SyncVar defaults not syncing | `value: X -> 0` on clients | Set values in `OnStartServer()`, not constructor |
| Duplicate actions on turn change | Both players execute turn-start code | Only active player should execute actions |
| Initialize before Spawn | Values not syncing | Use `OnStartServer()` callback instead |
