<template>
  <div class="personal-records">
    <div class="records-list">
      <div v-if="!records.length" class="no-records">
        <div class="no-records-icon">🏆</div>
        <p>まだパーソナルレコードがありません</p>
        <p class="sub-text">最初のワークアウトを記録してPRを作りましょう！</p>
      </div>
      
      <div
        v-for="record in sortedRecords"
        :key="record.exercise"
        class="record-item"
        @click="viewRecordHistory(record)"
      >
        <div class="record-info">
          <div class="exercise-name">{{ record.exercise }}</div>
          <div class="record-details">
            <div class="weight-info">
              <span class="weight-value">{{ record.weight }}kg</span>
              <span class="weight-label">最高重量</span>
            </div>
            <div class="date-info">
              <span class="date-value">{{ formatDate(record.date) }}</span>
              <span class="date-label">{{ getDaysAgo(record.date) }}</span>
            </div>
          </div>
        </div>
        
        <div class="record-actions">
          <div class="pr-badge">
            🏆 PR
          </div>
          <button @click.stop="challengeRecord(record)" class="challenge-btn">
            💪 挑戦
          </button>
        </div>
        
        <!-- Progress indicator -->
        <div class="improvement-indicator" v-if="getImprovement(record)">
          <span class="improvement-text">
            前回から +{{ getImprovement(record) }}kg
          </span>
        </div>
      </div>
    </div>

    <div class="records-summary">
      <div class="summary-stats">
        <div class="summary-item">
          <span class="summary-number">{{ totalPRs }}</span>
          <span class="summary-label">総PR数</span>
        </div>
        <div class="summary-item">
          <span class="summary-number">{{ recentPRs }}</span>
          <span class="summary-label">今月のPR</span>
        </div>
        <div class="summary-item">
          <span class="summary-number">{{ totalWeight }}kg</span>
          <span class="summary-label">合計重量</span>
        </div>
      </div>
      
      <button @click="viewAllRecords" class="view-all-btn">
        全記録を表示
      </button>
    </div>

    <!-- Challenge Modal -->
    <div v-if="challengeModal" class="modal-overlay" @click="closeChallengeModal">
      <div class="modal-content" @click.stop>
        <button class="close-btn" @click="closeChallengeModal">×</button>
        <div class="modal-header">
          <h2>🎯 {{ challengeModal.exercise }} チャレンジ</h2>
          <p>現在のPR: {{ challengeModal.weight }}kg</p>
        </div>
        <div class="modal-body">
          <div class="challenge-suggestions">
            <h3>推奨チャレンジ</h3>
            <div class="suggestion-list">
              <div 
                v-for="suggestion in getChallengeSuggestions(challengeModal)"
                :key="suggestion.weight"
                class="suggestion-item"
                @click="selectChallenge(suggestion)"
              >
                <div class="suggestion-weight">{{ suggestion.weight }}kg</div>
                <div class="suggestion-increase">+{{ suggestion.increase }}kg</div>
                <div class="suggestion-difficulty">{{ suggestion.difficulty }}</div>
              </div>
            </div>
          </div>
          
          <div class="custom-challenge">
            <h3>カスタムチャレンジ</h3>
            <div class="weight-input-group">
              <input 
                v-model.number="customWeight" 
                type="number" 
                :min="challengeModal.weight"
                class="weight-input"
                placeholder="目標重量"
              >
              <span class="input-unit">kg</span>
            </div>
            <button 
              @click="startCustomChallenge" 
              :disabled="!isValidCustomWeight"
              class="start-challenge-btn"
            >
              チャレンジ開始
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  records: {
    type: Array,
    default: () => []
  }
})

const emit = defineEmits(['challenge-record', 'view-record-history'])

const challengeModal = ref(null)
const customWeight = ref('')

const sortedRecords = computed(() => {
  return [...props.records].sort((a, b) => new Date(b.date) - new Date(a.date))
})

const totalPRs = computed(() => {
  return props.records.length
})

const recentPRs = computed(() => {
  const oneMonthAgo = new Date()
  oneMonthAgo.setMonth(oneMonthAgo.getMonth() - 1)
  return props.records.filter(record => new Date(record.date) >= oneMonthAgo).length
})

const totalWeight = computed(() => {
  return props.records.reduce((sum, record) => sum + record.weight, 0)
})

const isValidCustomWeight = computed(() => {
  return customWeight.value && 
         customWeight.value > challengeModal.value?.weight &&
         customWeight.value <= challengeModal.value?.weight * 1.5
})

