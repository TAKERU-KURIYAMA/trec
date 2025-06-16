# Message トレーニング管理システム設計書

## 1. システム概要

### 1.1 プロジェクト概要
「Message」は、トレーニング・ワークアウトを記録・管理するための総合的なフィットネス管理システムです。

### 1.2 技術スタック
- **フロントエンド**: Vue 3 + Nuxt 3 + TypeScript
- **バックエンド**: ASP.NET Core 8.0 + Entity Framework Core
- **モバイルアプリ**: React Native + TypeScript
- **データベース**: PostgreSQL 15
- **認証**: JWT (JSON Web Token)
- **コンテナ**: Docker + Docker Compose
- **CI/CD**: GitHub Actions

## 2. 機能一覧

### 2.1 認証・ユーザー管理
- ユーザー登録
- ログイン/ログアウト
- JWTトークンベース認証
- パスワードハッシュ化（BCrypt）
- トークン自動更新
- セキュアストレージ（暗号化）

### 2.2 トレーニングメニュー管理
- メニュー一覧表示
- タグによる分類（部位別、難易度別など）
- メニュー検索・フィルタリング
- お気に入り機能
- メニュー詳細表示

### 2.3 トレーニング記録
- ワークアウトセッション開始
- セット・レップス・重量の記録
- リアルタイムタイマー
- 休憩タイマー
- セッション完了・保存
- 記録の編集・削除

### 2.4 トレーニング履歴
- 履歴一覧表示
- 日付範囲フィルタリング
- メニュー別フィルタリング
- 当日記録の編集
- 過去記録の詳細表示
- 統計情報表示

### 2.5 プリセット管理
- トレーニングプリセット作成
- プリセット一覧表示
- プリセット編集・削除
- デフォルトセット設定
- プリセットからのワークアウト開始

### 2.6 ダッシュボード・分析
- トレーニング統計表示
- 進捗グラフ
- パーソナルレコード
- 最近のアクティビティ
- カレンダービュー
- スマートレコメンデーション

### 2.7 通知・フィードバック
- トースト通知
- ワークアウト通知
- 音声フィードバック（モバイル）
- 振動フィードバック（モバイル）
- 達成バッジ

### 2.8 データ管理
- オフラインデータ永続化
- 自動同期
- データエクスポート
- バックアップ機能

## 3. システムアーキテクチャ

```mermaid
graph TB
    subgraph "Client Layer"
        A[Web Frontend<br/>Vue.js/Nuxt.js]
        B[Mobile App<br/>React Native]
    end
    
    subgraph "API Gateway"
        C[Nginx<br/>Reverse Proxy]
    end
    
    subgraph "Application Layer"
        D[ASP.NET Core API<br/>RESTful Services]
        E[Authentication Service<br/>JWT]
    end
    
    subgraph "Data Layer"
        F[PostgreSQL<br/>Database]
        G[Redis Cache<br/>Optional]
    end
    
    subgraph "External Services"
        H[Docker Registry]
        I[Monitoring<br/>Prometheus/Grafana]
    end
    
    A --> C
    B --> C
    C --> D
    D --> E
    D --> F
    D --> G
    D --> I
```

## 4. データベース設計（ER図）

```mermaid
erDiagram
    Users ||--o{ UserTokens : has
    Users ||--o{ UserData : has
    Users ||--o{ TrainingRecordSets : creates
    Users ||--o{ DailyTrainingRecords : has
    
    TrainingMenus ||--o{ TrainingTags : has
    TrainingMenus ||--o{ TrainingRecordSets : uses
    TrainingMenus ||--o{ DailyTrainingRecords : uses
    
    TagMasters ||--o{ TrainingTags : defines
    
    Users {
        string UserCommonId PK
        string UserId UK
        string Email UK
        string PasswordHash
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    UserTokens {
        string UserCommonId FK
        string Token
        datetime ExpiresAt
        datetime CreatedAt
    }
    
    TrainingMenus {
        string MenuId PK
        string JPName
        string ENName
        string Description
        datetime CreatedAt
    }
    
    TagMasters {
        string TagId PK
        string JPName
        string ENName
    }
    
    TrainingTags {
        string MenuId FK
        string TagId FK
        string JPName
        string ENName
    }
    
    TrainingRecordSets {
        int RecordId PK
        string UserCommonId FK
        string MenuId FK
        date TrainingDate
        int SetNumber
        int Reps
        decimal Weight
        datetime CreatedAt
    }
    
    DailyTrainingRecords {
        string UserCommonId FK
        string MenuId FK
        date TrainingDate
        int SetCount
        int TotalReps
        int MaxReps
        decimal MaxWeight
        decimal MaxRepsWeight
        int MaxWeightReps
        decimal TotalLoadAmount
        datetime CreatedAt
        datetime UpdatedAt
    }
```

