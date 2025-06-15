<template>
  <div class="goal-history">
    <!-- Header -->
    <div class="page-header">
      <div class="header-content">
        <NuxtLink to="/goals" class="back-link">
          <Icon name="mdi:arrow-left" />
          目標一覧に戻る
        </NuxtLink>
        <h1 v-if="goal">{{ goal.title }}</h1>
        <p v-if="goal">目標の詳細履歴と進捗</p>
      </div>
      <div class="header-actions" v-if="goal">
        <button @click="exportGoalData" class="btn btn-outline">
          <Icon name="mdi:download" />
          データエクスポート
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-state">
      <div class="loading-spinner">⚡</div>
      <p>履歴を読み込み中...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="error-state">
      <div class="error-icon">⚠️</div>
      <h3>エラーが発生しました</h3>
      <p>{{ error }}</p>
      <button @click="loadGoalHistory" class="btn btn-primary">再試行</button>
    </div>

    <!-- Goal Detail -->
    <div v-else-if="goal" class="goal-detail">
      <!-- Goal Overview -->
      <div class="goal-overview">
        <div class="overview-card primary">
          <div class="overview-header">
            <h3>現在の進捗</h3>
            <div class="goal-status" :class="`status-${goal.status}`">
              {{ getStatusText(goal.status) }}
            </div>
          </div>
          <div class="progress-section">
            <div class="progress-values">
              <span class="current-value">{{ formatValue(goal.currentValue, goal.unit) }}</span>
              <span class="separator">/</span>
              <span class="target-value">{{ formatValue(goal.targetValue, goal.unit) }}</span>
            </div>
            <div class="progress-bar">
              <div 
                class="progress-fill"
                :style="{ width: Math.min(goal.progress, 100) + '%' }"
              ></div>
            </div>
            <div class="progress-percentage">{{ Math.round(goal.progress) }}%</div>
          </div>
        </div>

        <div class="overview-card">
          <h3>期間</h3>
          <div class="timeline-info">
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

        <div class="overview-card">
          <h3>統計</h3>
          <div class="stats-grid">
            <div class="stat-item">
              <span class="stat-number">{{ progressHistory.length }}</span>
              <span class="stat-label">更新回数</span>
            </div>
            <div class="stat-item">
              <span class="stat-number">{{ averageProgressPerWeek }}%</span>
              <span class="stat-label">週平均進捗</span>
            </div>
            <div class="stat-item">
              <span class="stat-number">{{ estimatedCompletionDays }}</span>
              <span class="stat-label">予想完了日数</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Milestones Section -->
      <div v-if="goal.milestones && goal.milestones.length > 0" class="milestones-section">
        <h2>マイルストーン</h2>
        <div class="milestones-timeline">
          <div 
            v-for="milestone in goal.milestones" 
            :key="milestone.id"
            class="milestone-item"
            :class="{ completed: milestone.completed }"
          >
            <div class="milestone-connector"></div>
            <div class="milestone-marker">
              <Icon :name="milestone.completed ? 'mdi:check' : 'mdi:circle-outline'" />
            </div>
            <div class="milestone-content">
              <h4>{{ milestone.title }}</h4>
              <div class="milestone-details">
                <span class="milestone-target">{{ formatValue(milestone.targetValue, goal.unit) }}</span>
                <span 
                  v-if="milestone.completed && milestone.completedAt" 
                  class="milestone-date"
                >
                  {{ formatDate(milestone.completedAt) }}達成
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Progress History Chart -->
      <div class="progress-chart-section">
        <h2>進捗グラフ</h2>
        <div class="chart-container">
          <canvas ref="chartCanvas" width="800" height="400"></canvas>
        </div>
      </div>

      <!-- Progress History List -->
      <div class="progress-history-section">
        <div class="section-header">
          <h2>進捗履歴</h2>
          <div class="history-filters">
            <select v-model="historyFilter" class="filter-select">
              <option value="all">すべて</option>
              <option value="last30">過去30日</option>
              <option value="last90">過去90日</option>
            </select>
          </div>
        </div>

        <div v-if="filteredProgressHistory.length === 0" class="empty-history">
          <div class="empty-icon">📈</div>
          <p>まだ進捗履歴がありません</p>
          <NuxtLink to="/goals" class="btn btn-primary">
            目標を更新する
          </NuxtLink>
        </div>

        <div v-else class="history-list">
          <div 
            v-for="entry in filteredProgressHistory" 
            :key="entry.id"
            class="history-entry"
          >
            <div class="entry-date">
              <div class="date-day">{{ formatDay(entry.date) }}</div>
              <div class="date-month">{{ formatMonth(entry.date) }}</div>
            </div>
            <div class="entry-content">
              <div class="entry-header">
                <div class="value-change">
                  <span class="old-value">{{ formatValue(entry.oldValue, goal.unit) }}</span>
                  <Icon name="mdi:arrow-right" />
                  <span class="new-value">{{ formatValue(entry.newValue, goal.unit) }}</span>
                </div>
                <div class="progress-change">
                  +{{ Math.round(entry.progressIncrease) }}%
                </div>
              </div>
              <div v-if="entry.note" class="entry-note">
                {{ entry.note }}
              </div>
              <div class="entry-meta">
                <span class="entry-time">{{ formatTime(entry.date) }}</span>
                <span v-if="entry.milestoneAchieved" class="milestone-badge">
                  🏆 マイルストーン達成
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Goal Not Found -->
    <div v-else class="not-found-state">
      <div class="not-found-icon">🎯</div>
      <h3>目標が見つかりません</h3>
      <p>指定された目標は存在しないか、削除されている可能性があります。</p>
      <NuxtLink to="/goals" class="btn btn-primary">
        目標一覧に戻る
      </NuxtLink>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick } from 'vue'
