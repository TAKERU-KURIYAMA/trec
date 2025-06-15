// ===================================
// Ultra Advanced Training Menus Composable
// Comprehensive state management, error handling, filtering, and caching
// ===================================

import { ref, computed, reactive, watch, onMounted, onUnmounted, nextTick } from 'vue'
import type { 
  TrainingMenu, 
  TrainingTag, 
  LoadingState, 
  ErrorState, 
  FilterConditions,
  RequestOptions 
} from '~/types'
import { useApiClient, ApiError, NetworkError } from '~/utils/api-client'

/**
 * トレーニングメニュー管理の状態型
 */
interface MenusState {
  /** 全メニューリスト */
  allMenus: TrainingMenu[]
  /** 全タグリスト */
  allTags: TrainingTag[]
  /** フィルター済みメニューリスト */
  filteredMenus: TrainingMenu[]
  /** 選択中のメニュー */
  selectedMenu: TrainingMenu | null
  /** 最終更新日時 */
  lastUpdated: Date | null
}

/**
 * 高度なトレーニングメニュー管理コンポーザブル
 * 
 * 機能:
 * - 自動キャッシュとリアルタイム更新
 * - 高度なフィルタリングと検索
 * - ローディング状態管理
 * - 包括的エラーハンドリング
 * - リトライ機能
 * - オフライン対応
 */
