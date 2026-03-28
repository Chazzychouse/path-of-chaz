---
name: jj commit pattern for subagents
description: Subagents must use correct jj workflow — describe then new, not new then commit — to avoid empty intermediate changes
type: feedback
---

When committing via jj in subagent instructions, do NOT use `jj new -m "msg"` then `jj commit -m "msg"`. This creates an empty intermediate change because `jj new` starts a fresh change, then `jj commit` commits it empty (the file changes were in the previous change).

**Why:** First implementation run created a messy history with empty commits interleaved with undescribed changes containing the actual work. Required manual squashing to clean up.

**How to apply:** In subagent prompts, instruct them to use this pattern instead:
1. Make file changes (jj auto-tracks them)
2. `jj describe -m "commit message"` (describes the current change)
3. `jj new` (starts a fresh empty change for the next task)

This keeps each change self-contained with its description.
