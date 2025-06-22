# API仕様

🔌 **Message システムのRESTful API仕様書**

このドキュメントでは、Message システムのAPIエンドポイント、認証方式、データ形式について詳細に説明します。

## 📋 API概要

### Base URL
```
Production:  https://api.message-app.com
Staging:     https://staging-api.message-app.com  
Development: https://localhost:5000
```

### API バージョン
- **現在のバージョン**: v1
- **エンドポイント**: `/api/v1/*`

### レスポンス形式
全てのAPIレスポンスは以下の統一フォーマットを使用します：

```json
{
  "success": true,
  "data": { ... },
  "message": "成功メッセージ",
  "timestamp": "2024-01-15T10:00:00Z",
  "requestId": "uuid-string"
}
```

### エラーレスポンス
```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "エラーメッセージ",
    "details": { ... }
  },
  "timestamp": "2024-01-15T10:00:00Z",
  "requestId": "uuid-string"
}
```

## 🔐 認証

### JWT認証
Message APIは JWT（JSON Web Token）を使用した認証システムを採用しています。

#### 認証フロー
```mermaid
sequenceDiagram
    participant Client
    participant API
    participant AuthService
    
    Client->>API: POST /api/auth/login
    API->>AuthService: 認証情報検証
    AuthService-->>API: 認証結果
    API-->>Client: JWT Token + Refresh Token
    
    Note over Client: 以降のリクエストでJWTを使用
    
    Client->>API: GET /api/protected (Authorization: Bearer jwt)
    API->>API: JWT検証
    API-->>Client: レスポンス
```

#### トークン種類

| トークン | 有効期限 | 用途 |
|----------|----------|------|
| **Access Token** | 1時間 | API リクエストの認証 |
| **Refresh Token** | 7日間 | Access Token の更新 |

#### 認証ヘッダー
```http
Authorization: Bearer {access_token}
```

### 認証が必要なエンドポイント
ほとんどのAPIエンドポイントで認証が必要です。例外：

- `GET /api/training/menus` - トレーニングメニュー一覧
- `GET /api/health` - ヘルスチェック
- `POST /api/auth/login` - ログイン
- `POST /api/auth/register` - ユーザー登録

## 📊 レート制限

APIの安定性を保つため、以下のレート制限を設けています：

| エンドポイント | 制限 | 時間窓 |
|----------------|------|--------|
| 認証API | 10リクエスト | 1分 |
| 一般API | 100リクエスト | 1分 |
| サプリメントAPI | 50リクエスト | 1分 |

### レート制限ヘッダー
```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1640995200
```

## 🎯 主要エンドポイント