import { useRoute } from 'vue-router'
import { useGoals } from '~/composables/useGoals'
import { useNotifications } from '~/composables/useNotifications'

// ===================================
// Setup
// ===================================

const route = useRoute()
const { getGoalById, formatGoalValue, getStatusText, calculateRemainingDays } = useGoals()
const notifications = useNotifications()

const goalId = computed(() => route.params.id as string)
const goal = ref(null)
const progressHistory = ref([])
const loading = ref(true)
const error = ref(null)
const historyFilter = ref('all')
const chartCanvas = ref(null)

// ===================================
// Computed Properties
// ===================================

const filteredProgressHistory = computed(() => {
  const now = new Date()
  return progressHistory.value.filter(entry => {
    if (historyFilter.value === 'all') return true
    if (historyFilter.value === 'last30') {
      const thirtyDaysAgo = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000)
      return new Date(entry.date) >= thirtyDaysAgo
    }
    if (historyFilter.value === 'last90') {
      const ninetyDaysAgo = new Date(now.getTime() - 90 * 24 * 60 * 60 * 1000)
      return new Date(entry.date) >= ninetyDaysAgo
    }
    return true
  }).sort((a, b) => new Date(b.date) - new Date(a.date))
})

const averageProgressPerWeek = computed(() => {
  if (progressHistory.value.length < 2) return 0
  
  const entries = [...progressHistory.value].sort((a, b) => new Date(a.date) - new Date(b.date))
  const firstEntry = entries[0]
  const lastEntry = entries[entries.length - 1]
  
  const daysDiff = Math.ceil((new Date(lastEntry.date) - new Date(firstEntry.date)) / (1000 * 60 * 60 * 24))
  const weeksDiff = daysDiff / 7
  
  if (weeksDiff === 0) return 0
  
  const progressDiff = lastEntry.newValue - firstEntry.oldValue
  const progressPercentage = (progressDiff / goal.value.targetValue) * 100
  
  return Math.round((progressPercentage / weeksDiff) * 10) / 10
})

