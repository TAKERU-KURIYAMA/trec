<template>
  <div class="home-page">
    <!-- Welcome Section -->
    <section class="welcome-section">
      <div class="welcome-content">
        <h1 class="welcome-title">🏋️‍♂️ トレーニングメニュー</h1>
        <p class="welcome-description">
          あなたの目標達成をサポートする、豊富なトレーニングメニューから選択してください
        </p>
        
        <!-- Quick Stats -->
        <div class="quick-stats">
          <div class="stat-card">
            <div class="stat-icon">📊</div>
            <div class="stat-content">
              <span class="stat-number">{{ stats.totalMenus }}</span>
              <span class="stat-label">利用可能メニュー</span>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">🎯</div>
            <div class="stat-content">
              <span class="stat-number">{{ stats.totalTags }}</span>
              <span class="stat-label">カテゴリー</span>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">💪</div>
            <div class="stat-content">
              <span class="stat-number">{{ stats.filteredCount }}</span>
              <span class="stat-label">表示中</span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Search and Filters -->
    <section class="search-section">
      <div class="search-container">
        <div class="search-input-group">
          <div class="search-icon">🔍</div>
          <input 
            v-model="searchQuery" 
            class="search-input"
            placeholder="メニュー名やタグで検索..." 
          />
          <button v-if="searchQuery" @click="clearSearch" class="clear-btn">✕</button>
        </div>
        
        <div class="filter-tags">
          <button 
            @click="resetFilters"
            class="tag-filter"
            :class="{ active: selectedTag === '' }"
          >
            すべて
          </button>
          <button
            v-for="tagGroup in popularTags"
            :key="tagGroup.tagId"
            @click="handleTagClick(tagGroup.tagId)"
            class="tag-filter"
            :class="{ active: selectedTag === tagGroup.tagId }"
          >
            {{ tagGroup.icon }} {{ tagGroup.name }}
            <span class="tag-count">({{ tagGroup.count }})</span>
          </button>
        </div>
      </div>
    </section>

    <!-- Menu Grid -->
    <section class="menu-section">
      <div class="section-header">
        <h2>
          {{ filteredMenus.length > 0 ? `${filteredMenus.length}件のメニューが見つかりました` : 'メニューが見つかりませんでした' }}
        </h2>
        <div class="view-options">
          <button 
            @click="viewMode = 'grid'"
            class="view-btn"
            :class="{ active: viewMode === 'grid' }"
          >
            ⊞ グリッド
          </button>
          <button 
            @click="viewMode = 'list'"
            class="view-btn"
            :class="{ active: viewMode === 'list' }"
          >
            ☰ リスト
          </button>
        </div>
      </div>

      <div v-if="loading.isLoading" class="loading-state">
        <div class="loading-spinner">⚡</div>
        <p>{{ loading.message || 'メニューを読み込み中...' }}</p>
      </div>

      <div v-else-if="error.hasError" class="error-state">
        <div class="error-icon">⚠️</div>
        <h3>エラーが発生しました</h3>
        <p>{{ error.message }}</p>
        <button v-if="error.retryable" @click="handleRetry" class="btn btn-primary">再試行</button>
        <button @click="resetFilters" class="btn btn-secondary">フィルターをリセット</button>
      </div>

      <div v-else-if="filteredMenus.length === 0" class="empty-state">
        <div class="empty-icon">🔍</div>
        <h3>メニューが見つかりませんでした</h3>
        <p>検索条件を変更するか、フィルターをリセットしてください</p>
        <button @click="resetFilters" class="btn btn-primary">フィルターをリセット</button>
      </div>

      <div v-else class="menu-grid" :class="`view-${viewMode}`">
        <EnhancedTrainingCard
          v-for="menu in filteredMenus"
          :key="menu.menuId"
          :menu="menu"
          :view-mode="viewMode"
          @click="goToMenu(menu.menuId)"
          @favorite="toggleFavorite(menu)"
        />
      </div>
    </section>

    <!-- Quick Actions -->
    <section class="quick-actions">
      <h3>クイックアクション</h3>
      <div class="action-grid">
        <NuxtLink to="/dashboard" class="action-card">
          <div class="action-icon">📊</div>
          <div class="action-content">
            <h4>ダッシュボード</h4>
            <p>進捗と統計を確認</p>
          </div>
        </NuxtLink>
        
        <button @click="startRandomWorkout" class="action-card">
          <div class="action-icon">🎲</div>
          <div class="action-content">
            <h4>ランダムワークアウト</h4>
            <p>おまかせでメニュー選択</p>
          </div>
        </button>
        
        <button @click="createCustomWorkout" class="action-card">
          <div class="action-icon">➕</div>
          <div class="action-content">
            <h4>カスタムワークアウト</h4>
            <p>オリジナルメニュー作成</p>
          </div>
        </button>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useTrainingMenus } from '~/composables/useTrainingMenus'
