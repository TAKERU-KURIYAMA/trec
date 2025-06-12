// ===================================
// Ultra Advanced React Native API Client
// Offline support, intelligent caching, comprehensive error handling
// ===================================

import AsyncStorage from '@react-native-async-storage/async-storage';
import NetInfo from '@react-native-netinfo/netinfo';
import {
  ApiResponse,
  ApiErrorResponse,
  NetworkState,
  CacheEntry,
  OfflineQueueItem,
  ConfigOptions,
} from '../types/enhanced';

/**
 * APIエラークラス
 */
export class ApiError extends Error {
  constructor(
    public code: string,
    message: string,
    public statusCode: number,
    public response?: ApiErrorResponse
  ) {
    super(message);
    this.name = 'ApiError';
  }

  get isRetryable(): boolean {
    return this.statusCode >= 500 || this.statusCode === 0;
  }

  get userMessage(): string {
    switch (this.code) {
      case '10001':
        return 'サーバーでエラーが発生しました。時間をおいて再度お試しください。';
      case '00001':
        return '入力内容に不備があります。内容を確認してください。';
      case '10007':
        return 'ログインが必要です。再度ログインしてください。';
      case '10008':
        return 'データが見つかりませんでした。';
      default:
        return this.message || '予期しないエラーが発生しました。';
    }
  }
}

/**
 * ネットワークエラークラス
 */
export class NetworkError extends Error {
  constructor(message: string, public originalError?: Error) {
    super(message);
    this.name = 'NetworkError';
  }

  get isRetryable(): boolean {
    return true;
  }

  get userMessage(): string {
    return 'インターネット接続を確認して再度お試しください。';
  }
}

/**
 * インテリジェントキャッシュマネージャー
 */
class CacheManager {
  private static instance: CacheManager;
  private cachePrefix = '@TrecPlans:cache:';

  static getInstance(): CacheManager {
    if (!CacheManager.instance) {
      CacheManager.instance = new CacheManager();
    }
    return CacheManager.instance;
  }

  async get<T>(key: string): Promise<T | null> {
    try {
      const cacheKey = this.cachePrefix + key;
      const cachedData = await AsyncStorage.getItem(cacheKey);
      
      if (!cachedData) return null;

      const entry: CacheEntry<T> = JSON.parse(cachedData);
      
      if (new Date() > new Date(entry.expiresAt)) {
        await this.delete(key);
        return null;
      }

      return entry.data;
    } catch (error) {
      console.warn('[Cache] Failed to get cache entry:', error);
      return null;
    }
  }

  async set<T>(key: string, data: T, lifetimeSeconds: number): Promise<void> {
    try {
      const cacheKey = this.cachePrefix + key;
      const entry: CacheEntry<T> = {
        data,
        timestamp: new Date(),
        expiresAt: new Date(Date.now() + lifetimeSeconds * 1000),
        version: 1,
      };

      await AsyncStorage.setItem(cacheKey, JSON.stringify(entry));
    } catch (error) {
      console.warn('[Cache] Failed to set cache entry:', error);
    }
  }

  async delete(key: string): Promise<void> {
    try {
      const cacheKey = this.cachePrefix + key;
      await AsyncStorage.removeItem(cacheKey);
    } catch (error) {
      console.warn('[Cache] Failed to delete cache entry:', error);
    }
  }

  async clear(): Promise<void> {
    try {
      const keys = await AsyncStorage.getAllKeys();
      const cacheKeys = keys.filter(key => key.startsWith(this.cachePrefix));
      await AsyncStorage.multiRemove(cacheKeys);
    } catch (error) {
      console.warn('[Cache] Failed to clear cache:', error);
    }
  }

  async cleanup(): Promise<void> {
    try {
      const keys = await AsyncStorage.getAllKeys();
      const cacheKeys = keys.filter(key => key.startsWith(this.cachePrefix));
      
      for (const key of cacheKeys) {
        const data = await AsyncStorage.getItem(key);
        if (data) {
          try {
            const entry: CacheEntry<any> = JSON.parse(data);
            if (new Date() > new Date(entry.expiresAt)) {
              await AsyncStorage.removeItem(key);
            }
          } catch {
            await AsyncStorage.removeItem(key);
          }
        }
      }
    } catch (error) {
      console.warn('[Cache] Failed to cleanup cache:', error);
    }
  }
}

