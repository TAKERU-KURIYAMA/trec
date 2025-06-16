<template>
  <div class="training-history">
    <!-- 未ログイン時のメッセージ -->
    <div v-if="!authStore.isAuthenticated" class="auth-required-container">
      <div class="auth-required-content">
        <Icon name="mdi:history" size="80" class="auth-icon" />
        <h2>トレーニング履歴を見るにはログインが必要です</h2>
        <p>ログインして、過去のトレーニング記録を確認し、進捗を追跡しましょう</p>
        <div class="auth-actions">
          <button @click="openLoginModal" class="btn btn-primary">
            <Icon name="mdi:login" />
            ログイン
          </button>
          <button @click="openRegisterModal" class="btn btn-secondary">
            <Icon name="mdi:account-plus" />
            新規登録
          </button>
        </div>
      </div>
    </div>

    <!-- ログイン済みユーザー向けコンテンツ -->
    <div v-else>
    <!-- Header -->
    <div class="page-header">
      <div class="header-content">
        <h1>トレーニング履歴</h1>
        <p>過去のワークアウト記録を確認できます</p>
      </div>
      <div class="header-actions">
        <button @click="exportTrainingHistory" class="btn btn-outline">
          <Icon name="mdi:download" />
          データエクスポート
        </button>
        <NuxtLink to="/" class="btn btn-outline">
          <Icon name="mdi:arrow-left" />
          メニュー一覧に戻る
        </NuxtLink>
        <button @click="startNewWorkout" class="btn btn-primary">
          <Icon name="mdi:plus" />
          新しいワークアウト
        </button>
      </div>
    </div>

    <!-- Quick Stats -->
    <div class="stats-section">
      <div class="stat-card">
        <div class="stat-icon">📊</div>
        <div class="stat-content">
          <span class="stat-number">{{ stats.totalWorkouts }}</span>
          <span class="stat-label">総ワークアウト数</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon">🔥</div>
        <div class="stat-content">
          <span class="stat-number">{{ stats.thisWeekWorkouts }}</span>
          <span class="stat-label">今週のセッション</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon">💪</div>
        <div class="stat-content">
          <span class="stat-number">{{ stats.totalSets }}</span>
          <span class="stat-label">総セット数</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon">⚡</div>
        <div class="stat-content">
          <span class="stat-number">{{ Math.round(stats.totalVolume) }}</span>
          <span class="stat-label">総負荷量 (kg)</span>
        </div>
      </div>
    </div>

    <!-- Filter and Search -->
    <div class="filter-section">
      <div class="filter-group">
        <select v-model="selectedMenu" class="menu-select">
          <option value="">すべてのメニュー</option>
          <option v-for="menu in menus" :key="menu.menuId" :value="menu.menuId">
            {{ menu.jpName }}
          </option>
        </select>
        <input 
          type="date" 
          v-model="startDate" 
          class="date-input"
          placeholder="開始日"
        />
        <input 
          type="date" 
          v-model="endDate" 
          class="date-input"
          placeholder="終了日"
        />
        <button @click="applyFilters" class="btn btn-primary">
          検索
        </button>
        <button @click="resetFilters" class="btn btn-secondary">
          フィルターリセット
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading.isLoading" class="loading-state">
      <div class="loading-spinner">⚡</div>
      <p>{{ loading.message || '履歴を読み込み中...' }}</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error.hasError" class="error-state">
      <div class="error-icon">⚠️</div>
      <h3>エラーが発生しました</h3>
      <p>{{ error.message }}</p>
      <button @click="retryLoad" class="btn btn-primary">再試行</button>
    </div>

    <!-- History Content -->
    <div v-else class="history-content">
      <!-- Empty State -->
      <div v-if="historyRecords.length === 0" class="empty-state">
        <div class="empty-icon">📝</div>
        <h3>まだトレーニング記録がありません</h3>
        <p>最初のワークアウトを記録してみましょう！</p>
        <button @click="startNewWorkout" class="btn btn-primary">
          <Icon name="mdi:plus" />
          ワークアウトを始める
        </button>
      </div>

      <!-- History List -->
      <div v-else class="history-list">
        <div 
          v-for="record in filteredRecords" 
          :key="record.recordId"
          class="history-item"
        >
          <div class="record-header">
            <div class="record-info">
              <h3>{{ record.menuName || getMenuName(record.menuId) }}</h3>
              <div class="record-meta">
                <span class="record-date">{{ formatDate(record.trainingDate) }}</span>
                <span class="record-time" v-if="record.createdAt">{{ formatTime(record.createdAt) }}</span>
              </div>
            </div>
            <div class="record-actions">
              <button @click="viewDetails(record)" class="btn btn-outline btn-sm">
                <Icon name="mdi:eye" />
                詳細
              </button>
              <button @click="repeatWorkout(record)" class="btn btn-primary btn-sm">
                <Icon name="mdi:repeat" />
                再実行
              </button>
            </div>
          </div>
          
          <div class="record-stats">
            <div class="stat">
              <span class="stat-label">セット数:</span>
              <span class="stat-value">{{ record.setCount }}</span>
            </div>
            <div class="stat">
              <span class="stat-label">総回数:</span>
              <span class="stat-value">{{ record.totalReps }}</span>
            </div>
            <div class="stat" v-if="record.maxWeight">
              <span class="stat-label">最大重量:</span>
              <span class="stat-value">{{ record.maxWeight }} kg</span>
            </div>
            <div class="stat" v-if="record.totalLoadAmount">
              <span class="stat-label">総負荷量:</span>
              <span class="stat-value">{{ Math.round(record.totalLoadAmount) }} kg</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Detail Modal -->
    <div v-if="showDetailModal" class="modal-overlay" @click="closeDetailModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>
            <Icon name="mdi:clipboard-text" />
            トレーニング詳細
          </h2>
          <button @click="closeDetailModal" class="modal-close-btn">
            <Icon name="mdi:close" size="24" />
          </button>
        </div>

        <div v-if="loading.isLoading" class="modal-loading">
          <div class="loading-spinner">⚡</div>
          <p>詳細を読み込み中...</p>
        </div>

        <div v-else-if="error.hasError" class="modal-error">
          <div class="error-icon">⚠️</div>
          <p>{{ error.message }}</p>
          <button @click="retryLoad" class="btn btn-primary">再試行</button>
        </div>

        <div v-else-if="selectedRecord && currentDetails" class="modal-body">
          <!-- Workout Info -->
          <div class="workout-info">
            <div class="workout-header">
              <h3>{{ selectedRecord.menuName || getMenuName(selectedRecord.menuId) }}</h3>
              <div class="workout-meta">
                <span class="workout-date">{{ formatDate(selectedRecord.trainingDate) }}</span>
                <span class="workout-time" v-if="selectedRecord.createdAt">{{ formatTime(selectedRecord.createdAt) }}</span>
              </div>
            </div>

            <!-- Summary Stats -->
            <div class="summary-stats">
              <div class="summary-stat">
                <span class="stat-label">セット数</span>
                <span class="stat-value">{{ currentDetails.summary.totalSets }}</span>
              </div>
              <div class="summary-stat">
                <span class="stat-label">総回数</span>
                <span class="stat-value">{{ currentDetails.summary.totalReps }}</span>
              </div>
              <div class="summary-stat">
                <span class="stat-label">最大重量</span>
                <span class="stat-value">{{ currentDetails.summary.maxWeight }} kg</span>
              </div>
              <div class="summary-stat">
                <span class="stat-label">総負荷量</span>
                <span class="stat-value">{{ Math.round(currentDetails.summary.totalVolume) }} kg</span>
              </div>
            </div>
          </div>

          <!-- Set Details -->
          <div class="sets-section">
            <div class="sets-header">
              <h4>セット詳細</h4>
              <button 
                v-if="!editMode && isToday" 
                @click="startEdit" 
                class="btn btn-secondary btn-sm"
              >
                <Icon name="mdi:pencil" />
                編集
              </button>
              <div v-else class="edit-actions">
                <button @click="cancelEdit" class="btn btn-outline btn-sm">
                  キャンセル
                </button>
                <button @click="saveEdits" class="btn btn-primary btn-sm" :disabled="saving">
                  <Icon v-if="saving" name="mdi:loading" class="spin" />
                  <Icon v-else name="mdi:content-save" />
                  保存
                </button>
              </div>
            </div>

            <div class="sets-list">
              <div 
                v-for="(set, index) in editableSets" 
                :key="set.setNumber"
                class="set-detail-item"
                :class="{ 'editing': editMode }"
              >
                <div class="set-number">{{ set.setNumber }}</div>
                <div class="set-data">
                  <div class="set-reps">
                    <span class="data-label">回数:</span>
                    <input 
                      v-if="editMode"
                      v-model.number="set.reps"
                      type="number"
                      min="1"
                      max="100"
                      class="edit-input"
                    />
                    <span v-else class="data-value">{{ set.reps }}</span>
                  </div>
                  <div class="set-weight">
                    <span class="data-label">重量:</span>
                    <input 
                      v-if="editMode"
                      v-model.number="set.weight"
                      type="number"
                      min="0"
                      step="0.5"
                      class="edit-input"
                      placeholder="0"
                    />
                    <span v-else-if="set.weight" class="data-value">{{ set.weight }} kg</span>
                    <span v-else class="data-value">-</span>
                  </div>
                  <div class="set-note">
                    <span class="data-label">メモ:</span>
                    <input 
                      v-if="editMode"
                      v-model="set.note"
                      type="text"
                      class="edit-input"
                      placeholder="メモ"
                    />
                    <span v-else-if="set.note" class="data-value">{{ set.note }}</span>
                    <span v-else class="data-value">-</span>
                  </div>
                </div>
                <div v-if="editMode && isToday" class="set-actions">
                  <button @click="removeSet(index)" class="btn btn-danger btn-mini">
                    <Icon name="mdi:delete" size="16" />
                  </button>
                </div>
              </div>
              
              <!-- Add Set Button -->
              <div v-if="editMode && isToday" class="add-set-section">
                <button @click="addNewSet" class="btn btn-outline add-set-btn">
                  <Icon name="mdi:plus" />
                  セット追加
                </button>
              </div>
            </div>
          </div>

          <!-- Modal Actions -->
          <div class="modal-actions">
            <button @click="repeatWorkout(selectedRecord)" class="btn btn-primary">
              <Icon name="mdi:repeat" />
              同じワークアウトを実行
            </button>
            <button @click="closeDetailModal" class="btn btn-secondary">
              閉じる
            </button>
          </div>
        </div>
      </div>
    </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTrainingMenus } from '~/composables/useTrainingMenus'
