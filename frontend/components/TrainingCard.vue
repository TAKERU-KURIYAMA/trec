<template>
  <div 
    class="training-card" 
    :class="{ 
      'card-clickable': clickable,
      'card-favorite': isFavorite 
    }"
    @click="handleClick"
  >
    <!-- Header -->
    <div class="card-header">
      <h3 class="card-title">{{ menu.jpName }}</h3>
      <button 
        v-if="showFavoriteButton"
        @click.stop="$emit('favorite', menu)"
        class="favorite-btn"
        :class="{ active: isFavorite }"
      >
        {{ isFavorite ? '❤️' : '🤍' }}
      </button>
    </div>

    <!-- Description -->
    <p v-if="menu.description" class="card-description">
      {{ menu.description }}
    </p>

    <!-- English Name -->
    <p v-if="menu.enName && showEnglishName" class="card-en-name">
      {{ menu.enName }}
    </p>

    <!-- Tags -->
    <div v-if="menu.tagIds && menu.tagIds.length > 0" class="tags">
      <span
        v-for="tagId in displayTags"
        :key="tagId"
        class="tag"
      >
        {{ getTagName(tagId) }}
      </span>
      <span v-if="menu.tagIds.length > maxTags" class="tag tag-more">
        +{{ menu.tagIds.length - maxTags }}
      </span>
    </div>

    <!-- Footer -->
    <div v-if="showFooter" class="card-footer">
      <div class="card-meta">
        <span v-if="menu.createdAt" class="created-date">
          {{ formatDate(menu.createdAt) }}
        </span>
      </div>
      <div class="card-actions">
        <slot name="actions" :menu="menu">
          <button v-if="clickable" class="action-btn primary">
            詳細を見る
          </button>
        </slot>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { TrainingMenu, TrainingCardProps } from '~/types'

// ===================================
// Props & Emits
// ===================================

interface Props extends TrainingCardProps {
  /** お気に入りボタンを表示するかどうか */
  showFavoriteButton?: boolean
  /** 英語名を表示するかどうか */
  showEnglishName?: boolean
  /** フッターを表示するかどうか */
  showFooter?: boolean
  /** 最大表示タグ数 */
  maxTags?: number
  /** お気に入り状態 */
  isFavorite?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  clickable: true,
  size: 'medium',
  showFavoriteButton: true,
  showEnglishName: false,
  showFooter: true,
  maxTags: 3,
  isFavorite: false
})

const emit = defineEmits<{
  click: [menu: TrainingMenu]
  favorite: [menu: TrainingMenu]
}>()

// ===================================
// Computed Properties
// ===================================

/** 表示するタグのリスト */
const displayTags = computed(() => {
  return props.menu.tagIds.slice(0, props.maxTags)
})

// ===================================
// Methods
// ===================================

/**
 * カードクリック時の処理
 */
function handleClick() {
  if (props.clickable) {
    emit('click', props.menu)
  }
}

/**
 * タグIDからタグ名を取得
 * TODO: 実際の実装では、タグマスターから名前を取得
 */
function getTagName(tagId: string): string {
  // 簡単なマッピング（実際は親コンポーネントから渡すか、ストアから取得）
  const tagNameMap: Record<string, string> = {
    'upper_body': '上半身',
    'lower_body': '下半身',
    'cardio': '有酸素',
    'core': 'コア',
    'chest': '胸',
    'back': '背中',
    'shoulders': '肩',
    'arms': '腕',
    'legs': '脚',
    'abs': '腹筋'
  }
  return tagNameMap[tagId] || tagId
}

/**
 * 日付をフォーマット
 */
function formatDate(date: Date): string {
  return date.toLocaleDateString('ja-JP', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}
</script>

<style scoped>
/* ===================================
   Training Card Component Styles
   Ultra-modern design with animations
   ================================= */

.training-card {
  background: white;
  border: 1px solid #e0e6ed;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

.training-card:hover {
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
  transform: translateY(-2px);
}

.card-clickable {
  cursor: pointer;
}

.card-clickable:active {
  transform: translateY(0);
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.08);
}

.card-favorite {
  border-color: #ff6b6b;
  background: linear-gradient(135deg, #fff 0%, #ffe8e8 100%);
}

/* Header */
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 12px;
  gap: 12px;
}

.card-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #2c3e50;
  margin: 0;
  line-height: 1.4;
  flex: 1;
}

