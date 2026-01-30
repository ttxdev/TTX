FROM docker.io/denoland/deno:2.6.7 AS build

WORKDIR /opt/app
COPY deno.json deno.lock ./
RUN deno i
COPY . .
RUN deno task build
RUN deno cache _fresh/server.js
EXPOSE 8000

CMD ["serve", "-A", "_fresh/server.js"]
