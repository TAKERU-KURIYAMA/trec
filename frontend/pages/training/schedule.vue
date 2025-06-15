<template>
  <div class="training-schedule">
    <!-- Header -->
    <div class="page-header">
      <div class="header-content">
        <h1>トレーニング予定</h1>
        <p>今後のワークアウトを計画・管理できます</p>
      </div>
      <div class="header-actions">
        <button @click="exportSchedules" class="btn btn-outline">
          <Icon name="mdi:download" />
          データエクスポート
        </button>
        <button @click="toggleView" class="btn btn-outline">
          <Icon :name="viewMode === 'calendar' ? 'mdi:format-list-bulleted' : 'mdi:calendar'" />
          {{ viewMode === 'calendar' ? 'リスト表示' : 'カレンダー表示' }}
        </button>
        <button @click="openCreateModal" class="btn btn-primary">
          <Icon name="mdi:plus" />
          予定追加
        </button>
      </div>
    </div>

    <!-- View Toggle and Quick Stats -->
    <div class="stats-section">
      <div class="stat-card">
        <div class="stat-icon">📅</div>
        <div class="stat-content">
          <span class="stat-number">{{ upcomingSchedules.length }}</span>
          <span class="stat-label">今後の予定</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon">🗓️</div>
        <div class="stat-content">
          <span class="stat-number">{{ thisWeekSchedules.length }}</span>
          <span class="stat-label">今週の予定</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon">🎯</div>
        <div class="stat-content">
          <span class="stat-number">{{ todaySchedules.length }}</span>
          <span class="stat-label">今日の予定</span>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading.isLoading" class="loading-state">
      <div class="loading-spinner">⚡</div>
      <p>{{ loading.message || '予定を読み込み中...' }}</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error.hasError" class="error-state">
      <div class="error-icon">⚠️</div>
      <h3>エラーが発生しました</h3>
      <p>{{ error.message }}</p>
      <button @click="loadSchedules" class="btn btn-primary">再試行</button>
    </div>

    <!-- Schedule Content -->
    <div v-else class="schedule-content">
      <!-- Calendar View -->
      <div v-if="viewMode === 'calendar'" class="calendar-view">
        <div class="calendar-header">
          <button @click="previousMonth" class="nav-btn">
            <Icon name="mdi:chevron-left" />
          </button>
          <h2>{{ currentMonthText }}</h2>
          <button @click="nextMonth" class="nav-btn">
            <Icon name="mdi:chevron-right" />
          </button>
        </div>
        
        <div class="calendar-grid">
          <div class="calendar-weekdays">
            <div v-for="day in weekdays" :key="day" class="weekday">{{ day }}</div>
          </div>
          <div class="calendar-days">
            <div 
              v-for="day in calendarDays" 
              :key="day.date"
              :class="['calendar-day', {
                'other-month': day.isOtherMonth,
                'today': day.isToday,
                'has-schedule': day.hasSchedule
              }]"
              @click="selectDate(day.date)"
            >
              <span class="day-number">{{ day.day }}</span>
              <div v-if="day.schedules.length > 0" class="schedule-indicators">
                <div 
                  v-for="schedule in day.schedules.slice(0, 2)" 
                  :key="schedule.scheduleId"
                  class="schedule-dot"
                  :title="schedule.menuName"
                ></div>
                <div v-if="day.schedules.length > 2" class="more-indicator">
                  +{{ day.schedules.length - 2 }}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- List View -->
      <div v-else class="list-view">
        <!-- Filters -->
        <div class="filter-section">
          <div class="filter-group">
            <select v-model="selectedMenuFilter" class="menu-select">
              <option value="">すべてのメニュー</option>
              <option v-for="menu in menus" :key="menu.menuId" :value="menu.menuId">
                {{ menu.jpName }}
              </option>
            </select>
            <input type="date" v-model="startDateFilter" class="date-input" />
            <input type="date" v-model="endDateFilter" class="date-input" />
            <button @click="applyFilters" class="btn btn-secondary">絞り込み</button>
            <button @click="resetFilters" class="btn btn-outline">リセット</button>
          </div>
        </div>

        <!-- Schedule List -->
        <div class="schedule-list">
          <div v-if="filteredSchedules.length === 0" class="empty-state">
            <div class="empty-icon">📅</div>
            <h3>予定がありません</h3>
            <p>新しいトレーニング予定を追加してみましょう！</p>
            <button @click="openCreateModal" class="btn btn-primary">
              <Icon name="mdi:plus" />
              最初の予定を追加
            </button>
          </div>
          
          <div v-else>
            <div 
              v-for="schedule in filteredSchedules" 
              :key="schedule.scheduleId"
              class="schedule-item"
            >
              <div class="schedule-header">
                <div class="schedule-info">
                  <h3>{{ schedule.menuName }}</h3>
                  <div class="schedule-meta">
                    <span class="schedule-date">{{ formatDate(schedule.scheduledDate) }}</span>
                    <span v-if="schedule.scheduledTime" class="schedule-time">{{ schedule.scheduledTime }}</span>
                    <span v-if="schedule.presetName" class="preset-tag">{{ schedule.presetName }}</span>
                  </div>
                  <p v-if="schedule.notes" class="schedule-notes">{{ schedule.notes }}</p>
                </div>
                <div class="schedule-actions">
                  <button @click="startScheduledWorkout(schedule)" class="btn btn-primary btn-sm">
                    <Icon name="mdi:play" />
                    開始
                  </button>
                  <button @click="editSchedule(schedule)" class="btn btn-outline btn-sm">
                    <Icon name="mdi:pencil" />
                    編集
                  </button>
                  <button @click="deleteSchedule(schedule)" class="btn btn-danger btn-sm">
                    <Icon name="mdi:delete" />
                    削除
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <div v-if="showModal" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>
            <Icon :name="editMode ? 'mdi:pencil' : 'mdi:plus'" />
            {{ editMode ? '予定編集' : '新しい予定' }}
          </h2>
          <button @click="closeModal" class="modal-close-btn">
            <Icon name="mdi:close" size="24" />
          </button>
        </div>

        <div class="modal-body">
          <div class="form-section">
            <div class="form-group">
              <label for="schedule-date">予定日</label>
              <input 
                id="schedule-date"
                type="date" 
                v-model="formData.scheduledDate"
                class="input-field"
                required
              />
            </div>

            <div class="form-group">
              <label for="schedule-time">時間（任意）</label>
              <input 
                id="schedule-time"
                type="time" 
                v-model="formData.scheduledTime"
                class="input-field"
              />
            </div>

            <div class="form-group">
              <label for="schedule-menu">トレーニングメニュー</label>
              <select 
                id="schedule-menu"
                v-model="formData.menuId"
                class="input-field"
                required
              >
                <option value="">メニューを選択</option>
                <option v-for="menu in menus" :key="menu.menuId" :value="menu.menuId">
                  {{ menu.jpName }}
                </option>
              </select>
            </div>

            <div class="form-group" v-if="availablePresets.length > 0">
              <label for="schedule-preset">マイセット（任意）</label>
              <select 
                id="schedule-preset"
                v-model="formData.presetId"
                class="input-field"
              >
                <option value="">マイセットなし</option>
                <option v-for="preset in availablePresets" :key="preset.presetId" :value="preset.presetId">
                  {{ preset.name }}
                </option>
              </select>
            </div>

            <div class="form-group">
              <label for="schedule-notes">メモ（任意）</label>
              <textarea 
                id="schedule-notes"
                v-model="formData.notes"
                class="input-field"
                rows="3"
                placeholder="例: 胸のトレーニング、重量を5kg増やす"
              ></textarea>
            </div>
          </div>
        </div>

        <div class="modal-actions">
          <button @click="closeModal" class="btn btn-outline">
            キャンセル
          </button>
          <button 
            @click="saveSchedule"
            class="btn btn-primary"
            :disabled="!isFormValid || saving"
          >
            <Icon v-if="saving" name="mdi:loading" class="spin" />
            <Icon v-else :name="editMode ? 'mdi:content-save' : 'mdi:plus'" />
            {{ saving ? '保存中...' : (editMode ? '更新' : '作成') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useTrainingMenus } from '~/composables/useTrainingMenus'
import { useApiClient } from '~/utils/api-client'
import { useGlobalNotifications } from '~/composables/useNotifications'

// ===================================
// Types
// ===================================

interface TrainingSchedule {
  scheduleId: string
  menuId: string
  menuName: string
  presetId?: string
  presetName?: string
  scheduledDate: string
  scheduledTime?: string
  notes?: string
  createdAt: Date
  isCompleted: boolean
}

interface CalendarDay {
  date: string
  day: number
  isOtherMonth: boolean
  isToday: boolean
  hasSchedule: boolean
  schedules: TrainingSchedule[]
}

// ===================================
// Setup
// ===================================

const router = useRouter()
const { training } = useApiClient()
const notifications = useGlobalNotifications()
const { menus } = useTrainingMenus({ autoLoad: true })

// ===================================
// State
// ===================================

const schedules = ref<TrainingSchedule[]>([])
const availablePresets = ref<any[]>([])
const loading = reactive({ isLoading: false, message: '' })
const error = reactive({ hasError: false, message: '' })
const saving = ref(false)

// View state
const viewMode = ref<'calendar' | 'list'>('calendar')
const currentDate = ref(new Date())
const selectedDate = ref('')

// Modal state
const showModal = ref(false)
const editMode = ref(false)
const formData = reactive({
  scheduleId: '',
  scheduledDate: '',
  scheduledTime: '',
  menuId: '',
  presetId: '',
  notes: ''
})

// Filter state
const selectedMenuFilter = ref('')
const startDateFilter = ref('')
const endDateFilter = ref('')

// ===================================
// Computed Properties
// ===================================

const weekdays = ['日', '月', '火', '水', '木', '金', '土']

const currentMonthText = computed(() => {
  return currentDate.value.toLocaleDateString('ja-JP', {
    year: 'numeric',
    month: 'long'
  })
})

const upcomingSchedules = computed(() => {
  const today = new Date().toISOString().split('T')[0]
  return schedules.value.filter(s => s.scheduledDate >= today && !s.isCompleted)
})

const thisWeekSchedules = computed(() => {
  const today = new Date()
  const oneWeekFromToday = new Date(today.getTime() + 7 * 24 * 60 * 60 * 1000)
  const todayStr = today.toISOString().split('T')[0]
  const weekEndStr = oneWeekFromToday.toISOString().split('T')[0]
  
  return schedules.value.filter(s => 
    s.scheduledDate >= todayStr && 
    s.scheduledDate <= weekEndStr &&
    !s.isCompleted
  )
})

const todaySchedules = computed(() => {
  const today = new Date().toISOString().split('T')[0]
  return schedules.value.filter(s => s.scheduledDate === today && !s.isCompleted)
})

const calendarDays = computed(() => {
  const days: CalendarDay[] = []
  const year = currentDate.value.getFullYear()
  const month = currentDate.value.getMonth()
  
  // First day of the month
  const firstDay = new Date(year, month, 1)
  const lastDay = new Date(year, month + 1, 0)
  
  // Start from Sunday of the week containing the first day
  const startDate = new Date(firstDay)
  startDate.setDate(startDate.getDate() - firstDay.getDay())
  
  // Generate 42 days (6 weeks)
  for (let i = 0; i < 42; i++) {
    const date = new Date(startDate)
    date.setDate(startDate.getDate() + i)
    
    const dateStr = date.toISOString().split('T')[0]
    const daySchedules = schedules.value.filter(s => s.scheduledDate === dateStr)
    
    days.push({
      date: dateStr,
      day: date.getDate(),
      isOtherMonth: date.getMonth() !== month,
      isToday: dateStr === new Date().toISOString().split('T')[0],
      hasSchedule: daySchedules.length > 0,
      schedules: daySchedules
    })
  }
  
  return days
})

const filteredSchedules = computed(() => {
  let filtered = schedules.value

  if (selectedMenuFilter.value) {
    filtered = filtered.filter(s => s.menuId === selectedMenuFilter.value)
  }

  if (startDateFilter.value) {
    filtered = filtered.filter(s => s.scheduledDate >= startDateFilter.value)
  }

  if (endDateFilter.value) {
    filtered = filtered.filter(s => s.scheduledDate <= endDateFilter.value)
  }

  return filtered.sort((a, b) => a.scheduledDate.localeCompare(b.scheduledDate))
})

const isFormValid = computed(() => {
  return formData.scheduledDate && formData.menuId
})

// ===================================
// Methods
// ===================================

async function loadSchedules() {
  try {
    setLoading(true, 'トレーニング予定を読み込み中...')
    clearError()

    // Mock data for now - replace with actual API call
    schedules.value = [
      {
        scheduleId: 'sch_1',
        menuId: 'bench_press',
        menuName: 'ベンチプレス',
        presetId: 'preset_1',
        presetName: '胸トレーニング基本',
        scheduledDate: '2024-01-15',
        scheduledTime: '10:00',
        notes: '胸のトレーニング強化週間',
        createdAt: new Date(),
        isCompleted: false
      },
      {
        scheduleId: 'sch_2',
        menuId: 'squat',
        menuName: 'スクワット',
        scheduledDate: '2024-01-16',
        scheduledTime: '14:00',
        notes: '下半身強化',
        createdAt: new Date(),
        isCompleted: false
      }
    ]

  } catch (err: any) {
    console.error('Schedule load error:', err)
    setError('予定の取得中にエラーが発生しました: ' + (err.message || 'Unknown error'))
  } finally {
    setLoading(false)
  }
}

async function loadPresets() {
  try {
    const response = await training.get('/presets')
    if (response.isSuccess && response.data?.presets) {
      availablePresets.value = response.data.presets
    }
  } catch (err) {
    console.warn('Failed to load presets:', err)
  }
}

function toggleView() {
  viewMode.value = viewMode.value === 'calendar' ? 'list' : 'calendar'
}

function previousMonth() {
  const newDate = new Date(currentDate.value)
  newDate.setMonth(newDate.getMonth() - 1)
  currentDate.value = newDate
}

function nextMonth() {
  const newDate = new Date(currentDate.value)
  newDate.setMonth(newDate.getMonth() + 1)
  currentDate.value = newDate
}

function selectDate(date: string) {
  selectedDate.value = date
  formData.scheduledDate = date
  openCreateModal()
}

function openCreateModal() {
  editMode.value = false
  resetForm()
  if (!formData.scheduledDate) {
    formData.scheduledDate = new Date().toISOString().split('T')[0]
  }
  showModal.value = true
}

function editSchedule(schedule: TrainingSchedule) {
  editMode.value = true
  formData.scheduleId = schedule.scheduleId
  formData.scheduledDate = schedule.scheduledDate
  formData.scheduledTime = schedule.scheduledTime || ''
  formData.menuId = schedule.menuId
  formData.presetId = schedule.presetId || ''
  formData.notes = schedule.notes || ''
  showModal.value = true
}

async function saveSchedule() {
  if (!isFormValid.value) return

  try {
    saving.value = true

    // Mock save - replace with actual API call
    const newSchedule: TrainingSchedule = {
      scheduleId: editMode.value ? formData.scheduleId : `sch_${Date.now()}`,
      menuId: formData.menuId,
      menuName: menus.value.find(m => m.menuId === formData.menuId)?.jpName || '',
      presetId: formData.presetId || undefined,
      presetName: availablePresets.value.find(p => p.presetId === formData.presetId)?.name,
      scheduledDate: formData.scheduledDate,
      scheduledTime: formData.scheduledTime || undefined,
      notes: formData.notes || undefined,
      createdAt: new Date(),
      isCompleted: false
    }

    if (editMode.value) {
      const index = schedules.value.findIndex(s => s.scheduleId === formData.scheduleId)
      if (index !== -1) {
        schedules.value[index] = newSchedule
      }
      notifications.success('更新完了', '予定が正常に更新されました')
    } else {
      schedules.value.push(newSchedule)
      notifications.success('作成完了', '予定が正常に作成されました')
    }

    closeModal()

  } catch (err: any) {
    console.error('Schedule save error:', err)
    notifications.error('保存エラー', '予定の保存に失敗しました')
  } finally {
    saving.value = false
  }
}

function deleteSchedule(schedule: TrainingSchedule) {
  if (confirm(`「${schedule.menuName}」の予定を削除しますか？`)) {
    const index = schedules.value.findIndex(s => s.scheduleId === schedule.scheduleId)
    if (index !== -1) {
      schedules.value.splice(index, 1)
      notifications.success('削除完了', '予定を削除しました')
    }
  }
}

function startScheduledWorkout(schedule: TrainingSchedule) {
  const query: any = {}
  if (schedule.presetId) {
    query.presetId = schedule.presetId
  }
  
  router.push({
    path: `/training/session/${schedule.menuId}`,
    query
  })
}

function applyFilters() {
  // Filters are reactive, so they automatically apply
}

function resetFilters() {
  selectedMenuFilter.value = ''
  startDateFilter.value = ''
  endDateFilter.value = ''
}

function closeModal() {
  showModal.value = false
  editMode.value = false
  resetForm()
}

function resetForm() {
  formData.scheduleId = ''
  formData.scheduledDate = ''
  formData.scheduledTime = ''
  formData.menuId = ''
  formData.presetId = ''
  formData.notes = ''
}

function formatDate(dateString: string): string {
  const date = new Date(dateString)
  return date.toLocaleDateString('ja-JP', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    weekday: 'short'
  })
}