import { useTrainingHistory, type TrainingHistoryRecord, type HistoryFilters } from '~/composables/useTrainingHistory'
import { useNotifications } from '~/composables/useNotifications'
import { useAuthStore } from '~/stores/auth'
import { globalAuthModal } from '~/composables/useAuthModal'
import { useApiClient } from '~/utils/api-client'

// ===================================
// Setup and Dependencies
// ===================================

const route = useRoute()
const router = useRouter()
const notifications = useNotifications()
const authStore = useAuthStore()
const { openLogin, openRegister } = globalAuthModal

const openLoginModal = () => openLogin()
const openRegisterModal = () => openRegister()

// Get parameters from query
const routeMenuId = computed(() => route.query.menuId as string | undefined)
const routeDate = computed(() => route.query.date as string | undefined)

// ===================================
// State Management
// ===================================

// Training data and history
const { menus, loading: menusLoading } = useTrainingMenus({ autoLoad: true })
const { 
  records: historyRecords, 
  currentDetails,
  loading, 
  error, 
  stats,
  fetchHistory, 
  fetchHistoryDetails,
  clearError,
  formatDate,
  formatTime 
} = useTrainingHistory()

// Filters
const selectedMenu = ref(routeMenuId.value || '')
const startDate = ref('')
const endDate = ref('')

