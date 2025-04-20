FROM node:20-slim

WORKDIR /app
COPY . .

# 🔒 npm 脆弱性回避のために更新
RUN npm install -g npm@latest

RUN npm install
RUN npm run build

EXPOSE 3000
CMD ["npm", "run", "preview"]
