# Build stage
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install --network-timeout=100000
COPY . .
RUN npm run build

# Production stage
FROM nginx:latest
COPY --from=build /app/dist/Angular/browser /usr/share/nginx/html
COPY docker-entrypoint.sh /usr/local/bin/docker-entrypoint.sh
RUN chmod +x /usr/local/bin/docker-entrypoint.sh
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
ENTRYPOINT ["docker-entrypoint.sh"]
