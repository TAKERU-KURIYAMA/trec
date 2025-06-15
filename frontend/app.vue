<template>
  <div class="app-container" :class="{ 'with-sidebar': showSidebar }">
    <!-- Sidebar for desktop -->
    <Sidebar v-if="showSidebar" ref="sidebarRef" />
    
    <!-- Main App Content -->
    <div class="app-main">
      <!-- Top Navigation (Mobile + Breadcrumb) -->
      <nav class="main-navigation">
        <div class="nav-left">
          <button 
            v-if="!showSidebar" 
            @click="toggleSidebar"
            class="mobile-menu-btn"
          >
            <Icon name="mdi:menu" size="24" />
          </button>
          <div class="nav-brand">
            <h1>💪 Message</h1>
            <span class="nav-subtitle">トレーニング記録アプリ</span>
          </div>
        </div>
        
        <!-- Breadcrumb Navigation -->
        <div class="breadcrumb" v-if="breadcrumbs.length > 0">
          <span 
            v-for="(crumb, index) in breadcrumbs" 
            :key="index"
            class="breadcrumb-item"
            :class="{ 'is-active': index === breadcrumbs.length - 1 }"
          >
            <Icon :name="crumb.icon" size="16" />
            {{ crumb.label }}
            <Icon 
              v-if="index < breadcrumbs.length - 1" 
              name="mdi:chevron-right" 
              size="16" 
              class="breadcrumb-separator"
            />
          </span>
        </div>
        
        <!-- Quick Actions (Desktop) -->
        <div class="nav-actions">
          <button @click="openQuickStart" class="quick-action-btn">
            <Icon name="mdi:play-circle" size="20" />
            <span class="quick-action-text">開始</span>
          </button>
          
          <!-- Auth Actions -->
          <div v-if="!authStore.isAuthenticated" class="auth-actions">
            <button @click="openLoginModal" class="auth-btn login-btn">
              <Icon name="mdi:login" size="20" />
              <span class="auth-btn-text">ログイン</span>
            </button>
            <button @click="openRegisterModal" class="auth-btn register-btn">
              <Icon name="mdi:account-plus" size="20" />
              <span class="auth-btn-text">登録</span>
            </button>
          </div>
          
          <!-- User Actions (when authenticated) -->
          <div v-else class="user-actions">
            <span class="user-welcome">{{ authStore.user?.displayName }}さん</span>
            <button @click="authStore.logout" class="auth-btn logout-btn">
              <Icon name="mdi:logout" size="20" />
              <span class="auth-btn-text">ログアウト</span>
            </button>
          </div>
        </div>
      </nav>
      
      <!-- Main Content Area -->
      <main class="main-content">
        <NuxtPage />
      </main>
    </div>
    
    <!-- Mobile Sidebar -->
    <Sidebar v-if="!showSidebar" ref="mobileSidebarRef" />
    
    <!-- Global Auth Modal -->
    <AuthModalProvider />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AuthModalProvider from '~/components/auth/AuthModalProvider.vue'
import { useAuthStore } from '~/stores/auth'
import { globalAuthModal } from '~/composables/useAuthModal'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const { openLogin, openRegister } = globalAuthModal

// State
const showSidebar = ref(false)
const sidebarRef = ref(null)
const mobileSidebarRef = ref(null)

// Check if desktop view
function checkDesktop() {
  if (typeof window !== 'undefined') {
    showSidebar.value = window.innerWidth >= 1024
  }
}