## 5. API設計

### 5.1 認証エンドポイント

| メソッド | エンドポイント | 説明 | 認証 |
|---------|---------------|------|------|
| POST | /api/auth/register | ユーザー登録 | 不要 |
| POST | /api/auth/login | ログイン | 不要 |
| POST | /api/auth/refresh | トークン更新 | 必要 |
| POST | /api/auth/logout | ログアウト | 必要 |

### 5.2 トレーニングエンドポイント

| メソッド | エンドポイント | 説明 | 認証 |
|---------|---------------|------|------|
| GET | /api/training/menu | メニュー・タグ一覧取得 | 不要 |
| GET | /api/training/menu/isolation | アイソレーション種目取得 | 不要 |
| POST | /api/training/record | トレーニング記録登録 | 必要 |
| GET | /api/training/history | トレーニング履歴取得 | 必要 |
| GET | /api/training/history/details | 詳細記録取得 | 必要 |
| GET | /api/training/presets | プリセット一覧取得 | 必要 |
| POST | /api/training/presets | プリセット作成 | 必要 |
| DELETE | /api/training/presets/{id} | プリセット削除 | 必要 |
| GET | /api/training/schedules | スケジュール取得 | 必要 |
| POST | /api/training/schedules | スケジュール作成 | 必要 |

### 5.3 管理者エンドポイント

| メソッド | エンドポイント | 説明 | 認証 |
|---------|---------------|------|------|
| POST | /api/admin/users/create | ユーザー作成 | Admin |
| GET | /api/admin/users | ユーザー一覧 | Admin |
| PUT | /api/admin/users/{id} | ユーザー更新 | Admin |
| DELETE | /api/admin/users/{id} | ユーザー削除 | Admin |

### 5.4 レスポンス形式

```typescript
// 成功レスポンス
{
  "success": true,
  "data": {
    // レスポンスデータ
  },
  "message": "成功メッセージ",
  "timestamp": "2024-01-15T10:00:00Z"
}

// エラーレスポンス
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "エラーメッセージ",
    "details": {
      // 詳細情報
    }
  },
  "timestamp": "2024-01-15T10:00:00Z"
}
```

## 6. 画面フロー図

```mermaid
graph TD
    A[スプラッシュ画面] --> B{認証状態}
    B -->|未認証| C[ホーム画面<br/>ゲストモード]
    B -->|認証済| D[ホーム画面<br/>認証モード]
    
    C --> E[ログインモーダル]
    C --> F[登録モーダル]
    E --> D
    F --> D
    
    D --> G[メニュー選択]
    G --> H[ワークアウトセッション]
    H --> I[記録保存]
    I --> D
    
    D --> J[履歴画面]
    J --> K[履歴詳細]
    K --> L[記録編集]
    
    D --> M[ダッシュボード]
    M --> N[統計詳細]
    
    D --> O[プリセット画面]
    O --> P[プリセット作成]
    O --> Q[プリセット編集]
    
    D --> R[設定画面]
    R --> S[プロフィール編集]
    R --> T[ログアウト]
    T --> C
```

## 7. シーケンス図

### 7.1 ログインフロー

```mermaid
sequenceDiagram
    participant U as ユーザー
    participant F as フロントエンド
    participant A as API
    participant D as Database
    participant S as SecureStorage
    
    U->>F: ログイン画面表示
    F->>U: メール・パスワード入力
    U->>F: ログインボタンクリック
    F->>A: POST /api/auth/login
    A->>D: ユーザー検証
    D-->>A: ユーザー情報
    A->>A: パスワード検証
    A->>A: JWT生成
    A-->>F: トークン・ユーザー情報
    F->>S: トークン暗号化保存
    F->>F: 認証状態更新
    F->>U: ホーム画面遷移
```

### 7.2 トレーニング記録フロー

