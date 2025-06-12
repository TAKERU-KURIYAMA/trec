# Api - ASP.NET Core Web API プロジェクト

## 概要
トレーニング記録管理システムのバックエンドAPIです。ASP.NET Core 8.0を使用し、SQL Serverデータベースと連携してトレーニングメニューや記録を管理します。

## 必要要件
- .NET 8.0 SDK
- SQL Server (ローカルまたはDocker)
- Visual Studio 2022 または VS Code

## プロジェクト構成
```
api/
├── Controllers/         # APIコントローラー
│   ├── ApiController.cs    # 基本API（バージョン情報など）
│   └── TrainingController.cs # トレーニング関連API
├── Models/             # データベースモデル
│   ├── MessageRDBContext.cs # Entity Framework Context
│   ├── TrainingMenu.cs     # トレーニングメニュー
│   ├── TrainingRecordSet.cs # トレーニング記録
│   ├── User.cs             # ユーザー情報
│   └── ...
├── Services/           # ビジネスロジック
│   ├── AuthService.cs      # 認証サービス
│   └── JwtService.cs       # JWT処理
├── common/             # 共通ユーティリティ
│   ├── ApiResponse.cs      # APIレスポンス形式
│   ├── AppException.cs     # カスタム例外
│   └── Logger.cs           # ロギング
└── Program.cs          # アプリケーションエントリーポイント
```

## セットアップ

### 1. 依存関係のインストール
```bash
cd api
dotnet restore
```

### 2. 環境設定
`appsettings.json` または環境変数で以下を設定：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MessageDB;User Id=sa;Password=YourPassword;TrustServerCertificate=true"
  },
  "JWT": {
    "SECRET": "your-secret-key-here",
    "ISSUER": "your-issuer",
    "AUDIENCE": "your-audience"
  }
}
```

### 3. データベースのセットアップ
```bash
# Entity Framework ツールのインストール
dotnet tool install --global dotnet-ef

# マイグレーションの実行
dotnet ef database update
```

### 4. ビルド
```bash
dotnet build
```

## 実行方法

### ローカル実行
```bash
dotnet run
```
アプリケーションは `http://localhost:5000` で起動します。

### Docker実行
```bash
# イメージのビルド
docker build -t training-api .

# コンテナの実行
docker run -p 5000:80 -e ConnectionStrings__DefaultConnection="..." training-api
```

## API エンドポイント

### 基本API

#### GET /api/version
アプリケーションのバージョン情報を取得

**レスポンス例:**
```json
{
  "result": 0,
  "data": {
    "version": "version-1"
  }
}
```

### トレーニングAPI

#### GET /api/training/menu
トレーニングメニューとタグの一覧を取得

**レスポンス例:**
```json
{
  "result": 0,
  "data": {
    "response_menus": [
      {
        "menuId": "menu001",
        "jpName": "ベンチプレス",
        "enName": "Bench Press",
        "description": "胸部の基本的なトレーニング",
        "createdAt": "2024-01-01T00:00:00",
        "tags": [
          { "tagId": "chest" }
        ]
      }
    ],
    "response_tags": [
      {
        "tagId": "chest",
        "jpName": "胸部",
        "enName": "Chest"
      }
    ]
  }
}
```

## 開発ガイドライン

### 新しいAPIエンドポイントの追加
1. 適切なControllerクラスに新しいアクションメソッドを追加
2. HTTPメソッドとルート属性を設定
3. 共通のエラーハンドリングパターンに従う

### エラーハンドリング例
```csharp
try
{
    // ビジネスロジック
    return Common.Response.CreateOkResponse(data);
}
catch (AppException aex)
{
    return Common.Response.CreateErrorResponse(aex.Cause);
}
catch (Exception ex)
{
    return Common.Response.CreateErrorResponse(FoundationCode.Errors.SERVER_ERROR);
}
```

### データベースアクセス
Entity Framework Core を使用：
```csharp
var menus = await _context.TrainingMenus
    .Include(m => m.TrainingTags)
    .ToListAsync();
```

## デバッグ

### ログの確認
アプリケーションは構造化ログを出力します。開発環境ではコンソールに、本番環境では設定されたログプロバイダーに出力されます。

### データベース接続の確認
```bash
# SQL Server への接続テスト
dotnet run -- --check-db
```

## テスト
```bash
# 単体テストの実行
dotnet test

# 統合テストの実行
dotnet test --filter Category=Integration
```

## デプロイ

### IISへのデプロイ
```bash
dotnet publish -c Release -o ./publish
```

### Dockerコンテナとしてデプロイ
```bash
docker build -t training-api:latest .
docker push your-registry/training-api:latest
```

## トラブルシューティング

### CORS エラー
`Program.cs` でCORS設定を確認してください。開発環境では `AllowAnyOrigin()` が設定されています。

### JWT 認証エラー
- JWT設定（SECRET、ISSUER、AUDIENCE）が正しく設定されているか確認
- トークンの有効期限を確認

### データベース接続エラー
- 接続文字列が正しいか確認
- SQL Serverが起動しているか確認
- ファイアウォール設定を確認