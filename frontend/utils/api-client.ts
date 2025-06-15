// ===================================
// Ultra Advanced API Client
// Comprehensive error handling, caching, retry logic, and TypeScript support
// ===================================

import type { 
  ApiResponse, 
  ApiErrorResponse, 
  RequestOptions, 
  CacheEntry,
  TrainingMenu,
  TrainingTag,
  MenuApiResponse,
  DailyTrainingRecord,
  TrainingHistoryParams
} from '~/types'

/**
 * カスタム API エラークラス
 * APIレスポンスエラーを詳細に表現
 */
export class ApiError extends Error {
  constructor(
    public code: string,
    message: string,
    public statusCode: number,
    public response?: ApiErrorResponse
  ) {
    super(message)
    this.name = 'ApiError'
  }

  /**
   * リトライ可能なエラーかどうかを判定
   */
  get isRetryable(): boolean {
    // 5xx系エラーやネットワークエラーはリトライ可能
    return this.statusCode >= 500 || this.statusCode === 0
  }

  /**
   * ユーザー向けのエラーメッセージを取得
   */
  get userMessage(): string {
    switch (this.code) {
      case '10001':
        return 'サーバーでエラーが発生しました。時間をおいて再度お試しください。'
      case '00001':
        return '入力内容に不備があります。内容を確認して再度お試しください。'
      case '10007':
        return 'ログインが必要です。再度ログインしてください。'
      case '10008':
        return 'データが見つかりませんでした。'
      default:
        return this.message || '予期しないエラーが発生しました。'
    }
  }
}

/**
 * ネットワークエラークラス
 */
export class NetworkError extends Error {
  constructor(message: string, public originalError?: Error) {
    super(message)
    this.name = 'NetworkError'
  }

  get isRetryable(): boolean {
    return true
  }

  get userMessage(): string {
    return 'ネットワークエラーが発生しました。インターネット接続を確認して再度お試しください。'
  }
}

/**
 * メモリベースの簡易キャッシュ
 */
class SimpleCache {
  private cache = new Map<string, CacheEntry<any>>()

  /**
   * キャッシュからデータを取得
   */
  get<T>(key: string): T | null {
    const entry = this.cache.get(key)
    if (!entry) return null

    if (entry.expiresAt < new Date()) {
      this.cache.delete(key)
      return null
    }

    return entry.data
  }

  /**
   * キャッシュにデータを保存
   */
  set<T>(key: string, data: T, lifetimeSeconds: number): void {
    const expiresAt = new Date(Date.now() + lifetimeSeconds * 1000)
    this.cache.set(key, {
      data,
      expiresAt,
      createdAt: new Date()
    })
  }

  /**
   * キャッシュをクリア
   */
  clear(): void {
    this.cache.clear()
  }

  /**
   * 特定のキーのキャッシュを削除
   */
  delete(key: string): void {
    this.cache.delete(key)
  }

  /**
   * 期限切れのキャッシュエントリを削除
   */
  cleanup(): void {
    const now = new Date()
    for (const [key, entry] of this.cache.entries()) {
      if (entry.expiresAt < now) {
        this.cache.delete(key)
      }
    }
  }
}

/**
 * 高度なAPIクライアント
 * 自動リトライ、キャッシュ、エラーハンドリング、TypeScript対応
 */
export class ApiClient {
  private cache = new SimpleCache()
  private baseUrl: string
  private defaultTimeout = 10000 // 10秒
  private defaultRetries = 3
  private defaultCacheLifetime = 300 // 5分

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl.replace(/\/$/, '') // 末尾のスラッシュを除去
    
