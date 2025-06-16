# Message API仕様書

## API概要

- **ベースURL**: `https://api.example.com/api`
- **バージョン**: v1.0.0
- **認証方式**: JWT Bearer Token
- **レスポンス形式**: JSON
- **文字エンコーディング**: UTF-8

## 共通仕様

### HTTPステータスコード

| コード | 説明 |
|--------|------|
| 200 | 成功 |
| 201 | 作成成功 |
| 400 | リクエストエラー |
| 401 | 認証エラー |
| 403 | 権限エラー |
| 404 | リソースが見つからない |
| 500 | サーバーエラー |

### エラーコード一覧

| コード | 説明 | HTTPステータス |
|--------|------|----------------|
| 00001 | パラメータエラー | 400 |
| 10001 | サーバーエラー | 500 |
| 10002 | データベースエラー | 500 |
| 10007 | 認証エラー | 401 |
| 10008 | データが見つかりません | 404 |
| 10009 | 権限がありません | 403 |
| 20001 | ユーザーが既に存在します | 400 |
| 20002 | パスワードが一致しません | 400 |

## 認証API

### 1. ユーザー登録

```
POST /auth/register
```

#### リクエスト
```json
{
  "email": "user@example.com",
  "password": "StrongPassword123!",
  "name": "山田太郎"
}
```

#### レスポンス
```json
{
  "success": true,
  "data": {
    "userId": "USR_123456789",
    "userCommonId": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "name": "山田太郎",
    "createdAt": "2024-01-15T10:00:00Z"
  },
  "message": "ユーザー登録が完了しました"
}
```

### 2. ログイン

```
POST /auth/login
```

#### リクエスト
```json
{
  "email": "user@example.com",
  "password": "StrongPassword123!"
}
```

#### レスポンス
```json
{
  "success": true,
  "data": {
    "user": {
      "userId": "USR_123456789",
      "email": "user@example.com",
      "name": "山田太郎"
    },
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 604800,
    "refreshToken": "rf_eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  },
  "message": "ログインに成功しました"
}
```

### 3. トークン更新

```
POST /auth/refresh
```

#### リクエストヘッダー
```
Authorization: Bearer {current_token}
```

#### リクエスト
```json
{
  "refreshToken": "rf_eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

#### レスポンス
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 604800
  },
  "message": "トークンを更新しました"
}
```

## トレーニングAPI

### 1. メニュー・タグ一覧取得

```
GET /training/menu
```

認証: 不要

#### レスポンス
```json
{
  "success": true,
  "data": {
    "menus": [
      {
        "menuId": "bench_press",
        "jpName": "ベンチプレス",
        "enName": "Bench Press",
        "description": "胸部を鍛える基本的な種目",
        "createdAt": "2024-01-01T00:00:00Z",
        "tagIds": ["chest", "compound", "barbell"]
      }
    ],
    "tags": [
      {
        "tagId": "chest",
        "jpName": "胸",
        "enName": "Chest"
      }
    ],
    "meta": {
      "menu_count": 50,
      "tag_count": 20,
      "retrieved_at": "2024-01-15T10:00:00Z"
    }
  }
}
```

### 2. トレーニング記録登録

```
POST /training/record
```

認証: 必要

#### リクエスト
```json
{
  "menuId": "bench_press",
  "trainingDate": "2024-01-15",
  "sets": [
    {
      "setNumber": 1,
      "reps": 10,
      "weight": 60.0,
      "note": "ウォームアップ"
    },
    {
      "setNumber": 2,
      "reps": 8,
      "weight": 80.0
    },
    {
      "setNumber": 3,
      "reps": 6,
      "weight": 90.0
    }
  ]
}
```

#### レスポンス
```json
{
  "success": true,
  "data": {
    "recordId": "REC_123456789",
    "dailyRecordId": "USR_123_bench_press_2024-01-15",
    "setCount": 3,
    "totalReps": 24,
    "totalVolume": 1740.0,
    "maxWeight": 90.0,
    "trainingDate": "2024-01-15"
  },
  "message": "トレーニングレコードが正常に登録されました"
}
```

### 3. トレーニング履歴取得