function setLoading(isLoading: boolean, message: string = '') {
  loading.isLoading = isLoading
  loading.message = message
}

function setError(message: string) {
  error.hasError = true
  error.message = message
}

function clearError() {
  error.hasError = false
  error.message = ''
}

// ===================================
// Export Functionality
// ===================================

async function exportSchedules() {
  try {
    const exportData = {
      metadata: {
        exportType: 'training_schedules',
        exportedAt: new Date().toISOString(),
        exportedBy: 'Message Training App',
        totalSchedules: schedules.value.length,
        filters: {
          selectedMenuFilter: selectedMenuFilter.value,
          startDateFilter: startDateFilter.value,
          endDateFilter: endDateFilter.value
        }
      },
      menus: menus.value.map(menu => ({
        menuId: menu.menuId,
        jpName: menu.jpName,
        enName: menu.enName
      })),
      presets: availablePresets.value.map(preset => ({
        presetId: preset.presetId,
        name: preset.name,
        menuId: preset.menuId
      })),
      schedules: schedules.value.map(schedule => ({
        ...schedule,
        dayOfWeek: new Date(schedule.scheduledDate).toLocaleDateString('ja-JP', { weekday: 'long' }),
        isPast: schedule.scheduledDate < new Date().toISOString().split('T')[0],
        isFuture: schedule.scheduledDate > new Date().toISOString().split('T')[0],
        isToday: schedule.scheduledDate === new Date().toISOString().split('T')[0]
      })),
      summary: {
        totalSchedules: schedules.value.length,
        completedSchedules: schedules.value.filter(s => s.isCompleted).length,
        upcomingSchedules: upcomingSchedules.value.length,
        thisWeekSchedules: thisWeekSchedules.value.length,
        todaySchedules: todaySchedules.value.length,
        schedulesByMenu: getSchedulesByMenu(),
        schedulesByMonth: getSchedulesByMonth(),
        averageSchedulesPerWeek: calculateAverageSchedulesPerWeek()
      }
    }

    const dataStr = JSON.stringify(exportData, null, 2)
    const dataUri = 'data:application/json;charset=utf-8,'+ encodeURIComponent(dataStr)
    
    const exportFileDefaultName = `training_schedules_${new Date().toISOString().split('T')[0]}.json`
    
    const linkElement = document.createElement('a')
    linkElement.setAttribute('href', dataUri)
    linkElement.setAttribute('download', exportFileDefaultName)
    linkElement.click()
    
    notifications.success('エクスポート完了', 'トレーニング予定をダウンロードしました')
    
  } catch (err) {
    console.error('Export error:', err)
    notifications.error('エクスポートエラー', 'データのエクスポートに失敗しました')
  }
}

