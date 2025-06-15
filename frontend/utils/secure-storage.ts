/**
 * セキュアなトークンストレージユーティリティ
 * localStorage の代わりにより安全な方法でトークンを保存
 */

interface StorageData {
  value: string
  timestamp: number
  expiresAt?: number
}

class SecureStorage {
  private readonly prefix = 'message_app_'
  private readonly maxAge = 24 * 60 * 60 * 1000 // 24時間

  /**
   * データを暗号化してストレージに保存
   */
  setItem(key: string, value: string, expiresInMs?: number): void {
    if (typeof window === 'undefined') return

    try {
      const data: StorageData = {
        value: this.simpleEncrypt(value),
        timestamp: Date.now(),
        expiresAt: expiresInMs ? Date.now() + expiresInMs : undefined
      }

      localStorage.setItem(this.prefix + key, JSON.stringify(data))
      
      console.log(`SecureStorage: Stored data for key: ${key}`, {
        hasExpiry: !!data.expiresAt,
        expiresAt: data.expiresAt ? new Date(data.expiresAt).toISOString() : 'never',
        valueLength: value.length
      })
    } catch (error) {
      console.warn('Failed to store data securely:', error)
    }
  }

  /**
   * ストレージからデータを復号化して取得
   */
  getItem(key: string): string | null {
    if (typeof window === 'undefined') return null

    try {
      const fullKey = this.prefix + key
      const stored = localStorage.getItem(fullKey)
      console.log(`SecureStorage: Looking for key "${fullKey}":`, !!stored)
      
      if (!stored) {
        // Debug: 全てのキーを表示
        const allKeys = Object.keys(localStorage).filter(k => k.startsWith(this.prefix))
        console.log(`SecureStorage: Available keys:`, allKeys)
        console.log(`SecureStorage: No data found for key: ${key}`)
        return null
      }

      const data: StorageData = JSON.parse(stored)
      const now = Date.now()
      
      // 有効期限チェック
      if (data.expiresAt && now > data.expiresAt) {
        console.warn(`SecureStorage: Data expired for key: ${key}`, {
          expiresAt: new Date(data.expiresAt).toISOString(),
          now: new Date(now).toISOString()
        })
        this.removeItem(key)
        return null
      }

      // 最大保存期間チェック
      const age = now - data.timestamp
      if (age > this.maxAge) {
        console.warn(`SecureStorage: Data too old for key: ${key}`, {
          age: age / (1000 * 60 * 60) + ' hours',
          maxAge: this.maxAge / (1000 * 60 * 60) + ' hours'
        })
        this.removeItem(key)
        return null
      }

      console.log(`SecureStorage: Retrieved data for key: ${key}`, {
        age: age / (1000 * 60) + ' minutes',
        hasExpiry: !!data.expiresAt
      })

      return this.simpleDecrypt(data.value)
    } catch (error) {
      console.warn('Failed to retrieve data securely:', error)
      this.removeItem(key)
      return null
    }
  }

  /**
   * ストレージからデータを削除
   */
  removeItem(key: string): void {
    if (typeof window === 'undefined') return
    localStorage.removeItem(this.prefix + key)
  }

  /**
   * 全ての認証関連データをクリア
   */
  clearAuthData(): void {
    if (typeof window === 'undefined') return

    const keysToRemove = ['auth_token', 'auth_user', 'auth_refresh_token']
    keysToRemove.forEach(key => this.removeItem(key))
  }

  /**
   * 簡単な暗号化（Base64 + XOR）
   * 注意: これは高度なセキュリティを提供するものではありません
   * 本格的な実装では、より強力な暗号化を使用してください
   */
  private simpleEncrypt(text: string): string {
    const key = this.getOrCreateKey()
    let result = ''
    
    for (let i = 0; i < text.length; i++) {
      result += String.fromCharCode(
        text.charCodeAt(i) ^ key.charCodeAt(i % key.length)
      )
    }
    
    // UTF-8文字をサポートするためにencodeURIComponentとbtoaを組み合わせ
    try {
      return btoa(unescape(encodeURIComponent(result)))
    } catch (error) {
      console.warn('Failed to encrypt with btoa, falling back to base64 encoding:', error)
      // フォールバック: 手動でBase64エンコーディング
      return this.manualBase64Encode(result)
    }
  }

  /**
   * 簡単な復号化
   */
  private simpleDecrypt(encryptedText: string): string {
    try {
      const key = this.getOrCreateKey()
      let decoded: string
      
      try {
        decoded = decodeURIComponent(escape(atob(encryptedText)))
      } catch (error) {
        // フォールバック: 手動でBase64デコーディング
        decoded = this.manualBase64Decode(encryptedText)
      }
      
      let result = ''
      
      for (let i = 0; i < decoded.length; i++) {
        result += String.fromCharCode(
          decoded.charCodeAt(i) ^ key.charCodeAt(i % key.length)
        )
      }
      
      return result
    } catch (error) {
      throw new Error('Failed to decrypt data')
    }
  }

