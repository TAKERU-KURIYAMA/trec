import { defineStore } from 'pinia'
import { ref, computed, nextTick } from 'vue'
import { tokenManager } from '~/utils/secure-storage'
import { apiClient } from '~/utils/api-client'

export interface User {
  id: string
  loginId: string
  displayName: string
  isAdmin?: boolean
}

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(null)
  const user = ref<User | null>(null)
  const isLoading = ref(false)
  const isAdmin = ref(false)
  const isInitialized = ref(false)

  const isAuthenticated = computed(() => !!token.value && !!user.value)

  const setAuth = async (authToken: string, userData: User) => {
    token.value = authToken
    user.value = userData
    isAdmin.value = userData.isAdmin || userData.loginId === 'admin'
    
    console.log('🔐 Setting auth data:', {
      token: authToken ? `${authToken.substring(0, 20)}...` : null,
      user: userData,
      isAdmin: isAdmin.value
    })
    
    // Store securely for persistence
    if (process.client) {
      tokenManager.setAuthToken(authToken)
      tokenManager.setUserData(userData)
      
      // Global access for API client
      if (typeof window !== 'undefined') {
        window.__authToken = authToken
      }
      
      console.log('📱 Auth data stored to secure storage')
    }
    
    // Force reactive update for admin UI elements
    await nextTick()
    console.log('✅ Auth reactive update completed. Admin status:', isAdmin.value)
  }

  const clearAuth = () => {
    console.log('🗑️ Clearing authentication data')
    
    token.value = null
    user.value = null
    isAdmin.value = false
    
    // Clear secure storage
    if (process.client) {
      console.log('🗂️ Clearing data from secure storage')
      tokenManager.clearAuthData()
      
      // Clear global access
      if (typeof window !== 'undefined') {
        window.__authToken = null
      }
    }
  }

  const initAuth = async () => {
    if (process.client && !isInitialized.value) {
      isLoading.value = true
      console.log('🔧 Starting auth initialization...')
      const storedToken = tokenManager.getAuthToken()
      const storedUser = tokenManager.getUserData()
      
      console.log('Auth initialization:', {
        hasToken: !!storedToken,
        hasUser: !!storedUser,
        tokenValid: tokenManager.isTokenValid(),
        token: storedToken ? `${storedToken.substring(0, 20)}...` : null
      })
      
      if (storedToken && storedUser && tokenManager.isTokenValid()) {
        try {
          // 一時的にトークンを設定してAPIコール（API clientに認証情報を提供するため）
          token.value = storedToken
          
          try {
            // トークンの有効性を一般的な validate エンドポイントで検証
            // Authorization ヘッダーを明示的に指定
            const response = await apiClient.post('/api/auth/validate', null, {
              headers: {
                'Authorization': `Bearer ${storedToken}`
              }
            })
            
            if (response.isValid) {
              // API コールが成功した場合、認証状態を復元
              user.value = storedUser
              
              // 管理者権限を確認（storedUserのloginIdで判定）
              isAdmin.value = storedUser.loginId === 'admin' || storedUser.isAdmin === true
              
              // Global access for API client
              if (typeof window !== 'undefined') {
                window.__authToken = storedToken
              }
              
              console.log('認証状態を復元しました:', { 
                user: storedUser.displayName, 
                loginId: storedUser.loginId,
                isAdmin: isAdmin.value 
              })
              
              // 管理者権限を即座に反映させるため、強制的に再描画
              await nextTick()
            } else {
              // トークンが無効な場合
              console.warn('Token validation failed')
              clearAuth()
            }
          } catch (apiError) {
            // API エラーの場合
            console.error('Token validation error:', apiError)
            clearAuth()
          }
        } catch (error) {
          console.error('Failed to initialize auth from storage:', error)
          clearAuth()
        }
      } else {
        // トークンが無効またはユーザーデータが不正な場合
        console.log('Auth init skipped:', {
          reason: !storedToken ? 'no token' : !storedUser ? 'no user' : 'invalid token'
        })
        
        // 条件チェック詳細
        const canRestore = storedToken && !storedUser && tokenManager.isTokenValid()
        console.log('🔍 Checking if we can restore user data:', {
          hasToken: !!storedToken,
          hasUser: !!storedUser,
          tokenValid: storedToken ? tokenManager.isTokenValid() : false,
          canRestore
        })
        
        // ユーザーデータだけが欠けている場合は、APIから取得を試行
        if (canRestore) {
          console.log('🔄 Token exists but user data missing - attempting to retrieve from API')
          
          try {
            // 一時的にトークンを設定してAPIコール
            token.value = storedToken
            console.log('🔧 Set temporary token for API call')
            
            // APIからユーザー情報を取得（POSTメソッドで認証ヘッダーを明示的に指定）
            console.log('📡 Calling /api/auth/validate to restore user data')
            const response = await apiClient.post('/api/auth/validate', null, {
              headers: {
                'Authorization': `Bearer ${storedToken}`
              }
            })
            console.log('📨 API response:', response)
            
            if (response.isValid && response.claims) {
              // 取得したクレームからユーザーデータを構築
              const userData = {
                id: response.claims.user_id || response.userId,
                loginId: response.claims.login_id,
                displayName: response.claims.display_name || 'Unknown User'
              }
              
              console.log('🔨 Constructed user data:', userData)
              
              user.value = userData
              isAdmin.value = userData.loginId === 'admin' || userData.isAdmin === true
              
              // Global access for API client
              if (typeof window !== 'undefined') {
                window.__authToken = storedToken
              }
              
              // ユーザーデータを保存
              if (process.client) {
                tokenManager.setUserData(userData)
                console.log('💾 Saved user data to secure storage')
              }
              
              console.log('✅ Successfully restored user data from API:', userData)
              
              // 管理者権限を即座に反映させるため、強制的に再描画
              await nextTick()
            } else {
              console.warn('❌ Failed to retrieve user data from API - invalid response')
              console.log('Response details:', { isValid: response.isValid, hasClaims: !!response.claims })
              clearAuth()
            }
          } catch (error) {
            console.error('💥 Error retrieving user data from API:', error)
            clearAuth()
          }
        } else {
          console.log('🧹 Clearing auth completely:', {
            hasToken: !!storedToken,
            hasUser: !!storedUser,
            tokenValid: storedToken ? tokenManager.isTokenValid() : false
          })
          // トークンが無効な場合のみ完全にクリア
          clearAuth()
        }
      }
      
      isLoading.value = false
      isInitialized.value = true
      console.log('✅ Auth initialization completed')
    }
  }

  const logout = async () => {
    clearAuth()
    
    // Redirect to login page
    await navigateTo('/login')
  }

  // Auto-initialize auth when store is first accessed
  const ensureInitialized = async () => {
    if (!isInitialized.value && process.client) {
      await initAuth()
    }
  }

  return {
    token,
    user,
    isLoading,
    isAdmin,
    isAuthenticated,
    isInitialized,
    setAuth,
    clearAuth,
    initAuth,
    logout,
    ensureInitialized
  }
})