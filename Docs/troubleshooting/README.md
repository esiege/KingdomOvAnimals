# Troubleshooting Guide

This folder contains documentation for issues encountered during development and their solutions.

## Index

### FishNet Networking
- [fishnet-syncvar-issues.md](fishnet-syncvar-issues.md) - SyncVar synchronization problems and fixes

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
