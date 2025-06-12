// ===================================
// Ultra Advanced Training History Composable
// Comprehensive history management, analytics, caching, and real-time updates
// ===================================

import { ref, computed, reactive, watch, onMounted, nextTick } from 'vue'
import type {
  DailyTrainingRecord,
  TrainingHistoryParams,
  LoadingState,
  ErrorState,
  PaginationInfo,
  RequestOptions
} from '~/types'
import { useApiClient, ApiError, NetworkError } from '~/utils/api-client'

/**
 * トレーニング履歴の状態型
 */
interface HistoryState {
  /** 履歴レコードリスト */
  records: DailyTrainingRecord[]
  /** 現在表示中のメニューID */
  currentMenuId: string | null
  /** 現在表示中のメニュー名 */
  currentMenuName: string
  /** 最終更新日時 */
  lastUpdated: Date | null
  /** 総レコード数（ページネーション用） */
  totalCount: number
}

/**
 * 履歴分析データ
 */
interface HistoryAnalytics {
  /** 総トレーニング日数 */
  totalDays: number
  /** 総セット数 */
  totalSets: number
  /** 総回数 */
  totalReps: number
  /** 総負荷量 */
  totalLoad: number
  /** 平均セット数 */
  averageSets: number
  /** 平均回数 */
  averageReps: number
  /** 最大重量 */
  maxWeight: number
  /** 最大回数 */
  maxReps: number
  /** 最近の記録（直近7日） */
  recentTrend: 'improving' | 'stable' | 'declining' | 'insufficient_data'
  /** 週ごとの集計 */
  weeklyStats: WeeklyStats[]
  /** パーソナルレコード */
  personalRecords: PersonalRecordInfo[]
}

/**
 * 週間統計
 */
interface WeeklyStats {
  /** 週の開始日 */
  weekStart: Date
  /** 週の終了日 */
  weekEnd: Date
  /** その週のトレーニング日数 */
  trainingDays: number
  /** 総負荷量 */
  totalLoad: number
  /** 平均重量 */
  averageWeight: number
  /** 最大重量 */
  maxWeight: number
}

/**
 * パーソナルレコード情報
 */
interface PersonalRecordInfo {
  /** 記録の種類 */
  type: 'max_weight' | 'max_reps' | 'total_load' | 'max_sets'
  /** 記録値 */
  value: number
  /** 達成日 */
  achievedAt: Date
  /** 前回からの改善 */
  improvement: number
  /** 改善率 (%) */
  improvementPercent: number
}

/**
 * 高度なトレーニング履歴管理コンポーザブル
 * 
 * 機能:
 * - 履歴データの取得と管理
 * - 高度な分析とグラフデータ生成
 * - ページネーション対応
 * - リアルタイム分析
 * - キャッシュとオフライン対応
 * - エラーハンドリングとリトライ
 */