const estimatedCompletionDays = computed(() => {
  if (!goal.value || goal.value.status === 'completed') return 0
  if (averageProgressPerWeek.value <= 0) return '∞'
  
  const remainingProgress = 100 - goal.value.progress
  const remainingWeeks = remainingProgress / averageProgressPerWeek.value
  const remainingDays = Math.ceil(remainingWeeks * 7)
  
  return remainingDays > 365 ? '365+' : remainingDays
})

// ===================================
// Methods
// ===================================

async function loadGoalHistory() {
  loading.value = true
  error.value = null
  
  try {
    // Get goal data
    goal.value = getGoalById(goalId.value)
    
    if (!goal.value) {
      error.value = 'Goal not found'
      return
    }
    
    // Mock progress history - in real app, load from API
    await new Promise(resolve => setTimeout(resolve, 500))
    
    progressHistory.value = [
      {
        id: '1',
        date: '2024-01-15T10:30:00Z',
        oldValue: 0,
        newValue: 60,
        progressIncrease: 60,
        note: '初回記録',
        milestoneAchieved: false
      },
      {
        id: '2',
        date: '2024-02-01T14:20:00Z',
        oldValue: 60,
        newValue: 70,
        progressIncrease: 10,
        note: 'フォームが安定してきた',
        milestoneAchieved: false
      },
      {
        id: '3',
        date: '2024-02-15T16:45:00Z',
        oldValue: 70,
        newValue: 75,
        progressIncrease: 5,
        note: '',
        milestoneAchieved: false
      },
      {
        id: '4',
        date: '2024-03-01T11:15:00Z',
        oldValue: 75,
        newValue: 80,
        progressIncrease: 5,
        note: 'トレーニング頻度を増やした効果',
        milestoneAchieved: false
      },
      {
        id: '5',
        date: '2024-05-01T09:30:00Z',
        oldValue: 80,
        newValue: 85,
        progressIncrease: 5,
        note: '90kg達成まであと少し！',
        milestoneAchieved: true
      }
    ]
    
    // Draw chart after data loads
    await nextTick()
    drawProgressChart()
    
  } catch (err) {
    error.value = 'Failed to load goal history'
    console.error('Error loading goal history:', err)
  } finally {
    loading.value = false
  }
}

