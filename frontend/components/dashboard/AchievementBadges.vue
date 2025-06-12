<template>
  <div class="achievement-badges">
    <div class="achievements-grid">
      <div
        v-for="achievement in achievements"
        :key="achievement.id"
        class="achievement-badge"
        :class="{ 
          earned: achievement.earned, 
          locked: !achievement.earned 
        }"
        @click="showAchievementDetails(achievement)"
      >
        <div class="badge-icon">
          {{ achievement.icon }}
        </div>
        <div class="badge-content">
          <h3 class="badge-title">{{ achievement.name }}</h3>
          <p class="badge-description">{{ achievement.description }}</p>
          <div v-if="achievement.progress !== undefined" class="progress-container">
            <div class="progress-bar">
              <div 
                class="progress-fill" 
                :style="{ width: `${Math.min(achievement.progress, 100)}%` }"
              ></div>
            </div>
            <span class="progress-text">{{ achievement.progress }}%</span>
          </div>
        </div>
        <div v-if="achievement.earned" class="earned-indicator">
          ✅
        </div>
        <div v-else class="lock-indicator">
          🔒
        </div>
      </div>
    </div>

    <div class="achievement-stats">
      <div class="stat-item">
        <span class="stat-number">{{ earnedCount }}</span>
        <span class="stat-label">獲得済み</span>
      </div>
      <div class="stat-item">
        <span class="stat-number">{{ totalCount }}</span>
        <span class="stat-label">総数</span>
      </div>
      <div class="stat-item">
        <span class="stat-number">{{ completionPercentage }}%</span>
        <span class="stat-label">達成率</span>
      </div>
    </div>

    <!-- Achievement Detail Modal -->
    <div v-if="selectedAchievement" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <button class="close-btn" @click="closeModal">×</button>
        <div class="modal-header">
          <div class="modal-icon">{{ selectedAchievement.icon }}</div>
          <h2>{{ selectedAchievement.name }}</h2>
        </div>
        <div class="modal-body">
          <p class="modal-description">{{ selectedAchievement.description }}</p>
          <div v-if="selectedAchievement.earned" class="earned-info">
            <p class="earned-date">
              🎉 {{ formatEarnedDate(selectedAchievement.earnedDate) }}に獲得
            </p>
            <div class="reward-info">
              <h4>報酬</h4>
              <p>{{ selectedAchievement.reward || '達成の喜び！' }}</p>
            </div>
          </div>
          <div v-else-if="selectedAchievement.hint" class="hint-info">
            <h4>💡 ヒント</h4>
            <p>{{ selectedAchievement.hint }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  achievements: {
    type: Array,
    default: () => []
  }
})

const selectedAchievement = ref(null)

const earnedCount = computed(() => {
  return props.achievements.filter(a => a.earned).length
})

const totalCount = computed(() => {
  return props.achievements.length
})

const completionPercentage = computed(() => {
  if (totalCount.value === 0) return 0
  return Math.round((earnedCount.value / totalCount.value) * 100)
})

function showAchievementDetails(achievement) {
  selectedAchievement.value = achievement
}

function closeModal() {
  selectedAchievement.value = null
}