    // 定期的にキャッシュクリーンアップを実行（クライアントサイドのみ）
    if (typeof window !== 'undefined' && process.client) {
      // Nuxtのライフサイクルを使用してSSR中の実行を回避
      const startCleanup = () => {
        // onNuxtReadyを使用してNuxtの準備が完了してから実行
        if (typeof window.$nuxt !== 'undefined') {
          window.$nuxt.$nextTick(() => {
            setInterval(() => this.cache.cleanup(), 60000) // 1分ごと
          })
        } else {
          // Fallback: DOMContentLoadedイベントを使用
          if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
              setInterval(() => this.cache.cleanup(), 60000)
            })
          } else {
            setInterval(() => this.cache.cleanup(), 60000)
          }
        }
      }
      
      startCleanup()
    }
  }

  /**
   * 認証トークンを取得
   */
  private getAuthToken(): string | null {
    if (typeof window !== 'undefined') {
      // グローバルに設定されたトークンを最優先
      if (window.__authToken) {
        return window.__authToken
      }
      
      // フォールバック: セキュアストレージから取得
      return this.getTokenFromStorage()
    }
    return null
  }

  private getTokenFromStorage(): string | null {
    try {
      const stored = localStorage.getItem('message_app_auth_token')
      if (stored) {
        const data = JSON.parse(stored)
        // セキュアストレージの構造を考慮してvalueを復号化
        if (data.value) {
          // 簡易復号化を試行
          try {
            const key = localStorage.getItem('message_app_key') || 'default'
            let decoded = atob(data.value)
            let result = ''
            for (let i = 0; i < decoded.length; i++) {
              result += String.fromCharCode(
                decoded.charCodeAt(i) ^ key.charCodeAt(i % key.length)
              )
            }
            return result
          } catch {
            return data.value
          }
        }
        return data
      }
    } catch {
      // フォールバック: 文字列として保存されている場合
      return localStorage.getItem('message_app_auth_token')
    }
    return null
  }

  /**
   * 認証ヘッダーを取得
   */
  private getAuthHeaders(): Record<string, string> {
    const token = this.getAuthToken()
    return token ? { Authorization: `Bearer ${token}` } : {}
  }

  /**
   * HTTPリクエストを実行（リトライ機能付き）
   */
  private async makeRequest<T>(
    endpoint: string,
    options: RequestInit & RequestOptions = {}
  ): Promise<T> {
    const {
      timeout = this.defaultTimeout,
      retries = this.defaultRetries,
      useCache = false,
      cacheLifetime = this.defaultCacheLifetime,
      ...fetchOptions
    } = options

    const url = `${this.baseUrl}${endpoint}`
    const cacheKey = this.getCacheKey(url, fetchOptions)

    // キャッシュチェック
    if (useCache && fetchOptions.method !== 'POST' && fetchOptions.method !== 'PUT' && fetchOptions.method !== 'DELETE') {
      const cachedData = this.cache.get<T>(cacheKey)
      if (cachedData) {
        console.log(`[API Cache Hit] ${endpoint}`)
        return cachedData
      }
    }

    let lastError: Error | null = null

    // リトライループ
    for (let attempt = 1; attempt <= retries + 1; attempt++) {
      try {
        console.log(`[API Request] ${endpoint} (attempt ${attempt}/${retries + 1})`)
        
        const controller = new AbortController()
        const timeoutId = setTimeout(() => controller.abort(), timeout)

        const response = await fetch(url, {
          ...fetchOptions,
          signal: controller.signal,
          headers: {
            'Content-Type': 'application/json',
            ...this.getAuthHeaders(),
            ...fetchOptions.headers,
          },
        })

        clearTimeout(timeoutId)

        if (!response.ok) {
          const errorData = await this.parseErrorResponse(response)
          
          // 認証エラーの場合、トークンをクリア
          if (response.status === 401) {
            if (typeof window !== 'undefined') {
              // 直接localStorageをクリア
              localStorage.removeItem('auth_token')
              localStorage.removeItem('auth_user')
              localStorage.removeItem('message_app_auth_token')
              localStorage.removeItem('message_app_auth_user')
              
              // Pinia storeをクリア（可能であれば）
              try {
                if (window.$nuxt && window.$nuxt.$pinia) {
                  const { useAuthStore } = await import('~/stores/auth')
                  const authStore = useAuthStore()
                  authStore.clearAuth()
                }
              } catch (error) {
                console.warn('Failed to clear auth store:', error)
              }
              
              // ログインページへのリダイレクトは削除（ページ内でモーダル表示のため）
            }
          }
          
          throw new ApiError(
            errorData.code || `HTTP_${response.status}`,
            errorData.message || `HTTP Error ${response.status}`,
            response.status,
            errorData
          )
        }

        let data = await response.json()
        console.log(`[API Response Debug] ${endpoint}:`, data)

        // ASP.NET Core ActionResult wrapper の確認
        if ('value' in data && 'statusCode' in data) {
          console.log(`[API] Unwrapping ActionResult for ${endpoint}`)
          data = data.value
        }

        // 新しいAPIレスポンス形式をチェック（success/error形式）
        if ('success' in data) {
          if (!data.success) {
            throw new ApiError(
              data.error?.code || 'UNKNOWN_ERROR',
              data.error?.message || 'APIエラーが発生しました',
              200,
              data.error
            )
          }
          console.log(`[API Success] ${endpoint}`)
          
          // キャッシュに保存
          if (useCache && data.data) {
            this.cache.set(cacheKey, data.data, cacheLifetime)
          }
          
          return data.data as T
        }

        // 旧形式のAPIレスポンスのフォーマット確認
        if ('isSuccess' in data) {
          // ビジネスロジックエラーのチェック
          if (!data.isSuccess || data.errorCode) {
            throw new ApiError(
              data.errorCode || 'UNKNOWN_ERROR',
              data.userMessage || 'APIエラーが発生しました',
              200,
              data as ApiErrorResponse
            )
          }
          
          console.log(`[API Success] ${endpoint}`)
          
          // キャッシュに保存
          if (useCache && data.data) {
            this.cache.set(cacheKey, data.data, cacheLifetime)
          }
          
          return data.data as T
        }

        // どの形式にも該当しない場合
        console.warn(`[API Warning] Unknown response format for ${endpoint}:`, {
          hasSuccess: 'success' in data,
          hasIsSuccess: 'isSuccess' in data,
          keys: Object.keys(data)
        })
        
        // フォールバック: dataをそのまま返す
        return data as T

      } catch (error) {
        lastError = error as Error
        
        // AbortErrorはタイムアウトとして処理
        if (error instanceof Error && error.name === 'AbortError') {
          lastError = new NetworkError('リクエストがタイムアウトしました', error)
        }
        
        // NetworkErrorまたはリトライ可能なApiErrorの場合、リトライを実行
        const shouldRetry = attempt <= retries && (
          lastError instanceof NetworkError ||
          (lastError instanceof ApiError && lastError.isRetryable)
        )

        if (!shouldRetry) {
          break
        }

        // 指数バックオフでリトライ間隔を調整
        const delay = Math.min(1000 * Math.pow(2, attempt - 1), 10000)
        console.log(`[API Retry] ${endpoint} in ${delay}ms`)
        await new Promise(resolve => setTimeout(resolve, delay))
      }
    }

    // すべてのリトライが失敗した場合
    console.error(`[API Error] ${endpoint}:`, lastError)
    throw lastError || new Error('Unknown error occurred')
  }

  /**
   * エラーレスポンスを解析
   */
  private async parseErrorResponse(response: Response): Promise<ApiErrorResponse> {
    try {
      const contentType = response.headers.get('content-type')
      if (contentType && contentType.includes('application/json')) {
        return await response.json()
      }
    } catch {
      // JSON解析失敗時は無視
    }

    return {
      code: `HTTP_${response.status}`,
      message: `HTTP Error ${response.status}: ${response.statusText}`
    }
  }

  /**
   * キャッシュキーを生成
   */
  private getCacheKey(url: string, options: RequestInit): string {
    const key = `${options.method || 'GET'}:${url}`
    if (options.body) {
      return `${key}:${JSON.stringify(options.body)}`
    }
    return key
  }

  // ===================================
  // Public API Methods
  // ===================================

  /**
   * GET リクエスト
   */
  async get<T>(endpoint: string, options: RequestOptions = {}): Promise<T> {
    return this.makeRequest<T>(endpoint, {
      method: 'GET',
      useCache: true,
      ...options,
    })
  }

  /**
   * POST リクエスト
   */
  async post<T>(endpoint: string, data?: any, options: RequestOptions = {}): Promise<T> {
    return this.makeRequest<T>(endpoint, {
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined,
      ...options,
    })
  }

  /**
   * PUT リクエスト
   */
  async put<T>(endpoint: string, data?: any, options: RequestOptions = {}): Promise<T> {
    return this.makeRequest<T>(endpoint, {
      method: 'PUT',
      body: data ? JSON.stringify(data) : undefined,
      ...options,
    })
  }

  /**
   * DELETE リクエスト
   */
  async delete<T>(endpoint: string, options: RequestOptions = {}): Promise<T> {
    return this.makeRequest<T>(endpoint, {
      method: 'DELETE',
      ...options,
    })
  }

  /**
   * キャッシュをクリア
   */
  clearCache(): void {
    this.cache.clear()
  }

  /**
   * 特定のエンドポイントのキャッシュを削除
   */
  invalidateCache(endpoint: string): void {
    const url = `${this.baseUrl}${endpoint}`
    this.cache.delete(`GET:${url}`)
  }
}

