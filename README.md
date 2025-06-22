# TrecPlans - トレーニング記録アプリ

## 概要
TrecPlans は、Azure Functions (.NET 8) をバックエンドとして使用するトレーニング記録管理アプリケーションです。ユーザーがトレーニングメニューを管理し、日々のトレーニング記録を追跡できます。

## システム構成

### アーキテクチャ
- **フロントエンド**: Nuxt.js 3 (Vue.js)
- **バックエンド**: Azure Functions (.NET 8.0) with Entity Framework Core
- **データベース**: SQL Server 2022
- **キャッシュ**: Redis
- **プロキシ**: Nginx
- **コンテナ化**: Docker Compose

### 技術スタック
- **API**: C# Azure Functions v4, Entity Framework Core 8.0
- **認証**: JWT (JSON Web Token)
- **フロントエンド**: Vue.js 3, Nuxt.js, TypeScript
- **データベース**: Microsoft SQL Server 2022
- **インフラ**: Docker, Docker Compose, Nginx

## セットアップ手順

### 前提条件
- Docker および Docker Compose
- .NET 8.0 SDK （ローカル開発時）
- Node.js 18+ （フロントエンド開発時）

### 起動方法

1. **リポジトリのクローン**
```bash
git clone <repository-url>
cd Message
```

2. **Docker Composeで起動**
```bash
docker-compose up --build
```

3. **アクセス**
- アプリケーション: http://local-trecplans
- API: http://local-trecplans/api
- データベース: localhost:1433 (sa/Your_password123)
- Redis: localhost:6379

### 環境変数設定

APIサービスの主要な環境変数：

```yaml
# JWT設定
JWT__SECRET: "YwN7c!@#m02zL*sd82lMk!adwaf4d2rg"
JWT__ISSUER: "trecplans.local"  
JWT__AUDIENCE: "trecplans_user"

# データベース接続
CONNECTIONSTRINGS__DEFAULTCONNECTION: "Server=db;Database=MessageRDB;User=sa;Password=Your_password123;"

# Redis接続
REDIS__HOST: "redis"

# Azure Functions設定
FUNCTIONS_WORKER_RUNTIME: "dotnet-isolated"
AZUREWEBJOBSSTORAGE: "UseDevelopmentStorage=true"
```

## API エンドポイント

### 認証系
- `POST /api/account/user` - ユーザー登録
- `POST /api/account/token` - ログイン・トークン取得

### トレーニング系  
- `GET /api/training/menu` - トレーニングメニュー一覧取得
- `GET /api/training/record` - トレーニング記録取得
- `POST /api/training/record` - トレーニング記録登録
- `DELETE /api/training/record` - トレーニング記録削除
- `GET /api/training/daily` - 日別トレーニング記録取得

### その他
- `GET /api/version` - バージョン情報取得

## データベース構成

### 主要テーブル
- **Users** - ユーザー情報
- **TrainingMenus** - トレーニングメニューマスタ
- **TrainingRecordSets** - トレーニング記録（セット単位）
- **DailyTrainingRecord** - 日別集計記録
- **TrainingTags** - トレーニングタグ
- **UserTokens** - リフレッシュトークン管理

### データベース初期化
```bash
# 初期化スクリプトの実行
docker exec -it <db-container> /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P Your_password123 \
  -i /var/opt/mssql/DB_SCRIPT/01_init/01_init_up.sql
```

## 開発情報

### プロジェクト構成
```
Message/
├── api/                    # Azure Functions API
│   ├── API/               # エンドポイント実装
│   ├── Models/            # Entity Framework モデル
│   ├── Services/          # ビジネスロジック
│   ├── common/            # 共通ユーティリティ
│   └── Program.cs         # アプリケーション設定
├── frontend/              # Nuxt.js フロントエンド
├── DB_SCRIPT/            # データベーススキーマ
├── nginx/                # Nginx設定
└── docker-compose.yml    # コンテナ構成
```

### 既知の問題と対処法

1. **APIが動かない場合**
   - docker-compose.yml で適切なAPIサービス（`api`）がアクティブか確認
   - 環境変数（特にJWT設定とDB接続文字列）が正しく設定されているか確認
   - コンテナログを確認: `docker-compose logs api`

2. **データベース接続エラー**
   - SQL Serverコンテナが完全に起動するまで待機（初回は時間がかかる）
   - 接続文字列の認証情報を確認

3. **フロントエンドでAPI呼び出しエラー**
   - Nginxプロキシ設定を確認（`/api/` → APIコンテナへの転送）
   - CORS設定の確認

### 開発時のコマンド

```bash
# ローカル開発（API）
cd api
dotnet run

# ローカル開発（フロントエンド）
cd frontend  
npm run dev

# データベースマイグレーション
cd api
dotnet ef database update

# テスト実行
cd api
dotnet test
```

## ライセンス
[ライセンス情報を記載]

## 貢献方法
[貢献ガイドラインを記載]

## 更新履歴
- 2025/01/06: 自動確認設定のテスト実施 (auto-yes.shテスト)
- 2025/06/12: READMEの編集テスト実施
