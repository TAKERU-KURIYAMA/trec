// ===================================
// Enhanced TypeScript Types for React Native
// Comprehensive type definitions with React Native optimizations
// ===================================

/**
 * API共通レスポンス型
 */
export interface ApiResponse<T = any> {
  code: string
  message: string
  result?: T
}

/**
 * APIエラーレスポンス型
 */
export interface ApiErrorResponse {
  code: string
  message: string
  validationErrors?: Record<string, string[]>
  debug?: {
    internalCode?: string
    innerException?: string
  }
}

// ===================================
// Training Types (Enhanced)
// ===================================

/**
 * トレーニングメニュー（拡張版）
 */
export interface TrainingMenu {
  menuId: string
  jpName: string
  enName: string
  description?: string
  targetAreas: string[]
  difficulty: 'beginner' | 'intermediate' | 'advanced'
  equipment?: string[]
  estimatedDuration: number // 分
  caloriesPerMinute?: number
  instructions?: string[]
  videoUrl?: string
  imageUrl?: string
  tags: string[]
  popularity: number
  rating: number
  createdAt: Date
  updatedAt: Date
  // React Native specific
  isFavorite?: boolean
  lastPerformed?: Date
  personalBest?: PersonalBest
}

/**
 * パーソナルベスト
 */
export interface PersonalBest {
  maxWeight?: number
  maxReps?: number
  bestVolume?: number
  achievedAt: Date
}

/**
 * トレーニングセット（拡張版）
 */
export interface TrainingSet {
  setId: string
  setNumber: number
  reps: number
  weight?: number
  duration?: number // 秒（時間系の場合）
  distance?: number // メートル（有酸素の場合）
  restTime?: number // 秒
  completed: boolean
  rpe?: number // RPE (Rate of Perceived Exertion) 1-10
  notes?: string
  recordedAt: Date
}

/**
 * トレーニングレコード（拡張版）
 */
export interface TrainingRecord {
  recordId: string
  menuId: string
  userId: string
  sessionId: string
  date: string
  sets: TrainingSet[]
  totalVolume: number
  totalDuration: number
  averageRpe?: number
  sessionNotes?: string
  weather?: string
  mood?: 'great' | 'good' | 'okay' | 'tired' | 'bad'
  location?: string
  createdAt: Date
  updatedAt: Date
  // React Native specific
  isOfflineRecord?: boolean
  syncStatus?: 'synced' | 'pending' | 'failed'
}

/**
 * 日次集計レコード
 */
export interface DailyRecord {
  date: string
  records: TrainingRecord[]
  totalSets: number
  totalVolume: number
  totalDuration: number
  muscleGroupsWorked: string[]
  calories?: number
  averageMood?: string
  notes?: string
}

/**
 * ダッシュボード統計（拡張版）
 */
export interface DashboardStats {
  // 基本統計
  totalWorkouts: number
  currentStreak: number
  longestStreak: number
  totalVolume: number
  totalDuration: number
  favoriteExercise: string
  
  // 期間別統計
  weeklyProgress: WeeklyProgress[]
  monthlyProgress: MonthlyProgress[]
  yearlyProgress: YearlyProgress[]
  
  // パフォーマンス指標
  strengthProgress: StrengthProgress[]
  volumeProgress: VolumeProgress[]
  consistencyScore: number
  
  // 目標達成状況
  goals: Goal[]
  achievements: Achievement[]
  
  // React Native specific
  lastSyncAt?: Date
  offlineRecordsCount?: number
}

/**
 * 週間進捗
 */
export interface WeeklyProgress {
  weekStart: string
  weekEnd: string
  workouts: number
  volume: number
  duration: number
  muscleGroups: string[]
  consistency: number // 0-1
}

/**
 * 月間進捗
 */
export interface MonthlyProgress {
  month: string
  year: number
  workouts: number
  volume: number
  averageSessionDuration: number
  topExercises: Array<{ menuId: string; sessions: number }>
}

/**
 * 年間進捗
 */
export interface YearlyProgress {
  year: number
  totalWorkouts: number
  totalVolume: number
  totalHours: number
  monthlyBreakdown: MonthlyProgress[]
}

/**
 * 筋力進捗
 */
