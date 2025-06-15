<template>
  <div class="admin-tags">
    <!-- 管理者権限チェック -->
    <div v-if="!authStore.isAdmin" class="access-denied">
      <div class="access-denied-content">
        <Icon name="mdi:shield-alert" size="64" class="error-icon" />
        <h2>アクセス権限がありません</h2>
        <p>この機能にアクセスするには管理者権限が必要です。</p>
        <NuxtLink to="/" class="btn btn-primary">
          <Icon name="mdi:home" />
          ホームに戻る
        </NuxtLink>
      </div>
    </div>

    <!-- 管理者向けタグ管理画面 -->
    <div v-else class="admin-content">
      <!-- ページヘッダー -->
      <div class="page-header">
        <div class="header-content">
          <h1>
            <Icon name="mdi:tag-multiple" size="32" />
            タグ管理
          </h1>
          <p>トレーニングメニューのタグを管理します</p>
        </div>
        <div class="header-actions">
          <button @click="showCreateModal = true" class="btn btn-primary">
            <Icon name="mdi:plus" />
            新規タグ追加
          </button>
        </div>
      </div>

      <!-- タグリスト -->
      <div class="tags-section">
        <div v-if="loading" class="loading-container">
          <div class="loading-spinner">
            <Icon name="mdi:loading" size="32" class="spin" />
            <p>タグを読み込み中...</p>
          </div>
        </div>

        <div v-else-if="error" class="error-container">
          <Icon name="mdi:alert-circle" size="32" class="error-icon" />
          <p>{{ error }}</p>
          <button @click="loadTags" class="btn btn-outline">
            <Icon name="mdi:refresh" />
            再読み込み
          </button>
        </div>

        <div v-else class="tags-grid">
          <div
            v-for="tag in tags"
            :key="tag.tag_id"
            class="tag-card"
          >
            <div class="tag-header">
              <h3>{{ tag.tag_name }}</h3>
              <div class="tag-actions">
                <button
                  @click="viewTagMenus(tag)"
                  class="btn btn-sm btn-outline"
                  title="使用中のメニューを表示"
                >
                  <Icon name="mdi:eye" size="16" />
                </button>
              </div>
            </div>
            
            <div class="tag-details">
              <p class="tag-id">ID: {{ tag.tag_id }}</p>
              
              <div class="tag-usage">
                <span class="usage-label">使用中のメニュー:</span>
                <span class="menu-count">{{ tag.menu_count }}件</span>
              </div>
              
              <div v-if="tag.menus && tag.menus.length > 0" class="associated-menus">
                <div class="menus-list">
                  <span
                    v-for="menu in tag.menus.slice(0, 3)"
                    :key="menu.menu_id"
                    class="menu-badge"
                  >
                    {{ menu.menu_name }}
                  </span>
                  <span v-if="tag.menus.length > 3" class="more-menus">
                    +{{ tag.menus.length - 3 }}件
                  </span>
                </div>
              </div>
              
              <div class="tag-meta">
                <span class="created-at">
                  <Icon name="mdi:calendar" size="14" />
                  {{ formatDate(tag.created_at) }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- タグ作成モーダル -->
    <div v-if="showCreateModal" class="modal-overlay" @click="closeCreateModal">
      <div class="modal-container" @click.stop>
        <div class="modal-header">
          <h3>新規タグ追加</h3>
          <button @click="closeCreateModal" class="close-btn">
            <Icon name="mdi:close" size="20" />
          </button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="createTag">
            <div class="form-group">
              <label class="form-label">タグID <span class="required">*</span></label>
              <input
                v-model="newTag.tagId"
                type="text"
                class="form-input"
                required
                maxlength="64"
                placeholder="英数字でIDを入力"
              />
            </div>
            
            <div class="form-group">
              <label class="form-label">タグ名 <span class="required">*</span></label>
              <input
                v-model="newTag.tagName"
                type="text"
                class="form-input"
                required
                maxlength="100"
                placeholder="タグ名を入力"
              />
            </div>
            
            <div class="form-group">
              <label class="form-label">英語名</label>
              <input
                v-model="newTag.englishName"
                type="text"
                class="form-input"
                maxlength="100"
                placeholder="English name (optional)"
              />
            </div>
            
            <div v-if="createError" class="error-message">
              {{ createError }}
            </div>
            
            <div class="modal-actions">
              <button
                type="button"
                @click="closeCreateModal"
                class="btn btn-outline"
                :disabled="creating"
              >
                キャンセル
              </button>
              <button
                type="submit"
                class="btn btn-primary"
                :disabled="creating"
              >
                <Icon v-if="creating" name="mdi:loading" size="16" class="spin" />
                {{ creating ? '作成中...' : '作成' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- タグ使用メニューモーダル -->
    <div v-if="showMenusModal" class="modal-overlay" @click="closeMenusModal">
      <div class="modal-container" @click.stop>
        <div class="modal-header">
          <h3>「{{ viewingTag.tag_name }}」使用中のメニュー</h3>
          <button @click="closeMenusModal" class="close-btn">
            <Icon name="mdi:close" size="20" />
          </button>
        </div>
        <div class="modal-body">
          <div v-if="viewingTag.menus && viewingTag.menus.length > 0" class="menus-list-detail">
            <div
              v-for="menu in viewingTag.menus"
              :key="menu.menu_id"
              class="menu-item"
            >
              <div class="menu-info">
                <h4>{{ menu.menu_name }}</h4>
                <p class="menu-id">ID: {{ menu.menu_id }}</p>
              </div>
              <div class="menu-actions">
                <NuxtLink :to="`/admin/menus?highlight=${menu.menu_id}`" class="btn btn-sm btn-outline">
                  <Icon name="mdi:open-in-new" size="14" />
                  メニュー管理で表示
                </NuxtLink>
              </div>
            </div>
          </div>
          <div v-else class="no-menus">
            <Icon name="mdi:information" size="32" />
            <p>このタグはどのメニューでも使用されていません。</p>
          </div>
          
          <div class="modal-actions">
            <button @click="closeMenusModal" class="btn btn-outline">
              閉じる
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuthStore } from '~/stores/auth'
import { apiClient } from '~/utils/api-client'

// 管理者権限が必要なページ
definePageMeta({
  middleware: 'auth'
})

const authStore = useAuthStore()

// 状態管理
const loading = ref(false)
const error = ref('')
const tags = ref([])

// モーダル状態
const showCreateModal = ref(false)
const showMenusModal = ref(false)

// タグ作成
const newTag = ref({
  tagId: '',
  tagName: '',
  englishName: ''
})
const creating = ref(false)
const createError = ref('')

// タグ使用メニュー表示
const viewingTag = ref({})

// タグ一覧の読み込み
const loadTags = async () => {
  loading.value = true
  error.value = ''
  
  try {
    const response = await apiClient.get('/api/admin/tags')
    tags.value = response.tags || []
  } catch (err) {
    console.error('タグ読み込みエラー:', err)
    error.value = err.response?.data?.userMessage || 'タグの読み込みに失敗しました'
  } finally {
    loading.value = false
  }
}

// タグ作成モーダル
const closeCreateModal = () => {
  showCreateModal.value = false
  newTag.value = {
    tagId: '',
    tagName: '',
    englishName: ''
  }
  createError.value = ''
}

const createTag = async () => {
  creating.value = true
  createError.value = ''
  
  try {
    const response = await apiClient.post('/api/admin/tags', {
      tagId: newTag.value.tagId,
      tagName: newTag.value.tagName,
      englishName: newTag.value.englishName || null
    })
    
    await loadTags() // タグリストを再読み込み
    closeCreateModal()
  } catch (err) {
    console.error('タグ作成エラー:', err)
    createError.value = err.response?.data?.userMessage || 'タグの作成に失敗しました'
  } finally {
    creating.value = false
  }
}

// タグ使用メニュー表示モーダル
const viewTagMenus = (tag) => {
  viewingTag.value = tag
  showMenusModal.value = true
}

const closeMenusModal = () => {
  showMenusModal.value = false
  viewingTag.value = {}
}

// 日付フォーマット
const formatDate = (dateString) => {
  if (!dateString) return '不明'
  const date = new Date(dateString)
  return date.toLocaleDateString('ja-JP', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// 初期化
onMounted(() => {
  if (authStore.isAdmin) {
    loadTags()
  }
})

// SEO設定
useHead({
  title: 'タグ管理 - 管理者',
  meta: [
    { name: 'description', content: 'トレーニングメニューのタグ管理画面。タグの追加と使用状況を確認します。' },
    { name: 'robots', content: 'noindex, nofollow' }
  ]
})
</script>

<style scoped>
.admin-tags {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

/* アクセス拒否画面 */
.access-denied {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 60vh;
}

.access-denied-content {
  text-align: center;
  padding: 40px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.access-denied-content .error-icon {
  color: #f59e0b;
  margin-bottom: 20px;
}

.access-denied-content h2 {
  color: #374151;
  margin-bottom: 16px;
}

.access-denied-content p {
  color: #6b7280;
  margin-bottom: 24px;
}

/* ページヘッダー */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 30px;
  padding: 30px;
  background: linear-gradient(135deg, #8b5cf6 0%, #6366f1 100%);
  color: white;
  border-radius: 12px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
}

.header-content h1 {
  display: flex;
  align-items: center;
  gap: 12px;
  margin: 0 0 8px 0;
  font-size: 2rem;
}

.header-content p {
  margin: 0;
  opacity: 0.9;
}

/* タグセクション */
.tags-section {
  background: white;
  border-radius: 12px;
  padding: 30px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.loading-container,
.error-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  text-align: center;
}

.loading-spinner {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
}

.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.error-container .error-icon {
  color: #ef4444;
  margin-bottom: 16px;
}

/* タググリッド */
.tags-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 20px;
}

.tag-card {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 20px;
  transition: all 0.2s ease;
}

.tag-card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  border-color: #cbd5e1;
}

.tag-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 16px;
}

.tag-header h3 {
  margin: 0;
  color: #1f2937;
  font-size: 1.25rem;
  font-weight: 600;
}

.tag-actions {
  display: flex;
  gap: 8px;
}

.tag-details {
  color: #6b7280;
  font-size: 0.875rem;
}

.tag-id {
  font-family: monospace;
  background: #e5e7eb;
  padding: 4px 8px;
  border-radius: 4px;
  display: inline-block;
  margin-bottom: 12px;
}

.tag-usage {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
}

.usage-label {
  font-weight: 500;
  color: #374151;
}

.menu-count {
  background: #dbeafe;
  color: #1e40af;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 500;
}

.associated-menus {
  margin-bottom: 12px;
}

.menus-list {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.menu-badge {
  background: #f3e8ff;
  color: #6b21a8;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 500;
}

.more-menus {
  color: #9ca3af;
  font-size: 0.75rem;
  font-style: italic;
}

.tag-meta {
  display: flex;
  align-items: center;
  gap: 8px;
}

.created-at {
  display: flex;
  align-items: center;
  gap: 4px;
}

/* メニュー詳細リスト */
.menus-list-detail {
  max-height: 400px;
  overflow-y: auto;
}

.menu-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  margin-bottom: 12px;
  background: #f9fafb;
}

.menu-info h4 {
  margin: 0 0 4px 0;
  color: #1f2937;
  font-size: 1rem;
}

.menu-id {
  font-family: monospace;
  color: #6b7280;
  font-size: 0.875rem;
  margin: 0;
}

.no-menus {
  text-align: center;
  padding: 40px 20px;
  color: #6b7280;
}

.no-menus p {
  margin: 16px 0 0 0;
}

/* モーダル */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 20px;
}

.modal-container {
  background: white;
  border-radius: 12px;
  max-width: 500px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px 24px 16px;
  border-bottom: 1px solid #e5e7eb;
}

.modal-header h3 {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 600;
  color: #1f2937;
}

.close-btn {
  background: none;
  border: none;
  color: #6b7280;
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  transition: all 0.2s;
}

.close-btn:hover {
  background: #f3f4f6;
  color: #374151;
}

.modal-body {
  padding: 24px;
}

/* フォーム */
.form-group {
  margin-bottom: 20px;
}

.form-label {
  display: block;
  font-weight: 500;
  color: #374151;
  margin-bottom: 6px;
  font-size: 0.875rem;
}

.required {
  color: #ef4444;
}

.form-input {
  width: 100%;
  padding: 12px 16px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 1rem;
  transition: all 0.2s;
}

.form-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.form-input:disabled {
  background: #f9fafb;
  color: #6b7280;
  cursor: not-allowed;
}

/* エラーメッセージ */
.error-message {
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 8px;
  padding: 12px;
  color: #dc2626;
  font-size: 0.875rem;
  margin-bottom: 16px;
}

/* ボタン */
.btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  border: none;
  border-radius: 6px;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  text-decoration: none;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-primary {
  background: #3b82f6;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #2563eb;
}

.btn-outline {
  background: white;
  border: 1px solid #d1d5db;
  color: #374151;
}

.btn-outline:hover:not(:disabled) {
  background: #f9fafb;
  border-color: #9ca3af;
}

.btn-sm {
  padding: 6px 12px;
  font-size: 0.75rem;
}

.modal-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  margin-top: 24px;
}

/* レスポンシブ */
@media (max-width: 768px) {
  .admin-tags {
    padding: 10px;
  }
  
  .page-header {
    flex-direction: column;
    gap: 20px;
    text-align: center;
  }
  
  .tags-grid {
    grid-template-columns: 1fr;
  }
  
  .tag-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }
  
  .menu-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }
  
  .modal-actions {
    flex-direction: column;
  }
  
  .modal-container {
    margin: 10px;
    max-width: none;
  }
}
</style>