// Detail modal state
const showDetailModal = ref(false)
const selectedRecord = ref<TrainingHistoryRecord | null>(null)

// Edit mode state
const editMode = ref(false)
const editableSets = ref<any[]>([])
const originalSets = ref<any[]>([])

// ===================================
// Computed Properties
// ===================================

const totalLoading = computed(() => menusLoading.value || loading.value.isLoading)

const filteredRecords = computed(() => {
  return historyRecords.value
})

const isToday = computed(() => {
  if (!selectedRecord.value) return false
  const today = new Date().toISOString().split('T')[0]
  return selectedRecord.value.trainingDate === today
})

// ===================================
// Event Handlers
// ===================================

function startNewWorkout() {
  router.push('/')
}

async function applyFilters() {
  const filters: HistoryFilters = {}
  
  if (selectedMenu.value) filters.menuId = selectedMenu.value
  if (startDate.value) filters.startDate = startDate.value
  if (endDate.value) filters.endDate = endDate.value
  
  await fetchHistory(filters)
}

async function resetFilters() {
  selectedMenu.value = ''
  startDate.value = ''
  endDate.value = ''
  await fetchHistory()
}

async function retryLoad() {
  await applyFilters()
}

async function viewDetails(record: TrainingHistoryRecord) {
  selectedRecord.value = record
  await fetchHistoryDetails(record.menuId, record.trainingDate)
  
  // Initialize editable sets if this is today's record
  if (currentDetails.value && currentDetails.value.sets) {
    editableSets.value = JSON.parse(JSON.stringify(currentDetails.value.sets))
  }
  
  showDetailModal.value = true
}