export interface StrengthProgress {
  menuId: string
  menuName: string
  progressData: Array<{
    date: string
    maxWeight: number
    maxReps: number
    volume: number
  }>
  trendDirection: 'up' | 'down' | 'stable'
  improvementPercent: number
}

/**
 * ボリューム進捗
 */
export interface VolumeProgress {
  date: string
  totalVolume: number
  workoutCount: number
  averageIntensity: number
}

// ===================================
// Goal & Achievement Types
// ===================================

/**
 * 目標
 */
export interface Goal {
  goalId: string
  type: 'weight' | 'reps' | 'volume' | 'frequency' | 'duration'
  title: string
  description: string
  targetValue: number
  currentValue: number
  unit: string
  deadline?: Date
  menuId?: string
  status: 'active' | 'completed' | 'paused' | 'failed'
  progress: number // 0-1
  createdAt: Date
  completedAt?: Date
}

/**
 * 実績・達成
 */
export interface Achievement {
  achievementId: string
  title: string
  description: string
  icon: string
  category: 'consistency' | 'strength' | 'volume' | 'milestone'
  rarity: 'common' | 'rare' | 'epic' | 'legendary'
  unlockedAt: Date
  progress?: number // 0-1 for progress-based achievements
}

// ===================================
// User & Auth Types
// ===================================

/**
 * ユーザープロファイル
 */
export interface UserProfile {
  userId: string
  email: string
  username: string
  displayName: string
  avatar?: string
  birthDate?: Date
  gender?: 'male' | 'female' | 'other'
  height?: number // cm
  weight?: number // kg
  fitnessLevel: 'beginner' | 'intermediate' | 'advanced'
  goals: string[]
  preferences: UserPreferences
  stats: UserStats
  createdAt: Date
  updatedAt: Date
}

/**
 * ユーザー設定
 */
export interface UserPreferences {
  units: 'metric' | 'imperial'
  language: 'ja' | 'en'
  notifications: NotificationSettings
  privacy: PrivacySettings
  theme: 'light' | 'dark' | 'auto'
  defaultRestTime: number // 秒
  workoutReminders: boolean
  dataSync: boolean
}

/**
 * 通知設定
 */
export interface NotificationSettings {
  workoutReminders: boolean
  goalDeadlines: boolean
  achievements: boolean
  weeklyReports: boolean
  socialUpdates: boolean
}

/**
 * プライバシー設定
 */
export interface PrivacySettings {
  profileVisibility: 'public' | 'friends' | 'private'
  shareWorkouts: boolean
  shareProgress: boolean
  allowFriendRequests: boolean
}

/**
 * ユーザー統計
 */
export interface UserStats {
  totalWorkouts: number
  totalVolume: number
  totalHours: number
  currentStreak: number
  longestStreak: number
  favoriteExercises: string[]
  strengthLevel: number // 1-100
  enduranceLevel: number // 1-100
  consistencyScore: number // 0-1
}

// ===================================
// UI State Types
// ===================================

/**
 * ローディング状態
 */
export interface LoadingState {
  isLoading: boolean
  message?: string
  progress?: number // 0-1 for progress bars
}

/**
 * エラー状態
 */
export interface ErrorState {
  hasError: boolean
  message: string
  code?: string
  retryable: boolean
  timestamp: Date
  // React Native specific
  showToast?: boolean
  toastDuration?: number
}

/**
 * ネットワーク状態
 */
export interface NetworkState {
  isConnected: boolean
  isInternetReachable: boolean
  type: 'wifi' | 'cellular' | 'unknown'
  isExpensive?: boolean
}

/**
 * 同期状態
 */
export interface SyncState {
  issyncing: boolean
  lastSyncAt?: Date
  pendingChanges: number
  failedSync: boolean
  syncProgress?: number // 0-1
}

// ===================================
// Navigation Types
// ===================================

/**
 * ナビゲーションパラメータ
 */
export type RootStackParamList = {
  Home: undefined
  TrainingDetail: { menuId: string }
  WorkoutSession: { menuId: string; sessionId?: string }
  History: { menuId?: string }
  Dashboard: undefined
  Profile: undefined
  Settings: undefined
  Goals: undefined
  Achievements: undefined
  OnboardingStack: undefined
}

