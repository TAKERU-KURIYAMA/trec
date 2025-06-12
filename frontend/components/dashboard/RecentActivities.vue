<template>
  <div class="recent-activities">
    <div class="activities-list">
      <div v-if="!activities.length" class="no-activities">
        <div class="no-activities-icon">📝</div>
        <p>まだアクティビティがありません</p>
        <p class="sub-text">最初のワークアウトを記録しましょう！</p>
      </div>
      
      <div
        v-for="activity in activities"
        :key="activity.id"
        class="activity-item"
        @click="viewDetails(activity)"
      >
        <div class="activity-info">
          <div class="activity-header">
            <h3 class="exercise-name">{{ activity.exercise }}</h3>
            <span class="activity-date">{{ formatDate(activity.date) }}</span>
          </div>
          <div class="activity-details">
            <div class="detail-item">
              <span class="detail-label">重量:</span>
              <span class="detail-value">{{ activity.weight }}kg</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">レップ:</span>
              <span class="detail-value">{{ activity.reps }}回</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">セット:</span>
              <span class="detail-value">{{ activity.sets }}セット</span>
            </div>
            <div class="detail-item volume">
              <span class="detail-label">総ボリューム:</span>
              <span class="detail-value">{{ calculateVolume(activity) }}kg</span>
            </div>
          </div>
        </div>
        
        <div class="activity-actions">
          <button @click.stop="editActivity(activity)" class="action-btn edit-btn">
            ✏️
          </button>
          <button @click.stop="deleteActivity(activity)" class="action-btn delete-btn">
            🗑️
          </button>
        </div>
        
        <!-- Progress indicator if it's a personal record -->
        <div v-if="isPersonalRecord(activity)" class="pr-badge">
          🏆 PR!
        </div>
      </div>
    </div>
    
    <div class="activities-footer">
      <button @click="viewAllActivities" class="view-all-btn">
        すべてのアクティビティを表示
      </button>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  activities: {
    type: Array,
    default: () => []
  }
})

const emit = defineEmits(['edit-activity', 'delete-activity', 'view-details'])

function formatDate(dateString) {
  const date = new Date(dateString)
  const today = new Date()
  const yesterday = new Date(today)
  yesterday.setDate(yesterday.getDate() - 1)
  
  if (date.toDateString() === today.toDateString()) {
    return '今日'
  } else if (date.toDateString() === yesterday.toDateString()) {
    return '昨日'
  } else {
    return date.toLocaleDateString('ja-JP', { 
      month: 'short', 
      day: 'numeric',
      weekday: 'short'
    })
  }
}

function calculateVolume(activity) {
  return activity.weight * activity.reps * activity.sets
}

function isPersonalRecord(activity) {
  // This would typically check against historical data
  // For now, we'll mark activities with high volume as potential PRs
  const volume = calculateVolume(activity)
  return volume > 1000 // Simplified logic
}

function viewDetails(activity) {
  emit('view-details', activity)
}

function editActivity(activity) {
  emit('edit-activity', activity)
}

function deleteActivity(activity) {
  if (confirm(`${activity.exercise}の記録を削除しますか？`)) {
    emit('delete-activity', activity)
  }
}

function viewAllActivities() {
  // Navigate to full activities page
  navigateTo('/training/history')
}
</script>

<style scoped>
.recent-activities {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.activities-list {
  flex: 1;
  max-height: 400px;
  overflow-y: auto;
  margin-bottom: 15px;
}

.no-activities {
  text-align: center;
  padding: 40px 20px;
  color: #666;
}

.no-activities-icon {
  font-size: 3rem;
  margin-bottom: 15px;
}

.sub-text {
  font-size: 0.9rem;
  margin-top: 5px;
}

.activity-item {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 15px;
  margin-bottom: 12px;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
  background: white;
}

.activity-item:hover {
  border-color: #3498db;
  box-shadow: 0 2px 8px rgba(52, 152, 219, 0.15);
  transform: translateY(-1px);
}

.activity-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.exercise-name {
  margin: 0;
  color: #2c3e50;
  font-size: 1.1rem;
}

.activity-date {
  font-size: 0.9rem;
  color: #666;
  font-weight: 500;
}

.activity-details {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(100px, 1fr));
  gap: 10px;
  margin-bottom: 10px;
}

.detail-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 8px;
  background: #f8f9fa;
  border-radius: 6px;
}

.detail-item.volume {
  grid-column: span 2;
  background: linear-gradient(135deg, #3498db, #2980b9);
  color: white;
}

.detail-label {
  font-size: 0.8rem;
  color: #666;
  margin-bottom: 2px;
}

.volume .detail-label {
  color: rgba(255, 255, 255, 0.9);
}

.detail-value {
  font-weight: bold;
  font-size: 1rem;
}

.activity-actions {
  position: absolute;
  top: 10px;
  right: 10px;
  display: flex;
  gap: 5px;
  opacity: 0;
  transition: opacity 0.2s;
}

.activity-item:hover .activity-actions {
  opacity: 1;
}

.action-btn {
  background: none;
  border: none;
  font-size: 1.2rem;
  cursor: pointer;
  padding: 5px;
  border-radius: 4px;
  transition: background-color 0.2s;
}

.edit-btn:hover {
  background: rgba(52, 152, 219, 0.1);
}

.delete-btn:hover {
  background: rgba(231, 76, 60, 0.1);
}

.pr-badge {
  position: absolute;
  top: -5px;
  left: -5px;
  background: linear-gradient(135deg, #f39c12, #e67e22);
  color: white;
  font-size: 0.7rem;
  padding: 2px 8px;
  border-radius: 12px;
  font-weight: bold;
  box-shadow: 0 2px 4px rgba(0,0,0,0.2);
}

.activities-footer {
  border-top: 1px solid #e0e0e0;
  padding-top: 15px;
  text-align: center;
}

.view-all-btn {
  background: linear-gradient(135deg, #3498db, #2980b9);
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  transition: transform 0.2s;
}

.view-all-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(52, 152, 219, 0.3);
}

/* Scrollbar styling */
.activities-list::-webkit-scrollbar {
  width: 6px;
}

.activities-list::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.activities-list::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.activities-list::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}

@media (max-width: 768px) {
  .activity-details {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .detail-item.volume {
    grid-column: span 2;
  }
  
  .activity-actions {
    opacity: 1;
  }
}
</style>