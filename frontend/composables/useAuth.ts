export const useAuth = () => {
  const isLoggedIn = ref(false)
  const userInfo = ref<any>(null)
  const isAdmin = ref(false)

  const checkAuthStatus = async () => {
    try {
      const token = localStorage.getItem('auth_token')
      if (!token) {
        isLoggedIn.value = false
        isAdmin.value = false
        return false
      }

      // トークンの有効性をチェック
      const response = await $fetch('/api/auth/verify', {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      })

      if (response.success) {
        isLoggedIn.value = true
        userInfo.value = response.data
        
        // 管理者権限をチェック
        await checkAdminStatus()
        return true
      }
    } catch (error) {
      console.error('Auth check failed:', error)
    }

    isLoggedIn.value = false
    isAdmin.value = false
    return false
  }

  const checkAdminStatus = async () => {
    try {
      const token = localStorage.getItem('auth_token')
      if (!token) {
        isAdmin.value = false
        return false
      }

      const response = await $fetch('/api/admin/check', {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      })

      isAdmin.value = response.success
      return response.success
    } catch (error) {
      isAdmin.value = false
      return false
    }
  }

  const login = async (loginId: string, password: string) => {
    try {
      // パスワードをSHA256でハッシュ化
      const { hashPassword } = await import('~/utils/crypto')
      const hashedPassword = await hashPassword(password)

      const response = await $fetch('/api/auth/login', {
        method: 'POST',
        body: {
          login_id: loginId,
          password: hashedPassword
        }
      })

      if (response.success) {
        localStorage.setItem('auth_token', response.data.token)
        await checkAuthStatus()
        return { success: true }
      }

      return { success: false, message: response.message }
    } catch (error: any) {
      return { 
        success: false, 
        message: error.data?.message || 'ログインに失敗しました' 
      }
    }
  }

  const logout = () => {
    localStorage.removeItem('auth_token')
    isLoggedIn.value = false
    isAdmin.value = false
    userInfo.value = null
  }

  return {
    isLoggedIn: readonly(isLoggedIn),
    isAdmin: readonly(isAdmin),
    userInfo: readonly(userInfo),
    checkAuthStatus,
    checkAdminStatus,
    login,
    logout
  }
}