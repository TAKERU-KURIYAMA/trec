#!/bin/bash

# ===================================
# テストデータ自動セットアップスクリプト (Linux/Mac)
# ===================================
# このシェルスクリプトはLinux/MacやDockerでテストデータを自動投入します

set -e  # エラーが発生したら即座に終了

echo "==================================="
echo "テストデータ自動セットアップ"
echo "==================================="
echo

# 設定値（必要に応じて変更してください）
SERVER=${SQL_SERVER:-"localhost,1433"}
DATABASE=${SQL_DATABASE:-"TrecPlansRDB"}
USERNAME=${SQL_USERNAME:-"sa"}
PASSWORD=${SQL_PASSWORD:-""}

# Docker環境の場合の設定
if [ -n "$DOCKER_CONTAINER" ]; then
    echo "Docker環境を検出しました"
    SERVER="localhost"
    SQLCMD_PATH="/opt/mssql-tools/bin/sqlcmd"
else
    SQLCMD_PATH="sqlcmd"
fi

# パスワードが設定されていない場合は入力を求める
if [ -z "$PASSWORD" ]; then
    echo "SQL Server SAユーザーのパスワードを入力してください:"
    read -s PASSWORD
    echo
fi

echo "接続情報:"
echo "Server: $SERVER"
echo "Database: $DATABASE"
echo "Username: $USERNAME"
echo

# 接続テスト
echo "データベース接続をテスト中..."
if ! $SQLCMD_PATH -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -d "$DATABASE" -Q "SELECT 1 as connection_test;" >/dev/null 2>&1; then
    echo "エラー: データベースに接続できません。"
    echo "以下を確認してください:"
    echo "- SQL Serverが起動しているか"
    echo "- 接続情報が正しいか（サーバー名、ポート、ユーザー名、パスワード）"
    echo "- データベース $DATABASE が存在するか"
    exit 1
fi

echo "データベース接続成功！"
echo

# 実行確認
echo "以下のテストデータが投入されます:"
echo "- トレーニングメニュー（29種目）"
echo "- タグマスター（21種類）"
echo "- テストユーザー（5名、パスワード: Test123!）"
echo "- サンプルトレーニング記録（30日分）"
echo "- 追加機能データ（目標、アチーブメント等）"
echo
echo "既存のデータは上書きされる可能性があります。"
read -p "続行しますか？ (y/N): " CONFIRM

if [ "$CONFIRM" != "y" ] && [ "$CONFIRM" != "Y" ]; then
    echo "セットアップを中止しました。"
    exit 0
fi

echo
echo "==================================="
echo "テストデータ投入開始"
echo "==================================="

# 現在のディレクトリを取得
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# 各SQLスクリプトを順番に実行
echo "1. メニューとタグを投入中..."
if ! $SQLCMD_PATH -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -d "$DATABASE" -i "$SCRIPT_DIR/01_insert_menus_and_tags.sql"; then
    echo "エラー: メニューとタグの投入に失敗しました。"
    exit 1
fi
echo "   完了"
echo

echo "2. テストユーザーを作成中..."
if ! $SQLCMD_PATH -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -d "$DATABASE" -i "$SCRIPT_DIR/02_insert_test_users.sql"; then
    echo "エラー: テストユーザーの作成に失敗しました。"
    exit 1
fi
echo "   完了"
echo

echo "3. サンプルトレーニングデータを投入中..."
if ! $SQLCMD_PATH -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -d "$DATABASE" -i "$SCRIPT_DIR/03_insert_sample_training_data.sql"; then
    echo "エラー: サンプルトレーニングデータの投入に失敗しました。"
    exit 1
fi
echo "   完了"
echo

echo "4. 追加機能データを投入中..."
if ! $SQLCMD_PATH -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -d "$DATABASE" -i "$SCRIPT_DIR/04_insert_additional_features.sql"; then
    echo "エラー: 追加機能データの投入に失敗しました。"
    exit 1
fi
echo "   完了"
echo

# 結果確認
echo "==================================="
echo "投入結果の確認"
echo "==================================="
$SQLCMD_PATH -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -d "$DATABASE" -Q "SELECT '投入されたメニュー数' as 項目, COUNT(*) as 件数 FROM training_menu UNION ALL SELECT '投入されたタグ数', COUNT(*) FROM tag_master UNION ALL SELECT '投入されたユーザー数', COUNT(*) FROM [user] UNION ALL SELECT '投入されたトレーニング記録数', COUNT(*) FROM training_record_set;"

echo
echo "==================================="
echo "テストデータ投入完了！"
echo "==================================="
echo
echo "テストユーザー情報:"
echo "- beginner_user     / Test123!  (初心者太郎)"
echo "- intermediate_user / Test123!  (トレーニング花子)"
echo "- advanced_user     / Test123!  (マッスル次郎)"
echo "- demo              / Test123!  (デモユーザー)"
echo "- admin             / Test123!  (管理者)"
echo
echo "アプリケーションで上記のユーザーでログインしてテストできます。"
echo

# Docker環境の場合の追加情報
if [ -n "$DOCKER_CONTAINER" ]; then
    echo "Docker環境での実行が完了しました。"
    echo "アプリケーションコンテナを再起動してデータの反映を確認してください。"
fi