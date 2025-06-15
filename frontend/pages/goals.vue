<template>
  <div class="goals-management">
    <!-- Header -->
    <div class="page-header">
      <div class="header-content">
        <h1>🎯 目標管理</h1>
        <p>あなたのトレーニング目標を設定し、進捗を追跡しましょう</p>
      </div>
      <div class="header-actions">
        <button @click="showCreateModal = true" class="btn btn-primary">
          <Icon name="mdi:plus" />
          新しい目標
        </button>
      </div>
    </div>

    <!-- Goals Overview Stats -->
    <div class="goals-overview">
      <div class="overview-card">
        <div class="overview-icon">🎯</div>
        <div class="overview-content">
          <span class="overview-number">{{ totalGoals }}</span>
          <span class="overview-label">総目標数</span>
        </div>
      </div>
      <div class="overview-card">
        <div class="overview-icon">✅</div>
        <div class="overview-content">
          <span class="overview-number">{{ completedGoals }}</span>
          <span class="overview-label">達成済み</span>
        </div>
      </div>
      <div class="overview-card">
        <div class="overview-icon">🔥</div>
        <div class="overview-content">
          <span class="overview-number">{{ activeGoals }}</span>
          <span class="overview-label">進行中</span>
        </div>
      </div>
      <div class="overview-card">
        <div class="overview-icon">📈</div>
        <div class="overview-content">
          <span class="overview-number">{{ averageProgress }}%</span>
          <span class="overview-label">平均進捗</span>
        </div>
      </div>
    </div>

    <!-- Filter Tabs -->
    <div class="filter-tabs">
      <button 
        v-for="filter in filters" 
        :key="filter.key"
        @click="activeFilter = filter.key"
        :class="['filter-tab', { active: activeFilter === filter.key }]"
      >
        {{ filter.label }}
      </button>
    </div>

    <!-- Goals List -->
    <div class="goals-list">
      <div v-if="filteredGoals.length === 0" class="empty-state">
        <div class="empty-icon">🎯</div>
        <h3>{{ getEmptyMessage() }}</h3>
        <p>新しい目標を設定して、トレーニングを次のレベルに！</p>
        <button @click="showCreateModal = true" class="btn btn-primary">
          目標を作成
        </button>
      </div>

      <div 
        v-for="goal in filteredGoals" 
        :key="goal.id"
        class="goal-card"
        :class="[`status-${goal.status}`, `category-${goal.category}`]"
      >
        <div class="goal-header">
          <div class="goal-info">
            <div class="goal-category">{{ getCategoryText(goal.category) }}</div>
            <h3 class="goal-title">{{ goal.title }}</h3>
            <p class="goal-description">{{ goal.description }}</p>
          </div>
          <div class="goal-actions">
            <button @click="editGoal(goal)" class="action-btn edit-btn">
              <Icon name="mdi:pencil" />
            </button>
            <button @click="deleteGoal(goal)" class="action-btn delete-btn">
              <Icon name="mdi:delete" />
            </button>
          </div>
        </div>

        <div class="goal-details">
          <div class="goal-progress">
            <div class="progress-header">
              <span class="progress-label">進捗</span>
              <span class="progress-percentage">{{ Math.round(goal.progress) }}%</span>
            </div>
            <div class="progress-bar">
              <div 
                class="progress-fill"
                :style="{ width: Math.min(goal.progress, 100) + '%' }"
              ></div>
            </div>
            <div class="progress-details">
              <span class="current-value">{{ formatValue(goal.currentValue, goal.unit) }}</span>
              <span class="target-value">目標: {{ formatValue(goal.targetValue, goal.unit) }}</span>
            </div>
          </div>

          <div class="goal-timeline">
            <div class="timeline-item">
              <Icon name="mdi:calendar-start" />
              <span>開始: {{ formatDate(goal.startDate) }}</span>
            </div>
            <div class="timeline-item">
              <Icon name="mdi:calendar-end" />
              <span>期限: {{ formatDate(goal.endDate) }}</span>
            </div>
            <div class="timeline-item">
              <Icon name="mdi:clock-outline" />
              <span>残り: {{ getRemainingDays(goal.endDate) }}日</span>
            </div>
          </div>
        </div>

        <div v-if="goal.milestones && goal.milestones.length > 0" class="goal-milestones">
          <h4>マイルストーン</h4>
          <div class="milestones-list">
            <div 
              v-for="milestone in goal.milestones" 
              :key="milestone.id"
              class="milestone-item"
              :class="{ completed: milestone.completed }"
            >
              <div class="milestone-checkbox">
                <Icon :name="milestone.completed ? 'mdi:check' : 'mdi:circle-outline'" />
              </div>
              <div class="milestone-content">
                <span class="milestone-title">{{ milestone.title }}</span>
                <span class="milestone-target">{{ formatValue(milestone.targetValue, goal.unit) }}</span>
              </div>
            </div>
          </div>
        </div>

        <div class="goal-footer">
          <div class="goal-status">
            <span class="status-badge" :class="`status-${goal.status}`">
              {{ getStatusText(goal.status) }}
            </span>
          </div>
          <div class="goal-cta">
            <button 
              v-if="goal.status === 'active'" 
              @click="updateProgress(goal)" 
              class="btn btn-secondary btn-sm"
            >
              進捗更新
            </button>
            <button 
              v-if="goal.status === 'completed'" 
              @click="viewGoalHistory(goal)" 
              class="btn btn-outline btn-sm"
            >
              履歴表示
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Create/Edit Goal Modal -->
    <div v-if="showCreateModal || editingGoal" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>{{ editingGoal ? '目標を編集' : '新しい目標を作成' }}</h2>
          <button @click="closeModal" class="modal-close-btn">
            <Icon name="mdi:close" />
          </button>
        </div>

        <form @submit.prevent="saveGoal" class="goal-form">
          <div class="form-group">
            <label>目標カテゴリ</label>
            <select v-model="goalForm.category" class="form-select">
              <option value="weight">重量向上</option>
              <option value="reps">回数向上</option>
              <option value="frequency">頻度目標</option>
              <option value="bodyweight">体重管理</option>
              <option value="endurance">持久力向上</option>
              <option value="custom">カスタム</option>
            </select>
          </div>

          <div class="form-group">
            <label>目標タイトル</label>
            <input 
              v-model="goalForm.title" 
              type="text" 
              class="form-input"
              placeholder="例: ベンチプレス100kg達成"
              required
            />
          </div>

          <div class="form-group">
            <label>説明</label>
            <textarea 
              v-model="goalForm.description" 
              class="form-textarea"
              placeholder="目標の詳細説明"
              rows="3"
            ></textarea>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>現在の値</label>
              <input 
                v-model.number="goalForm.currentValue" 
                type="number" 
                class="form-input"
                step="0.1"
                required
              />
            </div>
            <div class="form-group">
              <label>目標値</label>
              <input 
                v-model.number="goalForm.targetValue" 
                type="number" 
                class="form-input"
                step="0.1"
                required
              />
            </div>
            <div class="form-group">
              <label>単位</label>
              <select v-model="goalForm.unit" class="form-select">
                <option value="kg">kg</option>
                <option value="回">回</option>
                <option value="日">日</option>
                <option value="分">分</option>
                <option value="km">km</option>
                <option value="%">%</option>
              </select>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>開始日</label>
              <input 
                v-model="goalForm.startDate" 
                type="date" 
                class="form-input"
                required
              />
            </div>
            <div class="form-group">
              <label>期限</label>
              <input 
                v-model="goalForm.endDate" 
                type="date" 
                class="form-input"
                required
              />
            </div>
          </div>

          <div class="form-actions">
            <button type="button" @click="closeModal" class="btn btn-outline">
              キャンセル
            </button>
            <button type="submit" class="btn btn-primary">
              {{ editingGoal ? '更新' : '作成' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Progress Update Modal -->
    <div v-if="updatingGoal" class="modal-overlay" @click="closeProgressModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>進捗を更新</h2>
          <button @click="closeProgressModal" class="modal-close-btn">
            <Icon name="mdi:close" />
          </button>
        </div>

        <div class="progress-update-form">
          <div class="current-goal-info">
            <h3>{{ updatingGoal.title }}</h3>
            <p class="goal-summary">
              現在: {{ formatValue(updatingGoal.currentValue, updatingGoal.unit) }} → 
              目標: {{ formatValue(updatingGoal.targetValue, updatingGoal.unit) }}
            </p>
          </div>

          <div class="form-group">
            <label>新しい値</label>
            <div class="input-with-unit">
              <input 
                v-model.number="progressForm.newValue" 
                type="number" 
                class="form-input"
                step="0.1"
                :min="updatingGoal.currentValue"
                required
              />
              <span class="input-unit">{{ updatingGoal.unit }}</span>
            </div>
          </div>

          <div class="form-group">
            <label>メモ（任意）</label>
            <textarea 
              v-model="progressForm.note" 
              class="form-textarea"
              placeholder="この進捗についてのメモ"
              rows="2"
            ></textarea>
          </div>

          <div class="progress-prediction">
            <div class="prediction-item">
              <span class="prediction-label">更新後の進捗:</span>
              <span class="prediction-value">{{ getProgressAfterUpdate() }}%</span>
            </div>
            <div class="prediction-item">
              <span class="prediction-label">目標までの残り:</span>
              <span class="prediction-value">{{ formatValue(getRemainingValue(), updatingGoal.unit) }}</span>
            </div>
          </div>

          <div class="form-actions">
            <button @click="closeProgressModal" class="btn btn-outline">
              キャンセル
            </button>
            <button @click="saveProgress" class="btn btn-primary">
              進捗を保存
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useNotifications } from '~/composables/useNotifications'

// ===================================
// Types & Interfaces
// ===================================

interface Goal {
  id: string
  title: string
  description: string
  category: 'weight' | 'reps' | 'frequency' | 'bodyweight' | 'endurance' | 'custom'
  currentValue: number
  targetValue: number
  unit: string
  progress: number
  status: 'active' | 'completed' | 'paused' | 'cancelled'
  startDate: string
  endDate: string
  milestones?: Milestone[]
  createdAt: Date
  updatedAt: Date
}

interface Milestone {
  id: string
  title: string
  targetValue: number
  completed: boolean
  completedAt?: Date
}

interface GoalForm {
  title: string
  description: string
  category: string
  currentValue: number
  targetValue: number
  unit: string
  startDate: string
  endDate: string
}

interface ProgressForm {
  newValue: number
  note: string
}

// ===================================
// State
// ===================================

const notifications = useNotifications()

const goals = ref<Goal[]>([])
const activeFilter = ref('all')
const showCreateModal = ref(false)
const editingGoal = ref<Goal | null>(null)
const updatingGoal = ref<Goal | null>(null)

const goalForm = ref<GoalForm>({
  title: '',
  description: '',
  category: 'weight',
  currentValue: 0,
  targetValue: 100,
  unit: 'kg',
  startDate: new Date().toISOString().split('T')[0],
  endDate: new Date(Date.now() + 90 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
})

const progressForm = ref<ProgressForm>({
  newValue: 0,
  note: ''
})

const filters = [
  { key: 'all', label: 'すべて' },
  { key: 'active', label: '進行中' },
  { key: 'completed', label: '達成済み' },
  { key: 'weight', label: '重量' },
  { key: 'reps', label: '回数' },
  { key: 'frequency', label: '頻度' }
]

// ===================================
// Computed Properties
// ===================================

const totalGoals = computed(() => goals.value.length)
const completedGoals = computed(() => goals.value.filter(g => g.status === 'completed').length)
const activeGoals = computed(() => goals.value.filter(g => g.status === 'active').length)
const averageProgress = computed(() => {
  if (goals.value.length === 0) return 0
  const sum = goals.value.reduce((acc, goal) => acc + goal.progress, 0)
  return Math.round(sum / goals.value.length)
})

const filteredGoals = computed(() => {
  return goals.value.filter(goal => {
    if (activeFilter.value === 'all') return true
    if (activeFilter.value === 'active') return goal.status === 'active'
    if (activeFilter.value === 'completed') return goal.status === 'completed'
    return goal.category === activeFilter.value
  }).sort((a, b) => {
    // Sort by status (active first), then by progress (higher first)
    if (a.status !== b.status) {
      if (a.status === 'active') return -1
      if (b.status === 'active') return 1
    }
    return b.progress - a.progress
  })
})

// ===================================
// Methods
// ===================================

function loadGoals() {
  // Mock data - in real app, load from API
  goals.value = [
    {
      id: '1',
      title: 'ベンチプレス100kg達成',
      description: '安全なフォームで100kgを1回挙げる',
      category: 'weight',
      currentValue: 85,
      targetValue: 100,
      unit: 'kg',
      progress: 85,
      status: 'active',
      startDate: '2024-01-01',
      endDate: '2024-06-30',
      milestones: [
        { id: 'm1', title: '90kg達成', targetValue: 90, completed: true, completedAt: new Date('2024-05-01') },
        { id: 'm2', title: '95kg達成', targetValue: 95, completed: false },
        { id: 'm3', title: '100kg達成', targetValue: 100, completed: false }
      ],
      createdAt: new Date('2024-01-01'),
      updatedAt: new Date()
    },
    {
      id: '2',
      title: '週3回のトレーニング継続',
      description: '3ヶ月間週3回のペースでトレーニングを継続',
      category: 'frequency',
      currentValue: 8,
      targetValue: 12,
      unit: '週',
      progress: 67,
      status: 'active',
      startDate: '2024-04-01',
      endDate: '2024-06-30',
      createdAt: new Date('2024-04-01'),
      updatedAt: new Date()
    },
    {
      id: '3',
      title: 'スクワット150kg達成',
      description: '正しいフォームでスクワット150kg',
      category: 'weight',
      currentValue: 150,
      targetValue: 150,
      unit: 'kg',
      progress: 100,
      status: 'completed',
      startDate: '2024-01-01',
      endDate: '2024-05-31',
      createdAt: new Date('2024-01-01'),
      updatedAt: new Date('2024-05-15')
    }
  ]
}

function getCategoryText(category: string): string {
  const categories = {
    weight: '重量向上',
    reps: '回数向上',
    frequency: '頻度目標',
    bodyweight: '体重管理',
    endurance: '持久力向上',
    custom: 'カスタム'
  }
  return categories[category] || category
}

function getStatusText(status: string): string {
  const statuses = {
    active: '進行中',
    completed: '達成済み',
    paused: '一時停止',
    cancelled: 'キャンセル'
  }
  return statuses[status] || status
}

function getEmptyMessage(): string {
  if (activeFilter.value === 'completed') return '達成した目標がありません'
  if (activeFilter.value === 'active') return '進行中の目標がありません'
  return 'まだ目標が設定されていません'
}

function formatValue(value: number, unit: string): string {
  return `${value}${unit}`
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('ja-JP')
}

function getRemainingDays(endDate: string): number {
  const end = new Date(endDate)
  const now = new Date()
  const diffTime = end.getTime() - now.getTime()
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
  return Math.max(0, diffDays)
}

function editGoal(goal: Goal) {
  editingGoal.value = goal
  goalForm.value = {
    title: goal.title,
    description: goal.description,
    category: goal.category,
    currentValue: goal.currentValue,
    targetValue: goal.targetValue,
    unit: goal.unit,
    startDate: goal.startDate,
    endDate: goal.endDate
  }
}

function deleteGoal(goal: Goal) {
  if (confirm(`「${goal.title}」を削除しますか？`)) {
    goals.value = goals.value.filter(g => g.id !== goal.id)
    notifications.success('削除完了', '目標を削除しました')
  }
}

function updateProgress(goal: Goal) {
  updatingGoal.value = goal
  progressForm.value = {
    newValue: goal.currentValue,
    note: ''
  }
}

function viewGoalHistory(goal: Goal) {
  // Navigate to goal history page
  navigateTo(`/goals/${goal.id}/history`)
}

function closeModal() {
  showCreateModal.value = false
  editingGoal.value = null
  resetForm()
}

function closeProgressModal() {
  updatingGoal.value = null
  progressForm.value = { newValue: 0, note: '' }
}

function resetForm() {
  goalForm.value = {
    title: '',
    description: '',
    category: 'weight',
    currentValue: 0,
    targetValue: 100,
    unit: 'kg',
    startDate: new Date().toISOString().split('T')[0],
    endDate: new Date(Date.now() + 90 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
  }
}

function saveGoal() {
  if (editingGoal.value) {
    // Update existing goal
    const index = goals.value.findIndex(g => g.id === editingGoal.value!.id)
    if (index !== -1) {
      goals.value[index] = {
        ...goals.value[index],
        ...goalForm.value,
        progress: (goalForm.value.currentValue / goalForm.value.targetValue) * 100,
        status: goalForm.value.currentValue >= goalForm.value.targetValue ? 'completed' : 'active',
        updatedAt: new Date()
      }
      notifications.success('更新完了', '目標を更新しました')
    }
  } else {
    // Create new goal
    const newGoal: Goal = {
      id: Date.now().toString(),
      ...goalForm.value,
      progress: (goalForm.value.currentValue / goalForm.value.targetValue) * 100,
      status: goalForm.value.currentValue >= goalForm.value.targetValue ? 'completed' : 'active',
      createdAt: new Date(),
      updatedAt: new Date()
    }
    goals.value.push(newGoal)
    notifications.success('作成完了', '新しい目標を作成しました')
  }
  
  closeModal()
}

function getProgressAfterUpdate(): number {
  if (!updatingGoal.value) return 0
  return Math.round((progressForm.value.newValue / updatingGoal.value.targetValue) * 100)
}

function getRemainingValue(): number {
  if (!updatingGoal.value) return 0
  return Math.max(0, updatingGoal.value.targetValue - progressForm.value.newValue)
}

function saveProgress() {
  if (!updatingGoal.value) return
  
  const goal = updatingGoal.value
  const newProgress = (progressForm.value.newValue / goal.targetValue) * 100
  
  // Update goal
  const index = goals.value.findIndex(g => g.id === goal.id)
  if (index !== -1) {
    goals.value[index] = {
      ...goals.value[index],
      currentValue: progressForm.value.newValue,
      progress: newProgress,
      status: newProgress >= 100 ? 'completed' : 'active',
      updatedAt: new Date()
    }
    
    // Check milestones
    if (goal.milestones) {
      goal.milestones.forEach(milestone => {
        if (!milestone.completed && progressForm.value.newValue >= milestone.targetValue) {
          milestone.completed = true
          milestone.completedAt = new Date()
          notifications.success('マイルストーン達成！', `「${milestone.title}」を達成しました`)
        }
      })
    }
    
    // Check goal completion
    if (newProgress >= 100) {
      notifications.success('目標達成！', `「${goal.title}」を達成しました！🎉`)
    } else {
      notifications.info('進捗更新', '目標の進捗を更新しました')
    }
  }
  
  closeProgressModal()
}

// ===================================
// Lifecycle
// ===================================

onMounted(() => {
  loadGoals()
})

// ===================================
// Page Meta
// ===================================

useHead({
  title: '目標管理 - Message',
  meta: [
    { name: 'description', content: 'トレーニング目標の設定と進捗管理' }
  ]
})
</script>

<style scoped>
.goals-management {
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
  font-weight: 700;
}

.header-content p {
  margin: 0;
  color: var(--text-secondary);
}

/* Overview Stats */
.goals-overview {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.overview-card {
  background: var(--card-bg);
  border-radius: 12px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 15px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  border: 1px solid var(--border);
}

.overview-icon {
  font-size: 2.5rem;
}

.overview-content {
  display: flex;
  flex-direction: column;
}

.overview-number {
  font-size: 2rem;
  font-weight: 700;
  color: var(--primary);
  line-height: 1;
}

.overview-label {
  font-size: 0.9rem;
  color: var(--text-secondary);
}

/* Filter Tabs */
.filter-tabs {
  display: flex;
  gap: 4px;
  background: var(--bg-secondary);
  border-radius: 12px;
  padding: 4px;
  margin-bottom: 30px;
  overflow-x: auto;
}

.filter-tab {
  flex: 1;
  min-width: fit-content;
  padding: 12px 16px;
  background: transparent;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  font-weight: 500;
  color: var(--text-secondary);
  white-space: nowrap;
}

.filter-tab.active {
  background: var(--primary);
  color: white;
  box-shadow: 0 2px 8px rgba(0, 122, 255, 0.3);
}

.filter-tab:hover:not(.active) {
  background: var(--bg-tertiary);
  color: var(--text-primary);
}

/* Goals List */
.goals-list {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.empty-state {
  text-align: center;
  padding: 60px 20px;
  color: var(--text-secondary);
}

.empty-icon {
  font-size: 4rem;
  margin-bottom: 20px;
}

.empty-state h3 {
  margin: 0 0 10px 0;
  color: var(--text-primary);
}

/* Goal Card */
.goal-card {
  background: var(--card-bg);
  border-radius: 16px;
  padding: 25px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  border: 2px solid var(--border);
  transition: all 0.2s;
}

.goal-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 25px rgba(0, 0, 0, 0.15);
}

.goal-card.status-completed {
  border-color: #27ae60;
  background: linear-gradient(135deg, rgba(39, 174, 96, 0.05), rgba(39, 174, 96, 0.1));
}

.goal-card.status-active {
  border-color: var(--primary);
}

.goal-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 20px;
  gap: 15px;
}

.goal-info {
  flex: 1;
}

.goal-category {
  display: inline-block;
  background: var(--primary);
  color: white;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 0.7rem;
  font-weight: bold;
  margin-bottom: 8px;
}

.goal-title {
  margin: 0 0 8px 0;
  color: var(--text-primary);
  font-size: 1.3rem;
  font-weight: 600;
}

.goal-description {
  margin: 0;
  color: var(--text-secondary);
  line-height: 1.4;
}

.goal-actions {
  display: flex;
  gap: 8px;
}

.action-btn {
  width: 36px;
  height: 36px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.edit-btn {
  background: #f39c12;
  color: white;
}

.delete-btn {
  background: #e74c3c;
  color: white;
}

.action-btn:hover {
  transform: scale(1.1);
}

/* Goal Details */
.goal-details {
  margin-bottom: 20px;
}

.goal-progress {
  margin-bottom: 15px;
}

.progress-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.progress-label {
  font-weight: 600;
  color: var(--text-primary);
}

.progress-percentage {
  font-weight: 700;
  color: var(--primary);
  font-size: 1.1rem;
}

.progress-bar {
  height: 8px;
  background: var(--border);
  border-radius: 4px;
  overflow: hidden;
  margin-bottom: 8px;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, var(--primary), #0056b3);
  border-radius: 4px;
  transition: width 0.3s ease;
}

.goal-card.status-completed .progress-fill {
  background: linear-gradient(90deg, #27ae60, #1e8449);
}

.progress-details {
  display: flex;
  justify-content: space-between;
  font-size: 0.9rem;
}

.current-value {
  font-weight: 600;
  color: var(--text-primary);
}

.target-value {
  color: var(--text-secondary);
}

.goal-timeline {
  display: flex;
  gap: 20px;
  flex-wrap: wrap;
}

.timeline-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.9rem;
  color: var(--text-secondary);
}

/* Milestones */
.goal-milestones {
  margin-bottom: 20px;
  padding: 15px;
  background: var(--bg-secondary);
  border-radius: 8px;
}

.goal-milestones h4 {
  margin: 0 0 10px 0;
  color: var(--text-primary);
  font-size: 1rem;
}

.milestones-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.milestone-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px;
  background: var(--card-bg);
  border-radius: 6px;
  border: 1px solid var(--border);
}

.milestone-item.completed {
  background: rgba(39, 174, 96, 0.1);
  border-color: #27ae60;
}

.milestone-checkbox {
  color: var(--text-secondary);
}

.milestone-item.completed .milestone-checkbox {
  color: #27ae60;
}

.milestone-content {
  flex: 1;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.milestone-title {
  font-weight: 500;
  color: var(--text-primary);
}

.milestone-target {
  font-weight: 600;
  color: var(--primary);
}

/* Goal Footer */
.goal-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 10px;
}

.status-badge {
  padding: 6px 12px;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: bold;
  color: white;
}

.status-badge.status-active {
  background: var(--primary);
}

.status-badge.status-completed {
  background: #27ae60;
}

.status-badge.status-paused {
  background: #f39c12;
}

/* Buttons */
.btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
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
  background: #6c757d;
  color: white;
}

.btn-outline {
  background: transparent;
  border: 2px solid var(--border);
  color: var(--text-primary);
}

.btn-outline:hover {
  background: var(--bg-secondary);
  border-color: var(--primary);
  color: var(--primary);
}

.btn-sm {
  padding: 6px 12px;
  font-size: 0.8rem;
}

/* Modal */
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

/* Form */
.goal-form {
  padding: 0 25px 25px 25px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 6px;
  font-weight: 600;
  color: var(--text-primary);
}

.form-input,
.form-select,
.form-textarea {
  width: 100%;
  padding: 12px;
  border: 2px solid var(--border);
  border-radius: 8px;
  font-size: 1rem;
  background: var(--card-bg);
  color: var(--text-primary);
  transition: border-color 0.2s;
}

.form-input:focus,
.form-select:focus,
.form-textarea:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: 0 0 0 3px rgba(0, 122, 255, 0.1);
}

.form-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 15px;
}