### 認証API
| メソッド | エンドポイント | 説明 |
|----------|----------------|------|
| `POST` | `/api/auth/login` | [ログイン](/api/auth#login) |
| `POST` | `/api/auth/register` | [ユーザー登録](/api/auth#register) |
| `POST` | `/api/auth/refresh` | [トークン更新](/api/auth#refresh) |
| `POST` | `/api/auth/logout` | [ログアウト](/api/auth#logout) |

### トレーニングAPI
| メソッド | エンドポイント | 説明 |
|----------|----------------|------|
| `GET` | `/api/training/menus` | [メニュー一覧](/api/training#menus) |
| `POST` | `/api/training/record` | [記録保存](/api/training#record) |
| `GET` | `/api/training/history` | [履歴取得](/api/training#history) |
| `GET` | `/api/training/stats` | [統計取得](/api/training#stats) |

### サプリメントAPI
| メソッド | エンドポイント | 説明 |
|----------|----------------|------|
| `GET` | `/api/supplement/supplements` | [サプリメント一覧](/api/supplement#list) |
| `POST` | `/api/supplement/supplements` | [サプリメント登録](/api/supplement#create) |
| `PUT` | `/api/supplement/supplements/{id}` | [サプリメント更新](/api/supplement#update) |
| `DELETE` | `/api/supplement/supplements/{id}` | [サプリメント削除](/api/supplement#delete) |
| `GET` | `/api/supplement/intakes` | [摂取記録取得](/api/supplement#intakes) |
| `POST` | `/api/supplement/intakes` | [摂取記録登録](/api/supplement#record-intake) |
| `GET` | `/api/supplement/schedules` | [スケジュール取得](/api/supplement#schedules) |
| `POST` | `/api/supplement/schedules` | [スケジュール作成](/api/supplement#create-schedule) |

### 管理者API
| メソッド | エンドポイント | 説明 |
|----------|----------------|------|
| `POST` | `/api/admin/users` | [ユーザー作成](/api/admin#create-user) |
| `GET` | `/api/admin/users` | [ユーザー一覧](/api/admin#list-users) |

## 📝 データ形式

### 共通データ型

#### 日時フォーマット
```json
{
  "created_at": "2024-01-15T10:00:00Z",  // ISO 8601 UTC
  "training_date": "2024-01-15",         // YYYY-MM-DD
  "training_time": "14:30:00"            // HH:MM:SS
}
```

#### ページング
```json
{
  "data": [...],
  "pagination": {
    "current_page": 1,
    "total_pages": 10,
    "total_count": 100,
    "page_size": 10,
    "has_next": true,
    "has_previous": false
  }
}
```

### リクエスト例

#### POST リクエスト
```http
POST /api/supplement/supplements
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...

{
  "supplementName": "プロテイン",
  "unit": "g",
  "description": "ホエイプロテイン（バニラ味）"
}
```

#### GET リクエスト（クエリパラメータ）
```http
GET /api/training/history?page=1&limit=20&menu_id=bench_press&date_from=2024-01-01&date_to=2024-01-31
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
```

## 🔍 HTTPステータスコード

| コード | 意味 | 使用例 |
|--------|------|--------|
| `200` | OK | 正常なGET, PUT |
| `201` | Created | 正常なPOST |
| `204` | No Content | 正常なDELETE |
| `400` | Bad Request | バリデーションエラー |
| `401` | Unauthorized | 認証エラー |
| `403` | Forbidden | 権限エラー |
| `404` | Not Found | リソースが見つからない |
| `409` | Conflict | データ重複エラー |
| `422` | Unprocessable Entity | ビジネスロジックエラー |
| `429` | Too Many Requests | レート制限 |
| `500` | Internal Server Error | サーバーエラー |

## 🛡️ セキュリティ

### HTTPS必須
本番環境では全ての通信でHTTPS（TLS 1.2以上）を使用します。

### CORS設定
```http
Access-Control-Allow-Origin: https://message-app.com
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS
Access-Control-Allow-Headers: Content-Type, Authorization
Access-Control-Max-Age: 86400
```

### セキュリティヘッダー
```http
Strict-Transport-Security: max-age=31536000; includeSubDomains
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
```

## 📈 パフォーマンス

### キャッシュ戦略
- **メニューデータ**: 10分間キャッシュ
- **ユーザー情報**: 5分間キャッシュ
- **統計データ**: 30分間キャッシュ

### レスポンス最適化
- **Gzip圧縮**: 全レスポンスで有効
- **JSON最適化**: 不要なフィールドの除外
- **ページング**: 大量データの分割取得

## 🧪 テスト環境

### テスト用アカウント
```json
{
  "email": "test@example.com",
  "password": "TestPassword123!",
  "role": "User"
}
```

### Postmanコレクション
[Postman Collection](./postman-collection.json) をダウンロードして、APIテストにご利用ください。

### cURL例
```bash
# ログイン
curl -X POST https://api.message-app.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"TestPassword123!"}'

# トレーニングメニュー取得
curl -X GET https://api.message-app.com/api/training/menus \
  -H "Authorization: Bearer {token}"
```

## 📚 詳細ドキュメント

詳細な仕様については、以下のページをご覧ください：

- [認証API](/api/auth) - ユーザー認証・管理
- [トレーニングAPI](/api/training) - ワークアウト・記録管理
- [サプリメントAPI](/api/supplement) - サプリメント管理
- [管理者API](/api/admin) - システム管理機能
- [エラーコード一覧](/api/errors) - エラー詳細
- [WebSocket API](/api/websocket) - リアルタイム通信

## 🆔 API変更履歴

### v1.2.0 (2024-01-15)
- ✅ サプリメント管理API追加
- ✅ 認証システム強化
- ✅ レート制限実装

### v1.1.0 (2023-12-01)
- ✅ トレーニング統計API拡張
- ✅ プリセット管理API追加
- ✅ エラーハンドリング改善

### v1.0.0 (2023-10-01)
- ✅ 初期リリース
- ✅ 基本的なCRUD操作
- ✅ JWT認証実装

---

**📞 サポート**: API利用に関するご質問は [GitHub Issues](https://github.com/Message-Team/Message/issues) までお寄せください。