function closeDetailModal() {
  showDetailModal.value = false
  selectedRecord.value = null
  editMode.value = false
}

function startEdit() {
  if (currentDetails.value && currentDetails.value.sets) {
    // Create deep copy of sets for editing
    originalSets.value = JSON.parse(JSON.stringify(currentDetails.value.sets))
    editableSets.value = JSON.parse(JSON.stringify(currentDetails.value.sets))
    editMode.value = true
  }
}

function cancelEdit() {
  editMode.value = false
  editableSets.value = []
  originalSets.value = []
}

async function saveEdits() {
  if (!selectedRecord.value || !isToday.value) return
  
  try {
    saving.value = true
    
    // Prepare the request data in the same format as creating a new record
    const requestData = {
      menuId: selectedRecord.value.menuId,
      trainingDate: selectedRecord.value.trainingDate,
      sets: editableSets.value.map((set, index) => ({
        setNumber: index + 1,
        reps: set.reps,
        weight: set.weight,
        note: set.note || ''
      }))
    }
    
    // Call the API to update the training record
    // The backend's CreateOrUpdateDailyRecord method will handle the update
    const { training } = useApiClient()
    await training.createTrainingRecord(requestData)
    
    // Update the current details with edited data
    if (currentDetails.value) {
      currentDetails.value.sets = [...editableSets.value]
      currentDetails.value.summary = calculateSummary(editableSets.value)
    }
    
    notifications.success('保存完了', '今日のトレーニング記録を更新しました')
    editMode.value = false
    editableSets.value = []
    originalSets.value = []
    
    // Refresh history to get updated data
    await applyFilters()
    
  } catch (err: any) {
    console.error('Save error:', err)
    notifications.error('保存エラー', '記録の保存に失敗しました')
  } finally {
    saving.value = false
  }
}

function addNewSet() {
  if (!editableSets.value) return
  
  const newSetNumber = editableSets.value.length + 1
  const newSet = {
    setNumber: newSetNumber,
    reps: 10,
    weight: 0,
    note: '',
    createdAt: new Date()
  }
  
  editableSets.value.push(newSet)
}

function removeSet(index: number) {
  if (editableSets.value.length <= 1) {
    notifications.warning('削除不可', '最低1セットは必要です')
    return
  }
  
  if (confirm('このセットを削除しますか？')) {
    editableSets.value.splice(index, 1)
    
    // Renumber sets
    editableSets.value.forEach((set, i) => {
      set.setNumber = i + 1
    })
  }
}

function calculateSummary(sets: any[]) {
  return {
    totalSets: sets.length,
    totalReps: sets.reduce((sum, set) => sum + (set.reps || 0), 0),
    maxWeight: Math.max(...sets.map(set => set.weight || 0), 0),
    totalVolume: sets.reduce((sum, set) => sum + ((set.reps || 0) * (set.weight || 0)), 0)
  }
}

function repeatWorkout(record: TrainingHistoryRecord) {
  // 認証チェック
  if (!authStore.isAuthenticated) {
    openLoginModal()
    return
  }
  
  router.push(`/training/session/${record.menuId}`)
}

function getMenuName(menuId: string): string {
  const menu = menus.value.find(m => m.menuId === menuId)
  return menu?.jpName || menuId
}

// ===================================
// Export Functionality
// ===================================