function getSchedulesByMenu(): { menuId: string, menuName: string, count: number }[] {
  const menuCounts = schedules.value.reduce((acc, schedule) => {
    acc[schedule.menuId] = (acc[schedule.menuId] || 0) + 1
    return acc
  }, {} as Record<string, number>)
  
  return Object.entries(menuCounts)
    .map(([menuId, count]) => ({
      menuId,
      menuName: menus.value.find(m => m.menuId === menuId)?.jpName || menuId,
      count
    }))
    .sort((a, b) => b.count - a.count)
}

function getSchedulesByMonth(): { month: string, count: number }[] {
  const monthCounts = schedules.value.reduce((acc, schedule) => {
    const month = new Date(schedule.scheduledDate).toLocaleDateString('ja-JP', { year: 'numeric', month: 'long' })
    acc[month] = (acc[month] || 0) + 1
    return acc
  }, {} as Record<string, number>)
  
  return Object.entries(monthCounts)
    .map(([month, count]) => ({ month, count }))
    .sort((a, b) => a.month.localeCompare(b.month))
}

function calculateAverageSchedulesPerWeek(): number {
  if (schedules.value.length === 0) return 0
  
  const dates = schedules.value.map(s => new Date(s.scheduledDate))
  const earliestDate = new Date(Math.min(...dates.map(d => d.getTime())))
  const latestDate = new Date(Math.max(...dates.map(d => d.getTime())))
  
  const daysDiff = Math.ceil((latestDate.getTime() - earliestDate.getTime()) / (1000 * 60 * 60 * 24))
  const weeksDiff = Math.max(daysDiff / 7, 1) // At least 1 week to avoid division by zero
  
  return Math.round((schedules.value.length / weeksDiff) * 10) / 10
}

