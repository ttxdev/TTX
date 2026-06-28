# syntax=docker/dockerfile:1
#
# Build the open-source background-jobs image. Build context must be the
# workspace root:
#
#   docker build -f src/jobs/Dockerfile -t ttx-jobs .
#
# The closed-source overrides (VADER analyzer + baseline value processor) live
# in the `TTX/private` repo, which builds its own binary against the `jobs`
# library; they are not part of this image.

ARG RUST_VERSION=1.93

FROM rust:${RUST_VERSION}-slim-bookworm AS builder
# Build deps for the TLS backends pulled in by rustls (ring / aws-lc-rs).
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        build-essential cmake clang libclang-dev perl pkg-config \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /opt/app
COPY . .
RUN --mount=type=cache,target=/usr/local/cargo/registry \
    --mount=type=cache,target=/opt/app/target \
    cargo build --release --bin jobs \
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