// Breadcrumb generation
const breadcrumbs = computed(() => {
  const path = route.path
  const crumbs = []
  
  if (path === '/') {
    crumbs.push({ label: 'メニュー一覧', icon: 'mdi:format-list-bulleted' })
  } else if (path === '/dashboard') {
    crumbs.push({ label: 'ダッシュボード', icon: 'mdi:chart-line' })
  } else if (path.includes('/training/history')) {
    crumbs.push({ label: 'トレーニング', icon: 'mdi:dumbbell' })
    crumbs.push({ label: '履歴', icon: 'mdi:history' })
  } else if (path.includes('/training/presets')) {
    crumbs.push({ label: 'トレーニング', icon: 'mdi:dumbbell' })
    crumbs.push({ label: 'マイセット', icon: 'mdi:bookmark-multiple' })
  } else if (path.includes('/training/schedule')) {
    crumbs.push({ label: 'トレーニング', icon: 'mdi:dumbbell' })
    crumbs.push({ label: '予定', icon: 'mdi:calendar-month' })
  } else if (path.includes('/training/session')) {
    crumbs.push({ label: 'トレーニング', icon: 'mdi:dumbbell' })
    crumbs.push({ label: 'セッション', icon: 'mdi:play-circle' })
  } else if (path.includes('/tools/1rm')) {
    crumbs.push({ label: 'ツール', icon: 'mdi:tools' })
    crumbs.push({ label: '1RM計算', icon: 'mdi:calculator' })
  } else if (path.includes('/tools/timer')) {
    crumbs.push({ label: 'ツール', icon: 'mdi:tools' })
    crumbs.push({ label: 'タイマー', icon: 'mdi:timer' })
  } else if (path.includes('/tools/goals')) {
    crumbs.push({ label: 'ツール', icon: 'mdi:tools' })
    crumbs.push({ label: '目標管理', icon: 'mdi:target' })
  }
  
  return crumbs
})

// Methods
function toggleSidebar() {
  if (showSidebar.value && sidebarRef.value) {
    sidebarRef.value.toggleSidebar()
  } else if (mobileSidebarRef.value) {
    mobileSidebarRef.value.toggleSidebar()
  }
}

function openQuickStart() {
  router.push('/')
}

function openLoginModal() {
  openLogin()
}

function openRegisterModal() {
  openRegister()
}

// Lifecycle
onMounted(async () => {
  checkDesktop()
  if (typeof window !== 'undefined') {
    window.addEventListener('resize', checkDesktop)
  }
  
  // 認証状態を初期化（リロード時の状態復元）
  try {
    await authStore.initAuth()
    console.log('Auth initialization completed:', {
      isAuthenticated: authStore.isAuthenticated,
      isAdmin: authStore.isAdmin,
      user: authStore.user?.displayName
    })
  } catch (error) {
    console.error('Failed to initialize auth:', error)
  }
})

onUnmounted(() => {
  if (typeof window !== 'undefined') {
    window.removeEventListener('resize', checkDesktop)
  }
})

// Meta
useHead({
  title: 'Message - トレーニング記録アプリ',
  meta: [
    { name: 'description', content: 'あなたのトレーニングを記録・分析する高機能アプリ' },
    { name: 'viewport', content: 'width=device-width, initial-scale=1' }
  ]
})
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: 'Helvetica Neue', 'Arial', 'Hiragino Sans', 'Hiragino Kaku Gothic ProN', 'Meiryo', sans-serif;
  background: #f5f7fa;
  color: #2c3e50;
}

/* ===================================
   App Layout
   =================================== */

.app-container {
  min-height: 100vh;
  display: flex;
}

.app-container.with-sidebar {
  display: flex;
}

.app-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

/* ===================================
   Top Navigation
   =================================== */

.main-navigation {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 15px 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
  position: sticky;
  top: 0;
  z-index: 100;
  min-height: 70px;
}

.nav-left {
  display: flex;
  align-items: center;
  gap: 15px;
}

.mobile-menu-btn {
  background: none;
  border: none;
  color: white;
  cursor: pointer;
  padding: 8px;
  border-radius: 6px;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

.mobile-menu-btn:hover {
  background: rgba(255, 255, 255, 0.1);
}

.nav-brand h1 {
  font-size: 1.8rem;
  margin-bottom: 2px;
}

.nav-subtitle {
  font-size: 0.9rem;
  opacity: 0.9;
}

/* ===================================
   Breadcrumb Navigation
   =================================== */

.breadcrumb {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.9rem;
  opacity: 0.9;
}

.breadcrumb-item {
  display: flex;
  align-items: center;
  gap: 6px;
}

.breadcrumb-item.is-active {
  font-weight: 600;
  opacity: 1;
}

.breadcrumb-separator {
  opacity: 0.6;
  margin: 0 4px;
}

/* ===================================
   Quick Actions
   =================================== */

.nav-actions {
  display: flex;
  gap: 10px;
  align-items: center;
}

.quick-action-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  background: rgba(255, 255, 255, 0.1);
  border: none;
  color: white;
  padding: 8px 12px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.9rem;
  font-weight: 500;
}

