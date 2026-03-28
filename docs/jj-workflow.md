# Jujutsu (jj) Workflow

## Start a new change

```bash
jj new -m "description of what you're doing"
```

Creates a new empty change on top of `main`. You're now working in it.

## Edit files

Just edit normally. jj auto-tracks all file changes — no `git add` needed.

## Check your work

```bash
jj diff          # what changed in this change
jj status        # summary
jj log           # see where you are in the graph
```

## Update the description (if needed)

```bash
jj describe -m "better description"
```

## Push

```bash
# Create a bookmark (like a branch) for your change
jj bookmark create my-feature -r @

# Push it
jj git push --bookmark my-feature
```

## After merge on GitHub

```bash
jj git fetch
jj rebase -d main    # move your working change onto updated main
```

## Quick fixes (single commit to main)

If you just want to commit directly to main:

```bash
# You're already working on top of main
# ... make edits ...
jj describe -m "what you did"
jj bookmark set main -r @
jj git push --bookmark main
jj new   # start a fresh working change
```

## Key differences from git

- **No staging area** — all changes are automatically part of the current change
- **`jj new`** instead of `git checkout -b` — creates a new change, not a branch
- **Bookmarks** are jj's name for branches — you only create them when you need to push
- **Everything is rewritable** — `jj squash`, `jj split`, `jj rebase` freely rewrite history
