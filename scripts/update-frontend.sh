#!/bin/bash

# フロントエンドのみを更新するスクリプト
# Usage: ./scripts/update-frontend.sh [dev|prod]

set -e

ENVIRONMENT=${1:-dev}

echo "🚀 フロントエンドを更新中... (環境: $ENVIRONMENT)"

case $ENVIRONMENT in
  "dev")
    COMPOSE_FILE="docker-compose.dev.yml"
    ;;
  "prod")
    COMPOSE_FILE="docker-compose.prod.yml"
    ;;
  *)
    echo "❌ 無効な環境指定: $ENVIRONMENT"
    echo "使用方法: $0 [dev|prod]"
    exit 1
    ;;
esac

# フロントエンドのビルドキャッシュをクリア
echo "📦 フロントエンドのキャッシュをクリア中..."
docker-compose -f $COMPOSE_FILE build --no-cache frontend

# フロントエンドサービスを再起動
echo "🔄 フロントエンドサービスを再起動中..."
docker-compose -f $COMPOSE_FILE up -d --force-recreate frontend

# Nginxも再起動（設定が変更されている場合）
echo "🔄 Nginxを再起動中..."
docker-compose -f $COMPOSE_FILE restart nginx

# ヘルスチェック
echo "⏳ サービスの起動を待機中..."
sleep 10

# フロントエンドのヘルスチェック
if curl -f http://localhost:80 > /dev/null 2>&1; then
    echo "✅ フロントエンドの更新が完了しました！"
    echo "🌐 アクセス先: http://localhost"
else
    echo "❌ フロントエンドの起動に失敗しました"
    echo "📋 ログを確認してください:"
    echo "   docker-compose -f $COMPOSE_FILE logs frontend"
    exit 1
fi

echo "📊 現在のサービス状況:"
docker-compose -f $COMPOSE_FILE ps