.form-actions {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
  padding-top: 20px;
  border-top: 1px solid var(--border);
}

/* Progress Update Form */
.progress-update-form {
  padding: 0 25px 25px 25px;
}

.current-goal-info {
  margin-bottom: 20px;
  padding: 15px;
  background: var(--bg-secondary);
  border-radius: 8px;
}

.current-goal-info h3 {
  margin: 0 0 5px 0;
  color: var(--text-primary);
}

.goal-summary {
  margin: 0;
  color: var(--text-secondary);
}

.input-with-unit {
  display: flex;
  align-items: center;
  gap: 8px;
}

.input-unit {
  font-weight: 600;
  color: var(--text-secondary);
}

.progress-prediction {
  background: var(--bg-secondary);
  padding: 15px;
  border-radius: 8px;
  margin-bottom: 20px;
}

.prediction-item {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
}

.prediction-item:last-child {
  margin-bottom: 0;
}

.prediction-label {
  color: var(--text-secondary);
}

.prediction-value {
  font-weight: 600;
  color: var(--primary);
}

/* CSS Variables */
:root {
  --card-bg: #ffffff;
  --bg-secondary: #f8f9fa;
  --bg-tertiary: #e9ecef;
  --text-primary: #212529;
  --text-secondary: #6c757d;
  --border: #dee2e6;
  --primary: #007bff;
}

html.dark {
  --card-bg: #1e1e1e;
  --bg-secondary: #2d2d2d;
  --bg-tertiary: #3d3d3d;
  --text-primary: #ffffff;
  --text-secondary: #b0b0b0;
  --border: #444444;
  --primary: #0d6efd;
}

/* Responsive */
@media (max-width: 768px) {
  .goals-management {
    padding: 15px;
  }
  
  .page-header {
    flex-direction: column;
    align-items: stretch;
  }
  
  .goals-overview {
    grid-template-columns: 1fr;
  }
  
  .goal-header {
    flex-direction: column;
    align-items: stretch;
    gap: 15px;
  }
  
  .goal-actions {
    align-self: flex-end;
  }
  
  .goal-timeline {
    flex-direction: column;
    gap: 10px;
  }
  
  .goal-footer {
    flex-direction: column;
    align-items: stretch;
  }
  
  .form-row {
    grid-template-columns: 1fr;
  }
  
  .form-actions {
    flex-direction: column;
  }
  
  .filter-tabs {
    overflow-x: auto;
  }
}
</style>