FROM docker.io/denoland/deno:2.6.7 AS build

WORKDIR /opt/app
COPY deno.json deno.lock ./
RUN deno i
COPY . .
EXPOSE 8000

CMD ["run", "-A", "main.ts"]