function drawProgressChart() {
  if (!chartCanvas.value || progressHistory.value.length === 0) return
  
  const canvas = chartCanvas.value
  const ctx = canvas.getContext('2d')
  const width = canvas.width
  const height = canvas.height
  
  // Clear canvas
  ctx.clearRect(0, 0, width, height)
  
  // Set up chart area
  const padding = 60
  const chartWidth = width - padding * 2
  const chartHeight = height - padding * 2
  
  // Prepare data
  const entries = [...progressHistory.value].sort((a, b) => new Date(a.date) - new Date(b.date))
  const values = entries.map(entry => entry.newValue)
  const dates = entries.map(entry => entry.date)
  
  const minValue = Math.min(...values, 0)
  const maxValue = Math.max(...values, goal.value.targetValue)
  const valueRange = maxValue - minValue || 1
  
  // Draw grid lines
  ctx.strokeStyle = '#e0e0e0'
  ctx.lineWidth = 1
  
  // Vertical grid lines
  for (let i = 0; i <= 5; i++) {
    const x = padding + (chartWidth / 5) * i
    ctx.beginPath()
    ctx.moveTo(x, padding)
    ctx.lineTo(x, padding + chartHeight)
    ctx.stroke()
  }
  
  // Horizontal grid lines
  for (let i = 0; i <= 4; i++) {
    const y = padding + (chartHeight / 4) * i
    ctx.beginPath()
    ctx.moveTo(padding, y)
    ctx.lineTo(padding + chartWidth, y)
    ctx.stroke()
  }
  
  // Draw target line
  const targetY = padding + chartHeight - ((goal.value.targetValue - minValue) / valueRange) * chartHeight
  ctx.strokeStyle = '#27ae60'
  ctx.lineWidth = 2
  ctx.setLineDash([5, 5])
  ctx.beginPath()
  ctx.moveTo(padding, targetY)
  ctx.lineTo(padding + chartWidth, targetY)
  ctx.stroke()
  ctx.setLineDash([])
  
  // Draw progress line
  if (values.length > 1) {
    ctx.strokeStyle = '#3498db'
    ctx.lineWidth = 3
    ctx.beginPath()
    
    values.forEach((value, index) => {
      const x = padding + (chartWidth / (values.length - 1)) * index
      const y = padding + chartHeight - ((value - minValue) / valueRange) * chartHeight
      
      if (index === 0) {
        ctx.moveTo(x, y)
      } else {
        ctx.lineTo(x, y)
      }
    })
    
    ctx.stroke()
    
    // Draw data points
    ctx.fillStyle = '#3498db'
    values.forEach((value, index) => {
      const x = padding + (chartWidth / (values.length - 1)) * index
      const y = padding + chartHeight - ((value - minValue) / valueRange) * chartHeight
      
      ctx.beginPath()
      ctx.arc(x, y, 6, 0, Math.PI * 2)
      ctx.fill()
    })
  }
  
  // Draw labels
  ctx.fillStyle = '#333'
  ctx.font = '12px Arial'
  ctx.textAlign = 'center'
  
  // X-axis labels (dates)
  dates.forEach((date, index) => {
    if (index % Math.ceil(dates.length / 5) === 0) {
      const x = padding + (chartWidth / (dates.length - 1)) * index
      const shortDate = new Date(date).toLocaleDateString('ja-JP', { month: 'short', day: 'numeric' })
      ctx.fillText(shortDate, x, height - 20)
    }
  })
  
  // Y-axis labels (values)
  ctx.textAlign = 'right'
  for (let i = 0; i <= 4; i++) {
    const value = minValue + (valueRange / 4) * (4 - i)
    const y = padding + (chartHeight / 4) * i + 5
    ctx.fillText(Math.round(value * 10) / 10 + goal.value.unit, padding - 10, y)
  }
  
  // Target line label
  ctx.textAlign = 'left'
  ctx.fillStyle = '#27ae60'
  ctx.fillText(`目標: ${goal.value.targetValue}${goal.value.unit}`, padding + 10, targetY - 5)
}

async function exportGoalData() {
  try {
    const exportData = {
      goal: goal.value,
      progressHistory: progressHistory.value,
      statistics: {
        averageProgressPerWeek: averageProgressPerWeek.value,
        estimatedCompletionDays: estimatedCompletionDays.value,
        totalUpdates: progressHistory.value.length
      },
      exportedAt: new Date().toISOString(),
      exportedBy: 'Message Training App'
    }
    
    const dataStr = JSON.stringify(exportData, null, 2)
    const dataUri = 'data:application/json;charset=utf-8,'+ encodeURIComponent(dataStr)
    
    const exportFileDefaultName = `goal_${goal.value.title.replace(/[^a-zA-Z0-9]/g, '_')}_${new Date().toISOString().split('T')[0]}.json`
    
    const linkElement = document.createElement('a')
    linkElement.setAttribute('href', dataUri)
    linkElement.setAttribute('download', exportFileDefaultName)
    linkElement.click()
    
    notifications.success('エクスポート完了', '目標データをダウンロードしました')
    
  } catch (err) {
    console.error('Export error:', err)
    notifications.error('エクスポートエラー', 'データのエクスポートに失敗しました')
  }
}

function formatValue(value, unit) {
  return formatGoalValue(value, unit)
}

function formatDate(dateString) {
  return new Date(dateString).toLocaleDateString('ja-JP')
}

function formatDay(dateString) {
  return new Date(dateString).getDate()
}

function formatMonth(dateString) {
  return new Date(dateString).toLocaleDateString('ja-JP', { month: 'short' })
}

function formatTime(dateString) {
  return new Date(dateString).toLocaleTimeString('ja-JP', { 
    hour: '2-digit', 
    minute: '2-digit' 
  })
}

function getRemainingDays(endDate) {
  return calculateRemainingDays(endDate)
}

