# syntax=docker/dockerfile:1
#
# Build the background-jobs image. Build context must be the workspace root:
#
#   docker build -f src/jobs/Dockerfile -t ttx-jobs .
#
# To compile in the closed-source `TTX.Private` overrides (VADER analyzer +
# baseline value processor), pass the feature at build time:
#
#   docker build -f src/jobs/Dockerfile --build-arg CARGO_FEATURES=private -t ttx-jobs .

ARG RUST_VERSION=1.93
# Extra cargo features for the jobs binary (e.g. `private`). Empty by default.
ARG CARGO_FEATURES=""

FROM rust:${RUST_VERSION}-slim-bookworm AS builder
# Build deps for the TLS backends pulled in by rustls (ring / aws-lc-rs).
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        build-essential cmake clang libclang-dev perl pkg-config \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /opt/app
# Re-declare inside the stage so it's visible to the build command below.
ARG CARGO_FEATURES
COPY . .
RUN --mount=type=cache,target=/usr/local/cargo/registry \
    --mount=type=cache,target=/opt/app/target \
    cargo build --release --bin jobs --features "${CARGO_FEATURES}" \
    && cp target/release/jobs /usr/local/bin/jobs

FROM debian:bookworm-slim AS runtime
# ca-certificates for outbound TLS (Twitch Helix, Redis).
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates \
    && rm -rf /var/lib/apt/lists/*
RUN useradd --system --user-group --no-create-home app
COPY --from=builder /usr/local/bin/jobs /usr/local/bin/jobs
USER app
# A worker process — no port. Configuration comes from the environment.
ENTRYPOINT ["jobs"]
