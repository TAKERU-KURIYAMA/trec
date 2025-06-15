<template>
  <div class="training-session">
    <!-- Header -->
    <div class="session-header">
      <button @click="goBack" class="back-btn">
        <Icon name="mdi:arrow-left" />
        戻る
      </button>
      
      <div class="menu-info" v-if="selectedMenu">
        <h1>{{ selectedMenu.jpName }}</h1>
        <p>{{ selectedMenu.description || selectedMenu.enName }}</p>
      </div>
      
      <div class="session-status">
        <div class="session-info">
          <span class="current-set">セット {{ currentSetNumber }}</span>
          <span class="total-time">経過時間: {{ totalElapsedTime }}</span>
        </div>
        <button 
          v-if="hasStarted && !isWorkoutComplete"
          @click="finishWorkout" 
          :disabled="!canFinishWorkout"
          class="btn btn-finish"
        >
          <Icon name="mdi:flag-checkered" />
          ワークアウト完了
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading.isLoading" class="loading-state">
      <div class="loading-spinner">⚡</div>
      <p>{{ loading.message || 'メニュー情報を読み込み中...' }}</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error.hasError" class="error-state">
      <div class="error-icon">⚠️</div>
      <h3>エラーが発生しました</h3>
      <p>{{ error.message }}</p>
      <button @click="retryLoad" class="btn btn-primary">再試行</button>
    </div>

    <!-- Main Session Content -->
    <div v-else-if="selectedMenu" class="session-content">
      <!-- Preset Options (if available) -->
      <div v-if="showPresetOptions && presetData" class="preset-options-section">
        <div class="preset-card">
          <div class="preset-header">
            <div class="preset-info">
              <h3>📋 マイセット利用可能</h3>
              <p class="preset-name">{{ presetData.name }}</p>
              <p class="preset-description" v-if="presetData.description">{{ presetData.description }}</p>
            </div>
            <button @click="dismissPreset" class="preset-dismiss-btn">
              <Icon name="mdi:close" size="20" />
            </button>
          </div>
          
          <div class="preset-sets-preview">
            <h4>プリセット内容 ({{ presetData.defaultSets?.length || 0 }}セット)</h4>
            <div class="preset-sets-list">
              <div 
                v-for="set in presetData.defaultSets?.slice(0, 3)" 
                :key="set.setNumber"
                class="preset-set-item"
              >
                <span class="set-num">{{ set.setNumber }}</span>
                <span class="set-info">{{ set.reps }}回</span>
                <span v-if="set.weight" class="set-weight">{{ set.weight }}kg</span>
              </div>
              <div v-if="presetData.defaultSets?.length > 3" class="more-sets">
                +{{ presetData.defaultSets.length - 3 }}セット
              </div>
            </div>
          </div>
          
          <div class="preset-actions">
            <button @click="applyPreset" class="btn btn-primary">
              <Icon name="mdi:download" />
              プリセットを適用
            </button>
            <button @click="dismissPreset" class="btn btn-secondary">
              <Icon name="mdi:cancel" />
              手動で入力
            </button>
          </div>
        </div>
      </div>

      <!-- Current Set Input -->
      <div class="current-set-section">
        <div class="set-card">
          <div class="set-header">
            <h2>現在のセット</h2>
            <div class="set-counter">{{ currentSetNumber }}</div>
          </div>

          <div class="input-form">
            <!-- 重量入力 -->
            <div class="input-group">
              <label for="weight-input">重量 (kg)</label>
              <input 
                id="weight-input"
                type="number" 
                v-model.number="weightInput"
                min="0"
                step="0.5"
                class="input-field"
                placeholder="重量を入力"
                @input="onWeightChange"
              />
            </div>

            <!-- 回数入力 -->
            <div class="input-group">
              <label for="reps-input">回数</label>
              <input 
                id="reps-input"
                type="number" 
                v-model.number="repsInput"
                min="1"
                max="100"
                class="input-field"
                placeholder="回数を入力"
                :disabled="!weightEntered"
                @input="onRepsChange"
              />
            </div>

            <!-- メモ入力 -->
            <div class="input-group">
              <label for="note-input">メモ（任意）</label>
              <input 
                id="note-input"
                type="text" 
                v-model="noteInput"
                class="input-field"
                placeholder="メモを入力"
                :disabled="!repsEntered"
              />
            </div>

            <!-- セット完了ボタン -->
            <div class="action-buttons">
              <button 
                @click="completeSet"
                :disabled="!canCompleteSet"
                class="btn btn-complete"
                :class="{ ready: canCompleteSet }"
              >
                <Icon name="mdi:check-circle" />
                セット完了
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Completed Sets -->
      <div class="completed-sets-section" v-if="completedSets.length > 0">
        <h3>完了セット</h3>
        <div class="sets-list">
          <div 
            v-for="(set, index) in completedSets" 
            :key="index"
            class="set-item"
          >
            <div class="set-number">{{ index + 1 }}</div>
            <div class="set-details">
              <span class="reps">{{ set.reps }}回</span>
              <span class="weight" v-if="set.weight">{{ set.weight }}kg</span>
              <span class="note" v-if="set.note">{{ set.note }}</span>
            </div>
            <button @click="editSet(index)" class="btn-mini btn-edit">
              <Icon name="mdi:pencil" size="16" />
            </button>
          </div>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="quick-actions">
        <button 
          @click="addSet"
          class="btn btn-secondary"
          :disabled="!canCompleteSet"
        >
          <Icon name="mdi:plus" />
          セット追加
        </button>
        
        <button 
          v-if="completedSets.length > 0"
          @click="removeLastSet"
          class="btn btn-warning"
        >
          <Icon name="mdi:minus" />
          最後を削除
        </button>
      </div>
    </div>

    <!-- Workout Summary Modal -->
    <div v-if="showSummaryModal" class="modal-overlay" @click="closeSummaryModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>🎉 ワークアウト完了！</h2>
        </div>
        
        <div class="workout-summary">
          <div class="summary-title">
            <h3>{{ selectedMenu?.jpName }} - 今日の結果</h3>
          </div>

          <div class="summary-stats">
            <div class="stat-card">
              <div class="stat-value">{{ completedSets.length }}</div>
              <div class="stat-label">セット数</div>
            </div>
            <div class="stat-card">
              <div class="stat-value">{{ totalReps }}</div>
              <div class="stat-label">総回数</div>
            </div>
            <div class="stat-card" v-if="totalVolume > 0">
              <div class="stat-value">{{ Math.round(totalVolume) }}</div>
              <div class="stat-label">総負荷 (kg)</div>
            </div>
            <div class="stat-card">
              <div class="stat-value">{{ totalWorkoutTime }}</div>
              <div class="stat-label">時間</div>
            </div>
          </div>

          <div class="summary-actions">
            <button @click="saveWorkout" class="btn btn-save" :disabled="saving">
              <Icon v-if="saving" name="mdi:loading" class="spin" />
              <Icon v-else name="mdi:content-save" />
              {{ saving ? '保存中...' : 'データを保存' }}
            </button>
            
            <div class="navigation-buttons" v-if="workoutSaved">
              <button @click="goToDashboard" class="btn btn-dashboard">
                <Icon name="mdi:chart-line" />
                分析画面
              </button>
              <button @click="goToMenuList" class="btn btn-menu">
                <Icon name="mdi:format-list-bulleted" />
                メニュー一覧
              </button>
            </div>
            
            <button @click="discardWorkout" class="btn btn-discard" v-if="!workoutSaved">
              <Icon name="mdi:delete" />
              破棄して終了
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTrainingMenus } from '~/composables/useTrainingMenus'
import { useApiClient } from '~/utils/api-client'
import { useGlobalNotifications } from '~/composables/useNotifications'
import type { TrainingMenu, TrainingRecordSet } from '~/types'