// ===================================
// Lifecycle
// ===================================

onMounted(() => {
  loadGoalHistory()
})

// ===================================
// Page Meta
// ===================================

useHead({
  title: computed(() => goal.value ? `${goal.value.title} - 目標履歴` : '目標履歴'),
  meta: [
    { name: 'description', content: '目標の詳細履歴と進捗分析' }
  ]
})
</script>

<style scoped>
.goal-history {
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

.back-link {
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--primary);
  text-decoration: none;
  font-weight: 500;
  margin-bottom: 10px;
  transition: color 0.2s;
}

.back-link:hover {
  color: var(--primary-dark);
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

/* Loading/Error States */
.loading-state,
.error-state,
.not-found-state {
  text-align: center;
  padding: 60px 20px;
  color: var(--text-secondary);
}

.loading-spinner,
.error-icon,
.not-found-icon {
  font-size: 4rem;
  margin-bottom: 20px;
}

.loading-spinner {
  animation: pulse 1.5s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 0.5; }
  50% { opacity: 1; }
}

/* Goal Overview */
.goal-overview {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.overview-card {
  background: var(--card-bg);
  border-radius: 16px;
  padding: 25px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  border: 2px solid var(--border);
}

.overview-card.primary {
  border-color: var(--primary);
  background: linear-gradient(135deg, rgba(0, 122, 255, 0.05), rgba(0, 122, 255, 0.1));
}

.overview-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.overview-card h3 {
  margin: 0;
  color: var(--text-primary);
  font-size: 1.2rem;
  font-weight: 600;
}

.goal-status {
  padding: 6px 12px;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: bold;
  color: white;
}

.goal-status.status-active {
  background: var(--primary);
}

.goal-status.status-completed {
  background: #27ae60;
}

.progress-section {
  text-align: center;
}

.progress-values {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  margin-bottom: 15px;
}

.current-value {
  font-size: 2rem;
  font-weight: 700;
  color: var(--primary);
}

.separator {
  font-size: 1.5rem;
  color: var(--text-secondary);
}

.target-value {
  font-size: 1.5rem;
  color: var(--text-secondary);
}

.progress-bar {
  height: 12px;
  background: var(--border);
  border-radius: 6px;
  overflow: hidden;
  margin-bottom: 10px;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, var(--primary), #0056b3);
  border-radius: 6px;
  transition: width 0.3s ease;
}

.progress-percentage {
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--primary);
}

.timeline-info {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.timeline-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.9rem;
  color: var(--text-secondary);
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 15px;
}

.stat-item {
  text-align: center;
}

.stat-number {
  display: block;
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--primary);
  line-height: 1;
}

.stat-label {
  font-size: 0.8rem;
  color: var(--text-secondary);
}

/* Milestones Timeline */
.milestones-section {
  margin-bottom: 30px;
}

.milestones-section h2 {
  margin: 0 0 20px 0;
  color: var(--text-primary);
  font-size: 1.5rem;
  font-weight: 600;
}

.milestones-timeline {
  position: relative;
  padding-left: 30px;
}

.milestone-item {
  position: relative;
  margin-bottom: 25px;
  padding-left: 30px;
}

.milestone-item:last-child {
  margin-bottom: 0;
}

.milestone-connector {
  position: absolute;
  left: -15px;
  top: 30px;
  bottom: -25px;
  width: 2px;
  background: var(--border);
}

.milestone-item:last-child .milestone-connector {
  display: none;
}

.milestone-marker {
  position: absolute;
  left: -23px;
  top: 0;
  width: 16px;
  height: 16px;
  border-radius: 50%;
  background: var(--card-bg);
  border: 2px solid var(--border);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  color: var(--text-secondary);
}

.milestone-item.completed .milestone-marker {
  border-color: #27ae60;
  background: #27ae60;
  color: white;
}

.milestone-content h4 {
  margin: 0 0 5px 0;
  color: var(--text-primary);
  font-size: 1rem;
  font-weight: 600;
}