// ===================================
// Lifecycle
// ===================================

onMounted(async () => {
  await Promise.all([
    loadSchedules(),
    loadPresets()
  ])
})

// ===================================
// Meta
// ===================================

useHead({
  title: 'トレーニング予定 - Message',
  meta: [
    { name: 'description', content: 'トレーニングの予定を計画・管理' }
  ]
})
</script>

<style scoped>
.training-schedule {
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

/* Loading and Error States */
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

@keyframes pulse {
  0%, 100% { opacity: 0.5; }
  50% { opacity: 1; }
}

.error-icon {
  font-size: 4rem;
  margin-bottom: 20px;
  color: var(--danger);
}

/* Calendar View */
.calendar-view {
  background: var(--card-bg);
  border-radius: 16px;
  padding: 25px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.calendar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
}

.calendar-header h2 {
  margin: 0;
  color: var(--text-primary);
  font-size: 1.5rem;
}

.nav-btn {
  background: var(--bg-secondary);
  border: 1px solid var(--border);
  border-radius: 8px;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s;
}

.nav-btn:hover {
  background: var(--primary);
  color: white;
  border-color: var(--primary);
}

.calendar-grid {
  width: 100%;
}

.calendar-weekdays {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 1px;
  margin-bottom: 10px;
}

.weekday {
  text-align: center;
  padding: 10px;
  font-weight: 600;
  color: var(--text-secondary);
  font-size: 0.9rem;
}

.calendar-days {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 1px;
}

.calendar-day {
  min-height: 100px;
  padding: 8px;
  background: var(--bg-secondary);
  border: 1px solid var(--border);
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  flex-direction: column;
}

.calendar-day:hover {
  background: var(--bg-tertiary);
  transform: scale(1.02);
}

.calendar-day.other-month {
  opacity: 0.4;
}

.calendar-day.today {
  background: var(--primary-light);
  border-color: var(--primary);
}

.calendar-day.has-schedule {
  background: linear-gradient(135deg, #E6F2FF, #F0F8FF);
  border-color: var(--primary);
}

.day-number {
  font-weight: 600;
  color: var(--text-primary);
}

.schedule-indicators {
  margin-top: 5px;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.schedule-dot {
  width: 100%;
  height: 4px;
  background: var(--primary);
  border-radius: 2px;
}

.more-indicator {
  font-size: 0.7rem;
  color: var(--text-secondary);
  text-align: center;
}

/* List View */
.list-view {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.filter-section {
  background: var(--card-bg);
  padding: 20px;
  border-radius: 12px;
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

.schedule-list {
  display: flex;
  flex-direction: column;
  gap: 15px;
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
  margin-bottom: 10px;
  color: var(--text-primary);
}

.schedule-item {
  background: var(--card-bg);
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  transition: all 0.2s;
}

.schedule-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.15);
}

.schedule-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  flex-wrap: wrap;
  gap: 15px;
}

.schedule-info h3 {
  margin: 0 0 8px 0;
  color: var(--text-primary);
  font-size: 1.2rem;
}

.schedule-meta {
  display: flex;
  gap: 15px;
  flex-wrap: wrap;
  margin-bottom: 8px;
  font-size: 0.9rem;
}

.schedule-date {
  color: var(--primary);
  font-weight: 600;
}

.schedule-time {
  color: var(--text-secondary);
}

.preset-tag {
  background: var(--bg-tertiary);
  color: var(--text-primary);
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 0.8rem;
}

.schedule-notes {
  margin: 0;
  color: var(--text-secondary);
  font-size: 0.9rem;
}

.schedule-actions {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
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

.btn-danger {
  background: var(--danger);
  color: white;
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
  max-width: 500px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
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

.modal-body {
  padding: 0 25px 25px 25px;
}

.form-section {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-weight: 600;
  color: var(--text-primary);
}

.input-field {
  padding: 12px;
  border: 1px solid var(--border);
  border-radius: 6px;
  font-size: 1rem;
  transition: border-color 0.2s;
}

.input-field:focus {
  outline: none;
  border-color: var(--primary);
}

.modal-actions {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
  padding: 15px 25px 25px 25px;
  border-top: 1px solid var(--border);
}

/* Animations */
.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* CSS Variables */
:root {
  --card-bg: #ffffff;
  --bg-secondary: #f5f5f5;
  --bg-tertiary: #e0e0e0;
  --text-primary: #212121;
  --text-secondary: #757575;
  --border: #e0e0e0;
  --primary: #2196f3;
  --primary-light: #e3f2fd;
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
  --primary-light: #1a237e;
  --danger: #ef5350;
}

/* Responsive */
@media (max-width: 768px) {
  .training-schedule {
    padding: 15px;
  }
  
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
  
  .schedule-header {
    flex-direction: column;
    align-items: stretch;
  }
  
  .schedule-actions {
    justify-content: center;
  }
  
  .calendar-day {
    min-height: 80px;
    padding: 4px;
  }
  
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
  
  .modal-actions {
    flex-direction: column;
    padding: 15px 20px 20px 20px;
  }
}
</style>