// ===================================
// Setup and Dependencies
// ===================================

const route = useRoute()
const router = useRouter()
const { training } = useApiClient()
const notifications = useGlobalNotifications()

const menuId = computed(() => route.params.menuId as string)
const presetId = computed(() => route.query.presetId as string | undefined)

// ===================================
// Simple State Management
// ===================================

// Menu data
const { menus, loading, error, refetch } = useTrainingMenus({ autoLoad: true })
const selectedMenu = computed(() => 
  menus.value.find(menu => menu.menuId === menuId.value)
)

// Workout session state
const hasStarted = ref(false)
const isWorkoutComplete = ref(false)
const startTime = ref<Date | null>(null)
const endTime = ref<Date | null>(null)
const currentSetNumber = ref(1)

// Simple input state
const weightInput = ref<number | null>(null)
const repsInput = ref<number | null>(null)
const noteInput = ref('')

// Completed sets
const completedSets = ref<TrainingRecordSet[]>([])

// UI state
const showSummaryModal = ref(false)
const saving = ref(false)
const workoutSaved = ref(false)

// Preset data
const presetData = ref<any>(null)
const showPresetOptions = ref(false)

// ===================================
// Simple Computed Properties
// ===================================

const weightEntered = computed(() => {
  return weightInput.value !== null && weightInput.value > 0
})