async function exportTrainingHistory() {
  try {
    const exportData = {
      metadata: {
        exportType: 'training_history',
        exportedAt: new Date().toISOString(),
        exportedBy: 'Message Training App',
        totalRecords: historyRecords.value.length,
        filters: {
          selectedMenu: selectedMenu.value,
          startDate: startDate.value,
          endDate: endDate.value
        }
      },
      statistics: stats.value,
      menus: menus.value.map(menu => ({
        menuId: menu.menuId,
        jpName: menu.jpName,
        enName: menu.enName
      })),
      trainingHistory: historyRecords.value.map(record => ({
        ...record,
        menuName: getMenuName(record.menuId)
      })),
      summary: {
        dateRange: {
          earliest: historyRecords.value.length > 0 ? 
            Math.min(...historyRecords.value.map(r => new Date(r.trainingDate).getTime())) : null,
          latest: historyRecords.value.length > 0 ? 
            Math.max(...historyRecords.value.map(r => new Date(r.trainingDate).getTime())) : null
        },
        uniqueMenus: [...new Set(historyRecords.value.map(r => r.menuId))].length,
        averageSessionsPerWeek: calculateAverageSessionsPerWeek()
      }
    }

    const dataStr = JSON.stringify(exportData, null, 2)
    const dataUri = 'data:application/json;charset=utf-8,'+ encodeURIComponent(dataStr)
    
    const exportFileDefaultName = `training_history_${new Date().toISOString().split('T')[0]}.json`
    
    const linkElement = document.createElement('a')
    linkElement.setAttribute('href', dataUri)
    linkElement.setAttribute('download', exportFileDefaultName)
    linkElement.click()
    
    notifications.success('エクスポート完了', 'トレーニング履歴をダウンロードしました')
    
  } catch (err) {
    console.error('Export error:', err)
    notifications.error('エクスポートエラー', 'データのエクスポートに失敗しました')
  }
}

function calculateAverageSessionsPerWeek() {
  if (historyRecords.value.length === 0) return 0
  
  const dates = historyRecords.value.map(r => new Date(r.trainingDate))
  const earliestDate = new Date(Math.min(...dates.map(d => d.getTime())))
  const latestDate = new Date(Math.max(...dates.map(d => d.getTime())))
  
  const daysDiff = Math.ceil((latestDate.getTime() - earliestDate.getTime()) / (1000 * 60 * 60 * 24))
  const weeksDiff = Math.max(daysDiff / 7, 1) // At least 1 week to avoid division by zero
  
  return Math.round((historyRecords.value.length / weeksDiff) * 10) / 10
}

// ===================================
// Lifecycle
// ===================================

onMounted(async () => {
  // Auto-load history on mount
  await applyFilters()
  
  // If specific date is requested, try to open it
  if (routeDate.value && routeMenuId.value) {
    const targetRecord = historyRecords.value.find(r => 
      r.trainingDate === routeDate.value && r.menuId === routeMenuId.value
    )
    if (targetRecord) {
      await viewDetails(targetRecord)
    }
  }
})

// Watch for route changes to update selected menu
watch(routeMenuId, (newMenuId) => {
  if (newMenuId) {
    selectedMenu.value = newMenuId
    applyFilters()
  }
})

// Watch for filter changes
watch([selectedMenu, startDate, endDate], () => {
  applyFilters()
})

// Page metadata
useHead({
  title: 'トレーニング履歴 - Message',
  meta: [
    { name: 'description', content: '過去のトレーニング記録を確認・管理' }
  ]
})
</script>

<style scoped>
/* ===================================
   Auth Required Container
   =================================== */
.auth-required-container {
  min-height: 60vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.auth-required-content {
  text-align: center;
  max-width: 500px;
  padding: 40px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
}

.auth-icon {
  color: #3498db;
  margin-bottom: 20px;
}

.auth-required-content h2 {
  margin-bottom: 15px;
  color: #2c3e50;
}

.auth-required-content p {
  color: #64748b;
  margin-bottom: 30px;
  line-height: 1.6;
}

.auth-actions {
  display: flex;
  gap: 15px;
  justify-content: center;
}

.btn-secondary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
}
.training-history {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

/* Header */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 30px;
  flex-wrap: wrap;
  gap: 20px;
}

.header-content h1 {
  margin: 0 0 5px 0;
  color: var(--text-primary);
  font-size: 2rem;
}

.header-content p {
  margin: 0;
  color: var(--text-secondary);
}