// ===================================
// Training API Methods
// ===================================

/**
 * トレーニング関連のAPI呼び出しを提供するクラス
 */
export class TrainingApi {
  constructor(private client: ApiClient) {}

  /**
   * トレーニングメニューとタグの一覧を取得
   * 自動キャッシュ機能付き
   */
  async getMenus(options: RequestOptions = {}): Promise<{ menus: TrainingMenu[], tags: TrainingTag[] }> {
    const response = await this.client.get<MenuApiResponse>('/training/menu', {
      cacheLifetime: 600, // 10分間キャッシュ
      ...options,
    })

    // APIレスポンスをフロントエンド用の型に変換
    const menus: TrainingMenu[] = response.menus.map(menu => ({
      menuId: menu.menuId,
      jpName: menu.jpName,
      enName: menu.enName,
      description: menu.description,
      createdAt: menu.createdAt ? new Date(menu.createdAt) : null,
      tagIds: menu.tagIds || []
    }))

    const tags: TrainingTag[] = response.tags.map(tag => ({
      tagId: tag.tagId,
      jpName: tag.jpName,
      enName: tag.enName
    }))

    return { menus, tags }
  }

  /**
   * トレーニング履歴を取得
   */
  async getTrainingHistory(params: TrainingHistoryParams, options: RequestOptions = {}): Promise<DailyTrainingRecord[]> {
    const queryParams = new URLSearchParams()
    queryParams.append('menuId', params.menuId)
    
    if (params.startDate) queryParams.append('startDate', params.startDate)
    if (params.endDate) queryParams.append('endDate', params.endDate)
    if (params.limit) queryParams.append('limit', params.limit.toString())

    return this.client.get<DailyTrainingRecord[]>(`/training/history?${queryParams}`, {
      cacheLifetime: 60, // 1分間キャッシュ
      ...options,
    })
  }