```mermaid
sequenceDiagram
    participant U as ユーザー
    participant M as モバイルアプリ
    participant S as Store
    participant A as API
    participant D as Database
    
    U->>M: メニュー選択
    M->>S: startWorkoutSession()
    S->>S: セッション初期化
    S-->>M: セッションID
    M->>U: ワークアウト画面表示
    
    loop 各セット
        U->>M: セット入力
        M->>S: updateCurrentSession()
        S->>S: セット追加
    end
    
    U->>M: 完了ボタン
    M->>S: completeCurrentSession()
    S->>A: POST /api/training/record
    A->>D: レコード保存
    A->>D: 日次記録更新
    D-->>A: 保存完了
    A-->>S: レスポンス
    S-->>M: 完了通知
    M->>U: 完了画面表示
```

### 7.3 履歴取得・編集フロー

```mermaid
sequenceDiagram
    participant U as ユーザー
    participant F as フロントエンド
    participant A as API
    participant D as Database
    
    U->>F: 履歴画面アクセス
    F->>A: GET /api/training/history
    A->>D: 履歴クエリ
    D-->>A: 履歴データ
    A-->>F: 履歴レスポンス
    F->>U: 履歴一覧表示
    
    U->>F: 記録選択
    F->>A: GET /api/training/history/details
    A->>D: 詳細クエリ
    D-->>A: セット詳細
    A-->>F: 詳細レスポンス
    F->>U: 詳細表示
    
    alt 当日記録の場合
        U->>F: 編集ボタン
        F->>U: 編集モード
        U->>F: セット編集
        F->>A: POST /api/training/record
        A->>D: 記録更新
        D-->>A: 更新完了
        A-->>F: 成功レスポンス
        F->>U: 更新完了通知
    end
```

## 8. セキュリティ設計

### 8.1 認証・認可
- JWT Bearer Token認証
- トークン有効期限: 7日間
- リフレッシュトークン実装
- Role-based Access Control (RBAC)

### 8.2 データ保護
- パスワード: BCryptハッシュ化
- API通信: HTTPS必須
- ローカルストレージ: XOR暗号化
- SQLインジェクション対策: パラメータ化クエリ

### 8.3 セキュリティヘッダー
```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Content-Security-Policy: default-src 'self'
```

## 9. パフォーマンス最適化

### 9.1 フロントエンド
- コード分割（Code Splitting）
- 遅延ローディング（Lazy Loading）
- 画像最適化（WebP対応）
- Service Worker（PWA）
- APIレスポンスキャッシュ

### 9.2 バックエンド
- Entity Framework クエリ最適化
- 非同期処理
- レスポンス圧縮（Gzip）
- データベースインデックス最適化
- Redis キャッシュ（オプション）

### 9.3 モバイルアプリ
- React Native Hermes エンジン
- 画像キャッシュ
- オフラインファースト設計
- バッチAPI呼び出し
- メモリ管理最適化

## 10. 運用・監視

### 10.1 ロギング
- 構造化ログ（JSON形式）
- ログレベル管理
- リクエスト/レスポンスログ
- エラートラッキング
- ビジネスイベントログ

### 10.2 モニタリング
- Prometheus + Grafana
- ヘルスチェックエンドポイント
- パフォーマンスメトリクス
- エラー率監視
- データベース接続監視

### 10.3 バックアップ
- 日次自動バックアップ
- S3へのバックアップ転送
- 保持期間: 30日
- ポイントインタイムリカバリ

## 11. 開発環境

### 11.1 必要な環境
- Node.js 18+
- .NET SDK 8.0+
- PostgreSQL 15+
- Docker Desktop
- Git

### 11.2 開発フロー
1. Feature Branch作成
2. ローカル開発・テスト
3. Pull Request作成
4. コードレビュー
5. CI/CDパイプライン実行
6. マージ・自動デプロイ

### 11.3 コーディング規約
- TypeScript: ESLint + Prettier
- C#: .NET コーディング規約
- コミットメッセージ: Conventional Commits
- ブランチ命名: feature/*, bugfix/*, hotfix/*

## 12. 今後の拡張予定

### Phase 2
- ソーシャル機能（フォロー、共有）
- AIトレーニングアドバイザー
- 栄養管理機能
- Apple Watch/Wear OS連携

### Phase 3
- マルチ言語対応
- チーム/ジム管理機能
- オンラインコーチング
- 動画分析機能