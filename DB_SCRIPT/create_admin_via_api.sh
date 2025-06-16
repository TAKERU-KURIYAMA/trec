#!/bin/bash

# APIを使用して管理者アカウントを作成するスクリプト
# アプリケーションが起動している状態で実行

echo "==================================="
echo "管理者アカウント作成スクリプト"
echo "==================================="

API_BASE_URL=${API_BASE_URL:-"http://local-trecplans"}

echo "API URL: $API_BASE_URL"
echo ""

# 管理者ユーザーの作成
echo "管理者ユーザーを作成中..."

curl -X POST "$API_BASE_URL/api/account/user" \
  -H "Content-Type: application/json" \
  -d '{
    "loginId": "admin",
    "password": "Test123!",
    "displayName": "システム管理者"
  }'

echo ""
echo ""

# 作成確認
echo "作成されたユーザーの確認:"
echo "ログインID: admin"
echo "パスワード: Test123!"
echo "表示名: システム管理者"
echo ""

# ログインテスト
echo "ログインテスト中..."

LOGIN_RESPONSE=$(curl -s -X POST "$API_BASE_URL/api/account/token" \
  -H "Content-Type: application/json" \
  -d '{
    "loginId": "admin",
    "password": "Test123!"
  }')

echo "ログイン結果:"
echo "$LOGIN_RESPONSE" | jq '.' 2>/dev/null || echo "$LOGIN_RESPONSE"

echo ""
echo "==================================="
echo "管理者アカウント作成完了"
echo "==================================="
echo ""
echo "次のステップ:"
echo "1. Webアプリにアクセス"
echo "2. ログインID: admin, パスワード: Test123! でログイン"
echo "3. サイドバーに管理メニューが表示されることを確認"