function formatDate(dateString) {
  const date = new Date(dateString)
  return date.toLocaleDateString('ja-JP', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

function getDaysAgo(dateString) {
  const date = new Date(dateString)
  const now = new Date()
  const diffTime = Math.abs(now - date)
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
  
  if (diffDays === 1) return '昨日'
  if (diffDays < 7) return `${diffDays}日前`
  if (diffDays < 30) return `${Math.floor(diffDays / 7)}週間前`
  return `${Math.floor(diffDays / 30)}ヶ月前`
}

function getImprovement(record) {
  // This would typically compare with previous records
  // For now, we'll simulate some improvement data
  const improvements = {
    'ベンチプレス': 2.5,
    'スクワット': 5,
    'デッドリフト': 5
  }
  return improvements[record.exercise] || null
}

function viewRecordHistory(record) {
  emit('view-record-history', record)
}

function challengeRecord(record) {
  challengeModal.value = record
  customWeight.value = ''
}

function closeChallengeModal() {
  challengeModal.value = null
  customWeight.value = ''
}

function getChallengeSuggestions(record) {
  const currentWeight = record.weight
  return [
    {
      weight: currentWeight + 2.5,
      increase: 2.5,
      difficulty: '初級'
    },
    {
      weight: currentWeight + 5,
      increase: 5,
      difficulty: '中級'
    },
    {
      weight: currentWeight + 7.5,
      increase: 7.5,
      difficulty: '上級'
    }
  ]
}

function selectChallenge(suggestion) {
  customWeight.value = suggestion.weight
}

function startCustomChallenge() {
  if (isValidCustomWeight.value) {
    emit('challenge-record', {
      exercise: challengeModal.value.exercise,
      currentPR: challengeModal.value.weight,
      targetWeight: customWeight.value
    })
    closeChallengeModal()
  }
}

function viewAllRecords() {
  navigateTo('/training/records')
}
</script>

<style scoped>
.personal-records {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.records-list {
  flex: 1;
  max-height: 300px;
  overflow-y: auto;
  margin-bottom: 15px;
}

.no-records {
  text-align: center;
  padding: 40px 20px;
  color: #666;
}

.no-records-icon {
  font-size: 3rem;
  margin-bottom: 15px;
}

.sub-text {
  font-size: 0.9rem;
  margin-top: 5px;
}

.record-item {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 15px;
  margin-bottom: 12px;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
  background: white;
}

.record-item:hover {
  border-color: #f39c12;
  box-shadow: 0 2px 8px rgba(243, 156, 18, 0.15);
  transform: translateY(-1px);
}

.record-info {
  margin-bottom: 10px;
}

.exercise-name {
  font-size: 1.1rem;
  font-weight: bold;
  color: #2c3e50;
  margin-bottom: 8px;
}

.record-details {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 15px;
}

.weight-info,
.date-info {
  text-align: center;
}

.weight-value,
.date-value {
  display: block;
  font-size: 1.3rem;
  font-weight: bold;
  color: #f39c12;
}

.weight-label,
.date-label {
  display: block;
  font-size: 0.8rem;
  color: #666;
  margin-top: 2px;
}

.record-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 10px;
}

.pr-badge {
  background: linear-gradient(135deg, #f39c12, #e67e22);
  color: white;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: bold;
}

.challenge-btn {
  background: linear-gradient(135deg, #e74c3c, #c0392b);
  color: white;
  border: none;
  padding: 6px 12px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.8rem;
  font-weight: bold;
  transition: transform 0.2s;
}

.challenge-btn:hover {
  transform: scale(1.05);
}

.improvement-indicator {
  position: absolute;
  top: 5px;
  right: 5px;
  background: #27ae60;
  color: white;
  padding: 2px 6px;
  border-radius: 10px;
  font-size: 0.7rem;
  font-weight: bold;
}

.records-summary {
  border-top: 1px solid #e0e0e0;
  padding-top: 15px;
}

.summary-stats {
  display: flex;
  justify-content: space-around;
  margin-bottom: 15px;
}

.summary-item {
  text-align: center;
}

.summary-number {
  display: block;
  font-size: 1.3rem;
  font-weight: bold;
  color: #f39c12;
}

.summary-label {
  font-size: 0.8rem;
  color: #666;
}

.view-all-btn {
  width: 100%;
  background: linear-gradient(135deg, #f39c12, #e67e22);
  color: white;
  border: none;
  padding: 10px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  transition: transform 0.2s;
}

.view-all-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(243, 156, 18, 0.3);
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
  max-width: 500px;
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

.suggestion-list {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 10px;
  margin-bottom: 20px;
}

.suggestion-item {
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  padding: 15px;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s;
}

.suggestion-item:hover {
  border-color: #f39c12;
  background: #fef9e7;
}

.suggestion-weight {
  font-size: 1.2rem;
  font-weight: bold;
  color: #f39c12;
}

.suggestion-increase {
  font-size: 0.9rem;
  color: #27ae60;
  margin: 5px 0;
}

.suggestion-difficulty {
  font-size: 0.8rem;
  color: #666;
}

.custom-challenge {
  border-top: 1px solid #e0e0e0;
  padding-top: 20px;
}

.weight-input-group {
  display: flex;
  align-items: center;
  margin-bottom: 15px;
}

.weight-input {
  flex: 1;
  padding: 10px;
  border: 2px solid #e0e0e0;
  border-radius: 6px;
  font-size: 1rem;
  margin-right: 5px;
}

.input-unit {
  font-weight: bold;
  color: #666;
}

.start-challenge-btn {
  width: 100%;
  background: linear-gradient(135deg, #e74c3c, #c0392b);
  color: white;
  border: none;
  padding: 12px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: bold;
  transition: opacity 0.2s;
}

.start-challenge-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Scrollbar styling */
.records-list::-webkit-scrollbar {
  width: 6px;
}

.records-list::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.records-list::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

@media (max-width: 768px) {
  .record-details {
    grid-template-columns: 1fr;
    gap: 10px;
  }
  
  .record-actions {
    flex-direction: column;
    gap: 10px;
    align-items: stretch;
  }
  
  .challenge-btn {
    width: 100%;
  }
}
</style>