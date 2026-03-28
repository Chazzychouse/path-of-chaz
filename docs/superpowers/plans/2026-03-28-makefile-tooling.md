# Makefile & Project Tooling Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a Makefile and VERSION file providing build, run, test, jj compound commands, and versioning targets.

**Architecture:** Single Makefile at repo root with `.PHONY` targets as thin wrappers around existing CLI tools. A `VERSION` file tracks the current version string. Self-documenting via `make help`.

**Tech Stack:** GNU Make, jj, dotnet, godot, gh CLI

---

### Task 1: Create VERSION file

**Files:**
- Create: `VERSION`

- [ ] **Step 1: Create the VERSION file**

```
0.0.0
```

- [ ] **Step 2: Verify**

Run: `cat VERSION`
Expected: `0.0.0`

- [ ] **Step 3: Commit**

```bash
jj new -m "chore: add VERSION file"
# file is auto-tracked by jj
jj commit -m "chore: add VERSION file"
```

---

### Task 2: Create Makefile with help target

**Files:**
- Create: `Makefile`

- [ ] **Step 1: Create the Makefile with help as the default target**

```makefile
.PHONY: help build run test push sync version tag release

help: ## Show available targets
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2}'
```

- [ ] **Step 2: Verify help works**

Run: `make`
Expected: Prints a formatted table with just the `help` target listed.

Run: `make help`
Expected: Same output.

- [ ] **Step 3: Commit**

```bash
jj new -m "chore: add Makefile with help target"
# file is auto-tracked
jj commit -m "chore: add Makefile with help target"
```

---

### Task 3: Add build, run, and test targets

**Files:**
- Modify: `Makefile`

- [ ] **Step 1: Add the three targets after the help target**

```makefile
build: ## Build the C# project
	dotnet build

run: ## Launch Godot with the project
	godot --path src/

test: ## Run xUnit tests
	dotnet test
```

- [ ] **Step 2: Verify help lists all targets**

Run: `make help`
Expected: Shows `help`, `build`, `run`, `test` with descriptions.

- [ ] **Step 3: Verify build works**

Run: `make build`
Expected: `dotnet build` runs and succeeds.

- [ ] **Step 4: Commit**

```bash
jj new -m "chore: add build, run, test Makefile targets"
jj commit -m "chore: add build, run, test Makefile targets"
```

---

### Task 4: Add jj compound targets

**Files:**
- Modify: `Makefile`

- [ ] **Step 1: Add push and sync targets**

```makefile
push: ## Create bookmark and push (usage: make push b=my-feature)
ifndef b
	$(error Usage: make push b=<bookmark-name>)
endif
	jj bookmark create $(b) -r @ && jj git push --bookmark $(b)

sync: ## Fetch remote and rebase onto main
	jj git fetch && jj rebase -d main
```

- [ ] **Step 2: Verify argument validation**

Run: `make push`
Expected: Error message: `Usage: make push b=<bookmark-name>`

- [ ] **Step 3: Verify help lists all targets**

Run: `make help`
Expected: Shows `push` and `sync` with descriptions alongside the existing targets.

- [ ] **Step 4: Commit**

```bash
jj new -m "chore: add jj compound targets to Makefile"
jj commit -m "chore: add jj compound targets to Makefile"
```

---

### Task 5: Add versioning targets

**Files:**
- Modify: `Makefile`

- [ ] **Step 1: Add version, tag, and release targets**

```makefile
version: ## Print current version
	@cat VERSION

tag: ## Tag a version (usage: make tag v=0.0.1)
ifndef v
	$(error Usage: make tag v=<version>)
endif
	@echo "$(v)" > VERSION
	jj describe -m "release: v$(v)"
	git tag "v$(v)"

release: ## Tag, push, and create GitHub release (usage: make release v=0.0.1)
ifndef v
	$(error Usage: make release v=<version>)
endif
	@$(MAKE) tag v=$(v)
	jj git push --bookmark main
	gh release create "v$(v)" --title "v$(v)" --generate-notes
```

- [ ] **Step 2: Verify version target**

Run: `make version`
Expected: `0.0.0`

- [ ] **Step 3: Verify argument validation**

Run: `make tag`
Expected: Error message: `Usage: make tag v=<version>`

Run: `make release`
Expected: Error message: `Usage: make release v=<version>`

- [ ] **Step 4: Verify help lists all targets**

Run: `make help`
Expected: Shows all 8 targets: `help`, `build`, `run`, `test`, `push`, `sync`, `version`, `tag`, `release`.

- [ ] **Step 5: Commit**

```bash
jj new -m "chore: add versioning targets to Makefile"
jj commit -m "chore: add versioning targets to Makefile"
```