const repsEntered = computed(() => {
  return repsInput.value !== null && repsInput.value > 0
})

const canCompleteSet = computed(() => {
  return weightEntered.value && repsEntered.value
})

const canFinishWorkout = computed(() => {
  return completedSets.value.length > 0 && !weightEntered.value && !repsEntered.value
})

const totalElapsedTime = computed(() => {
  if (!startTime.value) return '00:00:00'
  const now = endTime.value || new Date()
  const diff = now.getTime() - startTime.value.getTime()
  return formatDuration(diff)
})

const totalReps = computed(() => {
  return completedSets.value.reduce((sum, set) => sum + set.reps, 0)
})

const totalVolume = computed(() => {
  return completedSets.value.reduce((sum, set) => {
    return sum + (set.reps * (set.weight || 0))
  }, 0)
})

const totalWorkoutTime = computed(() => {
  if (!startTime.value || !endTime.value) return '00:00:00'
  const diff = endTime.value.getTime() - startTime.value.getTime()
  return formatDuration(diff)
})

// ===================================
// Preset Functions
// ===================================

async function loadPresetData() {
  if (!presetId.value) return

  try {
    const response = await training.get('/presets')
    
    if (response.isSuccess && response.data?.presets) {
      const preset = response.data.presets.find((p: any) => p.presetId === presetId.value)
      
      if (preset) {
        presetData.value = preset
        showPresetOptions.value = true
        notifications.info('プリセット読み込み', `「${preset.name}」のプリセットが利用できます`)
      }
    }
  } catch (err) {
    console.error('Failed to load preset:', err)
  }
}

function applyPreset() {
  if (!presetData.value) return

  const sets = presetData.value.defaultSets
  if (sets && sets.length > 0) {
    // Apply first set to current inputs
    const firstSet = sets[0]
    weightInput.value = firstSet.weight || null
    repsInput.value = firstSet.reps
    noteInput.value = firstSet.note || ''

    // Pre-fill completed sets for remaining sets
    for (let i = 1; i < sets.length; i++) {
      const set = sets[i]
      completedSets.value.push({
        reps: set.reps,
        weight: set.weight || 0,
        note: set.note || undefined,
        createdAt: new Date()
      })
    }

    currentSetNumber.value = sets.length + 1
    showPresetOptions.value = false
    
    notifications.success('プリセット適用', `${sets.length}セットのプリセットを適用しました`)
  }
}

function dismissPreset() {
  showPresetOptions.value = false
  presetData.value = null
}

// ===================================
// Simple Event Handlers
// ===================================

function onWeightChange() {
  // No automatic focus - let user decide
}

function onRepsChange() {
  // No automatic focus - let user decide
}

function completeSet() {
  if (!canCompleteSet.value) return

  // Start workout timer on first set
  if (!hasStarted.value) {
    hasStarted.value = true
    startTime.value = new Date()
    notifications.success('開始', 'ワークアウトを開始しました！')
  }

  // Add to completed sets
  const setData: TrainingRecordSet = {
    reps: repsInput.value!,
    weight: weightInput.value || 0,
    note: noteInput.value || undefined,
    createdAt: new Date()
  }

  completedSets.value.push(setData)
  
  // Clear inputs for next set
  clearInputs()
  currentSetNumber.value++

  notifications.success('完了', `セット${completedSets.value.length}が完了しました`)
}

