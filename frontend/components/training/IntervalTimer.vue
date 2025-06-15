<template>
  <div class="interval-timer">
    <div class="timer-display" :class="{ 'is-running': isRunning, 'is-warning': timeLeft <= 10 && timeLeft > 0 }">
      <div class="time-value">{{ formattedTime }}</div>
      <div class="timer-label">{{ timerLabel }}</div>
    </div>

    <div class="timer-controls">
      <button 
        v-if="!isRunning && timeLeft === selectedInterval"
        @click="startTimer"
        class="btn btn-primary"
      >
        <Icon name="mdi:play" />
        スタート
      </button>
      
      <button 
        v-else-if="isRunning"
        @click="pauseTimer"
        class="btn btn-warning"
      >
        <Icon name="mdi:pause" />
        一時停止
      </button>
      
      <button 
        v-else
        @click="resumeTimer"
        class="btn btn-success"
      >
        <Icon name="mdi:play" />
        再開
      </button>

      <button 
        @click="resetTimer"
        class="btn btn-secondary"
        :disabled="timeLeft === selectedInterval && !isRunning"
      >
        <Icon name="mdi:refresh" />
        リセット
      </button>
    </div>

    <div class="interval-presets">
      <h4>インターバル時間</h4>
      <div class="preset-buttons">
        <button 
          v-for="preset in presets" 
          :key="preset.value"
          @click="setInterval(preset.value)"
          class="preset-btn"
          :class="{ active: selectedInterval === preset.value }"
        >
          {{ preset.label }}
        </button>
      </div>
      
      <div class="custom-interval">
        <label>カスタム（秒）:</label>
        <input 
          type="number" 
          v-model.number="customInterval"
          @change="setCustomInterval"
          min="1"
          max="600"
          class="custom-input"
        />
      </div>
    </div>

    <!-- 音声通知設定 -->
    <div class="timer-settings">
      <label class="checkbox-label">
        <input 
          type="checkbox" 
          v-model="soundEnabled"
        />
        <Icon :name="soundEnabled ? 'mdi:volume-high' : 'mdi:volume-off'" />
        音声通知
      </label>
      
      <label class="checkbox-label">
        <input 
          type="checkbox" 
          v-model="vibrationEnabled"
        />
        <Icon name="mdi:vibrate" />
        バイブレーション
      </label>
    </div>

    <!-- 履歴 -->
    <div class="timer-history" v-if="history.length > 0">
      <h4>セット間隔履歴</h4>
      <div class="history-list">
        <div 
          v-for="(item, index) in history.slice(-5)" 
          :key="index"
          class="history-item"
        >
          <span class="set-number">セット {{ item.setNumber }}</span>
          <span class="interval-time">{{ formatSeconds(item.interval) }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onUnmounted } from 'vue'

interface IntervalHistoryItem {
  setNumber: number
  interval: number
  timestamp: Date
}

// Props
const props = defineProps<{
  currentSetNumber?: number
}>()

// Emits
const emit = defineEmits<{
  'interval-complete': [interval: number]
  'timer-started': []
  'timer-stopped': []
}>()

// State
const selectedInterval = ref(60) // デフォルト60秒
const customInterval = ref(60)
const timeLeft = ref(60)
const isRunning = ref(false)
const soundEnabled = ref(true)
const vibrationEnabled = ref(true)
const history = ref<IntervalHistoryItem[]>([])

// Timer
let intervalId: NodeJS.Timeout | null = null

// Presets
const presets = [
  { label: '30秒', value: 30 },
  { label: '45秒', value: 45 },
  { label: '60秒', value: 60 },
  { label: '90秒', value: 90 },
  { label: '2分', value: 120 },
  { label: '3分', value: 180 },
]

// Computed
const formattedTime = computed(() => {
  const minutes = Math.floor(timeLeft.value / 60)
  const seconds = timeLeft.value % 60
  return `${minutes}:${seconds.toString().padStart(2, '0')}`
})

const timerLabel = computed(() => {
  if (isRunning.value) return 'インターバル中'
  if (timeLeft.value < selectedInterval.value) return '一時停止中'
  return '準備完了'
})

// Methods
function startTimer() {
  isRunning.value = true
  emit('timer-started')
  runTimer()
}

function pauseTimer() {
  isRunning.value = false
  if (intervalId) {
    clearInterval(intervalId)
    intervalId = null
  }
}

function resumeTimer() {
  isRunning.value = true
  runTimer()
}

function resetTimer() {
  pauseTimer()
  timeLeft.value = selectedInterval.value
}

function runTimer() {
  intervalId = setInterval(() => {
    if (timeLeft.value > 0) {
      timeLeft.value--
      
      // カウントダウン音（残り3秒）
      if (timeLeft.value <= 3 && timeLeft.value > 0) {
        playBeep()
      }
      
      // タイマー終了
      if (timeLeft.value === 0) {
        completeTimer()
      }
    }
  }, 1000)
}

function completeTimer() {
  pauseTimer()
  
  // 完了音とバイブレーション
  if (soundEnabled.value) {
    playCompleteSound()
  }
  if (vibrationEnabled.value && typeof navigator !== 'undefined' && 'vibrate' in navigator) {
    navigator.vibrate([200, 100, 200])
  }
  
  // 履歴に追加
  if (props.currentSetNumber) {
    history.value.push({
      setNumber: props.currentSetNumber,
      interval: selectedInterval.value,
      timestamp: new Date()
    })
  }
  
  emit('interval-complete', selectedInterval.value)
  
  // 自動リセット
  setTimeout(() => {
    resetTimer()
  }, 2000)
}

function setInterval(seconds: number) {
  selectedInterval.value = seconds
  if (!isRunning.value) {
    timeLeft.value = seconds
  }
}

