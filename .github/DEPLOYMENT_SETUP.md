# GitHub Actions デプロイ設定ガイド

このプロジェクト用のGitHub Actionsワークフローが作成されました。以下の手順でセットアップを完了してください。

## 必要なSecrets設定

### 共通設定
GitHubリポジトリの Settings > Secrets and variables > Actions で以下のsecretsを設定してください。

#### インフラ・デプロイ関連
```
DEPLOY_HOST=your-server-ip-or-domain
DEPLOY_USER=deploy-user
DEPLOY_SSH_KEY=your-ssh-private-key

CONTAINER_REGISTRY=your-registry.com
CONTAINER_USERNAME=registry-username
CONTAINER_PASSWORD=registry-password

API_BASE_URL=https://api.yourdomain.com
API_URL=https://api.yourdomain.com
FRONTEND_URL=https://yourdomain.com

DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=message;Username=postgres;Password=your-password

AWS_ACCESS_KEY_ID=your-aws-access-key
AWS_SECRET_ACCESS_KEY=your-aws-secret-key
AWS_REGION=ap-northeast-1
BACKUP_BUCKET=your-backup-bucket
```

#### 通知設定
```
SLACK_WEBHOOK_URL=https://hooks.slack.com/services/your/webhook/url
```

#### モバイルアプリ関連
```
# Android
ANDROID_SIGNING_KEY=base64-encoded-keystore
ANDROID_KEY_ALIAS=your-key-alias
ANDROID_KEYSTORE_PASSWORD=keystore-password
ANDROID_KEY_PASSWORD=key-password
GOOGLE_PLAY_SERVICE_ACCOUNT=google-play-service-account-json

# iOS
APPLE_ID_USERNAME=your-apple-id
APPLE_ID_PASSWORD=app-specific-password
```

#### セキュリティスキャン関連
```
SONAR_TOKEN=your-sonarcloud-token
```

### Environment設定
Settings > Environments で以下の環境を作成：

#### production
- Protection rules: Require reviewers (1人以上)
- Deployment branches: main ブランチのみ

#### staging  
- Protection rules: 必要に応じて設定
- Deployment branches: develop ブランチも許可

## ワークフロー説明

### 1. ci.yml - 継続的インテグレーション
- **トリガー**: すべてのpush, PR
- **機能**: 
  - 変更検出による最適化
  - リンティング・フォーマットチェック
  - テスト実行（フロントエンド・API・モバイル）
  - セキュリティスキャン
  - カバレッジ計測

### 2. deploy-frontend.yml - フロントエンドデプロイ
- **トリガー**: mainブランチへのpush（frontendディレクトリ変更時）
- **機能**:
  - テスト → ビルド → Dockerイメージ作成 → デプロイ
  - ヘルスチェック
  - 失敗時通知

### 3. deploy-api.yml - APIデプロイ
- **トリガー**: mainブランチへのpush（api・Common.Sharedディレクトリ変更時）
- **機能**:
  - テスト → ビルド → DBマイグレーション → デプロイ
  - ロールバック機能
  - ヘルスチェック

### 4. deploy-mobile.yml - モバイルアプリデプロイ
- **トリガー**: mainブランチへのpush（mobile-appディレクトリ変更時）
- **機能**:
  - Android APK・iOS IPA ビルド
  - Google Play Store・App Store デプロイ
  - 段階的ロールアウト対応

### 5. database-migration.yml - データベースマイグレーション
- **トリガー**: 手動実行（workflow_dispatch）
- **機能**:
  - バックアップ作成
  - マイグレーション実行・ロールバック
  - 検証・通知

### 6. security-scan.yml - セキュリティスキャン
- **トリガー**: 定期実行（毎週月曜2:00 UTC）、push、PR
- **機能**:
  - シークレットスキャン
  - 依存関係脆弱性チェック
  - コンテナスキャン
  - ライセンスチェック
  - インフラスキャン

### 7. dependabot.yml - 依存関係更新
- **機能**: 
  - 週次での依存関係更新
  - プラットフォーム別設定
  - 自動PR作成

## 初期セットアップ手順

### 1. サーバー準備
```bash
# デプロイ用ユーザー作成
sudo useradd -m -s /bin/bash deploy
sudo usermod -aG docker deploy

# SSH鍵設定
sudo -u deploy mkdir -p /home/deploy/.ssh
# 公開鍵を /home/deploy/.ssh/authorized_keys に追加

# プロジェクトディレクトリ作成
sudo mkdir -p /opt/message
sudo chown deploy:deploy /opt/message

# バックアップディレクトリ作成
sudo mkdir -p /opt/backups
sudo chown deploy:deploy /opt/backups
```

### 2. Docker Compose設定
サーバーに `docker-compose.yml` を配置：

```yaml
version: '3.8'

services:
  frontend:
    image: your-registry/message-frontend:latest
    ports:
      - "3000:3000"
    environment:
      - NUXT_PUBLIC_API_BASE_URL=${API_BASE_URL}
    restart: unless-stopped

  api:
    image: your-registry/message-api:latest
    ports:
      - "5000:5000"
    environment:
      - ConnectionStrings__DefaultConnection=${DATABASE_CONNECTION_STRING}
    depends_on:
      - postgres
    restart: unless-stopped

  postgres:
    image: postgres:15
    environment:
      - POSTGRES_DB=message
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./backups:/backups
    restart: unless-stopped

volumes:
  postgres_data:
```

### 3. 環境変数設定
サーバーに `.env` ファイルを作成：

```bash
# /opt/message/.env
API_BASE_URL=https://api.yourdomain.com
DATABASE_CONNECTION_STRING=Host=postgres;Port=5432;Database=message;Username=postgres;Password=your-password
POSTGRES_PASSWORD=your-password
```

### 4. Nginx設定（オプション）
リバースプロキシ設定例：

```nginx
server {
    listen 80;
    server_name yourdomain.com;

    location / {
        proxy_pass http://localhost:3000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}

server {
    listen 80;
    server_name api.yourdomain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

### 5. 初回デプロイ
1. リポジトリのSecretsを設定
2. mainブランチにpush
3. GitHub Actionsのワークフローが自動実行される

### 6. 確認事項
- [ ] すべてのワークフローが正常に実行される
- [ ] デプロイが成功する
- [ ] ヘルスチェックが通る
- [ ] 通知が正常に送信される

## トラブルシューティング

### よくある問題

#### 1. SSH接続エラー
- SSH鍵の権限確認: `chmod 600 ~/.ssh/id_rsa`
- known_hosts設定
- ファイアウォール設定確認

#### 2. Docker権限エラー
```bash
sudo usermod -aG docker deploy
sudo systemctl restart docker
```

#### 3. データベース接続エラー
- 接続文字列の確認
- ポート開放状況確認
- パスワード設定確認

### デバッグ方法
1. GitHub ActionsのログからエラーDetailを確認
2. サーバーでコンテナログ確認: `docker-compose logs`
3. データベース接続テスト: `docker-compose exec postgres psql -U postgres -d message`

## カスタマイズ

### 通知設定
- Slack以外の通知（Discord、Teams等）に変更可能
- 通知内容・タイミングのカスタマイズ

### デプロイ戦略
- Blue-Green デプロイ
- Canary リリース
- A/B テスト対応

### セキュリティ強化
- OIDCプロバイダーの利用
- Vault統合
- 追加のセキュリティスキャンツール

このセットアップガイドに従って、段階的にデプロイパイプラインを構築してください。