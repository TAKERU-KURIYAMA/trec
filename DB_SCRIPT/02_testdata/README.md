# テストデータセットアップガイド

## 概要
このディレクトリには、トレーニング記録アプリケーションのテスト用データを投入するSQLスクリプトが含まれています。

## スクリプト実行順序

以下の順序でスクリプトを実行してください：

1. **01_insert_menus_and_tags.sql**
   - トレーニングメニュー（29種目）
   - タグマスター（21種類）
   - メニューとタグの関連付け

2. **02_insert_test_users.sql**
   - テストユーザー5名の作成
   - 全ユーザーのパスワード: `Test123!`

3. **03_insert_sample_training_data.sql**
   - 過去30日分のトレーニング記録
   - ユーザーレベルに応じた現実的なデータ
   - 日次集計データの自動生成

4. **04_insert_additional_features.sql**
   - トレーニングプログラム（拡張機能用）
   - ユーザー目標設定
   - 実績・アチーブメント
   - お気に入りメニュー
   - 体重・体脂肪率記録
   - 統計確認用ビュー

5. **05_add_exercise_type_tags.sql**
   - エクササイズタイプタグ（コンパウンド・アイソレーション分類）
   - 動作パターンタグ（プッシュ・プル・スクワット・ヒンジ）
   - 既存メニューへのタイプ別タグ付与

## テストユーザー情報

| ログインID | 表示名 | レベル | 特徴 |
|------------|--------|--------|------|
| beginner_user | 初心者太郎 | 初心者 | 週2回の軽いトレーニング |
| intermediate_user | トレーニング花子 | 中級者 | 週5回、徐々に重量アップ |
| advanced_user | マッスル次郎 | 上級者 | 週5回の高強度トレーニング |
| demo | デモユーザー | - | デモンストレーション用 |
| admin | 管理者 | - | 管理者権限テスト用 |

**共通パスワード**: `Test123!` （API経由で作成する場合）

## 重要な注意

**パスワード認証について:**
- SQLで直接投入されたユーザーはダミーハッシュのためログインできません
- 実際に使用可能なアカウントを作成するには以下の方法を使用してください：

### 管理者アカウントの作成（推奨）
```bash
# アプリケーション起動後に実行
./create_admin_via_api.sh
```

または手動でAPI呼び出し：
```bash
curl -X POST "http://local-trecplans/api/account/user" \
  -H "Content-Type: application/json" \
  -d '{"loginId": "admin", "password": "Test123!", "displayName": "管理者"}'
```

## 実行方法

### 一括実行スクリプト（推奨）

**SQL一発実行（最も簡単）:**
```sql
-- SQL Server Management Studio (SSMS) で実行
-- または sqlcmd で実行
sqlcmd -S localhost,1433 -U sa -P Your_password123 -d MessageRDB -i execute_all_testdata.sql
```

**Windows環境（バッチスクリプト）:**
```batch
# DB_SCRIPT/02_testdata ディレクトリで実行
execute_all_testdata.bat
```

**Linux/Mac/Docker環境（シェルスクリプト）:**
```bash
# DB_SCRIPT/02_testdata ディレクトリで実行
./execute_all_testdata.sh
```

環境変数でカスタマイズ可能:
```bash
DB_SERVER=localhost \
DB_USERNAME=sa \
DB_PASSWORD=Your_password123 \
DB_DATABASE=MessageRDB \
DB_CONTAINER=message_db_1 \
./execute_all_testdata.sh
```

### SQL Server Management Studio (SSMS) での実行
```sql
-- データベースに接続後、各スクリプトを順番に実行
USE MessageRDB;
GO

-- 各スクリプトを開いて実行
```

### Docker環境での実行
```bash
# SQLServerコンテナに接続
docker exec -it <container_name> /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Your_password123

# 各スクリプトを実行
1> USE MessageRDB;
2> GO
1> :r /path/to/01_insert_menus_and_tags.sql
2> GO
```

