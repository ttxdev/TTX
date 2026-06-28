# Basic developer tasks for the TTX Rust workspace.
#
# The Rust crates under src/ (ttx, api, jobs) form a single cargo workspace
# (see Cargo.toml), so the recipes below operate on the whole workspace. src/web
# is a separate Deno project.

set shell := ["bash", "-uc"]

# Compose command for local services (override e.g. `just compose=podman ...`).
compose := "docker compose"

# List available recipes.
default:
    @just --list

# Run tests for the whole workspace.
test:
    cargo test --workspace

# Type-check the whole workspace.
check:
    cargo check --workspace

# Lint with clippy (all targets), treating warnings as errors.
clippy:
    cargo clippy --workspace --all-targets -- -D warnings

# Format the whole workspace.
fmt:
    cargo fmt --all

# Verify formatting without writing changes.
fmt-check:
    cargo fmt --all -- --check

# Run the domain benchmarks.
bench:
    cargo bench -p ttx

[group("docker")]
up:
    {{compose}} up -d

[group("docker")]
down:
    {{compose}} down

[group("web")]
[working-directory("src/web")]
web:
    deno run dev

[group("jobs")]
[working-directory("src/jobs")]
jobs:
    cargo run

# Run the jobs runner with the closed-source `TTX.Private` overrides (VADER
# analyzer + baseline value processor) compiled in.
[group("jobs")]
[working-directory("src/jobs")]
jobs-private:
    cargo run --features private

[group("web")]
[working-directory("src/web")]
swagger:
   deno run swagger

[group("api")]
[working-directory("src/api")]
api:
    cargo run

[group("api")]
[working-directory("src/api")]
seed:
    cargo run seed

# Run the full local check suite: formatting, lints, and tests.
ci: fmt-check clippy test