function setCustomInterval() {
  if (customInterval.value >= 1 && customInterval.value <= 600) {
    setInterval(customInterval.value)
  }
}

function formatSeconds(seconds: number): string {
  const mins = Math.floor(seconds / 60)
  const secs = seconds % 60
  return mins > 0 ? `${mins}分${secs}秒` : `${secs}秒`
}

// 音声通知（簡易実装）
function playBeep() {
  if (!soundEnabled.value || typeof window === 'undefined') return
  
  try {
    const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)()
    const oscillator = audioContext.createOscillator()
    const gainNode = audioContext.createGain()
    
    oscillator.connect(gainNode)
    gainNode.connect(audioContext.destination)
    
    oscillator.frequency.value = 800
    gainNode.gain.value = 0.1
    
    oscillator.start()
    oscillator.stop(audioContext.currentTime + 0.1)
  } catch (error) {
    console.warn('Audio playback not supported:', error)
  }
}

function playCompleteSound() {
  if (!soundEnabled.value || typeof window === 'undefined') return
  
  try {
    const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)()
    const oscillator = audioContext.createOscillator()
    const gainNode = audioContext.createGain()
    
    oscillator.connect(gainNode)
    gainNode.connect(audioContext.destination)
    
    oscillator.frequency.value = 1000
    gainNode.gain.value = 0.2
    
    oscillator.start()
    oscillator.stop(audioContext.currentTime + 0.3)
  } catch (error) {
    console.warn('Audio playback not supported:', error)
  }
}

// Cleanup
onUnmounted(() => {
  if (intervalId) {
    clearInterval(intervalId)
  }
})

// Watch for external set number changes
watch(() => props.currentSetNumber, () => {
  if (!isRunning.value && timeLeft.value === 0) {
    resetTimer()
  }
})
</script>

<style scoped>
.interval-timer {
  background: var(--card-bg);
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.timer-display {
  text-align: center;
  margin-bottom: 1.5rem;
  padding: 2rem;
  background: var(--bg-secondary);
  border-radius: 8px;
  transition: all 0.3s ease;
}

.timer-display.is-running {
  background: var(--primary-light);
}

.timer-display.is-warning {
  background: var(--warning-light);
  animation: pulse 1s infinite;
}

@keyframes pulse {
  0% { opacity: 1; }
  50% { opacity: 0.7; }
  100% { opacity: 1; }
}

.time-value {
  font-size: 3.5rem;
  font-weight: 700;
  font-family: 'Roboto Mono', monospace;
  color: var(--text-primary);
  line-height: 1;
}

.timer-label {
  font-size: 0.875rem;
  color: var(--text-secondary);
  margin-top: 0.5rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.timer-controls {
  display: flex;
  gap: 0.75rem;
  justify-content: center;
  margin-bottom: 1.5rem;
}

.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.25rem;
  border: none;
  border-radius: 6px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

.btn-primary {
  background: var(--primary);
  color: white;
}

.btn-warning {
  background: var(--warning);
  color: white;
}

.btn-success {
  background: var(--success);
  color: white;
}

.btn-secondary {
  background: var(--bg-tertiary);
  color: var(--text-primary);
}

.interval-presets {
  margin-bottom: 1.5rem;
}

.interval-presets h4 {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-secondary);
  margin-bottom: 0.75rem;
}

.preset-buttons {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.preset-btn {
  padding: 0.5rem;
  border: 1px solid var(--border);
  background: var(--bg-secondary);
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.875rem;
}

.preset-btn:hover {
  border-color: var(--primary);
  background: var(--primary-light);
}

.preset-btn.active {
  background: var(--primary);
  color: white;
  border-color: var(--primary);
}

.custom-interval {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.custom-interval label {
  font-size: 0.875rem;
  color: var(--text-secondary);
}

.custom-input {
  width: 80px;
  padding: 0.5rem;
  border: 1px solid var(--border);
  border-radius: 4px;
  font-size: 0.875rem;
}

.timer-settings {
  display: flex;
  gap: 1.5rem;
  padding: 1rem;
  background: var(--bg-secondary);
  border-radius: 6px;
  margin-bottom: 1.5rem;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  font-size: 0.875rem;
  color: var(--text-primary);
}

.checkbox-label input[type="checkbox"] {
  cursor: pointer;
}

.timer-history {
  border-top: 1px solid var(--border);
  padding-top: 1rem;
}

.timer-history h4 {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-secondary);
  margin-bottom: 0.75rem;
}

.history-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.history-item {
  display: flex;
  justify-content: space-between;
  padding: 0.5rem 0.75rem;
  background: var(--bg-secondary);
  border-radius: 4px;
  font-size: 0.875rem;
}

.set-number {
  color: var(--text-secondary);
}

.interval-time {
  font-weight: 500;
  color: var(--primary);
}

/* ダークモード対応 */
:root {
  --card-bg: #ffffff;
  --bg-secondary: #f5f5f5;
  --bg-tertiary: #e0e0e0;
  --text-primary: #212121;
  --text-secondary: #757575;
  --border: #e0e0e0;
  --primary: #2196f3;
  --primary-light: #e3f2fd;
  --warning: #ff9800;
  --warning-light: #fff3e0;
  --success: #4caf50;
}

html.dark {
  --card-bg: #1e1e1e;
  --bg-secondary: #2d2d2d;
  --bg-tertiary: #3d3d3d;
  --text-primary: #ffffff;
  --text-secondary: #b0b0b0;
  --border: #3d3d3d;
  --primary: #42a5f5;
  --primary-light: #1e3a5f;
  --warning: #ffa726;
  --warning-light: #3e2723;
  --success: #66bb6a;
}
</style>