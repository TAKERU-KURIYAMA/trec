<template>
  <aside class="sidebar" :class="{ 'sidebar-open': isOpen }">
    <!-- Sidebar Header -->
    <div class="sidebar-header">
      <div class="sidebar-brand">
        <Icon name="mdi:dumbbell" size="32" />
        <div class="brand-text">
          <h3>Message</h3>
          <span>トレーニング</span>
        </div>
      </div>
      <button 
        @click="toggleSidebar" 
        class="sidebar-toggle"
        :class="{ 'is-open': isOpen }"
      >
        <Icon name="mdi:menu" size="24" />
      </button>
    </div>

    <!-- Navigation Menu -->
    <nav class="sidebar-nav">
      <!-- Main Features -->
      <div class="nav-section">
        <h4 class="nav-section-title">メイン</h4>
        <NuxtLink 
          to="/" 
          class="nav-item"
          :class="{ active: $route.path === '/' }"
          @click="closeSidebarOnMobile"
        >
          <Icon name="mdi:format-list-bulleted" />
          <span>メニュー一覧</span>
        </NuxtLink>
        
        <NuxtLink 
          v-if="authStore.isAuthenticated"
          to="/dashboard" 
          class="nav-item"
          :class="{ active: $route.path === '/dashboard' }"
          @click="closeSidebarOnMobile"
        >
          <Icon name="mdi:chart-line" />
          <span>ダッシュボード</span>
        </NuxtLink>
      </div>

      <!-- Training Features (requires authentication) -->
      <div v-if="authStore.isAuthenticated" class="nav-section">
        <h4 class="nav-section-title">トレーニング</h4>
        <NuxtLink 
          to="/training/history" 
          class="nav-item"
          :class="{ active: $route.path.includes('/training/history') }"
          @click="closeSidebarOnMobile"
        >
          <Icon name="mdi:history" />
          <span>履歴</span>
        </NuxtLink>
        
        <NuxtLink 
          to="/training/presets" 
          class="nav-item"
          :class="{ active: $route.path.includes('/training/presets') }"
          @click="closeSidebarOnMobile"
        >
          <Icon name="mdi:bookmark-multiple" />
          <span>マイセット</span>
        </NuxtLink>
        
        <NuxtLink 
          to="/training/schedule" 
          class="nav-item"
          :class="{ active: $route.path.includes('/training/schedule') }"
          @click="closeSidebarOnMobile"
        >
          <Icon name="mdi:calendar-month" />
          <span>予定</span>
        </NuxtLink>
      </div>

      <!-- Admin Section (only visible to admin and after auth loading) -->
      <div v-if="showAdminMenu" class="nav-section admin-section">
        <h4 class="nav-section-title">管理</h4>
        <NuxtLink 
          to="/admin/menus" 
          class="nav-item admin-item"
          :class="{ active: $route.path.includes('/admin/menus') }"
          @click="closeSidebarOnMobile"
        >
          <Icon name="mdi:cog" />
          <span>メニュー管理</span>
        </NuxtLink>
        
        <NuxtLink 
          to="/admin/tags" 
          class="nav-item admin-item"
          :class="{ active: $route.path.includes('/admin/tags') }"
          @click="closeSidebarOnMobile"
        >
          <Icon name="mdi:tag-multiple" />
          <span>タグ管理</span>
        </NuxtLink>
      </div>

      <!-- Tools -->
      <div class="nav-section">
        <h4 class="nav-section-title">ツール</h4>
        <NuxtLink 
          to="/tools/1rm-calculator" 
          class="nav-item"
          :class="{ active: $route.path.includes('/tools/1rm') }"
          @click="closeSidebarOnMobile"
        >
          <Icon name="mdi:calculator" />
          <span>1RM計算</span>
        </NuxtLink>
        
        <NuxtLink 
          to="/tools/timer" 
          class="nav-item"
          :class="{ active: $route.path.includes('/tools/timer') }"
          @click="closeSidebarOnMobile"
        >
          <Icon name="mdi:timer" />
          <span>タイマー</span>
        </NuxtLink>
        
        <NuxtLink 
          v-if="authStore.isAuthenticated"
          to="/goals" 
          class="nav-item"
          :class="{ active: $route.path.includes('/goals') }"
          @click="closeSidebarOnMobile"
        >
          <Icon name="mdi:target" />
          <span>目標管理</span>
        </NuxtLink>
      </div>

      <!-- Quick Actions (requires authentication) -->
      <div v-if="authStore.isAuthenticated" class="nav-section">
        <h4 class="nav-section-title">クイックアクション</h4>
        <button 
          @click="startQuickWorkout" 
          class="nav-item nav-action"
        >
          <Icon name="mdi:play-circle" />
          <span>トレーニング開始</span>
        </button>
        
        <button 
          @click="openTodaysHistory" 
          class="nav-item nav-action"
        >
          <Icon name="mdi:pencil" />
          <span>今日の記録編集</span>
        </button>
      </div>
    </nav>

    <!-- Sidebar Footer -->
    <div class="sidebar-footer">
      <!-- When not authenticated -->
      <div v-if="!authStore.isAuthenticated" class="auth-section">
        <div class="auth-buttons">
          <button @click="openLoginModal" class="sidebar-auth-btn login-btn">
            <Icon name="mdi:login" size="20" />
            <span>ログイン</span>
          </button>
          <button @click="openRegisterModal" class="sidebar-auth-btn register-btn">
            <Icon name="mdi:account-plus" size="20" />
            <span>新規登録</span>
          </button>
        </div>
      </div>
      
      <!-- When authenticated -->
      <div v-else class="user-info">
        <Icon name="mdi:account-circle" size="40" />
        <div class="user-details">
          <span class="user-name">{{ authStore.user?.displayName || 'ユーザー' }}</span>
          <span class="user-status">ログイン中</span>
        </div>
        <button @click="authStore.logout" class="sidebar-logout-btn">
          <Icon name="mdi:logout" size="20" />
        </button>
      </div>
    </div>
  </aside>

  <!-- Backdrop for mobile -->
  <div 
    v-if="isOpen && isMobile" 
    class="sidebar-backdrop"
    @click="closeSidebar"
  ></div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '~/stores/auth'
