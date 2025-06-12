<template>
  <div class="smart-recommendations">
    <div class="recommendations-list">
      <div v-if="!recommendations.length" class="no-recommendations">
        <div class="no-recommendations-icon">🤖</div>
        <p>現在、推奨事項はありません</p>
        <p class="sub-text">もっとトレーニングデータが蓄積されると、AIが最適な提案をします！</p>
      </div>
      
      <div
        v-for="recommendation in sortedRecommendations"
        :key="recommendation.id || recommendation.type"
        class="recommendation-item"
        :class="[`priority-${recommendation.priority}`, recommendation.type]"
        @click="expandRecommendation(recommendation)"
      >
        <div class="recommendation-header">
          <div class="recommendation-icon">
            {{ getRecommendationIcon(recommendation.type) }}
          </div>
          <div class="recommendation-info">
            <h3 class="recommendation-title">{{ recommendation.title }}</h3>
            <div class="recommendation-meta">
              <span class="priority-badge" :class="`priority-${recommendation.priority}`">
                {{ getPriorityText(recommendation.priority) }}
              </span>
              <span class="recommendation-type">{{ getTypeText(recommendation.type) }}</span>
            </div>
          </div>
          <div class="recommendation-actions">
            <button 
              @click.stop="applyRecommendation(recommendation)"
              class="apply-btn"
              :class="`priority-${recommendation.priority}`"
            >
              適用
            </button>
            <button 
              @click.stop="dismissRecommendation(recommendation)"
              class="dismiss-btn"
            >
              ✕
            </button>
          </div>
        </div>
        
        <div class="recommendation-description">
          {{ recommendation.description }}
        </div>
        
        <div v-if="recommendation.details" class="recommendation-details">
          <div class="details-toggle" @click.stop="toggleDetails(recommendation)">
            詳細を{{ recommendation.showDetails ? '隠す' : '表示' }}
            <span class="toggle-icon">{{ recommendation.showDetails ? '▲' : '▼' }}</span>
          </div>
          
          <div v-if="recommendation.showDetails" class="details-content">
            <div v-if="recommendation.benefits" class="benefits-section">
              <h4>🎯 期待される効果</h4>
              <ul>
                <li v-for="benefit in recommendation.benefits" :key="benefit">{{ benefit }}</li>
              </ul>
            </div>
            
            <div v-if="recommendation.steps" class="steps-section">
              <h4>📋 実施手順</h4>
              <ol>
                <li v-for="step in recommendation.steps" :key="step">{{ step }}</li>
              </ol>
            </div>
            
            <div v-if="recommendation.data" class="data-section">
              <h4>📊 関連データ</h4>
              <div class="data-grid">
                <div v-for="(value, key) in recommendation.data" :key="key" class="data-item">
                  <span class="data-label">{{ key }}:</span>
                  <span class="data-value">{{ value }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Progress indicator for ongoing recommendations -->
        <div v-if="recommendation.progress !== undefined" class="progress-container">
          <div class="progress-label">進捗: {{ recommendation.progress }}%</div>
          <div class="progress-bar">
            <div 
              class="progress-fill" 
              :style="{ width: `${recommendation.progress}%` }"
            ></div>
          </div>
        </div>
      </div>
    </div>

    <!-- Smart Insights Section -->
    <div class="insights-section">
      <h3>💡 スマートインサイト</h3>
      <div class="insights-grid">
        <div class="insight-card">
          <div class="insight-icon">📈</div>
          <div class="insight-content">
            <div class="insight-title">トレンド分析</div>
            <div class="insight-text">先週から重量が平均5%向上しています</div>
          </div>
        </div>
        
        <div class="insight-card">
          <div class="insight-icon">⚡</div>
          <div class="insight-content">
            <div class="insight-title">最適タイミング</div>
            <div class="insight-text">午前中のワークアウトで最高パフォーマンス</div>
          </div>
        </div>
        
        <div class="insight-card">
          <div class="insight-icon">🎯</div>
          <div class="insight-content">
            <div class="insight-title">目標予測</div>
            <div class="insight-text">現在のペースで月末に目標達成予定</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Action Modal -->
    <div v-if="activeModal" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <button class="close-btn" @click="closeModal">×</button>
        <div class="modal-header">
          <h2>{{ activeModal.title }}</h2>
        </div>
        <div class="modal-body">
          <p>{{ activeModal.description }}</p>
          <div v-if="activeModal.type === 'progressive_overload'" class="overload-calculator">
            <h4>推奨重量計算</h4>
            <div class="calculator-row">
              <label>現在の重量:</label>
              <input v-model.number="currentWeight" type="number" class="weight-input">
              <span>kg</span>
            </div>
            <div class="calculator-result">
              <span>推奨重量: {{ calculatedWeight }}kg (+{{ weightIncrease }}kg)</span>
            </div>
          </div>
          <div class="modal-actions">
            <button @click="confirmAction" class="confirm-btn">実行</button>
            <button @click="closeModal" class="cancel-btn">キャンセル</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  recommendations: {
    type: Array,
    default: () => []
  }
})

