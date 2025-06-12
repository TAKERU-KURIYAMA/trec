<template>
  <div class="training-calendar">
    <div class="calendar-header">
      <button @click="previousMonth" class="nav-btn">‹</button>
      <h3 class="month-title">{{ currentMonthYear }}</h3>
      <button @click="nextMonth" class="nav-btn">›</button>
    </div>

    <div class="calendar-grid">
      <div class="weekday-header">
        <div v-for="day in weekdays" :key="day" class="weekday">{{ day }}</div>
      </div>
      
      <div class="calendar-body">
        <div
          v-for="date in calendarDays"
          :key="date.dateString"
          class="calendar-day"
          :class="{
            'other-month': !date.isCurrentMonth,
            'today': date.isToday,
            'has-workout': date.workouts > 0,
            'weekend': date.isWeekend
          }"
          @click="selectDate(date)"
        >
          <div class="day-number">{{ date.day }}</div>
          <div v-if="date.workouts > 0" class="workout-indicator">
            <div class="workout-dots">
              <div
                v-for="n in Math.min(date.workouts, 3)"
                :key="n"
                class="workout-dot"
              ></div>
              <span v-if="date.workouts > 3" class="workout-count">+{{ date.workouts - 3 }}</span>
            </div>
          </div>
          <div v-if="date.isToday" class="today-indicator">今日</div>
        </div>
      </div>
    </div>

    <div class="calendar-legend">
      <div class="legend-item">
        <div class="legend-dot workout-dot"></div>
        <span>ワークアウト</span>
      </div>
      <div class="legend-item">
        <div class="legend-square today-square"></div>
        <span>今日</span>
      </div>
      <div class="legend-item">
        <div class="legend-square streak-square"></div>
        <span>連続記録</span>
      </div>
    </div>

    <div class="calendar-stats">
      <div class="stat-item">
        <span class="stat-number">{{ monthlyWorkouts }}</span>
        <span class="stat-label">今月</span>
      </div>
      <div class="stat-item">
        <span class="stat-number">{{ currentStreak }}</span>
        <span class="stat-label">連続日数</span>
      </div>
      <div class="stat-item">
        <span class="stat-number">{{ workoutDaysThisMonth }}</span>
        <span class="stat-label">トレーニング日</span>
      </div>
    </div>

    <!-- Day Detail Modal -->
    <div v-if="selectedDate" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <button class="close-btn" @click="closeModal">×</button>
        <div class="modal-header">
          <h2>{{ formatSelectedDate(selectedDate.dateString) }}</h2>
          <div class="date-info">
            {{ getDayOfWeek(selectedDate.dateString) }}
          </div>
        </div>
        <div class="modal-body">
          <div v-if="selectedDate.workouts > 0" class="workout-summary">
            <h3>🏋️ ワークアウト記録</h3>
            <div class="workout-details">
              <p>{{ selectedDate.workouts }}回のワークアウトを実施</p>
              <!-- Here you would show actual workout details -->
              <div class="mock-workouts">
                <div v-for="i in selectedDate.workouts" :key="i" class="workout-item">
                  <span class="workout-time">{{ getMockWorkoutTime(i) }}</span>
                  <span class="workout-name">{{ getMockWorkoutName(i) }}</span>
                </div>
              </div>
            </div>
          </div>
          <div v-else class="no-workout">
            <p>この日はお休みでした</p>
            <button @click="planWorkout(selectedDate)" class="plan-btn">
              ワークアウトを計画する
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

const props = defineProps({
  trainingData: {
    type: Array,
    default: () => []
  }
})

const emit = defineEmits(['plan-workout', 'view-workout'])

const currentDate = ref(new Date())
const selectedDate = ref(null)

const weekdays = ['日', '月', '火', '水', '木', '金', '土']

const currentMonthYear = computed(() => {
  return currentDate.value.toLocaleDateString('ja-JP', {
    year: 'numeric',
    month: 'long'
  })
})