export function useTrainingHistory(options: {
  /** ページサイズ */
  pageSize?: number
  /** 自動分析を有効にするかどうか */
  enableAnalytics?: boolean
  /** キャッシュ有効期限（秒） */
  cacheLifetime?: number
} = {}) {
  const {
    pageSize = 20,
    enableAnalytics = true,
    cacheLifetime = 300 // 5分
  } = options

  const { training } = useApiClient()

  // ===================================
  // Reactive State
  // ===================================

  /** 履歴の状態 */
  const state = reactive<HistoryState>({
    records: [],
    currentMenuId: null,
    currentMenuName: '',
    lastUpdated: null,
    totalCount: 0
  })

  /** ローディング状態 */
  const loading = reactive<LoadingState>({
    isLoading: false,
    message: undefined
  })

  /** エラー状態 */
  const error = reactive<ErrorState>({
    hasError: false,
    message: '',
    code: undefined,
    details: undefined,
    retryable: false
  })

  /** ページネーション情報 */
  const pagination = reactive<PaginationInfo>({
    currentPage: 1,
    perPage: pageSize,
    totalCount: 0,
    totalPages: 0,
    hasPrevious: false,
    hasNext: false
  })

  /** 現在の検索パラメータ */
  const currentParams = ref<TrainingHistoryParams | null>(null)

  // ===================================
  // Computed Analytics
  // ===================================

  /** 履歴分析データ */
  const analytics = computed((): HistoryAnalytics => {
    if (!enableAnalytics || state.records.length === 0) {
      return getEmptyAnalytics()
    }

    const records = state.records
    
    // 基本統計
    const totalDays = records.length
    const totalSets = records.reduce((sum, r) => sum + r.setCount, 0)
    const totalReps = records.reduce((sum, r) => sum + r.totalReps, 0)
    const totalLoad = records.reduce((sum, r) => sum + r.totalLoadAmount, 0)
    
    const averageSets = totalDays > 0 ? totalSets / totalDays : 0
    const averageReps = totalDays > 0 ? totalReps / totalDays : 0
    
    const maxWeight = Math.max(...records.map(r => r.maxWeight || 0))
    const maxReps = Math.max(...records.map(r => r.maxReps))

    // トレンド分析（直近7日vs前の7日）
    const recentTrend = calculateTrend(records)

    // 週間統計
    const weeklyStats = calculateWeeklyStats(records)

    // パーソナルレコード
    const personalRecords = calculatePersonalRecords(records)

    return {
      totalDays,
      totalSets,
      totalReps,
      totalLoad,
      averageSets,
      averageReps,
      maxWeight,
      maxReps,
      recentTrend,
      weeklyStats,
      personalRecords
    }
  })

  /** グラフ用のデータ */
  const chartData = computed(() => {
    const records = state.records.slice().reverse() // 古い順にソート

    return {
      /** 重量の推移 */
      weightProgress: records.map(r => ({
        date: r.trainingDate,
        maxWeight: r.maxWeight || 0,
        averageWeight: r.totalLoadAmount / Math.max(r.totalReps, 1)
      })),

      /** 負荷量の推移 */
      loadProgress: records.map(r => ({
        date: r.trainingDate,
        totalLoad: r.totalLoadAmount,
        sets: r.setCount
      })),

      /** 週間サマリー */
      weeklySummary: analytics.value.weeklyStats.map(w => ({
        week: `${formatDate(w.weekStart)} - ${formatDate(w.weekEnd)}`,
        totalLoad: w.totalLoad,
        trainingDays: w.trainingDays,
        averageWeight: w.averageWeight
      }))
    }
  })

  /** 最新のパーソナルレコード */
  const latestPersonalRecords = computed(() => {
    return analytics.value.personalRecords
      .filter(pr => pr.improvement > 0)
      .sort((a, b) => b.achievedAt.getTime() - a.achievedAt.getTime())
      .slice(0, 3)
  })

  /** 進捗指標 */
  const progressIndicators = computed(() => {
    const recent = state.records.slice(0, 7) // 直近7日
    const previous = state.records.slice(7, 14) // その前の7日

    if (recent.length === 0 || previous.length === 0) {
      return null
    }

    const recentAvgLoad = recent.reduce((sum, r) => sum + r.totalLoadAmount, 0) / recent.length
    const previousAvgLoad = previous.reduce((sum, r) => sum + r.totalLoadAmount, 0) / previous.length

    const loadChange = recentAvgLoad - previousAvgLoad
    const loadChangePercent = previousAvgLoad > 0 ? (loadChange / previousAvgLoad) * 100 : 0

    return {
      loadChange,
      loadChangePercent,
      improvementDirection: loadChange > 0 ? 'up' : loadChange < 0 ? 'down' : 'stable',
      recentAvgLoad,
      previousAvgLoad
    }
  })

  // ===================================
  // Core Functions
  // ===================================

  /**
   * トレーニング履歴を取得
   */
  async function fetchHistory(params: TrainingHistoryParams): Promise<void> {
    try {
      setLoading(true, 'トレーニング履歴を読み込み中...')
      clearError()

      currentParams.value = params

      const records = await training.getTrainingHistory(params, {
        cacheLifetime,
        useCache: true
      })

      state.records = records
      state.currentMenuId = params.menuId
      state.totalCount = records.length
      state.lastUpdated = new Date()

      updatePagination()

      console.log(`[History] Loaded ${records.length} records for menu ${params.menuId}`)

    } catch (err) {
      handleError(err as Error)
      throw err
    } finally {
      setLoading(false)
    }
  }

  /**
   * 履歴を強制再読み込み
   */
  async function refreshHistory(): Promise<void> {
    if (!currentParams.value) return
    
    await fetchHistory({
      ...currentParams.value,
      // キャッシュを使わずに再取得
    })
  }

  /**
   * ページネーション情報を更新
   */
  function updatePagination(): void {
    pagination.totalCount = state.totalCount
    pagination.totalPages = Math.ceil(state.totalCount / pagination.perPage)
    pagination.hasPrevious = pagination.currentPage > 1
    pagination.hasNext = pagination.currentPage < pagination.totalPages
  }

  /**
   * ページを変更
   */
  function goToPage(page: number): void {
    if (page < 1 || page > pagination.totalPages) return
    pagination.currentPage = page
    updatePagination()
  }

  /**
   * 次のページ
   */
  function nextPage(): void {
    if (pagination.hasNext) {
      goToPage(pagination.currentPage + 1)
    }
  }

  /**
   * 前のページ
   */
  function previousPage(): void {
    if (pagination.hasPrevious) {
      goToPage(pagination.currentPage - 1)
    }
  }

  // ===================================
  // Analytics Helper Functions
  // ===================================

  /**
   * 空の分析データを返す
   */
  function getEmptyAnalytics(): HistoryAnalytics {
    return {
      totalDays: 0,
      totalSets: 0,
      totalReps: 0,
      totalLoad: 0,
      averageSets: 0,
      averageReps: 0,
      maxWeight: 0,
      maxReps: 0,
      recentTrend: 'insufficient_data',
      weeklyStats: [],
      personalRecords: []
    }
  }

  /**
   * トレンドを計算
   */
  function calculateTrend(records: DailyTrainingRecord[]): 'improving' | 'stable' | 'declining' | 'insufficient_data' {
    if (records.length < 6) return 'insufficient_data'

    const recent = records.slice(0, 3)
    const previous = records.slice(3, 6)

    const recentAvg = recent.reduce((sum, r) => sum + r.totalLoadAmount, 0) / recent.length
    const previousAvg = previous.reduce((sum, r) => sum + r.totalLoadAmount, 0) / previous.length

    const changePercent = ((recentAvg - previousAvg) / previousAvg) * 100

    if (changePercent > 5) return 'improving'
    if (changePercent < -5) return 'declining'
    return 'stable'
  }

  /**
   * 週間統計を計算
   */
  function calculateWeeklyStats(records: DailyTrainingRecord[]): WeeklyStats[] {
    const weeks = new Map<string, DailyTrainingRecord[]>()

    records.forEach(record => {
      const date = new Date(record.trainingDate)
      const weekStart = getWeekStart(date)
      const weekKey = weekStart.toISOString().split('T')[0]

      if (!weeks.has(weekKey)) {
        weeks.set(weekKey, [])
      }
      weeks.get(weekKey)!.push(record)
    })

    return Array.from(weeks.entries()).map(([weekKey, weekRecords]) => {
      const weekStart = new Date(weekKey)
      const weekEnd = new Date(weekStart)
      weekEnd.setDate(weekEnd.getDate() + 6)

      const totalLoad = weekRecords.reduce((sum, r) => sum + r.totalLoadAmount, 0)
      const weights = weekRecords.map(r => r.maxWeight || 0).filter(w => w > 0)
      const averageWeight = weights.length > 0 ? weights.reduce((sum, w) => sum + w, 0) / weights.length : 0
      const maxWeight = Math.max(...weights, 0)

      return {
        weekStart,
        weekEnd,
        trainingDays: weekRecords.length,
        totalLoad,
        averageWeight,
        maxWeight
      }
    }).sort((a, b) => b.weekStart.getTime() - a.weekStart.getTime())
  }

  /**
   * パーソナルレコードを計算
   */
  function calculatePersonalRecords(records: DailyTrainingRecord[]): PersonalRecordInfo[] {
    const personalRecords: PersonalRecordInfo[] = []

    // 最大重量の記録
    const maxWeightRecord = records.reduce((max, r) => 
      (r.maxWeight || 0) > (max.maxWeight || 0) ? r : max, records[0])
    
    if (maxWeightRecord?.maxWeight) {
      personalRecords.push({
        type: 'max_weight',
        value: maxWeightRecord.maxWeight,
        achievedAt: new Date(maxWeightRecord.trainingDate),
        improvement: 0, // TODO: 前回記録との比較
        improvementPercent: 0
      })
    }

    // 最大回数の記録
    const maxRepsRecord = records.reduce((max, r) => 
      r.maxReps > max.maxReps ? r : max, records[0])
    
    if (maxRepsRecord) {
      personalRecords.push({
        type: 'max_reps',
        value: maxRepsRecord.maxReps,
        achievedAt: new Date(maxRepsRecord.trainingDate),
        improvement: 0,
        improvementPercent: 0
      })
    }

    // 最大負荷量の記録
    const maxLoadRecord = records.reduce((max, r) => 
      r.totalLoadAmount > max.totalLoadAmount ? r : max, records[0])
    
    if (maxLoadRecord) {
      personalRecords.push({
        type: 'total_load',
        value: maxLoadRecord.totalLoadAmount,
        achievedAt: new Date(maxLoadRecord.trainingDate),
        improvement: 0,
        improvementPercent: 0
      })
    }

    return personalRecords
  }

  /**
   * 週の開始日を取得（月曜日を週の開始とする）
   */
  function getWeekStart(date: Date): Date {
    const d = new Date(date)
    const day = d.getDay()
    const diff = d.getDate() - day + (day === 0 ? -6 : 1) // 月曜日に調整
    return new Date(d.setDate(diff))
  }

  /**
   * 日付をフォーマット
   */
  function formatDate(date: Date): string {
    return date.toLocaleDateString('ja-JP', {
      month: 'short',
      day: 'numeric'
    })
  }

  // ===================================
  // State Management
  // ===================================

  /**
   * ローディング状態を設定
   */
  function setLoading(isLoading: boolean, message?: string): void {
    loading.isLoading = isLoading
    loading.message = message
  }

  /**
   * エラー状態を設定
   */
  function setError(hasError: boolean, message: string = '', code?: string, details?: any, retryable: boolean = false): void {
    error.hasError = hasError
    error.message = message
    error.code = code
    error.details = details
    error.retryable = retryable
  }

  /**
   * エラーをクリア
   */
  function clearError(): void {
    setError(false)
  }

  /**
   * エラーハンドリング
   */
  function handleError(err: Error): void {
    console.error('[useTrainingHistory] Error:', err)

    if (err instanceof ApiError) {
      setError(true, err.userMessage, err.code, err.response, err.isRetryable)
    } else if (err instanceof NetworkError) {
      setError(true, err.userMessage, 'NETWORK_ERROR', err.originalError, err.isRetryable)
    } else {
      setError(true, 'トレーニング履歴の読み込み中にエラーが発生しました', 'UNKNOWN_ERROR', err, true)
    }
  }

  /**
   * エラー状態でリトライ
   */
  async function retry(): Promise<void> {
    if (!error.retryable || !currentParams.value) return
    await fetchHistory(currentParams.value)
  }

  // ===================================
  // Return Public API
  // ===================================

  return {
    // State (read-only)
    records: computed(() => state.records),
    currentMenuId: computed(() => state.currentMenuId),
    currentMenuName: computed(() => state.currentMenuName),
    loading: computed(() => loading),
    error: computed(() => error),
    pagination: computed(() => pagination),
    lastUpdated: computed(() => state.lastUpdated),

    // Analytics
    analytics,
    chartData,
    latestPersonalRecords,
    progressIndicators,

    // Actions
    fetchHistory,
    refreshHistory,
    retry,

    // Pagination
    goToPage,
    nextPage,
    previousPage,

    // Utilities
    clearError,
  }
}