const emit = defineEmits(['apply-recommendation', 'dismiss-recommendation'])

const activeModal = ref(null)
const currentWeight = ref(70)

const sortedRecommendations = computed(() => {
  const priorityOrder = { high: 3, medium: 2, low: 1 }
  return [...props.recommendations]
    .sort((a, b) => priorityOrder[b.priority] - priorityOrder[a.priority])
    .map(rec => ({ ...rec, showDetails: false }))
})

const calculatedWeight = computed(() => {
  return currentWeight.value + weightIncrease.value
})

const weightIncrease = computed(() => {
  // Progressive overload recommendation: 2.5kg for upper body, 5kg for lower body
  return 2.5
})

function getRecommendationIcon(type) {
  const icons = {
    rest: '😴',
    progressive_overload: '📈',
    new_exercise: '🆕',
    form_improvement: '🎯',
    nutrition: '🥗',
    recovery: '💆',
    schedule: '📅',
    equipment: '🏋️'
  }
  return icons[type] || '💡'
}

function getPriorityText(priority) {
  const texts = {
    high: '高優先度',
    medium: '中優先度',
    low: '低優先度'
  }
  return texts[priority] || priority
}

function getTypeText(type) {
  const texts = {
    rest: '休息',
    progressive_overload: 'プログレッシブオーバーロード',
    new_exercise: '新しいエクササイズ',
    form_improvement: 'フォーム改善',
    nutrition: '栄養',
    recovery: '回復',
    schedule: 'スケジュール',
    equipment: '器具'
  }
  return texts[type] || type
}

function expandRecommendation(recommendation) {
  // Add detailed information to recommendations
  const details = getRecommendationDetails(recommendation)
  Object.assign(recommendation, details)
}

function getRecommendationDetails(rec) {
  const detailsMap = {
    rest: {
      benefits: [
        '筋肉の回復促進',
        'パフォーマンス向上',
        'オーバートレーニング防止'
      ],
      steps: [
        '今日はトレーニングを休む',
        '軽いストレッチやウォーキング',
        '十分な睡眠を取る',
        '水分補給を心がける'
      ],
      data: {
        '連続トレーニング日数': '4日',
        '推奨休息時間': '24時間',
        '疲労レベル': '中程度'
      }
    },
    progressive_overload: {
      benefits: [
        '筋力の継続的向上',
        'プラトー打破',
        '効率的な筋肥大'
      ],
      steps: [
        '現在の重量を確認',
        '2.5kg増量して実施',
        'フォームを維持',
        '無理せず調整'
      ],
      data: {
        '現在のベンチプレス': '70kg',
        '推奨次回重量': '72.5kg',
        '向上率': '3.6%'
      }
    },
    new_exercise: {
      benefits: [
        '筋肉のバランス改善',
        '新しい刺激による成長',
        'トレーニングの多様性'
      ],
      steps: [
        '適切なフォームを学習',
        '軽い重量から開始',
        '既存のルーティンに組み込み',
        '進捗を記録'
      ],
      data: {
        '未実施部位': '肩',
        '推奨エクササイズ': 'ショルダープレス',
        '開始重量': '20kg'
      }
    }
  }
  
  return detailsMap[rec.type] || {}
}

function toggleDetails(recommendation) {
  recommendation.showDetails = !recommendation.showDetails
}

function applyRecommendation(recommendation) {
  if (recommendation.type === 'progressive_overload') {
    activeModal.value = recommendation
  } else {
    emit('apply-recommendation', recommendation)
  }
}

function dismissRecommendation(recommendation) {
  emit('dismiss-recommendation', recommendation)
}

function closeModal() {
  activeModal.value = null
}

function confirmAction() {
  if (activeModal.value) {
    emit('apply-recommendation', {
      ...activeModal.value,
      appliedWeight: calculatedWeight.value
    })
    closeModal()
  }
}
</script>