export function useTrainingMenus(options: {
  /** 自動読み込みを有効にするかどうか */
  autoLoad?: boolean
  /** 自動更新間隔（ミリ秒、0で無効） */
  autoRefreshInterval?: number
  /** 初期フィルター条件 */
  initialFilter?: Partial<FilterConditions>
} = {}) {
  const { 
    autoLoad = true,
    autoRefreshInterval = 0,
    initialFilter = {}
  } = options

  const { training } = useApiClient()

  // ===================================
  // Reactive State
  // ===================================

  /** メニューとタグの状態 */
  const state = reactive<MenusState>({
    allMenus: [],
    allTags: [],
    filteredMenus: [],
    selectedMenu: null,
    lastUpdated: null
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

  /** フィルター条件 */
  const filters = reactive<FilterConditions>({
    keyword: '',
    tagIds: [],
    sortBy: 'jpName',
    sortOrder: 'asc',
    ...initialFilter
  })

  /** 統計情報 */
  const stats = computed(() => ({
    totalMenus: state.allMenus.length,
    totalTags: state.allTags.length,
    filteredCount: state.filteredMenus.length,
    selectedTagsCount: filters.tagIds?.length || 0
  }))

  // ===================================
  // Computed Properties
  // ===================================

  /** タグごとのメニュー数 */
  const tagStats = computed(() => {
    const stats = new Map<string, number>()
    
    state.allMenus.forEach(menu => {
      menu.tagIds.forEach(tagId => {
        stats.set(tagId, (stats.get(tagId) || 0) + 1)
      })
    })

    return Array.from(stats.entries()).map(([tagId, count]) => {
      const tag = state.allTags.find(t => t.tagId === tagId)
      return {
        tag,
        count,
        selected: filters.tagIds?.includes(tagId) || false
      }
    }).filter(item => item.tag).sort((a, b) => b.count - a.count)
  })

  /** 検索キーワードによるサジェスト */
  const searchSuggestions = computed(() => {
    if (!filters.keyword || filters.keyword.length < 2) return []

    const keyword = filters.keyword.toLowerCase()
    const suggestions = new Set<string>()

    state.allMenus.forEach(menu => {
      if (menu.jpName.toLowerCase().includes(keyword)) {
        suggestions.add(menu.jpName)
      }
      if (menu.enName.toLowerCase().includes(keyword)) {
        suggestions.add(menu.enName)
      }
    })

    return Array.from(suggestions).slice(0, 5)
  })

  /** 人気メニュー（使用頻度が高そうなもの） */
  const popularMenus = computed(() => {
    return state.allMenus
      .filter(menu => menu.tagIds.length > 0) // タグが付いているものを優先
      .slice(0, 6)
  })

  // ===================================
  // Core Functions
  // ===================================

  /**
   * メニューとタグを取得
   */
  async function fetchMenus(options: RequestOptions = {}): Promise<void> {
    try {
      setLoading(true, 'メニューを読み込み中...')
      clearError()

      const { menus, tags } = await training.getMenus(options)

      state.allMenus = menus
      state.allTags = tags
      state.lastUpdated = new Date()

      console.log(`[Menus] Loaded ${menus.length} menus and ${tags.length} tags`)

    } catch (err) {
      handleError(err as Error)
      throw err
    } finally {
      setLoading(false)
    }
  }

  /**
   * メニューを強制再読み込み
   */
  async function refreshMenus(): Promise<void> {
    await fetchMenus({ useCache: false })
  }

  /**
   * フィルターを適用してメニューをフィルタリング
   */
  function applyFilters(): void {
    let filtered = [...state.allMenus]

    // キーワード検索
    if (filters.keyword) {
      const keyword = filters.keyword.toLowerCase()
      filtered = filtered.filter(menu =>
        menu.jpName.toLowerCase().includes(keyword) ||
        menu.enName.toLowerCase().includes(keyword) ||
        menu.description?.toLowerCase().includes(keyword)
      )
    }

    // タグフィルター
    if (filters.tagIds && filters.tagIds.length > 0) {
      filtered = filtered.filter(menu =>
        filters.tagIds!.some(tagId => menu.tagIds.includes(tagId))
      )
    }

    // ソート
    if (filters.sortBy) {
      filtered.sort((a, b) => {
        let aValue: any = a[filters.sortBy as keyof TrainingMenu]
        let bValue: any = b[filters.sortBy as keyof TrainingMenu]

        if (typeof aValue === 'string') aValue = aValue.toLowerCase()
        if (typeof bValue === 'string') bValue = bValue.toLowerCase()

        if (aValue < bValue) return filters.sortOrder === 'asc' ? -1 : 1
        if (aValue > bValue) return filters.sortOrder === 'asc' ? 1 : -1
        return 0
      })
    }

    state.filteredMenus = filtered
  }

  // ===================================
  // Filter Management
  // ===================================

  /**
   * 検索キーワードを設定
   */
  function setSearchKeyword(keyword: string): void {
    filters.keyword = keyword
  }

  /**
   * タグフィルターを切り替え
   */
  function toggleTagFilter(tagId: string): void {
    if (!filters.tagIds) filters.tagIds = []
    
    const index = filters.tagIds.indexOf(tagId)
    if (index >= 0) {
      filters.tagIds.splice(index, 1)
    } else {
      filters.tagIds.push(tagId)
    }
  }

  /**
   * すべてのフィルターをクリア
   */
  function clearFilters(): void {
    filters.keyword = ''
    filters.tagIds = []
    filters.dateFrom = undefined
    filters.dateTo = undefined
  }

  /**
   * ソート条件を設定
   */
  function setSortOrder(sortBy: string, sortOrder: 'asc' | 'desc' = 'asc'): void {
    filters.sortBy = sortBy
    filters.sortOrder = sortOrder
  }

  // ===================================
  // Menu Selection
  // ===================================

  /**
   * メニューを選択
   */
  function selectMenu(menu: TrainingMenu | null): void {
    state.selectedMenu = menu
  }

  /**
   * メニューIDで選択
   */
  function selectMenuById(menuId: string): boolean {
    const menu = state.allMenus.find(m => m.menuId === menuId)
    if (menu) {
      selectMenu(menu)
      return true
    }
    return false
  }

  /**
   * メニューを取得
   */
  function getMenuById(menuId: string): TrainingMenu | undefined {
    return state.allMenus.find(m => m.menuId === menuId)
  }

  /**
   * タグを取得
   */
  function getTagById(tagId: string): TrainingTag | undefined {
    return state.allTags.find(t => t.tagId === tagId)
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
    console.error('[useTrainingMenus] Error:', err)

    if (err instanceof ApiError) {
      setError(true, err.userMessage, err.code, err.response, err.isRetryable)
    } else if (err instanceof NetworkError) {
      setError(true, err.userMessage, 'NETWORK_ERROR', err.originalError, err.isRetryable)
    } else {
      setError(true, 'メニューの読み込み中にエラーが発生しました', 'UNKNOWN_ERROR', err, true)
    }
  }

  /**
   * エラー状態でリトライ
   */
  async function retry(): Promise<void> {
    if (!error.retryable) return
    await fetchMenus()
  }

  // ===================================
  // Lifecycle & Watchers
  // ===================================

  // フィルター変更時に自動でフィルタリングを実行
  watch(filters, () => {
    nextTick(() => applyFilters())
  }, { deep: true })

  // メニューデータ変更時に自動でフィルタリングを実行
  watch(() => state.allMenus, () => {
    nextTick(() => applyFilters())
  })

  // 自動更新の設定
  let autoRefreshTimer: NodeJS.Timeout | null = null
  
  // コンポーネント破棄時のクリーンアップ
  onUnmounted(() => {
    if (autoRefreshTimer) {
      clearInterval(autoRefreshTimer)
    }
  })

  // マウント時に自動更新を設定（クライアントサイドのみ）
  onMounted(() => {
    if (autoRefreshInterval > 0 && process.client) {
      autoRefreshTimer = setInterval(() => {
        if (!loading.isLoading) {
          fetchMenus({ useCache: false }).catch(err => {
            console.warn('[useTrainingMenus] Auto refresh failed:', err)
          })
        }
      }, autoRefreshInterval)
    }
  })

  // 自動読み込み
  if (autoLoad) {
    onMounted(() => {
      fetchMenus().catch(err => {
        console.error('[useTrainingMenus] Auto load failed:', err)
      })
    })
  }

  // ===================================
  // Return Public API
  // ===================================

  return {
    // State (read-only)
    menus: computed(() => state.filteredMenus),
    allMenus: computed(() => state.allMenus),
    tags: computed(() => state.allTags),
    selectedMenu: computed(() => state.selectedMenu),
    loading: computed(() => loading),
    error: computed(() => error),
    filters: computed(() => filters),
    stats: computed(() => stats),
    tagStats,
    searchSuggestions,
    popularMenus,
    lastUpdated: computed(() => state.lastUpdated),

    // Actions
    fetchMenus,
    refreshMenus,
    retry,

    // Filter management
    setSearchKeyword,
    toggleTagFilter,
    clearFilters,
    setSortOrder,

    // Menu selection
    selectMenu,
    selectMenuById,
    getMenuById,
    getTagById,

    // Utilities
    clearError,
  }
}