  /**
   * トレーニングレコードを登録
   */
  async createTrainingRecord(data: any, options: RequestOptions = {}): Promise<any> {
    const result = await this.client.post('/training/record', data, options)
    
    // 関連するキャッシュを無効化
    this.client.invalidateCache('/training/history')
    this.client.invalidateCache('/training/dashboard')
    
    return result
  }

  /**
   * トレーニングレコードを削除
   */
  async deleteTrainingRecord(recordId: string, options: RequestOptions = {}): Promise<void> {
    await this.client.delete(`/training/record/${recordId}`, options)
    
    // 関連するキャッシュを無効化
    this.client.invalidateCache('/training/history')
    this.client.invalidateCache('/training/dashboard')
  }

  /**
   * 汎用 GET リクエスト
   */
  async get<T>(endpoint: string, options: RequestOptions = {}): Promise<T> {
    // トレーニング関連のエンドポイントのプレフィックスを自動追加
    const fullEndpoint = endpoint.startsWith('/') ? `/training${endpoint}` : `/training/${endpoint}`
    return this.client.get<T>(fullEndpoint, options)
  }

  /**
   * 汎用 POST リクエスト
   */
  async post<T>(endpoint: string, data?: any, options: RequestOptions = {}): Promise<T> {
    // トレーニング関連のエンドポイントのプレフィックスを自動追加
    const fullEndpoint = endpoint.startsWith('/') ? `/training${endpoint}` : `/training/${endpoint}`
    return this.client.post<T>(fullEndpoint, data, options)
  }

  /**
   * 汎用 PUT リクエスト
   */
  async put<T>(endpoint: string, data?: any, options: RequestOptions = {}): Promise<T> {
    // トレーニング関連のエンドポイントのプレフィックスを自動追加
    const fullEndpoint = endpoint.startsWith('/') ? `/training${endpoint}` : `/training/${endpoint}`
    return this.client.put<T>(fullEndpoint, data, options)
  }

  /**
   * 汎用 DELETE リクエスト
   */
  async delete<T>(endpoint: string, options: RequestOptions = {}): Promise<T> {
    // トレーニング関連のエンドポイントのプレフィックスを自動追加
    const fullEndpoint = endpoint.startsWith('/') ? `/training${endpoint}` : `/training/${endpoint}`
    return this.client.delete<T>(fullEndpoint, options)
  }
}

// ===================================
// Global API Client Instance
// ===================================

/**
 * グローバルAPIクライアントインスタンス
 * Nuxtのランタイム設定から自動的にベースURLを取得
 */
export function createApiClient(): { client: ApiClient; training: TrainingApi } {
  const config = useRuntimeConfig()
  const client = new ApiClient(config.public.apiBaseUrl as string)
  const training = new TrainingApi(client)

  return { client, training }
}

/**
 * デフォルトAPIクライアント（使いやすさのため）
 */
let defaultClient: { client: ApiClient; training: TrainingApi } | null = null

export function useApiClient() {
  if (!defaultClient) {
    defaultClient = createApiClient()
  }
  return defaultClient
}

/**
 * 簡単にAPIクライアントを使用するためのエクスポート
 * Development: direct API access on port 5001
 * Production: relative /api path through nginx proxy
 */
export const apiClient = new ApiClient(
  typeof window !== 'undefined' 
    ? (window.location.port === '8080' ? '' : 'http://localhost:5001')
    : ''
)

// 開発環境でのデバッグ用
if (process.dev) {
  // @ts-ignore
  globalThis.__apiClient = useApiClient()
}