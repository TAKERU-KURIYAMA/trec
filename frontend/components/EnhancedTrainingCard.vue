<template>
  <div 
    class="enhanced-training-card"
    :class="[`view-${viewMode}`, { favorite: menu.isFavorite }]"
    @click="handleClick"
  >
    <!-- Grid View -->
    <div v-if="viewMode === 'grid'" class="grid-content">
      <div class="card-header">
        <div class="menu-category">
          <span class="category-icon">{{ getCategoryIcon() }}</span>
          <span class="category-text">{{ getPrimaryCategory() }}</span>
        </div>
        <button 
          @click.stop="toggleFavorite"
          class="favorite-btn"
          :class="{ active: menu.isFavorite }"
        >
          {{ menu.isFavorite ? '❤️' : '🤍' }}
        </button>
      </div>

      <div class="card-body">
        <h3 class="menu-title">{{ menu.jpName }}</h3>
        <p class="menu-description">{{ menu.description || 'トレーニングメニューの説明' }}</p>
        
        <div class="menu-tags">
          <span 
            v-for="tagId in menu.tagIds?.slice(0, 3)"
            :key="tagId"
            class="tag"
          >
            {{ tagId }}
          </span>
          <span v-if="menu.tagIds?.length > 3" class="tag-more">
            +{{ menu.tagIds.length - 3 }}
          </span>
        </div>
      </div>

      <div class="card-footer">
        <div class="menu-stats">
          <div class="stat-item">
            <span class="stat-icon">🔥</span>
            <span class="stat-text">{{ getDifficulty() }}</span>
          </div>
          <div class="stat-item">
            <span class="stat-icon">⏱️</span>
            <span class="stat-text">{{ getEstimatedTime() }}</span>
          </div>
        </div>
        <button class="start-btn">
          開始 →
        </button>
      </div>
    </div>

    <!-- List View -->
    <div v-else class="list-content">
      <div class="list-main">
        <div class="list-info">
          <div class="list-header">
            <h3 class="menu-title">{{ menu.jpName }}</h3>
            <div class="menu-meta">
              <span class="category-badge">{{ getPrimaryCategory() }}</span>
              <span class="difficulty-badge" :class="getDifficultyClass()">
                {{ getDifficulty() }}
              </span>
            </div>
          </div>
          <p class="menu-description">{{ menu.description || 'トレーニングメニューの説明' }}</p>
          <div class="menu-tags">
            <span 
              v-for="tagId in menu.tagIds"
              :key="tagId"
              class="tag"
            >
              {{ tagId }}
            </span>
          </div>
        </div>
        
        <div class="list-stats">
          <div class="stat-group">
            <div class="stat-item">
              <span class="stat-label">難易度:</span>
              <span class="stat-value">{{ getDifficulty() }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">時間:</span>
              <span class="stat-value">{{ getEstimatedTime() }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">カテゴリ:</span>
              <span class="stat-value">{{ getPrimaryCategory() }}</span>
            </div>
          </div>
        </div>
      </div>

      <div class="list-actions">
        <button 
          @click.stop="toggleFavorite"
          class="favorite-btn list-favorite"
          :class="{ active: menu.isFavorite }"
        >
          {{ menu.isFavorite ? '❤️' : '🤍' }}
        </button>
        <button class="start-btn list-start">
          開始
        </button>
      </div>
    </div>

    <!-- Hover overlay for additional info -->
    <div class="hover-overlay">
      <div class="overlay-content">
        <h4>詳細情報</h4>
        <ul class="detail-list">
          <li>推奨レベル: {{ getRecommendedLevel() }}</li>
          <li>主要筋群: {{ getTargetMuscles() }}</li>
          <li>必要器具: {{ getRequiredEquipment() }}</li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  menu: {
    type: Object,
    required: true
  },
  viewMode: {
    type: String,
    default: 'grid'
  }
})

const emit = defineEmits(['click', 'favorite'])

function handleClick() {
  emit('click', props.menu.menuId)
}

function toggleFavorite() {
  emit('favorite', props.menu)
}

function getPrimaryCategory() {
  if (!props.menu.tagIds?.length) return 'その他'
  return props.menu.tagIds[0]
}

function getCategoryIcon() {
  const category = getPrimaryCategory()
  const icons = {
    '上半身': '💪',
    '下半身': '🦵',
    '有酸素': '🫀',
    'コア': '🎯',
    '全身': '🏋️',
    'ストレッチ': '🧘',
    'その他': '⚡'
  }
  return icons[category] || '⚡'
}

function getDifficulty() {
  // Mock difficulty based on menu name or tags
  const menuName = props.menu.jpName?.toLowerCase() || ''
  if (menuName.includes('初心者') || menuName.includes('ビギナー')) return '初級'
  if (menuName.includes('上級') || menuName.includes('アドバンス')) return '上級'
  return '中級'
}

function getDifficultyClass() {
  const difficulty = getDifficulty()
  return {
    'beginner': difficulty === '初級',
    'intermediate': difficulty === '中級',
    'advanced': difficulty === '上級'
  }
}

function getEstimatedTime() {
  // Mock estimated time based on menu type
  const category = getPrimaryCategory()
  const times = {
    '有酸素': '30-45分',
    '上半身': '45-60分',
    '下半身': '45-60分',
    'コア': '15-30分',
    'ストレッチ': '10-20分'
  }
  return times[category] || '30-45分'
}

function getRecommendedLevel() {
  const difficulty = getDifficulty()
  const levels = {
    '初級': '運動初心者',
    '中級': '定期的に運動している方',
    '上級': '上級者・アスリート'
  }
  return levels[difficulty] || '全レベル対応'
}