/**
 * オフラインキューマネージャー
 */
class OfflineQueueManager {
  private static instance: OfflineQueueManager;
  private queueKey = '@TrecPlans:offline_queue';

  static getInstance(): OfflineQueueManager {
    if (!OfflineQueueManager.instance) {
      OfflineQueueManager.instance = new OfflineQueueManager();
    }
    return OfflineQueueManager.instance;
  }

  async addToQueue(item: Omit<OfflineQueueItem, 'id' | 'timestamp' | 'retryCount'>): Promise<void> {
    try {
      const queue = await this.getQueue();
      const newItem: OfflineQueueItem = {
        ...item,
        id: Date.now().toString() + Math.random().toString(36).substr(2, 9),
        timestamp: new Date(),
        retryCount: 0,
      };
      
      queue.push(newItem);
      await AsyncStorage.setItem(this.queueKey, JSON.stringify(queue));
    } catch (error) {
      console.warn('[OfflineQueue] Failed to add item to queue:', error);
    }
  }

  async getQueue(): Promise<OfflineQueueItem[]> {
    try {
      const queueData = await AsyncStorage.getItem(this.queueKey);
      return queueData ? JSON.parse(queueData) : [];
    } catch (error) {
      console.warn('[OfflineQueue] Failed to get queue:', error);
      return [];
    }
  }

  async updateQueueItem(id: string, updates: Partial<OfflineQueueItem>): Promise<void> {
    try {
      const queue = await this.getQueue();
      const index = queue.findIndex(item => item.id === id);
      
      if (index >= 0) {
        queue[index] = { ...queue[index], ...updates };
        await AsyncStorage.setItem(this.queueKey, JSON.stringify(queue));
      }
    } catch (error) {
      console.warn('[OfflineQueue] Failed to update queue item:', error);
    }
  }

  async removeFromQueue(id: string): Promise<void> {
    try {
      const queue = await this.getQueue();
      const filteredQueue = queue.filter(item => item.id !== id);
      await AsyncStorage.setItem(this.queueKey, JSON.stringify(filteredQueue));
    } catch (error) {
      console.warn('[OfflineQueue] Failed to remove item from queue:', error);
    }
  }

  async clearQueue(): Promise<void> {
    try {
      await AsyncStorage.removeItem(this.queueKey);
    } catch (error) {
      console.warn('[OfflineQueue] Failed to clear queue:', error);
    }
  }
}

/**
 * 高度なReact Native APIクライアント
 */
export class ApiClient {
  private cache = CacheManager.getInstance();
  private offlineQueue = OfflineQueueManager.getInstance();
  private baseUrl: string;
  private networkState: NetworkState = {
    isConnected: false,
    isInternetReachable: false,
    type: 'unknown',
  };
  
  private config: ConfigOptions = {
    apiTimeout: 10000,
    cacheLifetime: 300,
    maxRetries: 3,
    syncInterval: 30000,
    backgroundSyncEnabled: true,
    crashReportingEnabled: true,
    analyticsEnabled: true,
  };

  constructor(baseUrl: string, config?: Partial<ConfigOptions>) {
    this.baseUrl = baseUrl.replace(/\/$/, '');
    this.config = { ...this.config, ...config };
    
    this.initializeNetworkListener();
    this.startPeriodicSync();
    this.startCacheCleanup();
  }

  /**
   * ネットワーク状態監視を初期化
   */
  private initializeNetworkListener(): void {
    NetInfo.addEventListener(state => {
      const wasOffline = !this.networkState.isConnected;
      
      this.networkState = {
        isConnected: state.isConnected || false,
        isInternetReachable: state.isInternetReachable || false,
        type: state.type as any,
        isExpensive: state.details?.isConnectionExpensive,
      };

      // オンラインになった時の処理
      if (wasOffline && this.networkState.isConnected) {
        this.syncOfflineQueue();
      }
    });
  }

  /**
   * 定期同期を開始
   */
  private startPeriodicSync(): void {
    if (this.config.backgroundSyncEnabled) {
      setInterval(() => {
        if (this.networkState.isConnected) {
          this.syncOfflineQueue();
        }
      }, this.config.syncInterval);
    }
  }

