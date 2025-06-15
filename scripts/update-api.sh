#!/bin/bash

# APIのみを更新するスクリプト
# Usage: ./scripts/update-api.sh [dev|prod]

set -e

ENVIRONMENT=${1:-dev}

echo "🚀 APIを更新中... (環境: $ENVIRONMENT)"

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

# APIのビルドキャッシュをクリア
echo "📦 APIのキャッシュをクリア中..."
docker-compose -f $COMPOSE_FILE build --no-cache api

# APIサービスを再起動
echo "🔄 APIサービスを再起動中..."
docker-compose -f $COMPOSE_FILE up -d --force-recreate api

# ヘルスチェック
echo "⏳ APIの起動を待機中..."
sleep 15

# APIのヘルスチェック
if curl -f http://localhost:5001/api/version > /dev/null 2>&1; then
    echo "✅ APIの更新が完了しました！"
    echo "🌐 APIアクセス先: http://localhost:5001"
else
    echo "❌ APIの起動に失敗しました"
    echo "📋 ログを確認してください:"
    echo "   docker-compose -f $COMPOSE_FILE logs api"
    exit 1
fi

echo "📊 現在のサービス状況:"
docker-compose -f $COMPOSE_FILE ps