<template>
  <div class="training-timer">
    <!-- Header -->
    <div class="page-header">
      <div class="header-content">
        <h1>トレーニングタイマー</h1>
        <p>セット間の休憩時間とワークアウト時間を管理</p>
      </div>
      <div class="header-actions">
        <button @click="resetAll" class="btn btn-outline">
          <Icon name="mdi:refresh" />
          リセット
        </button>
      </div>
    </div>

    <!-- Timer Modes -->
    <div class="timer-modes">
      <div class="mode-tabs">
        <button 
          @click="activeMode = 'rest'"
          :class="['mode-tab', { active: activeMode === 'rest' }]"
        >
          <Icon name="mdi:timer" />
          休憩タイマー
        </button>
        <button 
          @click="activeMode = 'workout'"
          :class="['mode-tab', { active: activeMode === 'workout' }]"
        >
          <Icon name="mdi:clock" />
          ワークアウト時間
        </button>
        <button 
          @click="activeMode = 'intervals'"
          :class="['mode-tab', { active: activeMode === 'intervals' }]"
        >
          <Icon name="mdi:repeat" />
          インターバル
        </button>
      </div>
    </div>

    <!-- Rest Timer Mode -->
    <div v-if="activeMode === 'rest'" class="timer-section">
      <div class="timer-card">
        <div class="card-header">
          <h2>⏱️ セット間休憩タイマー</h2>
          <p>目標時間を設定して、適切な休憩を取りましょう</p>
        </div>

        <!-- Quick Preset Buttons -->
        <div class="preset-times">
          <h3>クイック設定</h3>
          <div class="preset-grid">
            <button 
              v-for="preset in restPresets" 
              :key="preset.seconds"
              @click="setRestTime(preset.seconds)"
              class="preset-btn"
              :class="{ active: restTimer.target === preset.seconds }"
            >
              <span class="preset-time">{{ formatTime(preset.seconds) }}</span>
              <span class="preset-label">{{ preset.label }}</span>
            </button>
          </div>
        </div>

        <!-- Custom Time Input -->
        <div class="custom-time">
          <h3>カスタム時間</h3>
          <div class="time-inputs">
            <div class="time-input-group">
              <label>分</label>
              <input 
                v-model.number="customMinutes"
                type="number"
                min="0"
                max="10"
                class="time-input"
              />
            </div>
            <div class="time-input-group">
              <label>秒</label>
              <input 
                v-model.number="customSeconds"
                type="number"
                min="0"
                max="59"
                step="15"
                class="time-input"
              />
            </div>
            <button @click="setCustomRestTime" class="btn btn-secondary">
              設定
            </button>
          </div>
        </div>

        <!-- Rest Timer Display -->
        <div class="main-timer" :class="{ 
          running: restTimer.isRunning,
          warning: restTimer.current > restTimer.target * 0.8 && restTimer.current < restTimer.target,
          complete: restTimer.current >= restTimer.target
        }">
          <div class="timer-display">
            <div class="time-value">{{ formatTime(restTimer.current) }}</div>
            <div class="time-label">{{ getRestTimerLabel() }}</div>
          </div>
          
          <div class="timer-progress">
            <div 
              class="progress-bar"
              :style="{ width: getRestProgress() + '%' }"
            ></div>
          </div>
          
          <div class="timer-controls">
            <button 
              v-if="!restTimer.isRunning"
              @click="startRestTimer"
              class="btn btn-primary btn-large"
              :disabled="restTimer.target === 0"
            >
              <Icon name="mdi:play" />
              開始
            </button>
            <button 
              v-else
              @click="pauseRestTimer"
              class="btn btn-warning btn-large"
            >
              <Icon name="mdi:pause" />
              一時停止
            </button>
            <button 
              @click="resetRestTimer"
              class="btn btn-outline btn-large"
            >
              <Icon name="mdi:stop" />
              リセット
            </button>
          </div>
        </div>

        <!-- Rest History -->
        <div v-if="restHistory.length > 0" class="rest-history">
          <h3>休憩履歴</h3>
          <div class="history-list">
            <div 
              v-for="(rest, index) in restHistory.slice(-5)" 
              :key="index"
              class="history-item"
            >
              <span class="history-time">{{ formatTime(rest.duration) }}</span>
              <span class="history-timestamp">{{ formatTimestamp(rest.timestamp) }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Workout Timer Mode -->
    <div v-if="activeMode === 'workout'" class="timer-section">
      <div class="timer-card">
        <div class="card-header">
          <h2>🏋️‍♂️ ワークアウトタイマー</h2>
          <p>トレーニング全体の時間を計測します</p>
        </div>

        <!-- Workout Timer Display -->
        <div class="main-timer" :class="{ running: workoutTimer.isRunning }">
          <div class="timer-display">
            <div class="time-value">{{ formatTime(workoutTimer.elapsed) }}</div>
            <div class="time-label">ワークアウト時間</div>
          </div>
          
          <div class="timer-stats">
            <div class="stat-item">
              <span class="stat-label">セット数</span>
              <span class="stat-value">{{ workoutStats.sets }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">休憩回数</span>
              <span class="stat-value">{{ workoutStats.rests }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">総休憩時間</span>
              <span class="stat-value">{{ formatTime(workoutStats.totalRestTime) }}</span>
            </div>
          </div>
          
          <div class="timer-controls">
            <button 
              v-if="!workoutTimer.isRunning"
              @click="startWorkoutTimer"
              class="btn btn-primary btn-large"
            >
              <Icon name="mdi:play" />
              {{ workoutTimer.elapsed > 0 ? '再開' : '開始' }}
            </button>
            <button 
              v-else
              @click="pauseWorkoutTimer"
              class="btn btn-warning btn-large"
            >
              <Icon name="mdi:pause" />
              一時停止
            </button>
            <button 
              @click="resetWorkoutTimer"
              class="btn btn-outline btn-large"
            >
              <Icon name="mdi:stop" />
              終了
            </button>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="quick-actions">
          <button @click="recordSet" class="btn btn-success">
            <Icon name="mdi:plus" />
            セット完了
          </button>
          <button @click="startQuickRest" class="btn btn-secondary">
            <Icon name="mdi:timer" />
            休憩開始
          </button>
        </div>
      </div>
    </div>

    <!-- Interval Timer Mode -->
    <div v-if="activeMode === 'intervals'" class="timer-section">
      <div class="timer-card">
        <div class="card-header">
          <h2>🔄 インターバルタイマー</h2>
          <p>HIIT・タバタ式トレーニングに最適</p>
        </div>

        <!-- Interval Settings -->
        <div class="interval-settings">
          <h3>インターバル設定</h3>
          <div class="settings-grid">
            <div class="setting-group">
              <label>ワーク時間</label>
              <input 
                v-model.number="intervalSettings.workTime"
                type="number"
                min="5"
                max="300"
                step="5"
                class="setting-input"
              />
              <span>秒</span>
            </div>
            <div class="setting-group">
              <label>休憩時間</label>
              <input 
                v-model.number="intervalSettings.restTime"
                type="number"
                min="5"
                max="180"
                step="5"
                class="setting-input"
              />
              <span>秒</span>
            </div>
            <div class="setting-group">
              <label>ラウンド数</label>
              <input 
                v-model.number="intervalSettings.rounds"
                type="number"
                min="1"
                max="50"
                class="setting-input"
              />
              <span>回</span>
            </div>
          </div>

          <!-- Preset Intervals -->
          <div class="interval-presets">
            <button 
              v-for="preset in intervalPresets" 
              :key="preset.name"
              @click="setIntervalPreset(preset)"
              class="interval-preset-btn"
            >
              <div class="preset-name">{{ preset.name }}</div>
              <div class="preset-details">{{ preset.work }}s / {{ preset.rest }}s × {{ preset.rounds }}</div>
            </button>
          </div>
        </div>

        <!-- Interval Timer Display -->
        <div class="interval-timer" :class="{ 
          running: intervalTimer.isRunning,
          work: intervalTimer.phase === 'work',
          rest: intervalTimer.phase === 'rest',
          prepare: intervalTimer.phase === 'prepare'
        }">
          <div class="interval-info">
            <div class="current-round">
              ラウンド {{ intervalTimer.currentRound }} / {{ intervalSettings.rounds }}
            </div>
            <div class="phase-indicator">
              {{ getPhaseText() }}
            </div>
          </div>
          
          <div class="timer-display">
            <div class="time-value">{{ formatTime(intervalTimer.timeLeft) }}</div>
          </div>
          
          <div class="timer-progress">
            <div 
              class="progress-bar"
              :style="{ width: getIntervalProgress() + '%' }"
            ></div>
          </div>
          
          <div class="timer-controls">
            <button 
              v-if="!intervalTimer.isRunning"
              @click="startIntervalTimer"
              class="btn btn-primary btn-large"
            >
              <Icon name="mdi:play" />
              開始
            </button>
            <button 
              v-else
              @click="pauseIntervalTimer"
              class="btn btn-warning btn-large"
            >
              <Icon name="mdi:pause" />
              一時停止
            </button>
            <button 
              @click="resetIntervalTimer"
              class="btn btn-outline btn-large"
            >
              <Icon name="mdi:stop" />
              リセット
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Sound Settings -->
    <div class="sound-settings">
      <div class="setting-item">
        <label class="setting-label">
          <input 
            type="checkbox" 
            v-model="soundEnabled"
            class="setting-checkbox"
          />
          音声通知を有効にする
        </label>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted } from 'vue'

// ===================================
// Types & Interfaces
// ===================================

interface RestPreset {
  seconds: number
  label: string
}

interface IntervalPreset {
  name: string
  work: number
  rest: number
  rounds: number
}

interface RestHistoryItem {
  duration: number
  timestamp: Date
}

// ===================================
// State
// ===================================

const activeMode = ref<'rest' | 'workout' | 'intervals'>('rest')
const soundEnabled = ref(true)

// Rest Timer
const restTimer = reactive({
  target: 90,
  current: 0,
  isRunning: false,
  interval: null as number | null
})

const customMinutes = ref(1)
const customSeconds = ref(30)
const restHistory = ref<RestHistoryItem[]>([])

// Workout Timer
const workoutTimer = reactive({
  elapsed: 0,
  isRunning: false,
  startTime: null as Date | null,
  interval: null as number | null
})

const workoutStats = reactive({
  sets: 0,
  rests: 0,
  totalRestTime: 0
})

// Interval Timer
const intervalSettings = reactive({
  workTime: 20,
  restTime: 10,
  rounds: 8
})

const intervalTimer = reactive({
  isRunning: false,
  currentRound: 1,
  phase: 'prepare' as 'prepare' | 'work' | 'rest' | 'complete',
  timeLeft: 5,
  interval: null as number | null
})

// ===================================
// Presets
// ===================================

const restPresets: RestPreset[] = [
  { seconds: 60, label: '軽い運動' },
  { seconds: 90, label: '筋肥大' },
  { seconds: 120, label: '筋力向上' },
  { seconds: 180, label: '最大筋力' },
  { seconds: 300, label: '長時間休憩' }
]

const intervalPresets: IntervalPreset[] = [
  { name: 'タバタ', work: 20, rest: 10, rounds: 8 },
  { name: 'EMOM', work: 45, rest: 15, rounds: 10 },
  { name: 'HIIT Basic', work: 30, rest: 30, rounds: 6 },
  { name: 'スプリント', work: 15, rest: 45, rounds: 8 }
]

// ===================================
// Rest Timer Methods
// ===================================

function setRestTime(seconds: number) {
  restTimer.target = seconds
  resetRestTimer()
}

function setCustomRestTime() {
  const totalSeconds = (customMinutes.value || 0) * 60 + (customSeconds.value || 0)
  if (totalSeconds > 0) {
    setRestTime(totalSeconds)
  }
}

function startRestTimer() {
  if (restTimer.target === 0) return
  
  restTimer.isRunning = true
  restTimer.current = 0
  
  restTimer.interval = setInterval(() => {
    restTimer.current++
    
    if (restTimer.current >= restTimer.target) {
      playSound('complete')
      pauseRestTimer()
      
      // Add to history
      restHistory.value.push({
        duration: restTimer.current,
        timestamp: new Date()
      })
      
      workoutStats.rests++
      workoutStats.totalRestTime += restTimer.current
    } else if (restTimer.current === restTimer.target - 10) {
      playSound('warning')
    }
  }, 1000)
}

function pauseRestTimer() {
  restTimer.isRunning = false
  if (restTimer.interval) {
    clearInterval(restTimer.interval)
    restTimer.interval = null
  }
}

function resetRestTimer() {
  pauseRestTimer()
  restTimer.current = 0
}

function getRestTimerLabel(): string {
  if (restTimer.current >= restTimer.target) {
    return '休憩完了！'
  } else if (restTimer.current > restTimer.target * 0.8) {
    return '準備してください'
  } else {
    return '休憩中'
  }
}

function getRestProgress(): number {
  if (restTimer.target === 0) return 0
  return Math.min((restTimer.current / restTimer.target) * 100, 100)
}

// ===================================
// Workout Timer Methods
// ===================================

function startWorkoutTimer() {
  workoutTimer.isRunning = true
  workoutTimer.startTime = new Date(Date.now() - workoutTimer.elapsed * 1000)
  
  workoutTimer.interval = setInterval(() => {
    if (workoutTimer.startTime) {
      workoutTimer.elapsed = Math.floor((Date.now() - workoutTimer.startTime.getTime()) / 1000)
    }
  }, 1000)
}

function pauseWorkoutTimer() {
  workoutTimer.isRunning = false
  if (workoutTimer.interval) {
    clearInterval(workoutTimer.interval)
    workoutTimer.interval = null
  }
}

function resetWorkoutTimer() {
  pauseWorkoutTimer()
  workoutTimer.elapsed = 0
  workoutTimer.startTime = null
  workoutStats.sets = 0
  workoutStats.rests = 0
  workoutStats.totalRestTime = 0
}

function recordSet() {
  workoutStats.sets++
  playSound('success')
}

function startQuickRest() {
  activeMode.value = 'rest'
  setRestTime(90)
  startRestTimer()
}

// ===================================
// Interval Timer Methods
// ===================================

function setIntervalPreset(preset: IntervalPreset) {
  intervalSettings.workTime = preset.work
  intervalSettings.restTime = preset.rest
  intervalSettings.rounds = preset.rounds
  resetIntervalTimer()
}

function startIntervalTimer() {
  intervalTimer.isRunning = true
  intervalTimer.currentRound = 1
  intervalTimer.phase = 'prepare'
  intervalTimer.timeLeft = 5
  
  intervalTimer.interval = setInterval(() => {
    intervalTimer.timeLeft--
    
    if (intervalTimer.timeLeft <= 0) {
      advanceInterval()
    } else if (intervalTimer.timeLeft <= 3) {
      playSound('countdown')
    }
  }, 1000)
}

function advanceInterval() {
  switch (intervalTimer.phase) {
    case 'prepare':
      intervalTimer.phase = 'work'
      intervalTimer.timeLeft = intervalSettings.workTime
      playSound('start')
      break
      
    case 'work':
      if (intervalTimer.currentRound < intervalSettings.rounds) {
        intervalTimer.phase = 'rest'
        intervalTimer.timeLeft = intervalSettings.restTime
        playSound('rest')
      } else {
        intervalTimer.phase = 'complete'
        intervalTimer.timeLeft = 0
        pauseIntervalTimer()
        playSound('complete')
      }
      break
      
    case 'rest':
      intervalTimer.currentRound++
      intervalTimer.phase = 'work'
      intervalTimer.timeLeft = intervalSettings.workTime
      playSound('start')
      break
  }
}

function pauseIntervalTimer() {
  intervalTimer.isRunning = false
  if (intervalTimer.interval) {
    clearInterval(intervalTimer.interval)
    intervalTimer.interval = null
  }
}

function resetIntervalTimer() {
  pauseIntervalTimer()
  intervalTimer.currentRound = 1
  intervalTimer.phase = 'prepare'
  intervalTimer.timeLeft = 5
}

function getPhaseText(): string {
  switch (intervalTimer.phase) {
    case 'prepare': return '準備'
    case 'work': return 'ワーク'
    case 'rest': return '休憩'
    case 'complete': return '完了'
    default: return ''
  }
}

function getIntervalProgress(): number {
  let totalTime = 0
  
  switch (intervalTimer.phase) {
    case 'prepare':
      totalTime = 5
      break
    case 'work':
      totalTime = intervalSettings.workTime
      break
    case 'rest':
      totalTime = intervalSettings.restTime
      break
    default:
      return 100
  }
  
  return ((totalTime - intervalTimer.timeLeft) / totalTime) * 100
}

// ===================================
// Utility Methods
// ===================================

function formatTime(seconds: number): string {
  const mins = Math.floor(seconds / 60)
  const secs = seconds % 60
  return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
}

function formatTimestamp(timestamp: Date): string {
  return timestamp.toLocaleTimeString('ja-JP', {
    hour: '2-digit',
    minute: '2-digit'
  })
}

function playSound(type: 'start' | 'warning' | 'complete' | 'success' | 'rest' | 'countdown') {
  if (!soundEnabled.value) return
  
  // Create audio context and play beep sound
  try {
    const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)()
    const oscillator = audioContext.createOscillator()
    const gainNode = audioContext.createGain()
    
    oscillator.connect(gainNode)
    gainNode.connect(audioContext.destination)
    
    let frequency = 800
    let duration = 200
    
    switch (type) {
      case 'start':
        frequency = 880
        duration = 300
        break
      case 'warning':
        frequency = 440
        duration = 100
        break
      case 'complete':
        frequency = 523
        duration = 500
        break
      case 'success':
        frequency = 659
        duration = 200
        break
      case 'rest':
        frequency = 392
        duration = 300
        break
      case 'countdown':
        frequency = 698
        duration = 100
        break
    }
    
    oscillator.frequency.setValueAtTime(frequency, audioContext.currentTime)
    gainNode.gain.setValueAtTime(0.1, audioContext.currentTime)
    
    oscillator.start(audioContext.currentTime)
    oscillator.stop(audioContext.currentTime + duration / 1000)
  } catch (error) {
    console.warn('Audio not supported:', error)
  }
}