  /**
   * キャッシュクリーンアップを開始
   */
  private startCacheCleanup(): void {
    setInterval(() => {
      this.cache.cleanup();
    }, 60000); // 1分ごと
  }

  /**
   * HTTPリクエストを実行
   */
  private async makeRequest<T>(
    endpoint: string,
    options: RequestInit & {
      useCache?: boolean;
      cacheLifetime?: number;
      retries?: number;
      offlineSupport?: boolean;
    } = {}
  ): Promise<T> {
    const {
      useCache = false,
      cacheLifetime = this.config.cacheLifetime,
      retries = this.config.maxRetries,
      offlineSupport = true,
      ...requestOptions
    } = options;

    const url = `${this.baseUrl}${endpoint}`;
    const cacheKey = this.getCacheKey(url, requestOptions);

    // キャッシュチェック（オフライン時は強制的にキャッシュを使用）
    if (useCache || !this.networkState.isConnected) {
      const cachedData = await this.cache.get<T>(cacheKey);
      if (cachedData) {
        console.log(`[API Cache Hit] ${endpoint}`);
        return cachedData;
      }
      
      if (!this.networkState.isConnected) {
        throw new NetworkError('オフライン状態です。キャッシュされたデータがありません。');
      }
    }

    // オフライン時のキューイング（READ以外の操作）
    if (!this.networkState.isConnected && offlineSupport && requestOptions.method !== 'GET') {
      await this.offlineQueue.addToQueue({
        action: this.getActionFromMethod(requestOptions.method || 'POST'),
        endpoint,
        data: requestOptions.body ? JSON.parse(requestOptions.body as string) : undefined,
        maxRetries: retries,
      });
      
      throw new NetworkError('オフライン状態のため、オンライン時に同期されます。');
    }

    let lastError: Error | null = null;

    // リトライループ
    for (let attempt = 1; attempt <= retries + 1; attempt++) {
      try {
        console.log(`[API Request] ${endpoint} (attempt ${attempt}/${retries + 1})`);
        
        const token = await this.getAuthToken();
        const headers: Record<string, string> = {
          'Content-Type': 'application/json',
          ...requestOptions.headers as Record<string, string>,
        };

        if (token) {
          headers.Authorization = `Bearer ${token}`;
        }

        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), this.config.apiTimeout);

        const response = await fetch(url, {
          ...requestOptions,
          headers,
          signal: controller.signal,
        });

        clearTimeout(timeoutId);

        if (!response.ok) {
          const errorData = await this.parseErrorResponse(response);
          throw new ApiError(
            errorData.code || `HTTP_${response.status}`,
            errorData.message || `HTTP Error ${response.status}`,
            response.status,
            errorData
          );
        }

        const data = await response.json() as ApiResponse<T>;

        if (data.code !== '00000') {
          throw new ApiError(
            data.code,
            data.message || 'APIエラーが発生しました',
            200,
            data as ApiErrorResponse
          );
        }

        console.log(`[API Success] ${endpoint}`);

        // キャッシュに保存
        if (useCache && data.result) {
          await this.cache.set(cacheKey, data.result, cacheLifetime);
        }

        return data.result || data as T;

      } catch (error) {
        lastError = error as Error;
        
        if (error instanceof Error && error.name === 'AbortError') {
          lastError = new NetworkError('リクエストがタイムアウトしました', error);
        }
        
        const shouldRetry = attempt <= retries && (
          lastError instanceof NetworkError ||
          (lastError instanceof ApiError && lastError.isRetryable)
        );

        if (!shouldRetry) {
          break;
        }

        // 指数バックオフ
        const delay = Math.min(1000 * Math.pow(2, attempt - 1), 10000);
        console.log(`[API Retry] ${endpoint} in ${delay}ms`);
        await new Promise(resolve => setTimeout(resolve, delay));
      }
    }

    console.error(`[API Error] ${endpoint}:`, lastError);
    throw lastError || new Error('Unknown error occurred');
  }

  /**
   * オフラインキューを同期
   */
  private async syncOfflineQueue(): Promise<void> {
    if (!this.networkState.isConnected) return;

    const queue = await this.offlineQueue.getQueue();
    
    for (const item of queue) {
      try {
        await this.executeQueueItem(item);
        await this.offlineQueue.removeFromQueue(item.id);
      } catch (error) {
        console.warn(`[OfflineSync] Failed to sync item ${item.id}:`, error);
        
        if (item.retryCount < item.maxRetries) {
          await this.offlineQueue.updateQueueItem(item.id, {
            retryCount: item.retryCount + 1,
          });
        } else {
          await this.offlineQueue.removeFromQueue(item.id);
        }
      }
    }
  }

  /**
   * キューアイテムを実行
   */
  private async executeQueueItem(item: OfflineQueueItem): Promise<void> {
    const method = this.getMethodFromAction(item.action);
    await this.makeRequest(item.endpoint, {
      method,
      body: item.data ? JSON.stringify(item.data) : undefined,
      offlineSupport: false, // 無限ループを防ぐ
    });
  }

  /**
   * HTTPメソッドからアクションを取得
   */
  private getActionFromMethod(method: string): 'create' | 'update' | 'delete' {
    switch (method.toUpperCase()) {
      case 'POST': return 'create';
      case 'PUT':
      case 'PATCH': return 'update';
      case 'DELETE': return 'delete';
      default: return 'create';
    }
  }

  /**
   * アクションからHTTPメソッドを取得
   */
  private getMethodFromAction(action: 'create' | 'update' | 'delete'): string {
    switch (action) {
      case 'create': return 'POST';
      case 'update': return 'PUT';
      case 'delete': return 'DELETE';
    }
  }

  /**
   * 認証トークンを取得
   */
  private async getAuthToken(): Promise<string | null> {
    try {
      return await AsyncStorage.getItem('@TrecPlans:token');
    } catch {
      return null;
    }
  }

  /**
   * エラーレスポンスを解析
   */
  private async parseErrorResponse(response: Response): Promise<ApiErrorResponse> {
    try {
      const contentType = response.headers.get('content-type');
      if (contentType && contentType.includes('application/json')) {
        return await response.json();
      }
    } catch {
      // JSON解析失敗時は無視
    }

    return {
      code: `HTTP_${response.status}`,
      message: `HTTP Error ${response.status}: ${response.statusText}`,
    };
  }

  /**
   * キャッシュキーを生成
   */
  private getCacheKey(url: string, options: RequestInit): string {
    const key = `${options.method || 'GET'}:${url}`;
    if (options.body) {
      return `${key}:${JSON.stringify(options.body)}`;
    }
    return key;
  }

  // ===================================
  // Public API Methods
  // ===================================

  async get<T>(endpoint: string, options: { useCache?: boolean; cacheLifetime?: number } = {}): Promise<T> {
    return this.makeRequest<T>(endpoint, {
      method: 'GET',
      useCache: true,
      ...options,
    });
  }

  async post<T>(endpoint: string, data?: any, options: any = {}): Promise<T> {
    return this.makeRequest<T>(endpoint, {
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined,
      ...options,
    });
  }

  async put<T>(endpoint: string, data?: any, options: any = {}): Promise<T> {
    return this.makeRequest<T>(endpoint, {
      method: 'PUT',
      body: data ? JSON.stringify(data) : undefined,
      ...options,
    });
  }

  async delete<T>(endpoint: string, options: any = {}): Promise<T> {
    return this.makeRequest<T>(endpoint, {
      method: 'DELETE',
      ...options,
    });
  }

  /**
   * 手動同期
   */
  async sync(): Promise<void> {
    await this.syncOfflineQueue();
  }

  /**
   * キャッシュをクリア
   */
  async clearCache(): Promise<void> {
    await this.cache.clear();
  }

  /**
   * オフラインキューをクリア
   */
  async clearOfflineQueue(): Promise<void> {
    await this.offlineQueue.clearQueue();
  }

  /**
   * ネットワーク状態を取得
   */
  getNetworkState(): NetworkState {
    return this.networkState;
  }

  /**
   * 設定を更新
   */
  updateConfig(newConfig: Partial<ConfigOptions>): void {
    this.config = { ...this.config, ...newConfig };
  }
}

// ===================================
// Default API Client Instance
// ===================================

const API_BASE_URL = 'http://local-trecplans:8080/api';

export const apiClient = new ApiClient(API_BASE_URL, {
  apiTimeout: 15000,
  cacheLifetime: 600, // 10分
  maxRetries: 3,
  backgroundSyncEnabled: true,
});

export default apiClient;