function getTargetMuscles() {
  const category = getPrimaryCategory()
  const muscles = {
    '上半身': '胸筋、背筋、肩、腕',
    '下半身': '大腿四頭筋、ハムストリング、臀筋',
    '有酸素': '心肺機能、全身持久力',
    'コア': '腹筋、背筋、体幹',
    '全身': '全身の筋群'
  }
  return muscles[category] || '複数の筋群'
}

function getRequiredEquipment() {
  // Mock equipment based on category
  const category = getPrimaryCategory()
  const equipment = {
    '有酸素': 'ランニングマシン、バイク',
    '上半身': 'ダンベル、バーベル',
    '下半身': 'スクワットラック、レッグプレス',
    'コア': 'マット、ボール',
    'ストレッチ': 'マット'
  }
  return equipment[category] || '基本的な器具'
}
</script>

<style scoped>
.enhanced-training-card {
  background: white;
  border-radius: 12px;
  overflow: hidden;
  transition: all 0.3s;
  cursor: pointer;
  position: relative;
  border: 2px solid #e9ecef;
}

.enhanced-training-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 25px rgba(0,0,0,0.15);
  border-color: #3498db;
}

.enhanced-training-card.favorite {
  border-color: #e74c3c;
  box-shadow: 0 2px 10px rgba(231, 76, 60, 0.15);
}

/* Grid View Styles */
.grid-content {
  padding: 20px;
  height: 100%;
  display: flex;
  flex-direction: column;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.menu-category {
  display: flex;
  align-items: center;
  gap: 8px;
  background: #f8f9fa;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 0.85rem;
  color: #666;
}

.category-icon {
  font-size: 1rem;
}

.favorite-btn {
  background: none;
  border: none;
  font-size: 1.3rem;
  cursor: pointer;
  padding: 5px;
  border-radius: 50%;
  transition: all 0.2s;
}

.favorite-btn:hover {
  background: #f8f9fa;
  transform: scale(1.1);
}

.favorite-btn.active {
  animation: heartbeat 0.6s ease-in-out;
}

@keyframes heartbeat {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.2); }
}

.card-body {
  flex: 1;
  margin-bottom: 15px;
}

.menu-title {
  font-size: 1.3rem;
  font-weight: bold;
  color: #2c3e50;
  margin: 0 0 8px 0;
  line-height: 1.3;
}

.menu-description {
  color: #666;
  font-size: 0.9rem;
  line-height: 1.4;
  margin: 0 0 15px 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.menu-tags {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

.tag {
  background: #e8f4fd;
  color: #3498db;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 500;
}

.tag-more {
  background: #f8f9fa;
  color: #666;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 0.75rem;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-top: 1px solid #f0f0f0;
  padding-top: 15px;
}

.menu-stats {
  display: flex;
  gap: 15px;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 0.8rem;
  color: #666;
}

.stat-icon {
  font-size: 0.9rem;
}

.start-btn {
  background: linear-gradient(135deg, #3498db, #2980b9);
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.2s;
  font-size: 0.9rem;
}

.start-btn:hover {
  transform: translateX(2px);
  box-shadow: 0 2px 8px rgba(52, 152, 219, 0.3);
}

/* List View Styles */
.list-content {
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 20px;
}

.list-main {
  flex: 1;
  display: flex;
  gap: 20px;
  align-items: center;
}

.list-info {
  flex: 1;
}

.list-header {
  display: flex;
  align-items: center;
  gap: 15px;
  margin-bottom: 8px;
}

.menu-meta {
  display: flex;
  gap: 8px;
}

.category-badge {
  background: #e8f4fd;
  color: #3498db;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 500;
}

.difficulty-badge {
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 500;
}

.difficulty-badge.beginner {
  background: #d5f4e6;
  color: #27ae60;
}

.difficulty-badge.intermediate {
  background: #fef3cd;
  color: #f39c12;
}

.difficulty-badge.advanced {
  background: #fadbd8;
  color: #e74c3c;
}

.list-stats {
  min-width: 200px;
}

.stat-group {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.list-content .stat-item {
  display: flex;
  justify-content: space-between;
  font-size: 0.85rem;
}

.stat-label {
  color: #666;
}

.stat-value {
  color: #2c3e50;
  font-weight: 500;
}

.list-actions {
  display: flex;
  flex-direction: column;
  gap: 10px;
  align-items: center;
}

.list-favorite {
  font-size: 1.2rem;
}

.list-start {
  padding: 10px 20px;
  white-space: nowrap;
}

/* Hover Overlay */
.hover-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.9);
  color: white;
  padding: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: opacity 0.3s;
  pointer-events: none;
}

.enhanced-training-card:hover .hover-overlay {
  opacity: 1;
}

.overlay-content h4 {
  margin: 0 0 15px 0;
  text-align: center;
  color: #3498db;
}

.detail-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.detail-list li {
  padding: 5px 0;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  font-size: 0.9rem;
}

.detail-list li:last-child {
  border-bottom: none;
}

/* View Mode Specific Adjustments */
.view-list {
  border-radius: 8px;
}

.view-list:hover {
  transform: translateX(4px);
}

.view-list .hover-overlay {
  display: none; /* Hide overlay in list view for better UX */
}

@media (max-width: 768px) {
  .list-content {
    flex-direction: column;
    align-items: stretch;
    gap: 15px;
  }
  
  .list-main {
    flex-direction: column;
    align-items: stretch;
    gap: 15px;
  }
  
  .list-stats {
    min-width: auto;
  }
  
  .list-actions {
    flex-direction: row;
    justify-content: space-between;
  }
  
  .menu-stats {
    justify-content: center;
  }
  
  .card-footer {
    flex-direction: column;
    gap: 15px;
    align-items: stretch;
  }
  
  .start-btn {
    width: 100%;
  }
}
</style>