export type TabParamList = {
  HomeTab: undefined
  WorkoutTab: undefined
  HistoryTab: undefined
  DashboardTab: undefined
  ProfileTab: undefined
}

// ===================================
// Component Props Types
// ===================================

/**
 * トレーニングカードのプロパティ
 */
export interface TrainingCardProps {
  menu: TrainingMenu
  onPress: (menu: TrainingMenu) => void
  onFavoritePress?: (menu: TrainingMenu) => void
  size?: 'small' | 'medium' | 'large'
  showStats?: boolean
  disabled?: boolean
}

/**
 * セット入力コンポーネントのプロパティ
 */
export interface SetInputProps {
  set: TrainingSet
  setNumber: number
  onSetChange: (set: TrainingSet) => void
  onSetComplete: (set: TrainingSet) => void
  previousSet?: TrainingSet
  isActive: boolean
  disabled?: boolean
}

// ===================================
// Chart & Analytics Types
// ===================================

/**
 * チャートデータポイント
 */
export interface ChartDataPoint {
  x: string | number
  y: number
  label?: string
  color?: string
}

/**
 * プログレスチャートデータ
 */
export interface ProgressChartData {
  title: string
  data: ChartDataPoint[]
  type: 'line' | 'bar' | 'area'
  color: string
  unit: string
  period: 'week' | 'month' | 'year'
  trend: 'up' | 'down' | 'stable'
  trendPercent: number
}

// ===================================
// Workout Session Types
// ===================================

/**
 * ワークアウトセッション
 */
export interface WorkoutSession {
  sessionId: string
  menuId: string
  userId: string
  status: 'planned' | 'active' | 'paused' | 'completed' | 'cancelled'
  startTime?: Date
  endTime?: Date
  currentSetIndex: number
  plannedSets: TrainingSet[]
  completedSets: TrainingSet[]
  notes?: string
  restStartTime?: Date
  restDuration?: number
  isRestActive: boolean
  // React Native specific
  timerState: TimerState
  backgroundTime?: Date
}

/**
 * タイマー状態
 */
export interface TimerState {
  isRunning: boolean
  startTime?: Date
  elapsedTime: number
  targetTime?: number
  type: 'workout' | 'rest' | 'preparation'
}

// ===================================
// Cache & Storage Types
// ===================================

/**
 * キャッシュエントリ
 */
export interface CacheEntry<T> {
  data: T
  timestamp: Date
  expiresAt: Date
  version: number
}

/**
 * オフラインキュー項目
 */
export interface OfflineQueueItem {
  id: string
  action: 'create' | 'update' | 'delete'
  endpoint: string
  data?: any
  timestamp: Date
  retryCount: number
  maxRetries: number
}

// ===================================
// Device & Platform Types
// ===================================

/**
 * デバイス情報
 */
export interface DeviceInfo {
  platform: 'ios' | 'android'
  version: string
  model: string
  screenWidth: number
  screenHeight: number
  isTablet: boolean
  hasNotch: boolean
  safeAreaInsets: {
    top: number
    bottom: number
    left: number
    right: number
  }
}

/**
 * 権限状態
 */
export interface PermissionState {
  camera: 'granted' | 'denied' | 'undetermined'
  notifications: 'granted' | 'denied' | 'undetermined'
  location: 'granted' | 'denied' | 'undetermined'
  storage: 'granted' | 'denied' | 'undetermined'
}

// ===================================
// Utility Types
// ===================================

/**
 * ページネーション情報
 */
export interface PaginationInfo {
  page: number
  limit: number
  total: number
  hasMore: boolean
}

/**
 * フィルター条件
 */
export interface FilterOptions {
  search?: string
  tags?: string[]
  difficulty?: string[]
  targetAreas?: string[]
  equipment?: string[]
  duration?: {
    min: number
    max: number
  }
  sortBy?: 'name' | 'popularity' | 'difficulty' | 'duration' | 'recent'
  sortOrder?: 'asc' | 'desc'
}

/**
 * 設定オプション
 */
export interface ConfigOptions {
  apiTimeout: number
  cacheLifetime: number
  maxRetries: number
  syncInterval: number
  backgroundSyncEnabled: boolean
  crashReportingEnabled: boolean
  analyticsEnabled: boolean
}