import { useGlobalNotifications } from '~/composables/useNotifications'
import EnhancedTrainingCard from '~/components/EnhancedTrainingCard.vue'
import type { TrainingMenu } from '~/types'

const router = useRouter()
const notifications = useGlobalNotifications()

// Ultra-refactored menu management with advanced features
const {
  menus,
  allMenus,
  tags,
  loading,
  error,
  stats,
  tagStats,
  searchSuggestions,
  popularMenus,
  setSearchKeyword,
  toggleTagFilter,
  clearFilters: clearAllFilters,
  setSortOrder,
  retry
} = useTrainingMenus({
  autoLoad: true,
  autoRefreshInterval: 5 * 60 * 1000, // 5分間隔で自動更新
  initialFilter: {
    sortBy: 'jpName',
    sortOrder: 'asc'
  }
})

// UI state
const viewMode = ref<'grid' | 'list'>('grid')
const favorites = ref(new Set<string>())
const searchQuery = ref('')
const selectedTag = ref('')

// ===================================
// Computed Properties
// ===================================

const uniqueTags = computed(() => {
  return tags.value.map(tag => tag.jpName).filter(Boolean)
})

const popularTags = computed(() => {
  return tagStats.value.slice(0, 6).map(stat => ({
    name: stat.tag?.jpName || '',
    icon: getTagIcon(stat.tag?.jpName || ''),
    count: stat.count,
    tagId: stat.tag?.tagId || ''
  })).filter(tag => tag.name)
})

const filteredMenus = computed(() => {
  return menus.value.map(menu => ({
    ...menu,
    isFavorite: favorites.value.has(menu.menuId)
  }))
})

// ===================================
// Helper Functions
// ===================================

function getTagIcon(tagName: string): string {
  const iconMap: Record<string, string> = {
    '上半身': '💪',
    '下半身': '🦵', 
    '有酸素': '🫀',
    'コア': '🎯',
    '背中': '🏋️',
    '胸': '💪',
    '肩': '🤲',
    '腕': '💪',
    '脚': '🦵',
    '腹筋': '🎯'
  }
  return iconMap[tagName] || '🏃'
}

// ===================================
// Event Handlers
// ===================================

function clearSearch() {
  searchQuery.value = ''
  setSearchKeyword('')
}

function resetFilters() {
  searchQuery.value = ''
  selectedTag.value = ''
  clearAllFilters()
}

function handleTagClick(tagId: string) {
  if (selectedTag.value === tagId) {
    selectedTag.value = ''
  } else {
    selectedTag.value = tagId
  }
  toggleTagFilter(tagId)
}

function handleSearchInput() {
  setSearchKeyword(searchQuery.value)
}

// ===================================
// Watchers
// ===================================

// 検索クエリの変更を監視
watch(searchQuery, () => {
  handleSearchInput()
})