function resetAll() {
  resetRestTimer()
  resetWorkoutTimer()
  resetIntervalTimer()
  restHistory.value = []
  customMinutes.value = 1
  customSeconds.value = 30
}

// ===================================
// Lifecycle
// ===================================

onMounted(() => {
  // Auto-start workout timer if coming from a training session
  if (document.referrer.includes('/training/session/')) {
    activeMode.value = 'workout'
    startWorkoutTimer()
  }
})

onUnmounted(() => {
  pauseRestTimer()
  pauseWorkoutTimer()
  pauseIntervalTimer()
})

// ===================================
// Page Meta
// ===================================

useHead({
  title: 'トレーニングタイマー - Message',
  meta: [
    { name: 'description', content: 'セット間休憩とワークアウト時間管理ツール' }
  ]
})
</script>

<style scoped>
.training-timer {
  max-width: 900px;
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

/* Mode Tabs */
.timer-modes {
  margin-bottom: 30px;
}

.mode-tabs {
  display: flex;
  background: var(--bg-secondary);
  border-radius: 12px;
  padding: 4px;
  gap: 4px;
}

.mode-tab {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 12px 16px;
  background: transparent;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  font-weight: 500;
  color: var(--text-secondary);
  font-size: 0.9rem;
}

.mode-tab.active {
  background: var(--primary);
  color: white;
  box-shadow: 0 2px 8px rgba(0, 122, 255, 0.3);
}

.mode-tab:hover:not(.active) {
  background: var(--bg-tertiary);
  color: var(--text-primary);
}

/* Timer Card */
.timer-section {
  margin-bottom: 30px;
}

.timer-card {
  background: var(--card-bg);
  border-radius: 16px;
  padding: 30px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  border: 2px solid var(--border);
}

.card-header {
  text-align: center;
  margin-bottom: 30px;
}

.card-header h2 {
  margin: 0 0 10px 0;
  color: var(--text-primary);
  font-size: 1.5rem;
  font-weight: 700;
}

.card-header p {
  margin: 0;
  color: var(--text-secondary);
  font-size: 1rem;
}

/* Preset Times */
.preset-times h3,
.custom-time h3,
.interval-settings h3 {
  margin: 0 0 15px 0;
  color: var(--text-primary);
  font-size: 1.2rem;
  font-weight: 600;
}

.preset-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 12px;
  margin-bottom: 25px;
}

