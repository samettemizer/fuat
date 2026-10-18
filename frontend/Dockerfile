FROM node:22-alpine AS build

WORKDIR /uygulama

COPY package.json package-lock.json ./
RUN npm ci --no-fund --no-audit

COPY . .

# API adresi derleme aninda gomulur; compose'da yayinlanan API portu ile ayni olmali.
ARG VITE_API_BASE_URL=http://localhost:8080
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

RUN npm run build

FROM nginx:1.27-alpine AS webserver

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /uygulama/dist /usr/share/nginx/html

EXPOSE 80

#HEALTHCHECK --interval=30s --timeout=3s --start-period=5s \
#  CMD wget -q --spider http://localhost/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
