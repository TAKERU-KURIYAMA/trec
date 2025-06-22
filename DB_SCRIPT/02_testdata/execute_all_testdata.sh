#!/bin/bash

# ===================================
# テストデータ一括投入スクリプト (Linux/Mac/Docker)
# ===================================

echo "==================================="
echo "トレーニングアプリ テストデータ投入"
echo "==================================="
echo

# 環境変数設定（必要に応じて変更）
SERVER=${DB_SERVER:-localhost}
USERNAME=${DB_USERNAME:-sa}
PASSWORD=${DB_PASSWORD:-Your_password123}
DATABASE=${DB_DATABASE:-TrecPlansRDB}
CONTAINER_NAME=${DB_CONTAINER:-message_db_1}

echo "データベース: $DATABASE"
echo "サーバー: $SERVER"
echo "コンテナ: $CONTAINER_NAME"
echo

# Dockerコンテナが動いているかチェック
if ! docker ps | grep -q "$CONTAINER_NAME"; then
    echo "エラー: SQLServerコンテナ '$CONTAINER_NAME' が見つかりません"
    echo "docker-compose up で起動してください"
    exit 1
fi

# 各スクリプトを順次実行
echo "[1/5] メニューとタグの投入..."
if ! docker exec -i "$CONTAINER_NAME" /opt/mssql-tools/bin/sqlcmd -S $SERVER -U $USERNAME -P $PASSWORD -d $DATABASE < 01_insert_menus_and_tags.sql; then
    echo "エラー: メニューとタグの投入に失敗しました"
    exit 1
fi

echo "[2/5] テストユーザーの投入..."
if ! docker exec -i "$CONTAINER_NAME" /opt/mssql-tools/bin/sqlcmd -S $SERVER -U $USERNAME -P $PASSWORD -d $DATABASE < 02_insert_test_users.sql; then
    echo "エラー: テストユーザーの投入に失敗しました"
    exit 1
fi

echo "[3/5] サンプルトレーニングデータの投入..."
if ! docker exec -i "$CONTAINER_NAME" /opt/mssql-tools/bin/sqlcmd -S $SERVER -U $USERNAME -P $PASSWORD -d $DATABASE < 03_insert_sample_training_data.sql; then
    echo "エラー: サンプルトレーニングデータの投入に失敗しました"
    exit 1
fi

echo "[4/5] 追加機能データの投入..."
if ! docker exec -i "$CONTAINER_NAME" /opt/mssql-tools/bin/sqlcmd -S $SERVER -U $USERNAME -P $PASSWORD -d $DATABASE < 04_insert_additional_features.sql; then
    echo "エラー: 追加機能データの投入に失敗しました"
    exit 1
fi

echo "[5/5] エクササイズタイプタグの投入..."
if ! docker exec -i "$CONTAINER_NAME" /opt/mssql-tools/bin/sqlcmd -S $SERVER -U $USERNAME -P $PASSWORD -d $DATABASE < 05_add_exercise_type_tags.sql; then
    echo "エラー: エクササイズタイプタグの投入に失敗しました"
    exit 1
fi

echo
echo "==================================="
echo "テストデータ投入完了！"
echo "==================================="
echo
echo "投入されたデータ:"
echo "- トレーニングメニュー: 29種目"
echo "- タグマスター: 27種類"
echo "- テストユーザー: 5名"
echo "- サンプルトレーニング記録: 約90件"
echo "- 追加機能データ: プログラム・目標・アチーブメント等"
echo "- エクササイズ分類: コンパウンド13種目・アイソレーション11種目"
echo
echo "次のステップ: 管理者アカウントの作成"
echo "実行: ./create_admin_via_api.sh"
echo