  /**
   * 手動Base64エンコーディング（UTF-8対応）
   */
  private manualBase64Encode(str: string): string {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/='
    let result = ''
    let i = 0
    
    while (i < str.length) {
      const a = str.charCodeAt(i++)
      const b = i < str.length ? str.charCodeAt(i++) : 0
      const c = i < str.length ? str.charCodeAt(i++) : 0
      
      const n = (a << 16) | (b << 8) | c
      
      result += chars.charAt((n >> 18) & 63)
      result += chars.charAt((n >> 12) & 63)
      result += i - 2 < str.length ? chars.charAt((n >> 6) & 63) : '='
      result += i - 1 < str.length ? chars.charAt(n & 63) : '='
    }
    
    return result
  }

  /**
   * 手動Base64デコーディング（UTF-8対応）
   */
  private manualBase64Decode(str: string): string {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/='
    let result = ''
    let i = 0
    
    str = str.replace(/[^A-Za-z0-9+/]/g, '')
    
    while (i < str.length) {
      const encoded1 = chars.indexOf(str.charAt(i++))
      const encoded2 = chars.indexOf(str.charAt(i++))
      const encoded3 = chars.indexOf(str.charAt(i++))
      const encoded4 = chars.indexOf(str.charAt(i++))
      
      const n = (encoded1 << 18) | (encoded2 << 12) | (encoded3 << 6) | encoded4
      
      result += String.fromCharCode((n >> 16) & 255)
      if (encoded3 !== 64) result += String.fromCharCode((n >> 8) & 255)
      if (encoded4 !== 64) result += String.fromCharCode(n & 255)
    }
    
    return result
  }

  /**
   * 暗号化キーを生成または取得
   */
  private getOrCreateKey(): string {
    const keyName = 'message_app_key'
    let key = localStorage.getItem(keyName)
    
    if (!key) {
      // ランダムなキーを生成
      key = Math.random().toString(36).substring(2, 15) + 
            Math.random().toString(36).substring(2, 15) +
            Date.now().toString(36)
      localStorage.setItem(keyName, key)
    }
    
    return key
  }

  /**
   * トークンの有効期限をチェック
   */
  isTokenExpired(token: string): boolean {
    if (!token) return true

    try {
      // JWTトークンの場合、payloadから有効期限を取得
      const parts = token.split('.')
      if (parts.length !== 3) {
        console.warn('Invalid JWT format:', { partsLength: parts.length })
        return true
      }

      const payload = JSON.parse(atob(parts[1]))
      const exp = payload.exp * 1000 // 秒から ミリ秒に変換
      const now = Date.now()
      const isExpired = now >= exp
      
      console.log('JWT expiration check:', {
        exp: new Date(exp).toISOString(),
        now: new Date(now).toISOString(),
        isExpired,
        remainingMs: exp - now
      })
      
      return isExpired
    } catch (error) {
      console.warn('Failed to parse token expiration:', error)
      return true
    }
  }
}

export const secureStorage = new SecureStorage()

/**
 * 認証トークン管理用のヘルパー関数
 */
export const tokenManager = {
  /**
   * 認証トークンを保存
   */
  setAuthToken(token: string): void {
    // トークンの有効期限を23時間に設定（サーバー側より少し短く）
    secureStorage.setItem('auth_token', token, 23 * 60 * 60 * 1000)
  },

  /**
   * 認証トークンを取得
   */
  getAuthToken(): string | null {
    const token = secureStorage.getItem('auth_token')
    
    if (token) {
      const isExpired = secureStorage.isTokenExpired(token)
      console.log('Token check:', {
        hasToken: !!token,
        isExpired,
        tokenPrefix: token.substring(0, 20) + '...'
      })
      
      if (isExpired) {
        console.warn('Token expired, removing from storage')
        secureStorage.removeItem('auth_token')
        return null
      }
    }
    
    return token
  },

  /**
   * ユーザー情報を保存
   */
  setUserData(userData: any): void {
    secureStorage.setItem('auth_user', JSON.stringify(userData))
  },

  /**
   * ユーザー情報を取得
   */
  getUserData(): any | null {
    const userData = secureStorage.getItem('auth_user')
    return userData ? JSON.parse(userData) : null
  },

  /**
   * 全ての認証データをクリア
   */
  clearAuthData(): void {
    secureStorage.clearAuthData()
  },

  /**
   * トークンが有効かチェック
   */
  isTokenValid(): boolean {
    const token = this.getAuthToken()
    return token !== null && !secureStorage.isTokenExpired(token)
  }
}