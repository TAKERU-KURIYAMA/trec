# API契約書（API Contract）

🤝 **バックエンドとフロントエンド間のAPI仕様合意書**

## 📋 概要

このドキュメントは、Message システムのバックエンド（ASP.NET Core + SQL Server）とフロントエンド（Web・Mobile）間のAPI仕様を定義します。両チームはこの仕様に基づいて開発を進めます。

## 🌐 基本仕様

### Base URL
```
Development: http://localhost:5000/api
Staging: https://api-staging.message-app.com/api
Production: https://api.message-app.com/api
```

### レスポンス形式
```json
{
  "success": true,
  "data": { /* レスポンスデータ */ },
  "message": "操作が完了しました",
  "errors": []
}
```

### エラーレスポンス形式
```json
{
  "success": false,
  "data": null,
  "message": "エラーメッセージ",
  "errors": [
    {
      "field": "フィールド名",
      "code": "ERROR_CODE",
      "message": "詳細エラーメッセージ"
    }
  ]
}
```

### 認証
```http
Authorization: Bearer {JWT_ACCESS_TOKEN}
```

### HTTPステータスコード
| コード | 用途 |
|--------|------|
| 200 | 成功 |
| 201 | 作成成功 |
| 400 | バリデーションエラー |
| 401 | 認証エラー |
| 403 | 認可エラー |
| 404 | リソースが見つからない |
| 429 | レート制限 |
| 500 | サーバーエラー |

## 🔐 認証API

### 1. ユーザー登録

**POST** `/auth/register`

#### リクエスト
```json
{
  "email": "user@example.com",
  "password": "password123",
  "displayName": "ユーザー名"
}
```

#### レスポンス (201)
```json
{
  "success": true,
  "data": {
    "user": {
      "userId": 1,
      "userCommonId": "USR001",
      "email": "user@example.com",
      "displayName": "ユーザー名",
      "createdAt": "2024-01-01T00:00:00Z"
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 3600
  },
  "message": "ユーザー登録が完了しました"
}
```

#### バリデーションエラー (400)
```json
{
  "success": false,
  "data": null,
  "message": "入力データに誤りがあります",
  "errors": [
    {
      "field": "email",
      "code": "INVALID_EMAIL",
      "message": "有効なメールアドレスを入力してください"
    },
    {
      "field": "password",
      "code": "PASSWORD_TOO_SHORT",
      "message": "パスワードは8文字以上で入力してください"
    }
  ]
}
```

### 2. ログイン

**POST** `/auth/login`

#### リクエスト
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

#### レスポンス (200)
```json
{
  "success": true,
  "data": {
    "user": {
      "userId": 1,
      "userCommonId": "USR001",
      "email": "user@example.com",
      "displayName": "ユーザー名"
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 3600
  },
  "message": "ログインしました"
}
```

### 3. トークンリフレッシュ

**POST** `/auth/refresh`

#### リクエスト
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