.favorite-btn {
  background: none;
  border: none;
  font-size: 1.25rem;
  cursor: pointer;
  padding: 4px;
  border-radius: 50%;
  transition: all 0.2s ease;
  flex-shrink: 0;
}

.favorite-btn:hover {
  background: rgba(255, 107, 107, 0.1);
  transform: scale(1.1);
}

.favorite-btn.active {
  animation: heartbeat 0.6s ease;
}

@keyframes heartbeat {
  0% { transform: scale(1); }
  50% { transform: scale(1.2); }
  100% { transform: scale(1); }
}

/* Content */
.card-description {
  color: #666;
  font-size: 0.9rem;
  line-height: 1.5;
  margin: 0 0 12px 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.card-en-name {
  color: #95a5a6;
  font-size: 0.85rem;
  font-style: italic;
  margin: 0 0 12px 0;
}

/* Tags */
.tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-bottom: 16px;
}

.tag {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  font-size: 0.75rem;
  font-weight: 500;
  padding: 4px 8px;
  border-radius: 12px;
  white-space: nowrap;
  transition: all 0.2s ease;
}

.tag:hover {
  transform: translateY(-1px);
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.3);
}

.tag-more {
  background: #bdc3c7;
  color: #2c3e50;
}

/* Footer */
.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
  padding-top: 16px;
  border-top: 1px solid #f1f3f4;
  gap: 12px;
}

.card-meta {
  flex: 1;
}

.created-date {
  color: #95a5a6;
  font-size: 0.8rem;
}

.card-actions {
  display: flex;
  gap: 8px;
}

.action-btn {
  padding: 6px 12px;
  border: none;
  border-radius: 6px;
  font-size: 0.85rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.action-btn.primary {
  background: #3498db;
  color: white;
}

.action-btn.primary:hover {
  background: #2980b9;
  transform: translateY(-1px);
}

.action-btn.secondary {
  background: #ecf0f1;
  color: #2c3e50;
}

.action-btn.secondary:hover {
  background: #d5dbdb;
}

/* Size Variants */
.training-card.size-small {
  padding: 12px;
}

.training-card.size-small .card-title {
  font-size: 1rem;
}

.training-card.size-small .card-description {
  font-size: 0.8rem;
  -webkit-line-clamp: 1;
}

.training-card.size-large {
  padding: 24px;
}

.training-card.size-large .card-title {
  font-size: 1.5rem;
}

.training-card.size-large .card-description {
  font-size: 1rem;
  -webkit-line-clamp: 3;
}

/* Responsive Design */
@media (max-width: 768px) {
  .training-card {
    padding: 16px;
  }
  
  .card-header {
    margin-bottom: 10px;
  }
  
  .card-title {
    font-size: 1.1rem;
  }
  
  .card-footer {
    flex-direction: column;
    align-items: stretch;
    gap: 8px;
  }
  
  .card-actions {
    justify-content: center;
  }
}

/* Dark Mode Support */
@media (prefers-color-scheme: dark) {
  .training-card {
    background: #2c3e50;
    border-color: #34495e;
    color: #ecf0f1;
  }
  
  .card-title {
    color: #ecf0f1;
  }
  
  .card-description {
    color: #bdc3c7;
  }
  
  .card-footer {
    border-top-color: #34495e;
  }
  
  .action-btn.secondary {
    background: #34495e;
    color: #ecf0f1;
  }
  
  .action-btn.secondary:hover {
    background: #4a5f7a;
  }
}

/* Loading State */
.training-card.loading {
  opacity: 0.6;
  pointer-events: none;
}

.training-card.loading::after {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.4), transparent);
  animation: shimmer 1.5s infinite;
}

@keyframes shimmer {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(100%); }
}
</style>
