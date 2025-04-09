FROM node:22-alpine AS builder

ENV PUBLIC_DISCORD_URL="https://discord.gg/DEn7sAcDE6"
ENV PUBLIC_API_BASE_URL="https://api.ttx.gg"
ENV PUBLIC_TWITCH_CLIENT_ID="2dhkgmvursaw7ahdik16cqghw0zkr2"
ENV PUBLIC_TWITCH_REDIRECT_URL="https://ttx.gg/api/twitch/callback"
ENV PUBLIC_DISCORD_CLIENT_ID="1359360376763842782"

WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build
RUN npm prune --production

FROM node:22-alpine

WORKDIR /app
COPY --from=builder /app/build build/
COPY --from=builder /app/node_modules node_modules/
COPY package.json .
EXPOSE 3000
ENV NODE_ENV=production
CMD [ "node", "build" ]
