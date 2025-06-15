// ===================================
// Core Types for Training Application
// ===================================

/**
 * API共通レスポンス型（新形式）
 * 全てのAPIレスポンスで使用される基本構造
 */
export interface ApiResponse<T> {
  /** 成功フラグ */
  isSuccess: boolean
  /** 実際のデータ */
  data: T
  /** ユーザー向けメッセージ */
  userMessage: string
  /** エラーコード (nullの場合は成功) */
  errorCode: string | null
  /** 開発者向けメッセージ */
  developerMessage: string | null
  /** タイムスタンプ */
  timestamp: string
}

/**
 * APIエラーレスポンス型
 */
export interface ApiErrorResponse {
  isSuccess: false
  errorCode: string
  userMessage: string
  developerMessage?: string
  validationErrors?: Record<string, string[]>
  debug?: {
    internalCode?: string
    innerException?: string
  }
}

// ===================================
// Training Menu Types
// ===================================

/**
 * トレーニングメニュー（APIレスポンス形式）
 */
export interface TrainingMenuApiResponse {
  MenuId: string
  JPName: string
  ENName: string
  Description: string | null
  CreatedAt: string | null
  Tags: TagReference[]
}

/**
 * タグ参照情報
 */
export interface TagReference {
  TagId: string
}

/**
 * トレーニングタグ（APIレスポンス形式）
 */
export interface TrainingTagApiResponse {
  TagId: string
  JPName: string
  ENName: string
}

/**
 * メニュー取得APIのレスポンス型
 */
export interface MenuApiResponse {
  response_menus: TrainingMenuApiResponse[]
  response_tags: TrainingTagApiResponse[]
  meta: {
    menu_count: number
    tag_count: number
    retrieved_at: string
  }
}

/**
 * トレーニングメニュー（正規化済み）
 * フロントエンド内部で使用する型
 */
export interface TrainingMenu {
  /** メニューID */
  menuId: string
  /** 日本語名 */
  jpName: string
  /** 英語名 */
  enName: string
  /** 説明 */
  description: string | null
  /** 作成日時 */
  createdAt: Date | null
  /** 関連タグIDリスト */
  tagIds: string[]
}

/**
 * トレーニングタグ（正規化済み）
 */
export interface TrainingTag {
  /** タグID */
  tagId: string
  /** 日本語名 */
  jpName: string
  /** 英語名 */
  enName: string
}

// ===================================
// Training Record Types  
// ===================================

/**
 * トレーニングレコードセット
 */
export interface TrainingRecordSet {
  /** セットID */
  setId?: string
  /** 回数 */
  reps: number
  /** 重量 (kg) */
  weight?: number
  /** 休憩時間 (秒) */
  restTime?: number
  /** メモ */
  note?: string
  /** 作成日時 */
  createdAt: Date
}

/**
 * 日次トレーニングレコード
 */
export interface DailyTrainingRecord {
  /** レコードID */
  recordId: string
  /** ユーザー共通ID */
  userCommonId: string
  /** メニューID */
  menuId: string
  /** トレーニング日 */
  trainingDate: string
  /** セット数 */
  setCount: number
  /** 最大回数 */
  maxReps: number
  /** 最大回数時の重量 */
  maxRepsWeight?: number
  /** 最大重量 */
  maxWeight?: number
  /** 最大重量時の回数 */
  maxWeightReps?: number
  /** 総負荷量 */
  totalLoadAmount: number
  /** 総回数 */
  totalReps: number
  /** 作成日時 */
  createdAt: Date
  /** 更新日時 */
  updatedAt: Date
}

/**
 * トレーニング履歴の取得パラメータ
 */
export interface TrainingHistoryParams {
  /** メニューID */
  menuId: string
  /** 開始日 */
  startDate?: string
  /** 終了日 */
  endDate?: string
  /** 取得件数上限 */
  limit?: number
}

// ===================================
// User Types
// ===================================

/**
 * ユーザー情報
 */
export interface User {
  /** ユーザー共通ID */
  userCommonId: string
  /** ログインID */
  loginId: string
  /** 表示名 */
  displayName: string
  /** 作成日時 */
  createdAt: Date
  /** 更新日時 */
  updatedAt: Date
}

/**
 * ユーザー登録パラメータ
 */
export interface UserRegistrationParams {
  /** ログインID */
  loginId: string
  /** パスワード */
  password: string
  /** 表示名 */
  displayName: string
}

/**
 * ログインパラメータ
 */
export interface LoginParams {
  /** ログインID */
  loginId: string
  /** パスワード */
  password: string
}

/**
 * 認証レスポンス
 */
export interface AuthResponse {
  /** アクセストークン */
  accessToken: string
  /** トークンタイプ (通常は "Bearer") */
  tokenType: string
  /** 有効期限 (秒) */
  expiresIn: number
  /** ユーザー情報 */
  user: User
}

