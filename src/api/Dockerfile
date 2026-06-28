ARG RUST_VERSION=1.93

FROM rust:${RUST_VERSION}-slim-bookworm AS builder

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        build-essential cmake clang libclang-dev perl pkg-config \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /opt/app
COPY . .
RUN --mount=type=cache,target=/usr/local/cargo/registry \
    --mount=type=cache,target=/opt/app/target \
    cargo build --release --bin api \
    && cp target/release/api /usr/local/bin/api

FROM debian:bookworm-slim

RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates \
    && rm -rf /var/lib/apt/lists/*
RUN useradd --system --user-group --no-create-home app
COPY --from=builder /usr/local/bin/api /usr/local/bin/api
USER app
ENV BIND_ADDR=0.0.0.0:3000
EXPOSE 3000
ENTRYPOINT ["api"]