.header-actions {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

/* Stats Section */
.stats-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 15px;
  background: var(--card-bg);
  padding: 20px;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.stat-icon {
  font-size: 2.5rem;
}

.stat-content {
  display: flex;
  flex-direction: column;
}

.stat-number {
  font-size: 2rem;
  font-weight: 700;
  color: var(--primary);
  line-height: 1;
}

.stat-label {
  font-size: 0.9rem;
  color: var(--text-secondary);
}

/* Filter Section */
.filter-section {
  background: var(--card-bg);
  padding: 20px;
  border-radius: 12px;
  margin-bottom: 30px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.filter-group {
  display: flex;
  gap: 15px;
  flex-wrap: wrap;
  align-items: center;
}

.menu-select,
.date-input {
  padding: 10px 12px;
  border: 1px solid var(--border);
  border-radius: 6px;
  font-size: 0.9rem;
  min-width: 150px;
}

.menu-select:focus,
.date-input:focus {
  outline: none;
  border-color: var(--primary);
}

/* Loading and Error States */
.loading-state,
.error-state,
.empty-state {
  text-align: center;
  padding: 60px 20px;
  color: var(--text-secondary);
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

.error-icon,
.empty-icon {
  font-size: 4rem;
  margin-bottom: 20px;
}

.error-icon {
  color: var(--danger);
}

.empty-state h3,
.error-state h3 {
  margin-bottom: 10px;
  color: var(--text-primary);
}

/* History List */
.history-list {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.history-item {
  background: var(--card-bg);
  border-radius: 12px;
  padding: 25px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  transition: all 0.2s;
}

.history-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.15);
}

.record-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 15px;
  flex-wrap: wrap;
  gap: 15px;
}

.record-info h3 {
  margin: 0 0 5px 0;
  color: var(--text-primary);
  font-size: 1.3rem;
}

.record-meta {
  display: flex;
  gap: 15px;
  font-size: 0.9rem;
  color: var(--text-secondary);
}

.record-date {
  font-weight: 500;
}

.record-actions {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

.record-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 15px;
}

.stat {
  display: flex;
  justify-content: space-between;
  padding: 10px;
  background: var(--bg-secondary);
  border-radius: 6px;
  font-size: 0.9rem;
}

.stat-label {
  color: var(--text-secondary);
}

.stat-value {
  font-weight: 600;
  color: var(--text-primary);
}

/* Buttons */
.btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  border: none;
  border-radius: 6px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 0.9rem;
  text-decoration: none;
}

.btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.btn-primary {
  background: var(--primary);
  color: white;
}

.btn-secondary {
  background: var(--bg-tertiary);
  color: var(--text-primary);
}

.btn-outline {
  background: transparent;
  border: 1px solid var(--border);
  color: var(--text-primary);
}

.btn-outline:hover {
  background: var(--bg-secondary);
}

.btn-sm {
  padding: 6px 12px;
  font-size: 0.8rem;
}

/* Responsive Design */
@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: stretch;
  }

  .header-actions {
    justify-content: center;
  }

  .stats-section {
    grid-template-columns: 1fr;
  }

  .filter-group {
    flex-direction: column;
    align-items: stretch;
  }

  .menu-select,
  .date-input {
    min-width: auto;
  }

  .record-header {
    flex-direction: column;
    align-items: stretch;
  }

  .record-actions {
    justify-content: center;
  }

  .record-stats {
    grid-template-columns: 1fr;
  }
}

/* Dark mode variables */
:root {
  --card-bg: #ffffff;
  --bg-secondary: #f5f5f5;
  --bg-tertiary: #e0e0e0;
  --text-primary: #212121;
  --text-secondary: #757575;
  --border: #e0e0e0;
  --primary: #2196f3;
  --danger: #f44336;
}

html.dark {
  --card-bg: #1e1e1e;
  --bg-secondary: #2d2d2d;
  --bg-tertiary: #3d3d3d;
  --text-primary: #ffffff;
  --text-secondary: #b0b0b0;
  --border: #3d3d3d;
  --primary: #42a5f5;
  --danger: #ef5350;
}

/* ===================================
   Detail Modal Styles
   =================================== */

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.7);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
  padding: 20px;
}

.modal-content {
  background: var(--card-bg);
  border-radius: 16px;
  max-width: 600px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  border: 2px solid var(--border);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 25px 25px 0 25px;
  border-bottom: 1px solid var(--border);
  margin-bottom: 20px;
}

.modal-header h2 {
  margin: 0;
  color: var(--text-primary);
  font-size: 1.4rem;
  display: flex;
  align-items: center;
  gap: 10px;
}

.modal-close-btn {
  background: none;
  border: none;
  color: var(--text-secondary);
  cursor: pointer;
  padding: 5px;
  border-radius: 4px;
  transition: all 0.2s;
}

.modal-close-btn:hover {
  background: var(--bg-secondary);
  color: var(--text-primary);
}

.modal-loading,
.modal-error {
  text-align: center;
  padding: 40px 25px;
  color: var(--text-secondary);
}

