export default defineNuxtRouteMiddleware(async (to, from) => {
  const authStore = useAuthStore()
  
  console.log('🛡️ Admin middleware check:', {
    isAuthenticated: authStore.isAuthenticated,
    isAdmin: authStore.isAdmin,
    isLoading: authStore.isLoading,
    isInitialized: authStore.isInitialized,
    route: to.path
  })
  
  // 認証状態を確実に初期化
  await authStore.ensureInitialized()
  
  // 認証が読み込み中の場合は初期化完了まで待機
  if (authStore.isLoading) {
    console.log('⏳ Waiting for auth initialization to complete...')
    await new Promise<void>(resolve => {
      const checkAuth = () => {
        if (!authStore.isLoading) {
          console.log('✅ Auth initialization completed')
          resolve()
        } else {
          setTimeout(checkAuth, 50) // より短い間隔でチェック
        }
      }
      checkAuth()
    })
  }
  
  console.log('🔍 Final auth state check:', {
    isAuthenticated: authStore.isAuthenticated,
    isAdmin: authStore.isAdmin,
    shouldRedirectToLogin: !authStore.isAuthenticated,
    shouldRedirectToHome: authStore.isAuthenticated && !authStore.isAdmin
  })
  
  // 認証チェック
  if (!authStore.isAuthenticated) {
    console.log('❌ Not authenticated, redirecting to login')
    return navigateTo('/login')
  }
  
  // 管理者権限チェック
  if (!authStore.isAdmin) {
    console.log('❌ Not admin, redirecting to home')
    return navigateTo('/')
  }
  
  console.log('✅ Admin access granted')
})