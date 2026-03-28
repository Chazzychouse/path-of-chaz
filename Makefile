.PHONY: help build run test push sync version tag release

help: ## Show available targets
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2}'

build: ## Build the C# project
	dotnet build

run: ## Launch Godot with the project
	godot --path src/

test: ## Run xUnit tests
	dotnet test