const calendarDays = computed(() => {
  const year = currentDate.value.getFullYear()
  const month = currentDate.value.getMonth()
  
  // Get first day of month and last day of month
  const firstDay = new Date(year, month, 1)
  const lastDay = new Date(year, month + 1, 0)
  
  // Get first day to show (might be from previous month)
  const startDate = new Date(firstDay)
  startDate.setDate(startDate.getDate() - firstDay.getDay())
  
  // Get last day to show (might be from next month)
  const endDate = new Date(lastDay)
  endDate.setDate(endDate.getDate() + (6 - lastDay.getDay()))
  
  const days = []
  const current = new Date(startDate)
  
  while (current <= endDate) {
    const dateString = current.toISOString().split('T')[0]
    const workoutData = props.trainingData.find(d => d.date === dateString)
    const today = new Date()
    
    days.push({
      day: current.getDate(),
      dateString,
      isCurrentMonth: current.getMonth() === month,
      isToday: current.toDateString() === today.toDateString(),
      isWeekend: current.getDay() === 0 || current.getDay() === 6,
      workouts: workoutData ? workoutData.workouts : 0
    })
    
    current.setDate(current.getDate() + 1)
  }
  
  return days
})

const monthlyWorkouts = computed(() => {
  return calendarDays.value
    .filter(day => day.isCurrentMonth)
    .reduce((sum, day) => sum + day.workouts, 0)
})

const workoutDaysThisMonth = computed(() => {
  return calendarDays.value
    .filter(day => day.isCurrentMonth && day.workouts > 0)
    .length
})

const currentStreak = computed(() => {
  // Calculate current streak
  const today = new Date()
  let streak = 0
  let current = new Date(today)
  
  while (true) {
    const dateString = current.toISOString().split('T')[0]
    const workoutData = props.trainingData.find(d => d.date === dateString)
    
    if (workoutData && workoutData.workouts > 0) {
      streak++
      current.setDate(current.getDate() - 1)
    } else {
      break
    }
  }
  
  return streak
})

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

function selectDate(date) {
  selectedDate.value = date
}

function closeModal() {
  selectedDate.value = null
}

