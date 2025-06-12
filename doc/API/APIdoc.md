# 📋 Message Training API 設計書

このドキュメントは、Message Training アプリケーションのAPI設計と仕様を包括的に記載したものです。

## 📚 目次

1. [概要](#概要)
2. [アーキテクチャ](#アーキテクチャ)
3. [認証・認可](#認証認可)
4. [データモデル](#データモデル)
5. [API エンドポイント](#api-エンドポイント)
6. [エラーハンドリング](#エラーハンドリング)
7. [セキュリティ](#セキュリティ)

---

## 🎯 概要

Message Training API は、フィットネス・トレーニング管理アプリケーション向けのREST APIです。ユーザー認証、トレーニングメニュー管理、トレーニング記録の追跡機能を提供します。

### 主要機能
- ユーザー認証とJWTトークン管理
- トレーニングメニューとタグの管理
- トレーニング記録（セット単位）の登録・取得・削除
- 日次トレーニング統計の自動集計
- 多言語対応（日本語・英語）

---

## 🏗️ アーキテクチャ

### 技術スタック
- **Backend**: ASP.NET Core Web API
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Authentication**: JWT Bearer Token
- **Architecture**: Repository Pattern with Service Layer

### データベース構造
```
Users (ユーザー管理)
├── TrainingRecordSets (トレーニング記録)
└── DailyTrainingRecords (日次集計)

TrainingMenus (トレーニングメニュー)
├── TrainingRecordSets
├── DailyTrainingRecords
└── TrainingTags (メニュー-タグ関連)

TagMasters (タグマスター)
└── TrainingTags

Clients (クライアント管理)
├── ClientDataKeys
└── UserData
```

---

## 🔐 認証・認可

### JWT認証
- **トークン有効期限**: 1時間
- **リフレッシュトークン**: サポート
- **ヘッダー形式**: `Authorization: Bearer {token}`

### ユーザー登録・認証フロー
1. ユーザー登録（メール・パスワード・表示名）
2. パスワードハッシュ化（Salt付き）
3. ログイン認証
4. JWTトークン発行
5. APIアクセス時トークン検証

---

## 📊 データモデル

### 主要エンティティ

#### User（ユーザー）
```csharp
{
  "userCommonId": "string(16)",     // 主キー
  "loginId": "string(256)",         // ログインID（ユニーク）
  "passwordHash": "string(512)",    // ハッシュ化パスワード
  "passwordSalt": "string(512)",    // ソルト
  "displayName": "string(100)",     // 表示名
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

#### TrainingMenu（トレーニングメニュー）
```csharp
{
  "menuId": "string(64)",           // 主キー
  "jpName": "string(100)",          // 日本語名
  "enName": "string(100)",          // 英語名
  "description": "string(255)",     // 説明
  "createdAt": "datetime"
}
```

#### TrainingRecordSet（トレーニング記録セット）
```csharp
{
  "userCommonId": "string(16)",     // 複合主キー
  "menuId": "string(64)",           // 複合主キー
  "trainingDate": "DateOnly",       // 複合主キー
  "setNumber": "int",               // 複合主キー
  "reps": "int",                    // 回数
  "weight": "decimal(5,2)?",        // 重量（kg）
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

#### DailyTrainingRecord（日次トレーニング記録）
```csharp
{
  "userCommonId": "string(16)",     // 複合主キー
  "menuId": "string(64)",           // 複合主キー
  "trainingDate": "DateOnly",       // 複合主キー
  "setCount": "int",                // セット数
  "maxReps": "int",                 // 最大回数
  "maxRepsWeight": "decimal(5,2)?", // 最大回数時の重量
  "maxWeight": "decimal(5,2)?",     // 最大重量
  "totalLoadAmount": "decimal(10,2)?", // 総負荷量
  "totalReps": "int",               // 総回数
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

---

## 🌐 API エンドポイント

### 共通仕様
- **ベースURL**: `/api`
- **Content-Type**: `application/json`
- **文字エンコーディング**: UTF-8
- **認証**: JWT Bearer Token（認証不要エンドポイントを除く）

---

### 🔧 システム情報

#### GET `/api/version`
APIバージョン情報を取得

**認証**: 不要

**Response Body**:
```json
{
  "version": "version-1"
}
```

**Response Codes**:
- `200 OK`: 取得成功

---

### 👤 ユーザー認証

#### POST `/api/auth/register`
新規ユーザー登録

**認証**: 不要

**Request Body**:
```json
{
  "loginId": "user@example.com",
  "password": "YourPassword123!",
  "displayName": "表示名"
}
```

**Validation**:
- `loginId`: 必須、256文字以内、ユニーク
- `password`: 必須、適切な強度
- `displayName`: 任意、100文字以内

**Response Codes**:
- `204 No Content`: 登録成功
- `409 Conflict`: LoginID重複
- `400 Bad Request`: バリデーションエラー

---

#### POST `/api/auth/token`
ログイン認証・トークン発行

**認証**: 不要

**Request Body**:
```json
{
  "loginId": "user@example.com",
  "password": "YourPassword123!"
}
```

**Response Body**:
```json
{
  "accessToken": "JWT_TOKEN",
  "refreshToken": "REFRESH_TOKEN"
}
```

**Response Codes**:
- `200 OK`: 認証成功
- `401 Unauthorized`: 認証失敗

---

### 🏋️‍♂️ トレーニングメニュー

#### GET `/api/training/menu`
トレーニングメニューとタグ一覧を取得

**認証**: 不要

**Response Body**:
```json
{
  "response_menus": [
    {
      "menuId": "BENCH_PRESS",
      "jpName": "ベンチプレス",
      "enName": "Bench Press",
      "description": "胸筋を鍛える基本的なエクササイズ",
      "createdAt": "2025-01-01T00:00:00Z",
      "tags": [
        {
          "tagId": "CHEST"
        }
      ]
    }
  ],
  "response_tags": [
    {
      "tagId": "CHEST",
      "jpName": "胸筋",
      "enName": "Chest"
    }
  ]
}
```

**Response Codes**:
- `200 OK`: 取得成功

---

### 📝 トレーニング記録管理

#### POST `/api/training/record`
トレーニング記録登録・更新

**認証**: 必要

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Request Body**:
```json
{
  "menuId": "BENCH_PRESS",
  "trainingDate": "2025-01-15",
  "setNumber": 1,
  "reps": 10,
  "weight": 60.0
}
```

**Business Logic**:
- 既存セットの場合は更新
- 新規セットの場合は追加
- 日次統計の自動更新

**Response Body**:
```json
{
  "training_record_set": {
    "userCommonId": "USER12345",
    "menuId": "BENCH_PRESS",
    "trainingDate": "2025-01-15",
    "setNumber": 1,
    "reps": 10,
    "weight": 60.0,
    "createdAt": "2025-01-15T10:00:00Z",
    "updatedAt": "2025-01-15T10:00:00Z"
  }
}
```

**Response Codes**:
- `200 OK`: 登録・更新成功
- `401 Unauthorized`: 認証エラー
- `400 Bad Request`: バリデーションエラー

---

#### GET `/api/training/record`
特定日・メニューのトレーニング記録取得

**認証**: 必要

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Query Parameters**:
- `menuId`: トレーニングメニューID（必須）
- `trainingDate`: 日付 yyyy-MM-dd（必須）

**Response Body**:
```json
{
  "records": [
    {
      "menuId": "BENCH_PRESS",
      "trainingDate": "2025-01-15",
      "setNumber": 1,
      "reps": 10,
      "weight": 60.0,
      "createdAt": "2025-01-15T10:00:00Z",
      "updatedAt": "2025-01-15T10:00:00Z"
    }
  ]
}
```

**Response Codes**:
- `200 OK`: 取得成功
- `401 Unauthorized`: 認証エラー

---

#### DELETE `/api/training/record`
特定セットの削除

**認証**: 必要

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Query Parameters**:
- `menuId`: トレーニングメニューID（必須）
- `trainingDate`: 日付 yyyy-MM-dd（必須）
- `setNumber`: セット番号（必須）

**Response Body**:
```json
{}
```

**Response Codes**:
- `200 OK`: 削除成功
- `401 Unauthorized`: 認証エラー
- `404 Not Found`: 削除対象不存在

---

### 📈 トレーニング統計

#### GET `/api/training/daily`
日次トレーニング統計取得

**認証**: 必要

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Query Parameters**:
- `menuId`: トレーニングメニューID（必須）
- `fromDate`: 開始日 yyyy-MM-dd（任意、デフォルト: 1ヶ月前）
- `toDate`: 終了日 yyyy-MM-dd（任意、デフォルト: 今日）

**Response Body**:
```json
{
  "records": [
    {
      "trainingDate": "2025-01-15",
      "setCount": 3,
      "maxReps": 10,
      "maxRepsWeight": 60.0,
      "maxWeight": 65.0,
      "totalLoadAmount": 1800.0,
      "totalReps": 27
    }
  ]
}
```

**Response Codes**:
- `200 OK`: 取得成功
- `401 Unauthorized`: 認証エラー

---

#### GET `/api/training/history`
トレーニング履歴取得（期間指定）

**認証**: 必要

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Query Parameters**:
- `startDate`: 開始日 yyyy-MM-dd（任意）
- `endDate`: 終了日 yyyy-MM-dd（任意）

**Response Body**:
```json
[
  {
    "trainingDate": "2025-01-15",
    "menuId": "BENCH_PRESS",
    "sets": [
      {
        "setNumber": 1,
        "reps": 10,
        "weight": 60.0
      }
    ]
  }
]
```

**Response Codes**:
- `200 OK`: 取得成功
- `401 Unauthorized`: 認証エラー

---

## ⚠️ エラーハンドリング

### 標準エラーレスポンス形式
```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "エラーメッセージ",
    "details": "詳細情報（任意）"
  }
}
```

### 主要エラーコード
- `UNAUTHORIZED`: 認証エラー
- `FORBIDDEN`: 権限不足
- `VALIDATION_ERROR`: バリデーションエラー
- `DUPLICATE_RESOURCE`: リソース重複
- `RESOURCE_NOT_FOUND`: リソース不存在
- `INTERNAL_SERVER_ERROR`: サーバー内部エラー

### HTTPステータスコード
- `200 OK`: 成功
- `204 No Content`: 成功（レスポンスボディなし）
- `400 Bad Request`: リクエストエラー
- `401 Unauthorized`: 認証エラー
- `403 Forbidden`: 権限エラー
- `404 Not Found`: リソース不存在
- `409 Conflict`: リソース競合
- `500 Internal Server Error`: サーバーエラー

---

## 🔒 セキュリティ

### 認証・認可
- JWT Bearer Token認証
- トークン有効期限: 1時間
- リフレッシュトークンによる自動更新
- パスワードハッシュ化（Salt + Hash）

### データValidation
- 入力値の型・長さ・形式チェック
- SQLインジェクション対策（パラメータ化クエリ）
- XSS対策（入力値エスケープ）

### API制限
- CORS設定
- Rate Limiting（実装予定）
- HTTPS必須（本番環境）

### ログ・監査
- API呼び出しログ
- エラーログ
- セキュリティイベントログ

---

## 📝 実装状況

### ✅ 実装済み
- GET `/api/version`
- GET `/api/training/menu`
- レガシーAPI（Azure Functions）全機能

### 🚧 実装中
- ASP.NET Core Controllerへの移行
- 認証機能の新アーキテクチャ対応

### 📋 未実装
- POST `/api/auth/register`
- POST `/api/auth/token`
- POST `/api/training/record`
- GET `/api/training/record`
- DELETE `/api/training/record`
- GET `/api/training/daily`
- GET `/api/training/history`

---

## 🔄 マイグレーション計画

### Phase 1: Core API移行
1. 認証エンドポイントの実装
2. トレーニング記録CRUD操作
3. 統計API実装

### Phase 2: 機能拡張
1. フィルタリング・ソート機能
2. バッチ操作API
3. エクスポート機能

### Phase 3: 最適化
1. キャッシュ実装
2. パフォーマンス最適化
3. API Rate Limiting

---

*最終更新: 2025年1月*