// エラー処理
watch(error, (newError) => {
  if (newError.hasError) {
    notifications.error('エラー', newError.message)
  }
}, { deep: true })

// ===================================
// Lifecycle
// ===================================

onMounted(() => {
  // Load favorites from localStorage
  const savedFavorites = localStorage.getItem('favoriteMenus')
  if (savedFavorites) {
    try {
      favorites.value = new Set(JSON.parse(savedFavorites))
    } catch (err) {
      console.warn('お気に入りの読み込みに失敗しました:', err)
    }
  }

  // Success notification when menus are loaded
  watch(() => stats.value.totalMenus, (count) => {
    if (count > 0) {
      notifications.success('読み込み完了', `${count}件のメニューを読み込みました`)
    }
  }, { immediate: true })
})

// ===================================
// Menu Actions  
// ===================================

function goToMenu(menuId: string) {
  router.push(`/training/history?menuId=${menuId}`)
}

function toggleFavorite(menu: TrainingMenu) {
  if (favorites.value.has(menu.menuId)) {
    favorites.value.delete(menu.menuId)
    notifications.info('お気に入り解除', `${menu.jpName}をお気に入りから削除しました`)
  } else {
    favorites.value.add(menu.menuId)
    notifications.success('お気に入り登録', `${menu.jpName}をお気に入りに追加しました`)
  }
  
  // Save to localStorage
  localStorage.setItem('favoriteMenus', JSON.stringify(Array.from(favorites.value)))
}

function startRandomWorkout() {
  if (allMenus.value.length > 0) {
    const randomMenu = allMenus.value[Math.floor(Math.random() * allMenus.value.length)]
    notifications.info('ランダムワークアウト', `${randomMenu.jpName}が選ばれました！`)
    goToMenu(randomMenu.menuId)
  } else {
    notifications.warning('メニューなし', 'まだメニューが読み込まれていません')
  }
}

function createCustomWorkout() {
  // Navigate to custom workout creation page
  notifications.info('開発中', 'カスタムワークアウト機能は開発中です')
  // router.push('/training/create')
}

function handleRetry() {
  notifications.info('再試行中', 'メニューを再読み込みしています...')
  retry()
}

// Page metadata
useHead({
  title: 'トレーニングメニュー - Message',
  meta: [
    { name: 'description', content: 'あなたの目標達成をサポートする豊富なトレーニングメニュー' }
  ]
})
</script>

<style scoped>
.home-page {
  max-width: 1200px;
  margin: 0 auto;
}

/* Welcome Section */
.welcome-section {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 40px 20px;
  border-radius: 12px;
  margin-bottom: 30px;
  text-align: center;
}

.welcome-title {
  font-size: 2.5rem;
  margin-bottom: 15px;
  font-weight: bold;
}

.welcome-description {
  font-size: 1.1rem;
  margin-bottom: 30px;
  opacity: 0.9;
  line-height: 1.6;
}

.quick-stats {
  display: flex;
  justify-content: center;
  gap: 30px;
  flex-wrap: wrap;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 10px;
  background: rgba(255, 255, 255, 0.15);
  padding: 15px 20px;
  border-radius: 10px;
  backdrop-filter: blur(10px);
}

.stat-icon {
  font-size: 1.5rem;
}

.stat-content {
  text-align: left;
}

.stat-number {
  display: block;
  font-size: 1.8rem;
  font-weight: bold;
}

.stat-label {
  font-size: 0.9rem;
  opacity: 0.9;
}

/* Search Section */
.search-section {
  margin-bottom: 30px;
}

.search-container {
  background: white;
  padding: 25px;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
}

.search-input-group {
  position: relative;
  margin-bottom: 20px;
}

.search-icon {
  position: absolute;
  left: 15px;
  top: 50%;
  transform: translateY(-50%);
  font-size: 1.2rem;
  color: #666;
}