```
GET /training/history
```

認証: 必要

#### クエリパラメータ

| パラメータ | 型 | 必須 | 説明 |
|-----------|-----|------|------|
| menuId | string | × | メニューIDでフィルタ |
| startDate | string | × | 開始日（YYYY-MM-DD） |
| endDate | string | × | 終了日（YYYY-MM-DD） |
| limit | integer | × | 取得件数（1-100、デフォルト:50） |

#### レスポンス
```json
{
  "success": true,
  "data": {
    "records": [
      {
        "recordId": "USR_123_bench_press_2024-01-15",
        "menuId": "bench_press",
        "menuName": "ベンチプレス",
        "trainingDate": "2024-01-15",
        "setCount": 3,
        "totalReps": 24,
        "maxReps": 10,
        "maxWeight": 90.0,
        "maxRepsWeight": 60.0,
        "maxWeightReps": 6,
        "totalLoadAmount": 1740.0,
        "createdAt": "2024-01-15T10:30:00Z",
        "updatedAt": "2024-01-15T10:30:00Z"
      }
    ],
    "meta": {
      "total_count": 30,
      "has_more": true,
      "filters": {
        "menu_id": "bench_press",
        "start_date": "2024-01-01",
        "end_date": "2024-01-15",
        "limit": 50
      }
    }
  }
}
```

### 4. トレーニング詳細取得

```
GET /training/history/details
```

認証: 必要

#### クエリパラメータ

| パラメータ | 型 | 必須 | 説明 |
|-----------|-----|------|------|
| menuId | string | ○ | メニューID |
| trainingDate | string | ○ | トレーニング日（YYYY-MM-DD） |

#### レスポンス
```json
{
  "success": true,
  "data": {
    "menu_id": "bench_press",
    "training_date": "2024-01-15",
    "sets": [
      {
        "setNumber": 1,
        "reps": 10,
        "weight": 60.0,
        "createdAt": "2024-01-15T10:00:00Z"
      },
      {
        "setNumber": 2,
        "reps": 8,
        "weight": 80.0,
        "createdAt": "2024-01-15T10:05:00Z"
      },
      {
        "setNumber": 3,
        "reps": 6,
        "weight": 90.0,
        "createdAt": "2024-01-15T10:10:00Z"
      }
    ],
    "summary": {
      "total_sets": 3,
      "total_reps": 24,
      "max_weight": 90.0,
      "total_volume": 1740.0
    }
  }
}
```

### 5. プリセット一覧取得

```
GET /training/presets
```

認証: 必要

#### レスポンス
```json
{
  "success": true,
  "data": {
    "presets": [
      {
        "presetId": "preset_1",
        "name": "胸トレーニング基本",
        "description": "ベンチプレス中心の胸トレーニング",
        "menuId": "bench_press",
        "menuName": "ベンチプレス",
        "defaultSets": [
          {
            "setNumber": 1,
            "reps": 10,
            "weight": 60.0
          },
          {
            "setNumber": 2,
            "reps": 8,
            "weight": 70.0
          },
          {
            "setNumber": 3,
            "reps": 6,
            "weight": 80.0
          }
        ],
        "createdAt": "2024-01-01T00:00:00Z",
        "isDefault": true
      }
    ],
    "meta": {
      "total_count": 5
    }
  }
}
```

### 6. プリセット作成

```
POST /training/presets
```

認証: 必要

#### リクエスト
```json
{
  "name": "新しいプリセット",
  "description": "カスタムプリセット",
  "menuId": "squat",
  "defaultSets": [
    {
      "setNumber": 1,
      "reps": 12,
      "weight": 60.0
    },
    {
      "setNumber": 2,
      "reps": 10,
      "weight": 80.0
    }
  ]
}
```

#### レスポンス
```json
{
  "success": true,
  "data": {
    "presetId": "preset_abc123",
    "name": "新しいプリセット",
    "menuId": "squat",
    "setCount": 2,
    "createdAt": "2024-01-15T10:00:00Z"
  },
  "message": "プリセットが正常に作成されました"
}
```

### 7. アイソレーション種目取得

