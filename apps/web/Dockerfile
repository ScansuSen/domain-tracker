# Production-style image: builds the static Vite bundle, then serves it with nginx, which also
# proxies /api/* to the API container - the same "browser never makes a cross-origin request"
# trick the Vite dev proxy uses, so the API still needs no CORS policy. See nginx.conf.

FROM node:20-alpine AS build
WORKDIR /app
# .env is excluded from the build context (see .dockerignore) so dev-only values like
# API_PROXY_TARGET never end up baked into the image. VITE_API_BASE_URL is safe to default here
# since nginx.conf always proxies /api/ to the api container on the same origin.
ARG VITE_API_BASE_URL=/api
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:1.27-alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
