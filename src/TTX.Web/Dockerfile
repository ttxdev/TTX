FROM docker.io/denoland/deno:2.6.7 AS build

ENV FRESH_PUBLIC_API_BASE_URL "https://api.ttx.gg"
ENV FRESH_PUBLIC_DISCORD_URL "https://discord.gg/DEn7sAcDE6"
WORKDIR /opt/app
COPY deno.json deno.lock ./
RUN deno i
COPY . .
RUN deno task build
RUN deno cache _fresh/server.js
EXPOSE 8000

CMD ["serve", "-A", "_fresh/server.js"]