#### レスポンス (200)
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 3600
  }
}
```

### 4. ログアウト

**POST** `/auth/logout`

#### レスポンス (200)
```json
{
  "success": true,
  "data": null,
  "message": "ログアウトしました"
}
```

## 🏋️ トレーニングAPI

### 1. メニュー一覧取得

**GET** `/training/menus`

#### クエリパラメータ
| パラメータ | 型 | 必須 | 説明 |
|------------|----|----|------|
| `search` | string | ❌ | 検索キーワード |
| `category` | string | ❌ | カテゴリフィルタ |

#### レスポンス (200)
```json
{
  "success": true,
  "data": [
    {
      "menuId": "bench-press",
      "jpName": "ベンチプレス",
      "enName": "Bench Press",
      "description": "胸部の筋肉を鍛えるエクササイズ",
      "category": "胸筋",
      "bodyParts": ["chest", "triceps"],
      "difficulty": "intermediate",
      "equipment": ["barbell", "bench"]
    }
  ]
}
```

### 2. 記録作成

**POST** `/training/records`

#### リクエスト
```json
{
  "menuId": "bench-press",
  "trainingDate": "2024-01-01",
  "sets": [
    {
      "setNumber": 1,
      "weight": 70.0,
      "reps": 10,
      "restTime": 60,
      "memo": "調子良い"
    }
  ]
}
```

#### レスポンス (201)
```json
{
  "success": true,
  "data": {
    "recordId": 123,
    "menuId": "bench-press",
    "trainingDate": "2024-01-01",
    "sets": [
      {
        "setId": 456,
        "setNumber": 1,
        "weight": 70.0,
        "reps": 10,
        "restTime": 60,
        "memo": "調子良い",
        "createdAt": "2024-01-01T10:00:00Z"
      }
    ],
    "totalVolume": 700.0,
    "duration": 30,
    "createdAt": "2024-01-01T10:00:00Z"
  },
  "message": "記録を保存しました"
}
```

### 3. 記録履歴取得

**GET** `/training/records`

#### クエリパラメータ
| パラメータ | 型 | 必須 | 説明 |
|------------|----|----|------|
| `fromDate` | string (YYYY-MM-DD) | ❌ | 開始日 |
| `toDate` | string (YYYY-MM-DD) | ❌ | 終了日 |
| `menuId` | string | ❌ | メニューID |
| `page` | integer | ❌ | ページ番号（デフォルト: 1） |
| `limit` | integer | ❌ | 1ページの件数（デフォルト: 20） |

#### レスポンス (200)
```json
{
  "success": true,
  "data": {
    "records": [
      {
        "recordId": 123,
        "menuId": "bench-press",
        "menuName": "ベンチプレス",
        "trainingDate": "2024-01-01",
        "sets": [
          {
            "setNumber": 1,
            "weight": 70.0,
            "reps": 10,
            "restTime": 60
          }
        ],
        "totalVolume": 700.0,
        "maxWeight": 70.0,
        "totalReps": 10,
        "duration": 30,
        "createdAt": "2024-01-01T10:00:00Z"
      }
    ],
    "pagination": {
      "currentPage": 1,
      "totalPages": 5,
      "totalItems": 100,
      "hasNext": true,
      "hasPrevious": false
    }
  }
}
```

### 4. 進捗統計取得

**GET** `/training/stats`

#### クエリパラメータ
| パラメータ | 型 | 必須 | 説明 |
|------------|----|----|------|
| `period` | string | ❌ | 期間（7d, 30d, 90d, 1y） |
| `menuId` | string | ❌ | メニューID |

#### レスポンス (200)
```json
{
  "success": true,
  "data": {
    "period": "30d",
    "summary": {
      "totalWorkouts": 12,
      "totalSets": 156,
      "totalVolume": 8640.0,
      "averageWorkoutDuration": 45,
      "workoutFrequency": 0.4
    },
    "progress": [
      {
        "date": "2024-01-01",
        "workouts": 1,
        "volume": 700.0,
        "duration": 30
      }
    ],
    "personalRecords": [
      {
        "menuId": "bench-press",
        "menuName": "ベンチプレス",
        "maxWeight": 75.0,
        "maxVolume": 900.0,
        "bestSet": {
          "weight": 75.0,
          "reps": 12,
          "date": "2024-01-15"
        }
      }
    ]
  }
}
```

## 💊 サプリメントAPI

### 1. サプリメント一覧取得

**GET** `/supplement/supplements`

#### レスポンス (200)
```json
{
  "success": true,
  "data": [
    {
      "supplementId": 1,
      "supplementName": "プロテイン",
      "unit": "g",
      "description": "ホエイプロテイン",
      "isActive": true,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

### 2. サプリメント作成

**POST** `/supplement/supplements`

#### リクエスト
```json
{
  "supplementName": "プロテイン",
  "unit": "g",
  "description": "ホエイプロテイン"
}
```

#### レスポンス (201)
```json
{
  "success": true,
  "data": {
    "supplementId": 1,
    "supplementName": "プロテイン",
    "unit": "g",
    "description": "ホエイプロテイン",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "message": "サプリメントを登録しました"
}
```

### 3. 摂取記録作成

**POST** `/supplement/intake-records`

#### リクエスト
```json
{
  "supplementId": 1,
  "intakeDate": "2024-01-01",
  "intakeTime": "08:00:00",
  "amount": 30.0,
  "timingType": "morning",
  "memo": "朝食後"
}
```

#### レスポンス (201)
```json
{
  "success": true,
  "data": {
    "recordId": 123,
    "supplementId": 1,
    "supplementName": "プロテイン",
    "intakeDate": "2024-01-01",
    "intakeTime": "08:00:00",
    "amount": 30.0,
    "unit": "g",
    "timingType": "morning",
    "memo": "朝食後",
    "createdAt": "2024-01-01T08:00:00Z"
  },
  "message": "摂取記録を保存しました"
}
```

### 4. 摂取記録取得

**GET** `/supplement/intake-records`

#### クエリパラメータ
| パラメータ | 型 | 必須 | 説明 |
|------------|----|----|------|
| `supplementId` | integer | ❌ | サプリメントID |
| `fromDate` | string (YYYY-MM-DD) | ❌ | 開始日 |
| `toDate` | string (YYYY-MM-DD) | ❌ | 終了日 |

#### レスポンス (200)
```json
{
  "success": true,
  "data": [
    {
      "recordId": 123,
      "supplementId": 1,
      "supplementName": "プロテイン",
      "intakeDate": "2024-01-01",
      "intakeTime": "08:00:00",
      "amount": 30.0,
      "unit": "g",
      "timingType": "morning",
      "memo": "朝食後",
      "createdAt": "2024-01-01T08:00:00Z"
    }
  ]
}
```

### 5. スケジュール作成

**POST** `/supplement/schedules`

#### リクエスト
```json
{
  "supplementId": 1,
  "scheduledTime": "08:00:00",
  "dosage": 30.0,
  "frequency": "daily",
  "effectiveDate": "2024-01-01",
  "expirationDate": "2024-12-31",
  "isActive": true
}
```

#### レスポンス (201)
```json
{
  "success": true,
  "data": {
    "scheduleId": 456,
    "supplementId": 1,
    "supplementName": "プロテイン",
    "scheduledTime": "08:00:00",
    "dosage": 30.0,
    "unit": "g",
    "frequency": "daily",
    "effectiveDate": "2024-01-01",
    "expirationDate": "2024-12-31",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z"
  },
  "message": "スケジュールを作成しました"
}
```

### 6. 摂取統計取得

**GET** `/supplement/stats`

#### クエリパラメータ
| パラメータ | 型 | 必須 | 説明 |
|------------|----|----|------|
| `supplementId` | integer | ❌ | サプリメントID |
| `period` | string | ❌ | 期間（7d, 30d, 90d） |

#### レスポンス (200)
```json
{
  "success": true,
  "data": {
    "period": "30d",
    "summary": {
      "totalIntakes": 28,
      "totalAmount": 840.0,
      "averageDaily": 28.0,
      "adherenceRate": 93.3
    },
    "daily": [
      {
        "date": "2024-01-01",
        "totalAmount": 30.0,
        "intakeCount": 1,
        "scheduled": true,
        "completed": true
      }
    ],
    "byTiming": {
      "morning": 15,
      "afternoon": 8,
      "evening": 5
    }
  }
}
```

## 👤 ユーザーAPI

### 1. プロフィール取得

**GET** `/users/profile`

#### レスポンス (200)
```json
{
  "success": true,
  "data": {
    "userId": 1,
    "userCommonId": "USR001",
    "email": "user@example.com",
    "displayName": "ユーザー名",
    "profileImage": "https://example.com/profile.jpg",
    "preferences": {
      "timezone": "Asia/Tokyo",
      "language": "ja",
      "units": {
        "weight": "kg",
        "distance": "km"
      },
      "notifications": {
        "workoutReminders": true,
        "supplementReminders": true,
        "achievements": true
      }
    },
    "stats": {
      "totalWorkouts": 156,
      "workoutStreak": 7,
      "joinDate": "2023-01-01"
    },
    "createdAt": "2023-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

### 2. プロフィール更新

**PUT** `/users/profile`

#### リクエスト
```json
{
  "displayName": "新しいユーザー名",
  "profileImage": "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQ...",
  "preferences": {
    "timezone": "Asia/Tokyo",
    "language": "ja",
    "units": {
      "weight": "kg",
      "distance": "km"
    },
    "notifications": {
      "workoutReminders": true,
      "supplementReminders": false,
      "achievements": true
    }
  }
}
```

#### レスポンス (200)
```json
{
  "success": true,
  "data": {
    "userId": 1,
    "displayName": "新しいユーザー名",
    "profileImage": "https://example.com/new-profile.jpg",
    "preferences": {
      "timezone": "Asia/Tokyo",
      "language": "ja",
      "units": {
        "weight": "kg",
        "distance": "km"
      },
      "notifications": {
        "workoutReminders": true,
        "supplementReminders": false,
        "achievements": true
      }
    },
    "updatedAt": "2024-01-01T12:00:00Z"
  },
  "message": "プロフィールを更新しました"
}
```

### 3. FCMトークン登録

**POST** `/users/fcm-token`

#### リクエスト
```json
{
  "token": "fcm_token_string_here",
  "deviceType": "android" // or "ios"
}
```

#### レスポンス (200)
```json
{
  "success": true,
  "data": null,
  "message": "FCMトークンを登録しました"
}
```

## 📊 統計・分析API

### 1. ダッシュボード統計

**GET** `/analytics/dashboard`

#### クエリパラメータ
| パラメータ | 型 | 必須 | 説明 |
|------------|----|----|------|
| `period` | string | ❌ | 期間（7d, 30d, 90d） |

#### レスポンス (200)
```json
{
  "success": true,
  "data": {
    "period": "30d",
    "workoutSummary": {
      "totalWorkouts": 12,
      "totalTime": 540,
      "averageIntensity": 7.2,
      "workoutStreak": 5
    },
    "supplementSummary": {
      "totalSupplements": 3,
      "adherenceRate": 89.5,
      "missedDoses": 4
    },
    "achievements": [
      {
        "id": "workout_streak_7",
        "title": "7日連続ワークアウト",
        "description": "素晴らしい継続力です！",
        "earnedAt": "2024-01-15T00:00:00Z"
      }
    ],
    "trends": {
      "workoutFrequency": [
        { "date": "2024-01-01", "value": 1 },
        { "date": "2024-01-02", "value": 0 }
      ],
      "supplementAdherence": [
        { "date": "2024-01-01", "value": 100 },
        { "date": "2024-01-02", "value": 85 }
      ]
    }
  }
}
```

## ❌ エラーコード一覧

### 認証エラー
| コード | HTTPステータス | 説明 |
|--------|----------------|------|
| `INVALID_CREDENTIALS` | 401 | 認証情報が無効 |
| `EXPIRED_TOKEN` | 401 | トークンが期限切れ |
| `INVALID_TOKEN` | 401 | トークンが無効 |
| `INSUFFICIENT_PERMISSIONS` | 403 | 権限不足 |

### バリデーションエラー
| コード | HTTPステータス | 説明 |
|--------|----------------|------|
| `REQUIRED_FIELD` | 400 | 必須フィールドが未入力 |
| `INVALID_EMAIL` | 400 | メールアドレスの形式が無効 |
| `PASSWORD_TOO_SHORT` | 400 | パスワードが短すぎる |
| `INVALID_DATE_FORMAT` | 400 | 日付形式が無効 |
| `VALUE_OUT_OF_RANGE` | 400 | 値が範囲外 |

### ビジネスロジックエラー
| コード | HTTPステータス | 説明 |
|--------|----------------|------|
| `USER_ALREADY_EXISTS` | 409 | ユーザーが既に存在 |
| `RESOURCE_NOT_FOUND` | 404 | リソースが見つからない |
| `DUPLICATE_ENTRY` | 409 | 重複エントリ |
| `INVALID_OPERATION` | 422 | 無効な操作 |

### システムエラー
| コード | HTTPステータス | 説明 |
|--------|----------------|------|
| `INTERNAL_SERVER_ERROR` | 500 | 内部サーバーエラー |
| `DATABASE_ERROR` | 500 | データベースエラー |
| `EXTERNAL_SERVICE_ERROR` | 502 | 外部サービスエラー |
| `RATE_LIMIT_EXCEEDED` | 429 | レート制限超過 |

## 🔒 セキュリティ要件

### 1. 認証・認可
- JWT による認証
- リフレッシュトークンによる自動更新
- 適切なトークン有効期限設定

### 2. データ保護
- HTTPS による通信暗号化
- 機密データの適切なマスキング
- SQLインジェクション対策

### 3. レート制限
```
ログイン: 5回/分
一般API: 100回/分
データ取得API: 200回/分
```

### 4. ログ記録
- API アクセスログ
- エラーログ
- 認証ログ

## 📝 変更管理

### バージョニング
- セマンティックバージョニング（v1.0.0）
- 破壊的変更は新バージョンで提供
- 廃止予定APIは事前通知

### 変更プロセス
1. API仕様変更の提案
2. 両チーム間での合意
3. このドキュメントの更新
4. 実装・テスト
5. リリース

## 🧪 テスト要件

### 1. 単体テスト
- 各エンドポイントの正常系・異常系
- バリデーション機能
- 認証・認可機能

### 2. 統合テスト
- API エンドポイント間の連携
- データベース整合性
- 外部サービス連携

### 3. E2Eテスト
- 実際のユーザーフロー
- クロスブラウザ動作確認
- モバイルアプリ動作確認

## 📋 チェックリスト

### 開発開始前
- [ ] API仕様の完全理解
- [ ] 開発環境でのAPI動作確認
- [ ] 認証フローの動作確認
- [ ] エラーハンドリングの確認

### 開発中
- [ ] 仕様通りのリクエスト・レスポンス
- [ ] 適切なHTTPステータスコード
- [ ] エラー時の適切なレスポンス
- [ ] 認証が必要なエンドポイントの保護

### リリース前
- [ ] 全エンドポイントの動作確認
- [ ] パフォーマンステスト
- [ ] セキュリティチェック
- [ ] ドキュメント更新

---

## 🤝 合意事項

このドキュメントに記載された仕様は、バックエンドチームとフロントエンドチーム間で合意された内容です。

**バックエンドチーム責任範囲:**
- API エンドポイントの実装
- データベース設計・実装
- 認証・認可システム
- API ドキュメント維持

**フロントエンドチーム責任範囲:**
- API クライアント実装
- エラーハンドリング
- ユーザー体験の最適化
- フィードバック提供

**共同責任:**
- API仕様の維持・更新
- 統合テストの実施
- パフォーマンス最適化
- セキュリティ対策

---

**📞 連絡・質問**: API仕様に関する質問や変更要求は、このドキュメントを更新して両チーム間で合意してください。