```
GET /training/menu/isolation
```

認証: 不要

#### クエリパラメータ

| パラメータ | 型 | 必須 | 説明 |
|-----------|-----|------|------|
| bodyPartTag | string | × | 部位タグでフィルタ |

#### レスポンス
```json
{
  "success": true,
  "data": {
    "body_part_groups": [
      {
        "bodyPartId": "chest",
        "bodyPartName": "胸",
        "bodyPartNameEn": "Chest",
        "exerciseCount": 5,
        "exercises": [
          {
            "menuId": "cable_fly",
            "menuName": "ケーブルフライ",
            "menuNameEn": "Cable Fly",
            "description": "胸部のアイソレーション種目",
            "allTags": [
              {
                "tagId": "chest",
                "tagName": "胸",
                "tagNameEn": "Chest"
              },
              {
                "tagId": "isolation",
                "tagName": "アイソレーション",
                "tagNameEn": "Isolation"
              },
              {
                "tagId": "cable",
                "tagName": "ケーブル",
                "tagNameEn": "Cable"
              }
            ],
            "equipment": "ケーブル",
            "difficulty": "中級",
            "createdAt": "2024-01-01T00:00:00Z"
          }
        ]
      }
    ],
    "summary": {
      "total_body_parts": 7,
      "total_exercises": 35,
      "filter_applied": null
    },
    "meta": {
      "retrieved_at": "2024-01-15T10:00:00Z",
      "exercise_type": "isolation"
    }
  }
}
```

## 管理者API

### 1. ユーザー作成

```
POST /admin/users/create
```

認証: Admin権限必要

#### リクエスト
```json
{
  "email": "newuser@example.com",
  "password": "TempPassword123!",
  "name": "新規ユーザー",
  "role": "User"
}
```

#### レスポンス
```json
{
  "success": true,
  "data": {
    "userId": "USR_987654321",
    "email": "newuser@example.com",
    "name": "新規ユーザー",
    "role": "User",
    "createdAt": "2024-01-15T10:00:00Z"
  },
  "message": "ユーザーを作成しました"
}
```

### 2. ユーザー一覧取得

```
GET /admin/users
```

認証: Admin権限必要

#### クエリパラメータ

| パラメータ | 型 | 必須 | 説明 |
|-----------|-----|------|------|
| page | integer | × | ページ番号（デフォルト:1） |
| limit | integer | × | 1ページの件数（デフォルト:20） |
| search | string | × | 検索キーワード |

#### レスポンス
```json
{
  "success": true,
  "data": {
    "users": [
      {
        "userCommonId": "550e8400-e29b-41d4-a716-446655440000",
        "userId": "USR_123456789",
        "email": "user@example.com",
        "name": "山田太郎",
        "role": "User",
        "createdAt": "2024-01-01T00:00:00Z",
        "lastLoginAt": "2024-01-15T09:00:00Z"
      }
    ],
    "pagination": {
      "currentPage": 1,
      "totalPages": 5,
      "totalCount": 100,
      "hasNext": true,
      "hasPrevious": false
    }
  }
}
```

## ヘルスチェックAPI

### システムヘルスチェック

```
GET /health
```

認証: 不要

#### レスポンス
```json
{
  "status": "healthy",
  "version": "1.0.0",
  "timestamp": "2024-01-15T10:00:00Z",
  "services": {
    "database": "healthy",
    "cache": "healthy"
  }
}
```

## WebSocket API（将来実装予定）

### リアルタイムトレーニング同期

```
ws://api.example.com/ws/training
```

#### 接続時認証
```json
{
  "type": "auth",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

#### セット完了通知
```json
{
  "type": "set_completed",
  "data": {
    "sessionId": "session_123",
    "setNumber": 1,
    "reps": 10,
    "weight": 60.0,
    "timestamp": "2024-01-15T10:00:00Z"
  }
}
```

## レート制限

- 認証エンドポイント: 5リクエスト/分
- 通常エンドポイント: 100リクエスト/分
- 管理者エンドポイント: 30リクエスト/分

レート制限に達した場合、以下のレスポンスヘッダーが返されます：
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1642248000
```