.quick-action-btn:hover {
  background: rgba(255, 255, 255, 0.2);
  transform: translateY(-1px);
}

.quick-action-text {
  display: none;
}

/* ===================================
   Auth Actions
   =================================== */

.auth-actions {
  display: flex;
  gap: 8px;
  align-items: center;
}

.user-actions {
  display: flex;
  gap: 12px;
  align-items: center;
}

.user-welcome {
  color: white;
  font-size: 0.9rem;
  opacity: 0.9;
  font-weight: 500;
}

.auth-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  border: none;
  padding: 6px 10px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.85rem;
  font-weight: 500;
  color: white;
}

.login-btn {
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.3);
}

.login-btn:hover {
  background: rgba(255, 255, 255, 0.2);
  border-color: rgba(255, 255, 255, 0.5);
}

.register-btn {
  background: rgba(34, 197, 94, 0.8);
  border: 1px solid rgba(34, 197, 94, 0.6);
}

.register-btn:hover {
  background: rgba(34, 197, 94, 0.9);
  border-color: rgba(34, 197, 94, 0.8);
}

.logout-btn {
  background: rgba(239, 68, 68, 0.8);
  border: 1px solid rgba(239, 68, 68, 0.6);
}

.logout-btn:hover {
  background: rgba(239, 68, 68, 0.9);
  border-color: rgba(239, 68, 68, 0.8);
}

.auth-btn-text {
  display: none;
}

/* ===================================
   Main Content
   =================================== */

.main-content {
  flex: 1;
  padding: 20px;
  max-width: 100%;
  background: #f5f7fa;
}

/* ===================================
   Desktop Layout
   =================================== */

@media (min-width: 1024px) {
  .app-container.with-sidebar {
    padding-left: 0;
  }
  
  .mobile-menu-btn {
    display: none;
  }
  
  .quick-action-text {
    display: inline;
  }
  
  .auth-btn-text {
    display: inline;
  }
  
  .nav-brand h1 {
    font-size: 1.5rem;
  }
  
  .breadcrumb {
    font-size: 0.85rem;
  }
}

/* ===================================
   Mobile Layout
   =================================== */

@media (max-width: 1023px) {
  .main-navigation {
    padding: 12px 16px;
  }
  
  .nav-brand h1 {
    font-size: 1.4rem;
  }
  
  .nav-subtitle {
    display: none;
  }
  
  .breadcrumb {
    display: none;
  }
  
  .nav-actions {
    gap: 8px;
  }
  
  .quick-action-btn {
    padding: 6px 8px;
    font-size: 0.8rem;
  }
  
  .main-content {
    padding: 15px;
  }
}

@media (max-width: 480px) {
  .main-navigation {
    padding: 10px 12px;
  }
  
  .nav-brand h1 {
    font-size: 1.2rem;
  }
  
  .quick-action-btn {
    padding: 6px;
    min-width: 36px;
  }
}

/* Global utility classes */
.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.2s;
  text-decoration: none;
  display: inline-block;
  text-align: center;
}

.btn-primary {
  background: linear-gradient(135deg, #3498db, #2980b9);
  color: white;
}

.btn-primary:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(52, 152, 219, 0.3);
}

.btn-success {
  background: linear-gradient(135deg, #27ae60, #229954);
  color: white;
}

.btn-success:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(39, 174, 96, 0.3);
}

.btn-warning {
  background: linear-gradient(135deg, #f39c12, #e67e22);
  color: white;
}

.btn-warning:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(243, 156, 18, 0.3);
}

.card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
  border: 1px solid #e9ecef;
}

.text-center {
  text-align: center;
}

.mt-1 { margin-top: 0.5rem; }
.mt-2 { margin-top: 1rem; }
.mt-3 { margin-top: 1.5rem; }
.mb-1 { margin-bottom: 0.5rem; }
.mb-2 { margin-bottom: 1rem; }
.mb-3 { margin-bottom: 1.5rem; }
</style>