function formatEarnedDate(dateString) {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleDateString('ja-JP', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

// Add some mock data for achievements that don't have certain properties
const enhancedAchievements = computed(() => {
  return props.achievements.map(achievement => ({
    ...achievement,
    progress: achievement.progress !== undefined ? achievement.progress : (achievement.earned ? 100 : Math.random() * 80),
    earnedDate: achievement.earnedDate || (achievement.earned ? '2024-06-10' : null),
    hint: achievement.hint || getAchievementHint(achievement),
    reward: achievement.reward || getAchievementReward(achievement)
  }))
})

function getAchievementHint(achievement) {
  const hints = {
    '初回達成': '最初のワークアウトを完了してみましょう',
    '週間戦士': '7日連続でワークアウトを続けましょう',
    'パワーリフター': '徐々に重量を増やしていきましょう',
    '忍耐の達人': '継続は力なり、毎日少しずつでも続けましょう'
  }
  return hints[achievement.name] || 'がんばって達成しましょう！'
}

function getAchievementReward(achievement) {
  const rewards = {
    '初回達成': '新しいトレーニングメニューが解放されました',
    '週間戦士': 'プレミアム機能が一週間無料で使えます',
    'パワーリフター': '特別なトレーニングプランが解放されました',
    '忍耐の達人': 'マスタートレーナーバッジを獲得しました'
  }
  return rewards[achievement.name] || '達成の喜びと自信を獲得しました！'
}
</script>

<style scoped>
.achievement-badges {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.achievements-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 15px;
  margin-bottom: 20px;
  flex: 1;
}

.achievement-badge {
  border: 2px solid #e0e0e0;
  border-radius: 12px;
  padding: 15px;
  cursor: pointer;
  transition: all 0.3s;
  position: relative;
  background: white;
  height: fit-content;
}

.achievement-badge.earned {
  border-color: #27ae60;
  background: linear-gradient(135deg, #2ecc71, #27ae60);
  color: white;
  box-shadow: 0 4px 15px rgba(46, 204, 113, 0.3);
}

.achievement-badge.locked {
  opacity: 0.6;
  background: #f8f9fa;
}

.achievement-badge:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 20px rgba(0,0,0,0.15);
}

.achievement-badge.earned:hover {
  box-shadow: 0 6px 25px rgba(46, 204, 113, 0.4);
}

.badge-icon {
  font-size: 2.5rem;
  text-align: center;
  margin-bottom: 10px;
}

.badge-title {
  font-size: 1rem;
  font-weight: bold;
  margin: 0 0 5px 0;
  text-align: center;
}

.badge-description {
  font-size: 0.8rem;
  text-align: center;
  margin: 0 0 10px 0;
  line-height: 1.3;
}

.achievement-badge.earned .badge-description {
  color: rgba(255, 255, 255, 0.9);
}

.progress-container {
  margin-top: 10px;
}

.progress-bar {
  height: 6px;
  background: #e0e0e0;
  border-radius: 3px;
  overflow: hidden;
  margin-bottom: 5px;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(135deg, #3498db, #2980b9);
  transition: width 0.3s;
}

.achievement-badge.earned .progress-bar {
  background: rgba(255, 255, 255, 0.3);
}

.achievement-badge.earned .progress-fill {
  background: white;
}

.progress-text {
  font-size: 0.7rem;
  text-align: center;
  display: block;
  font-weight: bold;
}

.earned-indicator {
  position: absolute;
  top: -5px;
  right: -5px;
  background: #27ae60;
  border-radius: 50%;
  width: 25px;
  height: 25px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.8rem;
}

.lock-indicator {
  position: absolute;
  top: 5px;
  right: 5px;
  opacity: 0.5;
}

.achievement-stats {
  display: flex;
  justify-content: space-around;
  padding: 15px 0;
  border-top: 1px solid #e0e0e0;
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

.modal-icon {
  font-size: 4rem;
  margin-bottom: 10px;
}

.modal-header h2 {
  margin: 0;
  color: #2c3e50;
}

.modal-description {
  font-size: 1rem;
  line-height: 1.5;
  margin-bottom: 20px;
  text-align: center;
}

.earned-info {
  background: linear-gradient(135deg, #2ecc71, #27ae60);
  color: white;
  padding: 15px;
  border-radius: 8px;
  margin-bottom: 15px;
}

.earned-date {
  margin: 0 0 10px 0;
  font-weight: bold;
}

.reward-info h4,
.hint-info h4 {
  margin: 0 0 5px 0;
}

.hint-info {
  background: #f8f9fa;
  padding: 15px;
  border-radius: 8px;
  border-left: 4px solid #3498db;
}

@media (max-width: 768px) {
  .achievements-grid {
    grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  }
  
  .achievement-badge {
    padding: 10px;
  }
  
  .badge-icon {
    font-size: 2rem;
  }
  
  .modal-content {
    width: 95%;
    padding: 15px;
  }
}
</style>