import { globalAuthModal } from '~/composables/useAuthModal'

const router = useRouter()
const authStore = useAuthStore()
const { openLogin, openRegister } = globalAuthModal

// State
const isOpen = ref(false)
const isMobile = ref(false)

// Computed
const sidebarClasses = computed(() => ({
  'sidebar-open': isOpen.value,
  'sidebar-mobile': isMobile.value
}))

// Admin menu visibility - separate computed for better reactivity
const showAdminMenu = computed(() => {
  const isAuthenticated = authStore.isAuthenticated
  const isAdmin = authStore.isAdmin
  const isLoading = authStore.isLoading
  
  console.log('🔍 Admin menu visibility check:', {
    isAuthenticated,
    isAdmin,
    isLoading,
    shouldShow: isAuthenticated && isAdmin
  })
  
  // Show admin menu if user is authenticated and is admin
  // Don't let loading state hide it if we already know they're admin
  return isAuthenticated && isAdmin
})

// Methods
function toggleSidebar() {
  isOpen.value = !isOpen.value
}

function closeSidebar() {
  isOpen.value = false
}

function closeSidebarOnMobile() {
  if (isMobile.value) {
    closeSidebar()
  }
}

function checkMobile() {
  isMobile.value = window.innerWidth < 1024
  if (!isMobile.value) {
    isOpen.value = true // Auto-open on desktop
  }
}

function startQuickWorkout() {
  // Navigate to menu selection for quick workout
  router.push('/')
  closeSidebarOnMobile()
}

function openTodaysHistory() {
  // Navigate to today's history editing
  const today = new Date().toISOString().split('T')[0]
  router.push(`/training/history?date=${today}`)
  closeSidebarOnMobile()
}

function openLoginModal() {
  openLogin()
  closeSidebarOnMobile()
}

function openRegisterModal() {
  openRegister()
  closeSidebarOnMobile()
}

// Lifecycle
onMounted(() => {
  checkMobile()
  window.addEventListener('resize', checkMobile)
})

onUnmounted(() => {
  window.removeEventListener('resize', checkMobile)
})

// Expose toggle function for parent components
defineExpose({
  toggleSidebar,
  closeSidebar
})
</script>

<style scoped>
/* ===================================
   Sidebar Base Styles
   =================================== */

.sidebar {
  position: fixed;
  top: 0;
  left: 0;
  height: 100vh;
  width: 280px;
  background: linear-gradient(180deg, #2c3e50 0%, #3498db 100%);
  color: white;
  display: flex;
  flex-direction: column;
  transform: translateX(-100%);
  transition: transform 0.3s ease;
  z-index: 1000;
  box-shadow: 2px 0 10px rgba(0, 0, 0, 0.1);
}

.sidebar-open {
  transform: translateX(0);
}

/* ===================================
   Sidebar Header
   =================================== */

.sidebar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  background: rgba(0, 0, 0, 0.1);
}

.sidebar-brand {
  display: flex;
  align-items: center;
  gap: 12px;
}

.brand-text h3 {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 700;
}

.brand-text span {
  font-size: 0.85rem;
  opacity: 0.8;
}