function clearInputs() {
  weightInput.value = null
  repsInput.value = null
  noteInput.value = ''
}

function addSet() {
  // Same as complete set but doesn't require validation
  if (canCompleteSet.value) {
    completeSet()
  }
}

function editSet(index: number) {
  const setToEdit = completedSets.value[index]
  if (confirm('このセットを編集しますか？')) {
    completedSets.value.splice(index, 1)
    weightInput.value = setToEdit.weight || null
    repsInput.value = setToEdit.reps
    noteInput.value = setToEdit.note || ''
    currentSetNumber.value = index + 1
  }
}

function removeLastSet() {
  if (completedSets.value.length > 0) {
    if (confirm('最後のセットを削除しますか？')) {
      completedSets.value.pop()
      notifications.info('削除', '最後のセットを削除しました')
    }
  }
}

function finishWorkout() {
  if (completedSets.value.length === 0) {
    notifications.warning('セットなし', '少なくとも1セットは完了してください')
    return
  }

  endTime.value = new Date()
  isWorkoutComplete.value = true
  showSummaryModal.value = true
}

// ===================================
// Workout Management
// ===================================

async function saveWorkout() {
  if (completedSets.value.length === 0) return

  saving.value = true
  try {
    const workoutData = {
      menuId: menuId.value,
      trainingDate: startTime.value!.toISOString().split('T')[0],
      sets: completedSets.value.map((set, index) => ({
        setNumber: index + 1,
        reps: set.reps,
        weight: set.weight || null,
        note: set.note || null
      }))
    }

    const response = await training.createTrainingRecord(workoutData)
    
    workoutSaved.value = true
    
    notifications.success('保存完了', `ワークアウトが保存されました！\nセット数: ${response.setCount}, 総回数: ${response.totalReps}`)
    
  } catch (error: any) {
    console.error('Failed to save workout:', error)
    let errorMessage = 'ワークアウトの保存に失敗しました'
    if (error?.userMessage) {
      errorMessage = error.userMessage
    }
    notifications.error('保存エラー', errorMessage)
  } finally {
    saving.value = false
  }
}

function discardWorkout() {
  if (confirm('ワークアウトを破棄しますか？この操作は取り消せません。')) {
    router.push('/')
  }
}

function closeSummaryModal() {
  showSummaryModal.value = false
}

// ===================================
// Navigation Functions
// ===================================

function goBack() {
  if (hasStarted.value && !isWorkoutComplete.value) {
    if (confirm('ワークアウト中です。戻ると進捗が失われます。よろしいですか？')) {
      router.back()
    }
  } else {
    router.back()
  }
}

function goToDashboard() {
  router.push('/dashboard')
}

function goToMenuList() {
  router.push('/')
}

function retryLoad() {
  refetch()
}

// ===================================
// Utility Functions
// ===================================

function formatDuration(ms: number): string {
  const seconds = Math.floor(ms / 1000)
  const minutes = Math.floor(seconds / 60)
  const hours = Math.floor(minutes / 60)
  
  const h = hours.toString().padStart(2, '0')
  const m = (minutes % 60).toString().padStart(2, '0')
  const s = (seconds % 60).toString().padStart(2, '0')
  
  return `${h}:${m}:${s}`
}

// ===================================
// Lifecycle
// ===================================

onMounted(async () => {
  window.addEventListener('beforeunload', handleBeforeUnload)
  
  // Load preset data if presetId is provided
  if (presetId.value) {
    await loadPresetData()
  }
})

onUnmounted(() => {
  window.removeEventListener('beforeunload', handleBeforeUnload)
})

function handleBeforeUnload(event: BeforeUnloadEvent) {
  if (hasStarted.value && !isWorkoutComplete.value) {
    event.preventDefault()
    event.returnValue = 'ワークアウト中です。ページを離れると進捗が失われます。'
    return 'ワークアウト中です。ページを離れると進捗が失われます。'
  }
}