.search-input {
  width: 100%;
  padding: 15px 50px 15px 45px;
  border: 2px solid #e0e0e0;
  border-radius: 10px;
  font-size: 1rem;
  transition: border-color 0.2s;
}

.search-input:focus {
  outline: none;
  border-color: #3498db;
}

.clear-btn {
  position: absolute;
  right: 15px;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  cursor: pointer;
  color: #666;
  font-size: 1.1rem;
}

.filter-tags {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

.tag-filter {
  padding: 8px 15px;
  border: 2px solid #e0e0e0;
  border-radius: 20px;
  background: white;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 0.9rem;
}

.tag-filter:hover {
  border-color: #3498db;
  background: #f8f9fa;
}

.tag-filter.active {
  background: #3498db;
  color: white;
  border-color: #3498db;
}

.tag-count {
  opacity: 0.7;
  font-size: 0.8rem;
}

/* Menu Section */
.menu-section {
  margin-bottom: 30px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  flex-wrap: wrap;
  gap: 15px;
}

.section-header h2 {
  color: #2c3e50;
  margin: 0;
}

.view-options {
  display: flex;
  gap: 5px;
}

.view-btn {
  padding: 8px 15px;
  border: 2px solid #e0e0e0;
  border-radius: 6px;
  background: white;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 0.9rem;
}

.view-btn:hover {
  border-color: #3498db;
}

.view-btn.active {
  background: #3498db;
  color: white;
  border-color: #3498db;
}

.loading-state,
.empty-state,
.error-state {
  text-align: center;
  padding: 60px 20px;
  color: #666;
}

.loading-spinner {
  font-size: 3rem;
  margin-bottom: 15px;
  animation: pulse 1.5s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 0.5; }
  50% { opacity: 1; }
}

.empty-icon,
.error-icon {
  font-size: 4rem;
  margin-bottom: 20px;
}

.empty-state h3,
.error-state h3 {
  margin-bottom: 10px;
  color: #2c3e50;
}

.error-state .error-icon {
  color: #e74c3c;
}

.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 1rem;
  margin: 0 5px;
  transition: all 0.2s;
}

.btn-primary {
  background: #3498db;
  color: white;
}

.btn-primary:hover {
  background: #2980b9;
}

.btn-secondary {
  background: #95a5a6;
  color: white;
}

.btn-secondary:hover {
  background: #7f8c8d;
}

.menu-grid {
  display: grid;
  gap: 20px;
}

.menu-grid.view-grid {
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
}

.menu-grid.view-list {
  grid-template-columns: 1fr;
}

/* Quick Actions */
.quick-actions {
  background: white;
  padding: 25px;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
}

.quick-actions h3 {
  margin-bottom: 20px;
  color: #2c3e50;
}

.action-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 15px;
}

.action-card {
  display: flex;
  align-items: center;
  gap: 15px;
  padding: 20px;
  border: 2px solid #e0e0e0;
  border-radius: 10px;
  background: white;
  cursor: pointer;
  transition: all 0.2s;
  text-decoration: none;
  color: inherit;
}

.action-card:hover {
  border-color: #3498db;
  transform: translateY(-2px);
  box-shadow: 0 4px 15px rgba(52, 152, 219, 0.15);
}

.action-icon {
  font-size: 2rem;
  flex-shrink: 0;
}

.action-content h4 {
  margin: 0 0 5px 0;
  color: #2c3e50;
}

.action-content p {
  margin: 0;
  color: #666;
  font-size: 0.9rem;
}

@media (max-width: 768px) {
  .welcome-title {
    font-size: 2rem;
  }
  
  .quick-stats {
    gap: 15px;
  }
  
  .stat-card {
    padding: 10px 15px;
  }
  
  .section-header {
    flex-direction: column;
    align-items: stretch;
  }
  
  .view-options {
    justify-content: center;
  }
  
  .menu-grid.view-grid {
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  }
  
  .action-grid {
    grid-template-columns: 1fr;
  }
}
</style>