.sidebar-toggle {
  background: none;
  border: none;
  color: white;
  cursor: pointer;
  padding: 8px;
  border-radius: 6px;
  transition: all 0.2s ease;
  display: none;
}

.sidebar-toggle:hover {
  background: rgba(255, 255, 255, 0.1);
}

/* ===================================
   Navigation
   =================================== */

.sidebar-nav {
  flex: 1;
  padding: 20px 0;
  overflow-y: auto;
}

.nav-section {
  margin-bottom: 25px;
}

.nav-section-title {
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 1px;
  margin: 0 0 12px 20px;
  opacity: 0.6;
  font-weight: 600;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 20px;
  color: white;
  text-decoration: none;
  transition: all 0.2s ease;
  border: none;
  background: none;
  width: 100%;
  cursor: pointer;
  font-size: 0.95rem;
}

.nav-item:hover {
  background: rgba(255, 255, 255, 0.1);
  padding-left: 25px;
}

.nav-item.active {
  background: rgba(255, 255, 255, 0.15);
  border-right: 3px solid #fff;
  font-weight: 600;
}

.nav-action {
  font-family: inherit;
  text-align: left;
}

/* ===================================
   Admin Section
   =================================== */

.admin-section {
  border: 1px solid rgba(255, 215, 0, 0.3);
  border-radius: 8px;
  margin: 20px 15px;
  padding: 15px 0;
  background: rgba(255, 215, 0, 0.1);
}

.admin-section .nav-section-title {
  color: #ffd700;
  margin-left: 15px;
}

.admin-item {
  border-left: 3px solid transparent;
  margin: 0 10px;
  border-radius: 4px;
}

.admin-item:hover {
  background: rgba(255, 215, 0, 0.2);
  border-left-color: #ffd700;
}

.admin-item.active {
  background: rgba(255, 215, 0, 0.25);
  border-left-color: #ffd700;
  border-right: none;
}

/* ===================================
   Sidebar Footer
   =================================== */

.sidebar-footer {
  padding: 20px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  background: rgba(0, 0, 0, 0.1);
}

/* ===================================
   Auth Section (Not authenticated)
   =================================== */

.auth-section {
  width: 100%;
}

.auth-buttons {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.sidebar-auth-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 12px 16px;
  border: none;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  color: white;
}

.sidebar-auth-btn.login-btn {
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.3);
}

.sidebar-auth-btn.login-btn:hover {
  background: rgba(255, 255, 255, 0.2);
  border-color: rgba(255, 255, 255, 0.5);
}

.sidebar-auth-btn.register-btn {
  background: rgba(34, 197, 94, 0.8);
  border: 1px solid rgba(34, 197, 94, 0.6);
}

.sidebar-auth-btn.register-btn:hover {
  background: rgba(34, 197, 94, 0.9);
  border-color: rgba(34, 197, 94, 0.8);
}

/* ===================================
   User Info (Authenticated)
   =================================== */

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
  position: relative;
}

.user-details {
  display: flex;
  flex-direction: column;
  flex: 1;
}

.user-name {
  font-weight: 600;
  font-size: 0.95rem;
}

.user-status {
  font-size: 0.8rem;
  opacity: 0.7;
}

.sidebar-logout-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  background: rgba(239, 68, 68, 0.8);
  border: 1px solid rgba(239, 68, 68, 0.6);
  border-radius: 6px;
  color: white;
  cursor: pointer;
  transition: all 0.2s ease;
}

.sidebar-logout-btn:hover {
  background: rgba(239, 68, 68, 0.9);
  border-color: rgba(239, 68, 68, 0.8);
}

/* ===================================
   Mobile Backdrop
   =================================== */

.sidebar-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 999;
}

/* ===================================
   Responsive Design
   =================================== */

@media (max-width: 1023px) {
  .sidebar {
    z-index: 2000;
  }
  
  .sidebar-toggle {
    display: block;
  }
  
  .sidebar-header {
    padding: 15px 20px;
  }
  
  .nav-item {
    padding: 14px 20px;
    font-size: 1rem;
  }
}

@media (min-width: 1024px) {
  .sidebar {
    position: relative;
    transform: translateX(0);
    box-shadow: none;
    border-right: 1px solid rgba(255, 255, 255, 0.1);
  }
  
  .sidebar-backdrop {
    display: none;
  }
}

/* ===================================
   Accessibility
   =================================== */

@media (prefers-reduced-motion: reduce) {
  .sidebar,
  .nav-item {
    transition: none;
  }
}

.nav-item:focus {
  outline: 2px solid rgba(255, 255, 255, 0.5);
  outline-offset: -2px;
}

.sidebar-toggle:focus {
  outline: 2px solid rgba(255, 255, 255, 0.5);
  outline-offset: 2px;
}
</style>