#!/bin/bash

# アプリケーション層（API + フロントエンド）のみを更新するスクリプト
# データベースは触らずに、アプリケーションのみを更新
# Usage: ./scripts/update-app.sh [dev|prod]

set -e

ENVIRONMENT=${1:-dev}

echo "🚀 アプリケーション層を更新中... (環境: $ENVIRONMENT)"

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

# 現在のサービス状況を表示
echo "📊 更新前のサービス状況:"
docker-compose -f $COMPOSE_FILE ps

# API と フロントエンドのビルドキャッシュをクリア
echo "📦 アプリケーションのキャッシュをクリア中..."
docker-compose -f $COMPOSE_FILE build --no-cache api frontend

# アプリケーションサービスを再起動（データベースは除く）
echo "🔄 アプリケーションサービスを再起動中..."
docker-compose -f $COMPOSE_FILE up -d --force-recreate api frontend nginx

# ヘルスチェック
echo "⏳ サービスの起動を待機中..."
sleep 20

# APIのヘルスチェック
echo "🔍 APIのヘルスチェック中..."
if curl -f http://localhost:5001/api/version > /dev/null 2>&1; then
    echo "✅ API正常動作確認"
else
    echo "⚠️  API接続に問題があります"
fi

# フロントエンドのヘルスチェック
echo "🔍 フロントエンドのヘルスチェック中..."
if curl -f http://localhost:80 > /dev/null 2>&1; then
    echo "✅ フロントエンド正常動作確認"
else
    echo "⚠️  フロントエンド接続に問題があります"
fi

# データベース接続確認
echo "🔍 データベース接続確認中..."
if docker-compose -f $COMPOSE_FILE exec -T db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Your_password123" -Q "SELECT 1" > /dev/null 2>&1; then
    echo "✅ データベース接続正常"
else
    echo "⚠️  データベース接続に問題があります"
fi

echo ""
echo "✅ アプリケーション層の更新が完了しました！"
echo ""
echo "🌐 アクセス先:"
echo "   フロントエンド: http://localhost"
echo "   API (直接):   http://localhost:5001"
echo "   データベース:   localhost:1433"
echo ""
echo "📋 ログ確認コマンド:"
echo "   全体:         docker-compose -f $COMPOSE_FILE logs -f"
echo "   API:          docker-compose -f $COMPOSE_FILE logs -f api"
echo "   フロントエンド: docker-compose -f $COMPOSE_FILE logs -f frontend"
echo ""
echo "📊 更新後のサービス状況:"
docker-compose -f $COMPOSE_FILE ps