// Page metadata
useHead({
  title: computed(() => selectedMenu.value ? `${selectedMenu.value.jpName} - トレーニングセッション` : 'トレーニングセッション'),
  meta: [
    { name: 'description', content: 'トレーニングセッションを記録してください' }
  ]
})
</script>

<style scoped>
/* ===================================
   Color Variables & Base Styles
   =================================== */

:root {
  --primary: #007AFF;
  --primary-dark: #0056CC;
  --primary-light: #E6F2FF;
  
  --secondary: #6C757D;
  --secondary-dark: #5A6268;
  --secondary-light: #F8F9FA;
  
  --success: #28A745;
  --success-dark: #1E7E34;
  --success-light: #D4EDDA;
  
  --warning: #FFC107;
  --warning-dark: #E0A800;
  --warning-light: #FFF3CD;
  
  --danger: #DC3545;
  --danger-dark: #C82333;
  --danger-light: #F8D7DA;
  
  --finish: #17A2B8;
  --finish-dark: #138496;
  --finish-light: #D1ECF1;
  
  --bg-primary: #FFFFFF;
  --bg-secondary: #F8F9FA;
  --bg-tertiary: #E9ECEF;
  
  --text-primary: #212529;
  --text-secondary: #6C757D;
  --text-muted: #ADB5BD;
  
  --border: #DEE2E6;
  --border-light: #F1F3F4;
  
  --shadow-sm: 0 2px 4px rgba(0,0,0,0.1);
  --shadow-md: 0 4px 12px rgba(0,0,0,0.15);
  --shadow-lg: 0 8px 24px rgba(0,0,0,0.2);
}

html.dark {
  --primary: #0A84FF;
  --primary-dark: #0040DD;
  --primary-light: #1A1A1A;
  
  --bg-primary: #000000;
  --bg-secondary: #1C1C1E;
  --bg-tertiary: #2C2C2E;
  
  --text-primary: #FFFFFF;
  --text-secondary: #99999D;
  --text-muted: #636366;
  
  --border: #38383A;
  --border-light: #2C2C2E;
}

.training-session {
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
  min-height: 100vh;
  background: var(--bg-primary);
}

/* ===================================
   Header Styles
   =================================== */

.session-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
  flex-wrap: wrap;
  gap: 20px;
  padding: 20px;
  background: var(--bg-secondary);
  border-radius: 12px;
  box-shadow: var(--shadow-sm);
}

.back-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 15px;
  background: linear-gradient(135deg, var(--secondary), var(--secondary-dark));
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
  font-weight: 500;
  box-shadow: var(--shadow-sm);
}

.back-btn:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
}

.menu-info h1 {
  margin: 0 0 5px 0;
  color: var(--text-primary);
  font-size: 1.5rem;
  font-weight: 700;
}

.menu-info p {
  margin: 0;
  color: var(--text-secondary);
  font-size: 0.9rem;
}

.session-status {
  text-align: right;
}

.session-info {
  display: flex;
  flex-direction: column;
  gap: 5px;
  margin-bottom: 10px;
  font-size: 0.9rem;
}

.current-set {
  font-weight: 700;
  color: var(--primary);
  font-size: 1.1rem;
}

.total-time {
  color: var(--text-secondary);
}

/* ===================================
   Button Base Styles (2+ Colors Required)
   =================================== */

.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 12px 20px;
  font-size: 1rem;
  font-weight: 600;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
  text-decoration: none;
  min-height: 44px;
  box-shadow: var(--shadow-sm);
  position: relative;
  overflow: hidden;
}

.btn:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
}