.preset-btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 15px 10px;
  background: var(--bg-secondary);
  border: 2px solid var(--border);
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.2s;
}

.preset-btn:hover {
  border-color: var(--primary);
  transform: translateY(-2px);
}

.preset-btn.active {
  border-color: var(--primary);
  background: linear-gradient(135deg, rgba(0, 122, 255, 0.1), rgba(0, 122, 255, 0.05));
}

.preset-time {
  font-size: 1.2rem;
  font-weight: 700;
  color: var(--primary);
  margin-bottom: 4px;
}

.preset-label {
  font-size: 0.8rem;
  color: var(--text-secondary);
}

/* Custom Time */
.custom-time {
  margin-bottom: 30px;
}

.time-inputs {
  display: flex;
  align-items: end;
  gap: 15px;
  flex-wrap: wrap;
}

.time-input-group {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.time-input-group label {
  font-size: 0.9rem;
  color: var(--text-secondary);
  font-weight: 500;
}

.time-input {
  width: 80px;
  padding: 10px;
  border: 2px solid var(--border);
  border-radius: 6px;
  text-align: center;
  font-size: 1rem;
  font-weight: 600;
}

.time-input:focus {
  outline: none;
  border-color: var(--primary);
}

/* Main Timer */
.main-timer {
  background: var(--bg-secondary);
  border-radius: 20px;
  padding: 40px 30px;
  text-align: center;
  margin-bottom: 30px;
  border: 3px solid var(--border);
  transition: all 0.3s;
}

.main-timer.running {
  border-color: var(--primary);
  background: linear-gradient(135deg, rgba(0, 122, 255, 0.05), rgba(0, 122, 255, 0.1));
}

.main-timer.warning {
  border-color: #ffc107;
  background: linear-gradient(135deg, rgba(255, 193, 7, 0.05), rgba(255, 193, 7, 0.1));
}

.main-timer.complete {
  border-color: #28a745;
  background: linear-gradient(135deg, rgba(40, 167, 69, 0.05), rgba(40, 167, 69, 0.1));
}

.timer-display {
  margin-bottom: 20px;
}

.time-value {
  font-size: 4rem;
  font-weight: 700;
  color: var(--primary);
  margin-bottom: 10px;
  font-variant-numeric: tabular-nums;
}

.time-label {
  font-size: 1.1rem;
  color: var(--text-secondary);
  font-weight: 500;
}

.timer-progress {
  width: 100%;
  height: 8px;
  background: var(--border);
  border-radius: 4px;
  overflow: hidden;
  margin-bottom: 25px;
}

.progress-bar {
  height: 100%;
  background: linear-gradient(90deg, var(--primary), #0056b3);
  border-radius: 4px;
  transition: width 0.3s ease;
}

.timer-controls {
  display: flex;
  justify-content: center;
  gap: 15px;
  flex-wrap: wrap;
}

/* Workout Stats */
.timer-stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
  margin-bottom: 25px;
  padding: 20px;
  background: var(--bg-tertiary);
  border-radius: 12px;
}

.stat-item {
  text-align: center;
}

.stat-label {
  display: block;
  font-size: 0.9rem;
  color: var(--text-secondary);
  margin-bottom: 5px;
}

.stat-value {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--primary);
}