// ===================================
// UI State Types
// ===================================

/**
 * ローディング状態
 */
export interface LoadingState {
  /** ローディング中かどうか */
  isLoading: boolean
  /** ローディングメッセージ */
  message?: string
}

/**
 * エラー状態
 */
export interface ErrorState {
  /** エラーが発生しているかどうか */
  hasError: boolean
  /** エラーメッセージ */
  message: string
  /** エラーコード */
  code?: string
  /** エラーの詳細情報 */
  details?: any
  /** リトライ可能かどうか */
  retryable: boolean
}

/**
 * ページネーション情報
 */
export interface PaginationInfo {
  /** 現在のページ */
  currentPage: number
  /** 1ページあたりの件数 */
  perPage: number
  /** 総件数 */
  totalCount: number
  /** 総ページ数 */
  totalPages: number
  /** 前のページがあるかどうか */
  hasPrevious: boolean
  /** 次のページがあるかどうか */
  hasNext: boolean
}

/**
 * フィルター条件
 */
export interface FilterConditions {
  /** 検索キーワード */
  keyword?: string
  /** タグIDリスト */
  tagIds?: string[]
  /** 日付範囲開始 */
  dateFrom?: string
  /** 日付範囲終了 */
  dateTo?: string
  /** ソート項目 */
  sortBy?: string
  /** ソート順序 (asc | desc) */
  sortOrder?: 'asc' | 'desc'
}

// ===================================
// Component Props Types
// ===================================

/**
 * トレーニングカードのプロパティ
 */
export interface TrainingCardProps {
  /** トレーニングメニュー */
  menu: TrainingMenu
  /** クリック可能かどうか */
  clickable?: boolean
  /** 表示サイズ */
  size?: 'small' | 'medium' | 'large'
  /** 追加のCSSクラス */
  class?: string
}

/**
 * ダッシュボード統計データ
 */
export interface DashboardStats {
  /** 今週のトレーニング日数 */
  weeklyTrainingDays: number
  /** 今月のトレーニング日数 */
  monthlyTrainingDays: number
  /** 総トレーニング日数 */
  totalTrainingDays: number
  /** 今週の総負荷量 */
  weeklyTotalLoad: number
  /** 先週からの変化率 */
  weeklyLoadChangePercent: number
  /** 最近のパーソナルレコード */
  recentPersonalRecords: PersonalRecord[]
  /** 最近のアクティビティ */
  recentActivities: RecentActivity[]
}

/**
 * パーソナルレコード
 */
export interface PersonalRecord {
  /** メニューID */
  menuId: string
  /** メニュー名 */
  menuName: string
  /** 記録の種類 (max_weight | max_reps | total_load) */
  recordType: 'max_weight' | 'max_reps' | 'total_load'
  /** 記録値 */
  value: number
  /** 記録日 */
  achievedAt: Date
  /** 以前の記録からの向上 */
  improvement: number
}

/**
 * 最近のアクティビティ
 */
export interface RecentActivity {
  /** アクティビティID */
  activityId: string
  /** アクティビティタイプ */
  type: 'training_completed' | 'personal_record' | 'goal_achieved'
  /** メニューID */
  menuId: string
  /** メニュー名 */
  menuName: string
  /** 説明 */
  description: string
  /** 発生日時 */
  occurredAt: Date
}

// ===================================
// Utility Types
// ===================================

/**
 * リクエストオプション
 */
export interface RequestOptions {
  /** タイムアウト時間 (ミリ秒) */
  timeout?: number
  /** リトライ回数 */
  retries?: number
  /** キャッシュを使用するかどうか */
  useCache?: boolean
  /** キャッシュ有効期限 (秒) */
  cacheLifetime?: number
}

/**
 * キャッシュエントリ
 */
export interface CacheEntry<T> {
  /** キャッシュデータ */
  data: T
  /** 有効期限 */
  expiresAt: Date
  /** 作成日時 */
  createdAt: Date
}

/**
 * 通知メッセージ
 */
export interface NotificationMessage {
  /** メッセージID */
  id: string
  /** メッセージタイプ */
  type: 'success' | 'info' | 'warning' | 'error'
  /** タイトル */
  title: string
  /** メッセージ内容 */
  message: string
  /** 自動消去時間 (ミリ秒、0で手動消去のみ) */
  autoClose?: number
  /** アクションボタン */
  actions?: NotificationAction[]
}

/**
 * 通知アクション
 */
export interface NotificationAction {
  /** アクションラベル */
  label: string
  /** クリック時のハンドラ */
  handler: () => void
  /** ボタンスタイル */
  style?: 'primary' | 'secondary' | 'danger'
}