.btn:active {
  transform: translateY(0);
  box-shadow: var(--shadow-sm);
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

/* Primary Button */
.btn-primary {
  background: linear-gradient(135deg, #007AFF, #0056CC) !important;
  color: white !important;
  border: 2px solid #0056CC !important;
}

.btn-primary:hover:not(:disabled) {
  background: linear-gradient(135deg, #0056CC, #007AFF) !important;
}

/* Secondary Button */
.btn-secondary {
  background: linear-gradient(135deg, #6C757D, #5A6268) !important;
  color: white !important;
  border: 2px solid #5A6268 !important;
}

.btn-secondary:hover:not(:disabled) {
  background: linear-gradient(135deg, #5A6268, #6C757D) !important;
}

/* Complete Button */
.btn-complete {
  background: linear-gradient(135deg, #28A745, #1E7E34) !important;
  color: white !important;
  border: 2px solid #1E7E34 !important;
  font-size: 1.1rem;
  padding: 15px 30px;
  min-width: 180px;
}

.btn-complete:hover:not(:disabled) {
  background: linear-gradient(135deg, #1E7E34, #28A745) !important;
}

.btn-complete.ready {
  animation: pulse-glow 2s infinite;
  box-shadow: 0 0 20px rgba(40, 167, 69, 0.5);
}

/* Finish Button */
.btn-finish {
  background: linear-gradient(135deg, #17A2B8, #138496) !important;
  color: white !important;
  border: 2px solid #138496 !important;
}

.btn-finish:hover:not(:disabled) {
  background: linear-gradient(135deg, #138496, #17A2B8) !important;
}

/* Warning Button */
.btn-warning {
  background: linear-gradient(135deg, #FFC107, #E0A800) !important;
  color: black !important;
  border: 2px solid #E0A800 !important;
}

.btn-warning:hover:not(:disabled) {
  background: linear-gradient(135deg, #E0A800, #FFC107) !important;
  color: white !important;
}

/* Save Button */
.btn-save {
  background: linear-gradient(135deg, #28A745, #1E7E34) !important;
  color: white !important;
  border: 2px solid #1E7E34 !important;
  font-size: 1.1rem;
  padding: 15px 30px;
  width: 100%;
  margin-bottom: 15px;
}

/* Dashboard Button */
.btn-dashboard {
  background: linear-gradient(135deg, #007AFF, #0056CC) !important;
  color: white !important;
  border: 2px solid #0056CC !important;
  flex: 1;
}

/* Menu Button */
.btn-menu {
  background: linear-gradient(135deg, #6C757D, #5A6268) !important;
  color: white !important;
  border: 2px solid #5A6268 !important;
  flex: 1;
}

/* Discard Button */
.btn-discard {
  background: linear-gradient(135deg, #DC3545, #C82333) !important;
  color: white !important;
  border: 2px solid #C82333 !important;
  width: 100%;
}

/* Mini Button */
.btn-mini {
  padding: 8px;
  min-height: 32px;
  font-size: 0.8rem;
}

.btn-edit {
  background: linear-gradient(135deg, #6C757D, #5A6268) !important;
  color: white !important;
  border: 1px solid #5A6268 !important;
}

/* ===================================
   Loading & Error States
   =================================== */

.loading-state,
.error-state {
  text-align: center;
  padding: 60px 20px;
  color: var(--text-secondary);
}

.loading-spinner {
  font-size: 3rem;
  margin-bottom: 15px;
  animation: pulse 1.5s ease-in-out infinite;
}

.error-icon {
  font-size: 4rem;
  margin-bottom: 20px;
  color: var(--danger);
}

/* ===================================
   Session Content
   =================================== */

.session-content {
  display: grid;
  gap: 30px;
}

/* ===================================
   Preset Options Section
   =================================== */

.preset-options-section {
  margin-bottom: 20px;
}

.preset-card {
  background: linear-gradient(135deg, #E6F2FF, #F0F8FF) !important;
  border: 2px solid #007AFF !important;
  border-radius: 16px;
  padding: 25px;
  box-shadow: 0 4px 20px rgba(0, 122, 255, 0.15);
  position: relative;
  animation: slideIn 0.3s ease-out;
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.preset-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 20px;
}

.preset-info h3 {
  margin: 0 0 8px 0;
  color: #007AFF !important;
  font-size: 1.3rem;
  font-weight: 700;
}

.preset-name {
  margin: 0 0 5px 0;
  color: var(--text-primary);
  font-size: 1.1rem;
  font-weight: 600;
}

.preset-description {
  margin: 0;
  color: var(--text-secondary);
  font-size: 0.9rem;
}

.preset-dismiss-btn {
  background: rgba(255, 255, 255, 0.8) !important;
  border: 1px solid #007AFF !important;
  border-radius: 50%;
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s ease;
  color: #007AFF !important;
}

.preset-dismiss-btn:hover {
  background: rgba(220, 53, 69, 0.1) !important;
  border-color: #DC3545 !important;
  color: #DC3545 !important;
}

.preset-sets-preview {
  margin-bottom: 20px;
}

.preset-sets-preview h4 {
  margin: 0 0 15px 0;
  color: var(--text-primary);
  font-size: 1rem;
  font-weight: 600;
}

.preset-sets-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.preset-set-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px;
  background: rgba(255, 255, 255, 0.8) !important;
  border-radius: 8px;
  border: 1px solid rgba(0, 122, 255, 0.2) !important;
}

.set-num {
  background: linear-gradient(135deg, #007AFF, #0056CC) !important;
  color: white !important;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 0.9rem;
}

.set-info {
  color: var(--text-primary);
  font-weight: 600;
}

.set-weight {
  color: #007AFF !important;
  font-weight: 500;
}

.more-sets {
  text-align: center;
  color: var(--text-secondary);
  font-style: italic;
  font-size: 0.85rem;
  padding: 8px;
}

.preset-actions {
  display: flex;
  gap: 12px;
  justify-content: center;
}

.preset-actions .btn {
  flex: 1;
  max-width: 200px;
  justify-content: center;
}

/* Current Set Section */
.set-card {
  background: var(--bg-secondary);
  border-radius: 16px;
  padding: 30px;
  box-shadow: var(--shadow-md);
  border: 2px solid var(--border);
}

.set-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
}

.set-header h2 {
  margin: 0;
  color: var(--text-primary);
  font-size: 1.5rem;
  font-weight: 700;
}

.set-counter {
  background: linear-gradient(135deg, #007AFF, #0056CC) !important;
  color: white !important;
  width: 60px;
  height: 60px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  font-weight: 700;
  box-shadow: var(--shadow-md);
  border: 3px solid white;
}

/* Input Form */
.input-form {
  display: grid;
  gap: 20px;
}

.input-group {
  display: grid;
  gap: 8px;
}

.input-group label {
  font-weight: 600;
  color: var(--text-primary);
  font-size: 1rem;
}

.input-field {
  padding: 15px;
  border: 2px solid var(--border);
  border-radius: 8px;
  font-size: 1.1rem;
  background: var(--bg-primary);
  color: var(--text-primary);
  transition: all 0.3s ease;
  box-shadow: var(--shadow-sm);
}

.input-field:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: 0 0 0 3px rgba(0, 122, 255, 0.1);
}

.input-field:disabled {
  background: var(--bg-tertiary);
  color: var(--text-muted);
  cursor: not-allowed;
  opacity: 0.7;
}

.action-buttons {
  display: flex;
  justify-content: center;
  margin-top: 10px;
}

/* ===================================
   Completed Sets
   =================================== */

.completed-sets-section {
  background: var(--bg-secondary);
  border-radius: 16px;
  padding: 25px;
  box-shadow: var(--shadow-md);
  border: 2px solid var(--border);
}

.completed-sets-section h3 {
  margin: 0 0 20px 0;
  color: var(--text-primary);
  font-size: 1.3rem;
  font-weight: 700;
}

.sets-list {
  display: grid;
  gap: 12px;
}

.set-item {
  display: flex;
  align-items: center;
  gap: 15px;
  padding: 15px;
  background: var(--bg-primary);
  border-radius: 12px;
  border: 2px solid var(--border-light);
  transition: all 0.3s ease;
}

.set-item:hover {
  border-color: var(--primary);
  box-shadow: var(--shadow-sm);
}

.set-number {
  background: linear-gradient(135deg, #28A745, #1E7E34) !important;
  color: white !important;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 1.1rem;
  border: 2px solid white;
  box-shadow: var(--shadow-sm);
}

.set-details {
  flex: 1;
  display: flex;
  gap: 15px;
  align-items: center;
}

.reps {
  font-weight: 700;
  color: var(--text-primary);
  font-size: 1.1rem;
}

.weight {
  color: var(--primary);
  font-weight: 600;
}

.note {
  color: var(--text-secondary);
  font-style: italic;
}

/* ===================================
   Quick Actions
   =================================== */

.quick-actions {
  display: flex;
  gap: 15px;
  justify-content: center;
  flex-wrap: wrap;
}

/* ===================================
   Modal Styles
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
  z-index: 1000;
  padding: 20px;
}

.modal-content {
  background: var(--bg-primary);
  border-radius: 20px;
  max-width: 500px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: var(--shadow-lg);
  border: 3px solid var(--border);
}

.modal-header {
  padding: 25px 25px 0 25px;
  text-align: center;
}

.modal-header h2 {
  margin: 0;
  color: var(--text-primary);
  font-size: 1.8rem;
  font-weight: 700;
}

/* ===================================
   Workout Summary
   =================================== */

.workout-summary {
  padding: 25px;
}

.summary-title {
  text-align: center;
  margin-bottom: 25px;
}

.summary-title h3 {
  margin: 0;
  color: var(--primary);
  font-size: 1.3rem;
  font-weight: 600;
}

.summary-stats {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 15px;
  margin-bottom: 25px;
}

.stat-card {
  background: linear-gradient(135deg, #E6F2FF, #F8F9FA) !important;
  padding: 20px;
  border-radius: 12px;
  text-align: center;
  border: 2px solid #007AFF !important;
  box-shadow: var(--shadow-sm);
}

.stat-value {
  font-size: 2rem;
  font-weight: 700;
  color: #007AFF !important;
  margin-bottom: 5px;
}

.stat-label {
  font-size: 0.9rem;
  color: var(--text-secondary);
  font-weight: 500;
}

.summary-actions {
  display: grid;
  gap: 15px;
}

.navigation-buttons {
  display: flex;
  gap: 12px;
}

/* ===================================
   Animations
   =================================== */

@keyframes pulse {
  0%, 100% { opacity: 0.5; }
  50% { opacity: 1; }
}

@keyframes pulse-glow {
  0% {
    box-shadow: 0 0 0 0 rgba(40, 167, 69, 0.7);
  }
  70% {
    box-shadow: 0 0 0 15px rgba(40, 167, 69, 0);
  }
  100% {
    box-shadow: 0 0 0 0 rgba(40, 167, 69, 0);
  }
}

.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* ===================================
   Responsive Design
   =================================== */

@media (max-width: 768px) {
  .training-session {
    padding: 15px;
  }
  
  .session-header {
    flex-direction: column;
    align-items: stretch;
  }
  
  .session-status {
    text-align: center;
  }
  
  .session-info {
    flex-direction: row;
    justify-content: space-between;
  }
  
  .input-form {
    gap: 15px;
  }
  
  .quick-actions {
    flex-direction: column;
  }
  
  .summary-stats {
    grid-template-columns: 1fr;
  }
  
  .navigation-buttons {
    flex-direction: column;
  }
  
  .set-item {
    flex-direction: column;
    align-items: stretch;
    gap: 10px;
  }
  
  .set-details {
    justify-content: space-between;
  }
  
  /* Preset options responsive */
  .preset-card {
    padding: 20px;
  }
  
  .preset-header {
    flex-direction: column;
    gap: 15px;
    align-items: stretch;
  }
  
  .preset-dismiss-btn {
    align-self: flex-end;
  }
  
  .preset-actions {
    flex-direction: column;
  }
  
  .preset-actions .btn {
    max-width: none;
  }
}

/* ===================================
   Accessibility
   =================================== */

@media (prefers-reduced-motion: reduce) {
  .btn, .input-field, .set-item {
    transition: none;
  }
  
  .pulse-glow, .pulse, .spin {
    animation: none;
  }
}

/* Focus styles for keyboard navigation */
.btn:focus,
.input-field:focus {
  outline: 2px solid var(--primary);
  outline-offset: 2px;
}
</style>