<style scoped>
.smart-recommendations {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.recommendations-list {
  flex: 1;
  max-height: 400px;
  overflow-y: auto;
  margin-bottom: 20px;
}

.no-recommendations {
  text-align: center;
  padding: 40px 20px;
  color: #666;
}

.no-recommendations-icon {
  font-size: 3rem;
  margin-bottom: 15px;
}

.sub-text {
  font-size: 0.9rem;
  margin-top: 5px;
}

.recommendation-item {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 15px;
  margin-bottom: 12px;
  cursor: pointer;
  transition: all 0.2s;
  background: white;
}

.recommendation-item:hover {
  box-shadow: 0 2px 8px rgba(0,0,0,0.15);
  transform: translateY(-1px);
}

.recommendation-item.priority-high {
  border-left: 4px solid #e74c3c;
}

.recommendation-item.priority-medium {
  border-left: 4px solid #f39c12;
}

.recommendation-item.priority-low {
  border-left: 4px solid #3498db;
}

.recommendation-header {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  margin-bottom: 10px;
}

.recommendation-icon {
  font-size: 1.5rem;
  flex-shrink: 0;
}

.recommendation-info {
  flex: 1;
}

.recommendation-title {
  margin: 0 0 5px 0;
  font-size: 1rem;
  color: #2c3e50;
}

.recommendation-meta {
  display: flex;
  gap: 10px;
  align-items: center;
}

.priority-badge {
  font-size: 0.7rem;
  padding: 2px 6px;
  border-radius: 10px;
  color: white;
  font-weight: bold;
}

.priority-badge.priority-high {
  background: #e74c3c;
}

.priority-badge.priority-medium {
  background: #f39c12;
}

.priority-badge.priority-low {
  background: #3498db;
}

.recommendation-type {
  font-size: 0.8rem;
  color: #666;
}

.recommendation-actions {
  display: flex;
  gap: 5px;
  flex-shrink: 0;
}

.apply-btn {
  padding: 6px 12px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.8rem;
  font-weight: bold;
  color: white;
  transition: opacity 0.2s;
}

.apply-btn.priority-high {
  background: #e74c3c;
}

.apply-btn.priority-medium {
  background: #f39c12;
}

.apply-btn.priority-low {
  background: #3498db;
}

.apply-btn:hover {
  opacity: 0.9;
}

.dismiss-btn {
  background: none;
  border: 1px solid #ddd;
  border-radius: 4px;
  cursor: pointer;
  padding: 6px 8px;
  color: #666;
  font-size: 0.8rem;
  transition: background-color 0.2s;
}

.dismiss-btn:hover {
  background: #f8f9fa;
}

.recommendation-description {
  color: #666;
  line-height: 1.4;
  margin-bottom: 10px;
}

.details-toggle {
  font-size: 0.8rem;
  color: #3498db;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 5px;
  margin-bottom: 10px;
}

.details-toggle:hover {
  text-decoration: underline;
}

.toggle-icon {
  transition: transform 0.2s;
}

.details-content {
  background: #f8f9fa;
  padding: 15px;
  border-radius: 6px;
  margin-top: 10px;
}

.details-content h4 {
  margin: 0 0 8px 0;
  color: #2c3e50;
  font-size: 0.9rem;
}

.benefits-section ul,
.steps-section ol {
  margin: 0;
  padding-left: 20px;
}

.benefits-section li,
.steps-section li {
  margin-bottom: 4px;
  font-size: 0.9rem;
}

.data-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 8px;
}

.data-item {
  display: flex;
  justify-content: space-between;
  padding: 5px 10px;
  background: white;
  border-radius: 4px;
  font-size: 0.8rem;
}

.data-label {
  color: #666;
}

.data-value {
  font-weight: bold;
  color: #2c3e50;
}

.progress-container {
  margin-top: 10px;
}

.progress-label {
  font-size: 0.8rem;
  color: #666;
  margin-bottom: 5px;
}

.progress-bar {
  height: 6px;
  background: #e0e0e0;
  border-radius: 3px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(135deg, #3498db, #2980b9);
  transition: width 0.3s;
}

.insights-section {
  border-top: 1px solid #e0e0e0;
  padding-top: 20px;
}

.insights-section h3 {
  margin: 0 0 15px 0;
  color: #2c3e50;
}

.insights-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 10px;
}

.insight-card {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 15px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  gap: 10px;
}

.insight-icon {
  font-size: 1.5rem;
  flex-shrink: 0;
}

.insight-title {
  font-weight: bold;
  margin-bottom: 4px;
}

.insight-text {
  font-size: 0.9rem;
  opacity: 0.9;
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

.modal-header h2 {
  margin: 0 0 15px 0;
  color: #2c3e50;
}

.overload-calculator {
  background: #f8f9fa;
  padding: 15px;
  border-radius: 8px;
  margin: 15px 0;
}

.calculator-row {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 10px;
}

.weight-input {
  width: 80px;
  padding: 5px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.calculator-result {
  font-weight: bold;
  color: #27ae60;
}

.modal-actions {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
  margin-top: 20px;
}

.confirm-btn {
  background: #27ae60;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 6px;
  cursor: pointer;
}

.cancel-btn {
  background: #95a5a6;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 6px;
  cursor: pointer;
}

/* Scrollbar styling */
.recommendations-list::-webkit-scrollbar {
  width: 6px;
}

.recommendations-list::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.recommendations-list::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

@media (max-width: 768px) {
  .recommendation-header {
    flex-direction: column;
    gap: 10px;
  }
  
  .recommendation-actions {
    align-self: stretch;
    justify-content: space-between;
  }
  
  .insights-grid {
    grid-template-columns: 1fr;
  }
  
  .data-grid {
    grid-template-columns: 1fr;
  }
}
</style>