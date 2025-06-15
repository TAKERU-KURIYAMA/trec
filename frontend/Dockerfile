# ビルド用イメージ
FROM node:18 AS build

WORKDIR /app

# package.json / lock を先にコピーしてインストール（キャッシュ効かせるため）
COPY package*.json ./
RUN npm install

# 残りのソースをコピーして generate
COPY . .
RUN npm run generate

# 実行用イメージ（Nginx）
FROM nginx:alpine

COPY --from=build /app/.output/public /usr/share/nginx/html

# カスタムNginx設定をコピー（SPAルーティング対応）
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
EXPOSE 5001