.modal-body {
  padding: 0 25px 25px 25px;
}

/* Workout Info */
.workout-info {
  margin-bottom: 25px;
}

.workout-header {
  margin-bottom: 15px;
}

.workout-header h3 {
  margin: 0 0 5px 0;
  color: var(--text-primary);
  font-size: 1.3rem;
}

.workout-meta {
  display: flex;
  gap: 15px;
  font-size: 0.9rem;
  color: var(--text-secondary);
}

.workout-date {
  font-weight: 500;
}

/* Summary Stats */
.summary-stats {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
}

.summary-stat {
  background: var(--bg-secondary);
  padding: 15px;
  border-radius: 8px;
  text-align: center;
}

.summary-stat .stat-label {
  display: block;
  font-size: 0.8rem;
  color: var(--text-secondary);
  margin-bottom: 5px;
}

.summary-stat .stat-value {
  font-size: 1.4rem;
  font-weight: 700;
  color: var(--primary);
}

/* Sets Section */
.sets-section {
  margin-bottom: 25px;
}

.sets-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.sets-header h4 {
  margin: 0;
  color: var(--text-primary);
  font-size: 1.1rem;
}

.edit-actions {
  display: flex;
  gap: 8px;
}

.sets-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.set-detail-item {
  display: flex;
  align-items: center;
  gap: 15px;
  padding: 15px;
  background: var(--bg-secondary);
  border-radius: 10px;
  border: 1px solid var(--border);
}

.set-number {
  background: linear-gradient(135deg, var(--primary), var(--primary-dark));
  color: white;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 1.1rem;
  flex-shrink: 0;
}

.set-data {
  flex: 1;
  display: flex;
  gap: 20px;
  flex-wrap: wrap;
}

.set-reps,
.set-weight,
.set-note {
  display: flex;
  align-items: center;
  gap: 5px;
}

.data-label {
  font-size: 0.85rem;
  color: var(--text-secondary);
}

.data-value {
  font-weight: 600;
  color: var(--text-primary);
}

/* Modal Actions */
.modal-actions {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
  padding-top: 15px;
  border-top: 1px solid var(--border);
}

/* Mobile adjustments */
@media (max-width: 768px) {
  .modal-content {
    margin: 10px;
    max-height: 95vh;
  }
  
  .modal-header {
    padding: 20px 20px 0 20px;
  }
  
  .modal-body {
    padding: 0 20px 20px 20px;
  }
  
  .summary-stats {
    grid-template-columns: 1fr;
  }
  
  .set-data {
    flex-direction: column;
    gap: 10px;
  }
  
  .modal-actions {
    flex-direction: column;
  }
  
  .sets-header {
    flex-direction: column;
    align-items: stretch;
    gap: 10px;
  }
  
  .edit-input {
    width: 60px;
  }
  
  .edit-input[type="text"] {
    width: 100px;
  }
}

/* ===================================
   Edit Mode Styles
   =================================== */

.set-detail-item.editing {
  background: var(--bg-secondary);
  border: 2px solid var(--primary);
  border-radius: 8px;
}

.edit-input {
  padding: 6px 8px;
  border: 1px solid var(--border);
  border-radius: 4px;
  font-size: 0.9rem;
  width: 80px;
  background: var(--card-bg);
  color: var(--text-primary);
  transition: border-color 0.2s;
}

.edit-input:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: 0 0 0 2px rgba(33, 150, 243, 0.1);
}

.edit-input[type="text"] {
  width: 120px;
}

.set-actions {
  display: flex;
  align-items: center;
  justify-content: center;
  margin-left: 10px;
}

.btn-mini {
  padding: 4px;
  min-height: 28px;
  font-size: 0.7rem;
  min-width: 28px;
}

.add-set-section {
  text-align: center;
  padding: 15px;
  border: 2px dashed var(--border);
  border-radius: 8px;
  margin-top: 10px;
  background: var(--bg-secondary);
}

.add-set-btn {
  min-width: 120px;
  border-style: dashed;
  border-width: 2px;
  color: var(--primary);
  border-color: var(--primary);
}

.add-set-btn:hover {
  background: var(--primary);
  color: white;
  border-style: solid;
}

/* Edit mode visual indicators */
.set-detail-item.editing .set-number {
  background: linear-gradient(135deg, #28A745, #1E7E34);
}

.edit-actions {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}
</style>