function formatSelectedDate(dateString) {
  const date = new Date(dateString)
  return date.toLocaleDateString('ja-JP', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

function getDayOfWeek(dateString) {
  const date = new Date(dateString)
  return date.toLocaleDateString('ja-JP', { weekday: 'long' })
}

function getMockWorkoutTime(index) {
  const times = ['09:00', '14:30', '19:00']
  return times[index - 1] || '10:00'
}

function getMockWorkoutName(index) {
  const names = ['上半身トレーニング', '下半身トレーニング', '有酸素運動']
  return names[index - 1] || 'トレーニング'
}

function planWorkout(date) {
  emit('plan-workout', date)
  closeModal()
}
</script>

<style scoped>
.training-calendar {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.calendar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding: 0 10px;
}

.nav-btn {
  background: #3498db;
  color: white;
  border: none;
  border-radius: 50%;
  width: 40px;
  height: 40px;
  cursor: pointer;
  font-size: 1.2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background-color 0.2s;
}

.nav-btn:hover {
  background: #2980b9;
}

.month-title {
  margin: 0;
  font-size: 1.3rem;
  color: #2c3e50;
}

.calendar-grid {
  flex: 1;
  display: flex;
  flex-direction: column;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
  margin-bottom: 15px;
}

.weekday-header {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  background: #f8f9fa;
}

.weekday {
  padding: 10px;
  text-align: center;
  font-weight: bold;
  color: #666;
  border-right: 1px solid #e0e0e0;
}

.weekday:last-child {
  border-right: none;
}

.calendar-body {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  flex: 1;
}

.calendar-day {
  aspect-ratio: 1;
  border-right: 1px solid #e0e0e0;
  border-bottom: 1px solid #e0e0e0;
  padding: 8px;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  background: white;
}

.calendar-day:nth-child(7n) {
  border-right: none;
}

.calendar-day.other-month {
  background: #f8f9fa;
  color: #ccc;
}

.calendar-day.today {
  background: linear-gradient(135deg, #3498db, #2980b9);
  color: white;
}

.calendar-day.has-workout {
  background: #e8f5e8;
  border-color: #27ae60;
}

.calendar-day.today.has-workout {
  background: linear-gradient(135deg, #27ae60, #229954);
}

.calendar-day.weekend {
  background: #fef9e7;
}

.calendar-day:hover {
  transform: scale(1.05);
  box-shadow: 0 2px 8px rgba(0,0,0,0.15);
  z-index: 1;
}

.day-number {
  font-weight: bold;
  margin-bottom: 4px;
}

.workout-indicator {
  margin-top: auto;
}

.workout-dots {
  display: flex;
  gap: 2px;
  align-items: center;
}

.workout-dot {
  width: 6px;
  height: 6px;
  background: #27ae60;
  border-radius: 50%;
}

.calendar-day.today .workout-dot {
  background: white;
}

.workout-count {
  font-size: 0.6rem;
  margin-left: 2px;
  font-weight: bold;
}

.today-indicator {
  position: absolute;
  bottom: 2px;
  left: 50%;
  transform: translateX(-50%);
  font-size: 0.6rem;
  background: rgba(255, 255, 255, 0.2);
  padding: 1px 4px;
  border-radius: 8px;
}

.calendar-legend {
  display: flex;
  justify-content: center;
  gap: 20px;
  margin-bottom: 15px;
  flex-wrap: wrap;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 0.8rem;
  color: #666;
}

.legend-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.legend-square {
  width: 12px;
  height: 12px;
  border-radius: 2px;
}

.today-square {
  background: linear-gradient(135deg, #3498db, #2980b9);
}

.streak-square {
  background: linear-gradient(135deg, #27ae60, #229954);
}

.calendar-stats {
  display: flex;
  justify-content: space-around;
  padding: 15px;
  background: #f8f9fa;
  border-radius: 8px;
}

.stat-item {
  text-align: center;
}

.stat-number {
  display: block;
  font-size: 1.5rem;
  font-weight: bold;
  color: #2c3e50;
}

.stat-label {
  font-size: 0.8rem;
  color: #666;
}

/* Modal styles */
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
}

.modal-content {
  background: white;
  border-radius: 12px;
  padding: 20px;
  max-width: 400px;
  width: 90%;
  max-height: 80vh;
  overflow-y: auto;
  position: relative;
}

.close-btn {
  position: absolute;
  top: 10px;
  right: 15px;
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #666;
}

.modal-header {
  text-align: center;
  margin-bottom: 20px;
}

.modal-header h2 {
  margin: 0 0 5px 0;
  color: #2c3e50;
}

.date-info {
  color: #666;
  font-size: 0.9rem;
}

.workout-summary h3 {
  margin: 0 0 15px 0;
  color: #27ae60;
}

.mock-workouts {
  margin-top: 15px;
}

.workout-item {
  display: flex;
  justify-content: space-between;
  padding: 8px 0;
  border-bottom: 1px solid #e0e0e0;
}

.workout-time {
  color: #666;
  font-size: 0.9rem;
}

.no-workout {
  text-align: center;
  padding: 20px;
}

.plan-btn {
  background: linear-gradient(135deg, #3498db, #2980b9);
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 6px;
  cursor: pointer;
  margin-top: 15px;
}

@media (max-width: 768px) {
  .calendar-day {
    font-size: 0.8rem;
    padding: 4px;
  }
  
  .workout-dots {
    gap: 1px;
  }
  
  .workout-dot {
    width: 4px;
    height: 4px;
  }
  
  .calendar-legend {
    gap: 10px;
  }
  
  .calendar-stats {
    flex-direction: column;
    gap: 10px;
  }
}
</style>