/* Quick Actions */
.quick-actions {
  display: flex;
  justify-content: center;
  gap: 15px;
  margin-top: 20px;
}

/* Interval Settings */
.settings-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
  margin-bottom: 20px;
}

.setting-group {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.setting-group label {
  font-size: 0.9rem;
  color: var(--text-secondary);
  font-weight: 500;
}

.setting-input {
  width: 80px;
  padding: 8px;
  border: 2px solid var(--border);
  border-radius: 6px;
  text-align: center;
  font-weight: 600;
}

.setting-input:focus {
  outline: none;
  border-color: var(--primary);
}

.setting-group span {
  font-size: 0.8rem;
  color: var(--text-secondary);
}

/* Interval Presets */
.interval-presets {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 12px;
  margin-bottom: 25px;
}

.interval-preset-btn {
  padding: 15px;
  background: var(--bg-secondary);
  border: 2px solid var(--border);
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.2s;
  text-align: center;
}

.interval-preset-btn:hover {
  border-color: var(--primary);
  transform: translateY(-2px);
}

.preset-name {
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 5px;
}

.preset-details {
  font-size: 0.8rem;
  color: var(--text-secondary);
}

/* Interval Timer */
.interval-timer {
  background: var(--bg-secondary);
  border-radius: 20px;
  padding: 40px 30px;
  text-align: center;
  margin-bottom: 30px;
  border: 3px solid var(--border);
  transition: all 0.3s;
}

.interval-timer.work {
  border-color: #28a745;
  background: linear-gradient(135deg, rgba(40, 167, 69, 0.05), rgba(40, 167, 69, 0.1));
}

.interval-timer.rest {
  border-color: #ffc107;
  background: linear-gradient(135deg, rgba(255, 193, 7, 0.05), rgba(255, 193, 7, 0.1));
}

.interval-timer.prepare {
  border-color: var(--primary);
  background: linear-gradient(135deg, rgba(0, 122, 255, 0.05), rgba(0, 122, 255, 0.1));
}

.interval-info {
  margin-bottom: 20px;
}

.current-round {
  font-size: 1.1rem;
  color: var(--text-secondary);
  margin-bottom: 8px;
}

.phase-indicator {
  font-size: 1.3rem;
  font-weight: 600;
  color: var(--primary);
}

/* Rest History */
.rest-history {
  background: var(--bg-tertiary);
  border-radius: 12px;
  padding: 20px;
}

.rest-history h3 {
  margin: 0 0 15px 0;
  color: var(--text-primary);
  font-size: 1.1rem;
  font-weight: 600;
}

.history-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.history-item {
  display: flex;
  justify-content: space-between;
  padding: 8px 12px;
  background: var(--card-bg);
  border-radius: 6px;
  border: 1px solid var(--border);
}

.history-time {
  font-weight: 600;
  color: var(--primary);
}

.history-timestamp {
  color: var(--text-secondary);
  font-size: 0.9rem;
}

/* Sound Settings */
.sound-settings {
  margin-top: 30px;
  padding: 20px;
  background: var(--bg-secondary);
  border-radius: 12px;
  border: 1px solid var(--border);
}

.setting-item {
  display: flex;
  align-items: center;
  justify-content: center;
}

.setting-label {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  color: var(--text-primary);
  font-weight: 500;
}

.setting-checkbox {
  width: 18px;
  height: 18px;
  accent-color: var(--primary);
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

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.btn-large {
  padding: 15px 30px;
  font-size: 1.1rem;
}

.btn-primary {
  background: var(--primary);
  color: white;
}

.btn-secondary {
  background: #6c757d;
  color: white;
}

.btn-success {
  background: #28a745;
  color: white;
}

.btn-warning {
  background: #ffc107;
  color: #212529;
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
  .training-timer {
    padding: 15px;
  }
  
  .page-header {
    flex-direction: column;
    align-items: stretch;
  }
  
  .timer-card {
    padding: 20px;
  }
  
  .mode-tabs {
    flex-direction: column;
  }
  
  .preset-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .time-inputs {
    flex-direction: column;
    align-items: stretch;
  }
  
  .timer-controls {
    flex-direction: column;
  }
  
  .time-value {
    font-size: 3rem;
  }
  
  .settings-grid {
    grid-template-columns: 1fr;
  }
  
  .timer-stats {
    grid-template-columns: 1fr;
  }
  
  .quick-actions {
    flex-direction: column;
  }
}
</style>