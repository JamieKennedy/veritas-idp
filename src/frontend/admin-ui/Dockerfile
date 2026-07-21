# syntax=docker/dockerfile:1.7@sha256:a57df69d0ea827fb7266491f2813635de6f17269be881f696fbfdf2d83dda33e

FROM node:22-alpine@sha256:16e22a550f3863206a3f701448c45f7912c6896a62de43add43bb9c86130c3e2 AS build
WORKDIR /workspace

RUN corepack enable

COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
COPY src/frontend/admin-ui/package.json src/frontend/admin-ui/package.json
RUN pnpm install --frozen-lockfile

COPY assets assets
COPY src/frontend/admin-ui src/frontend/admin-ui

ARG VERSION=0.0.0-development
ENV VITE_APP_VERSION=${VERSION}

RUN pnpm --filter admin-ui build
RUN pnpm --filter admin-ui --prod deploy --legacy /app

FROM node:22-alpine@sha256:16e22a550f3863206a3f701448c45f7912c6896a62de43add43bb9c86130c3e2 AS final
ARG VERSION=0.0.0-development
ARG REVISION=unknown
ARG SOURCE=https://github.com/JamieKennedy/veritas

LABEL org.opencontainers.image.title="Veritas Admin UI" \
      org.opencontainers.image.description="Veritas administration interface" \
      org.opencontainers.image.source="${SOURCE}" \
      org.opencontainers.image.revision="${REVISION}" \
      org.opencontainers.image.version="${VERSION}"

WORKDIR /app

ENV HOST=0.0.0.0 \
    NODE_ENV=production \
    PORT=3000

RUN rm -rf /usr/local/lib/node_modules/npm /usr/local/lib/node_modules/corepack \
    && rm -f /usr/local/bin/npm /usr/local/bin/npx /usr/local/bin/corepack /usr/local/bin/pnpm /usr/local/bin/pnpx

COPY --from=build --chown=node:node /app ./
COPY --from=build --chown=node:node /workspace/src/frontend/admin-ui/dist ./dist

USER node
EXPOSE 3000

HEALTHCHECK --interval=10s --timeout=5s --start-period=10s --retries=6 \
    CMD wget --quiet --output-document=- http://127.0.0.1:3000/healthz >/dev/null || exit 1

CMD ["./node_modules/.bin/srvx", "--prod", "--static", "../dist/client", "server/entry.mjs"]
