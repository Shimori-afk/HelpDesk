FROM node:24-alpine AS build
WORKDIR /src

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

ARG API_URL=http://localhost:8080
ARG SIGNALR_URL=http://localhost:8080/hubs
RUN sed -i "s|http://localhost:8080/api|${API_URL}/api|; s|http://localhost:8080/hubs|${SIGNALR_URL}|" src/environments/environment.production.ts \
 && npm run build -- --configuration production

FROM nginx:alpine AS runtime
COPY --from=build /src/dist/helpdesk-ui/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
