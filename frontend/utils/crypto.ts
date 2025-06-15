/**
 * パスワードのSHA256ハッシュ化ユーティリティ
 * フロントエンド側でパスワードをハッシュ化してからサーバーに送信する
 */

import CryptoJS from 'crypto-js'

/**
 * 文字列をSHA256でハッシュ化
 * @param text ハッシュ化する文字列
 * @returns SHA256ハッシュ値（16進数文字列）
 */
export async function sha256Hash(text: string): Promise<string> {
  try {
    // Web Crypto APIが利用可能かチェック
    if (crypto && crypto.subtle) {
      // Web Crypto APIを使用してSHA256ハッシュを計算
      const encoder = new TextEncoder()
      const data = encoder.encode(text)
      const hashBuffer = await crypto.subtle.digest('SHA-256', data)
      
      // ArrayBufferを16進数文字列に変換
      const hashArray = Array.from(new Uint8Array(hashBuffer))
      const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('')
      
      return hashHex
    } else {
      throw new Error('Web Crypto API is not available')
    }
  } catch (error) {
    try {
      // crypto-jsを使用したフォールバック
      const hash = CryptoJS.SHA256(text).toString(CryptoJS.enc.Hex)
      return hash
    } catch (fallbackError) {
      console.error('❌ All hash methods failed:', fallbackError)
      
      // セキュリティ上の理由により、平文の送信は禁止
      throw new Error('パスワードのハッシュ化に失敗しました。ブラウザを更新して再度お試しください。')
    }
  }
}

/**
 * パスワードをクライアント側でハッシュ化
 * @param password 平文パスワード
 * @returns SHA256ハッシュ値（64文字の16進数文字列）
 */
export async function hashPassword(password: string): Promise<string> {
  return await sha256Hash(password)
}