.milestone-details {
  display: flex;
  gap: 15px;
  align-items: center;
}

.milestone-target {
  font-weight: 600;
  color: var(--primary);
}

.milestone-date {
  font-size: 0.8rem;
  color: var(--text-secondary);
}

/* Progress Chart */
.progress-chart-section {
  margin-bottom: 30px;
}

.progress-chart-section h2 {
  margin: 0 0 20px 0;
  color: var(--text-primary);
  font-size: 1.5rem;
  font-weight: 600;
}

.chart-container {
  background: var(--card-bg);
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  border: 1px solid var(--border);
  overflow-x: auto;
}

/* Progress History */
.progress-history-section {
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
  margin: 0;
  color: var(--text-primary);
  font-size: 1.5rem;
  font-weight: 600;
}

.filter-select {
  padding: 8px 12px;
  border: 2px solid var(--border);
  border-radius: 6px;
  background: var(--card-bg);
  color: var(--text-primary);
  font-size: 0.9rem;
}

.empty-history {
  text-align: center;
  padding: 40px 20px;
  color: var(--text-secondary);
}

.empty-icon {
  font-size: 3rem;
  margin-bottom: 15px;
}

.history-list {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.history-entry {
  display: flex;
  gap: 20px;
  background: var(--card-bg);
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  border: 1px solid var(--border);
  transition: all 0.2s;
}

.history-entry:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.15);
}

.entry-date {
  text-align: center;
  min-width: 60px;
}

.date-day {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--primary);
  line-height: 1;
}

.date-month {
  font-size: 0.8rem;
  color: var(--text-secondary);
}

.entry-content {
  flex: 1;
}

.entry-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
  flex-wrap: wrap;
  gap: 10px;
}

.value-change {
  display: flex;
  align-items: center;
  gap: 8px;
}

.old-value {
  color: var(--text-secondary);
  text-decoration: line-through;
}

.new-value {
  font-weight: 600;
  color: var(--primary);
}

.progress-change {
  background: #27ae60;
  color: white;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: bold;
}

.entry-note {
  background: var(--bg-secondary);
  padding: 8px 12px;
  border-radius: 6px;
  font-style: italic;
  color: var(--text-secondary);
  margin-bottom: 8px;
}

.entry-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.8rem;
  color: var(--text-secondary);
}

.milestone-badge {
  background: linear-gradient(135deg, #f39c12, #e67e22);
  color: white;
  padding: 2px 6px;
  border-radius: 10px;
  font-weight: bold;
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

/* CSS Variables */
:root {
  --card-bg: #ffffff;
  --bg-secondary: #f8f9fa;
  --text-primary: #212529;
  --text-secondary: #6c757d;
  --border: #dee2e6;
  --primary: #007bff;
  --primary-dark: #0056b3;
}

html.dark {
  --card-bg: #1e1e1e;
  --bg-secondary: #2d2d2d;
  --text-primary: #ffffff;
  --text-secondary: #b0b0b0;
  --border: #444444;
  --primary: #0d6efd;
  --primary-dark: #0b5ed7;
}

/* Responsive */
@media (max-width: 768px) {
  .goal-history {
    padding: 15px;
  }
  
  .page-header {
    flex-direction: column;
    align-items: stretch;
  }
  
  .goal-overview {
    grid-template-columns: 1fr;
  }
  
  .milestones-timeline {
    padding-left: 20px;
  }
  
  .milestone-item {
    padding-left: 20px;
  }
  
  .milestone-marker {
    left: -18px;
  }
  
  .milestone-connector {
    left: -10px;
  }
  
  .history-entry {
    flex-direction: column;
    gap: 15px;
  }
  
  .entry-date {
    align-self: flex-start;
  }
  
  .entry-header {
    flex-direction: column;
    align-items: stretch;
    gap: 10px;
  }
  
  .value-change {
    justify-content: center;
  }
  
  .stats-grid {
    grid-template-columns: 1fr;
  }
  
  .chart-container canvas {
    max-width: 100%;
    height: auto;
  }
}
</style>