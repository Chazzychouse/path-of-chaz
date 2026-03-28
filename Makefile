.PHONY: help build run test push sync version tag release

help: ## Show available targets
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2}'

build: ## Build the C# project
	dotnet build

run: ## Launch Godot with the project
	godot --path src/

test: ## Run xUnit tests
	dotnet test

push: ## Create bookmark and push (usage: make push b=my-feature)
ifndef b
	jj bookmark set main -r @ && jj git push --bookmark main
else
	jj bookmark create $(b) -r @ && jj git push --bookmark $(b)
endif

sync: ## Fetch remote and rebase onto main
	jj git fetch && jj rebase -d main

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