### コマンドラインからの一括実行
```bash
# Windows
sqlcmd -S localhost,1433 -U sa -P Your_password123 -d MessageRDB -i 01_insert_menus_and_tags.sql
sqlcmd -S localhost,1433 -U sa -P Your_password123 -d MessageRDB -i 02_insert_test_users.sql
sqlcmd -S localhost,1433 -U sa -P Your_password123 -d MessageRDB -i 03_insert_sample_training_data.sql
sqlcmd -S localhost,1433 -U sa -P Your_password123 -d MessageRDB -i 04_insert_additional_features.sql
sqlcmd -S localhost,1433 -U sa -P Your_password123 -d MessageRDB -i 05_add_exercise_type_tags.sql

# Linux/Mac (Docker環境)
docker exec -i message_db_1 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Your_password123 -d MessageRDB < 01_insert_menus_and_tags.sql
docker exec -i message_db_1 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Your_password123 -d MessageRDB < 02_insert_test_users.sql
docker exec -i message_db_1 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Your_password123 -d MessageRDB < 03_insert_sample_training_data.sql
docker exec -i message_db_1 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Your_password123 -d MessageRDB < 04_insert_additional_features.sql
docker exec -i message_db_1 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Your_password123 -d MessageRDB < 05_add_exercise_type_tags.sql
```

## 投入されるデータ

### トレーニングメニュー（主要なもの）
- **胸**: ベンチプレス、インクラインベンチプレス、ダンベルフライ、プッシュアップ
- **背中**: デッドリフト、懸垂、ベントオーバーロウ、ラットプルダウン
- **肩**: ショルダープレス、サイドレイズ、リアデルトフライ
- **腕**: バイセップカール、ハンマーカール、トライセップエクステンション
- **脚**: スクワット、レッグプレス、ルーマニアンデッドリフト、ランジ
- **体幹**: プランク、クランチ、ロシアンツイスト
- **有酸素**: ランニング、サイクリング、ローイング

### サンプルトレーニング記録
- **初心者ユーザー**: 最近2週間、週2回の軽いトレーニング
- **中級者ユーザー**: 過去30日間、週5回、漸進的過負荷
- **上級者ユーザー**: 過去30日間、週5回の分割トレーニング

### 追加機能データ
- 6つのトレーニングプログラム（初級〜上級）
- 各ユーザーの目標設定
- 8種類のアチーブメント定義
- お気に入りメニュー設定
- 体重・体脂肪率の推移データ

### エクササイズ分類データ
- **コンパウンド種目**: 13種目（複数筋群を使う複合運動）
  - ベンチプレス、インクラインベンチプレス、デッドリフト、懸垂、ベントオーバーロウ等
- **アイソレーション種目**: 11種目（単一筋群対象の単関節運動）
  - ダンベルフライ、サイドレイズ、バイセップカール、カーフレイズ等
- **動作パターン分類**: プッシュ、プル、スクワット、ヒンジ系の動作分類

## データ確認

### 統計サマリーの確認
```sql
-- ユーザーごとのトレーニング統計
SELECT * FROM v_user_training_summary;

-- 最近のトレーニング記録
SELECT TOP 10 
    u.login_id,
    tm.jp_name as menu_name,
    dtr.training_date,
    dtr.set_count,
    dtr.total_load_amount
FROM daily_training_record dtr
JOIN [user] u ON dtr.user_common_id = u.user_common_id
JOIN training_menu tm ON dtr.menu_id = tm.menu_id
ORDER BY dtr.training_date DESC;
```

## 注意事項
- このデータはテスト・開発用です。本番環境では使用しないでください。
- パスワードハッシュは簡易的なものです。本番環境では適切なハッシュ化を行ってください。
- 既存のデータがある場合は、重複エラーが発生する可能性があります。

## トラブルシューティング

### エラー: "Violation of PRIMARY KEY constraint"
既にデータが存在する場合に発生します。以下のクエリでデータをクリアしてから再実行してください：

```sql
-- テストデータのクリア（注意：全データが削除されます）
DELETE FROM daily_training_record;
DELETE FROM training_record_set;
DELETE FROM training_tag;
DELETE FROM training_menu;
DELETE FROM tag_master;
DELETE FROM user_token;
DELETE FROM user_data;
DELETE FROM [user];
```

### エラー: "Cannot insert the value NULL"
必須カラムにNULLを挿入しようとしている場合に発生します。テーブル定義を確認し、必要に応じてデフォルト値を設定してください。