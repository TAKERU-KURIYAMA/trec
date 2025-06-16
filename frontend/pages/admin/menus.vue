<template>
  <div class="admin-menus">
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

    <!-- 管理者向けメニュー管理画面 -->
    <div v-else class="admin-content">
      <!-- ページヘッダー -->
      <div class="page-header">
        <div class="header-content">
          <h1>
            <Icon name="mdi:cog" size="32" />
            メニュー管理
          </h1>
          <p>トレーニングメニューの追加・編集・削除を行います</p>
        </div>
        <div class="header-actions">
          <button @click="showCreateModal = true" class="btn btn-primary">
            <Icon name="mdi:plus" />
            新規メニュー追加
          </button>
        </div>
      </div>

      <!-- メニューリスト -->
      <div class="menus-section">
        <div v-if="loading" class="loading-container">
          <div class="loading-spinner">
            <Icon name="mdi:loading" size="32" class="spin" />
            <p>メニューを読み込み中...</p>
          </div>
        </div>

        <div v-else-if="error" class="error-container">
          <Icon name="mdi:alert-circle" size="32" class="error-icon" />
          <p>{{ error }}</p>
          <button @click="loadMenus" class="btn btn-outline">
            <Icon name="mdi:refresh" />
            再読み込み
          </button>
        </div>

        <div v-else class="menus-groups">
          <div
            v-for="(group, groupKey) in menuGroups"
            :key="groupKey"
            class="menu-group"
          >
            <div class="group-header">
              <Icon :name="group.icon" size="24" />
              <h3>{{ group.name }}</h3>
              <span class="group-count">{{ group.menus.length }}個</span>
            </div>
            
            <div class="menus-grid">
              <div
                v-for="menu in group.menus"
                :key="menu.menu_id"
                class="menu-card"
              >
            <div class="menu-header">
              <h3>{{ menu.menu_name }}</h3>
              <div class="menu-actions">
                <button
                  @click="editMenuTags(menu)"
                  class="btn btn-sm btn-outline"
                  title="タグ編集"
                >
                  <Icon name="mdi:tag" size="16" />
                </button>
                <button
                  @click="editMenu(menu)"
                  class="btn btn-sm btn-outline"
                  title="編集"
                >
                  <Icon name="mdi:pencil" size="16" />
                </button>
                <button
                  @click="deleteMenu(menu)"
                  class="btn btn-sm btn-danger"
                  title="削除"
                >
                  <Icon name="mdi:delete" size="16" />
                </button>
              </div>
            </div>
            
            <div class="menu-details">
              <p class="menu-id">ID: {{ menu.menu_id }}</p>
              <p v-if="menu.description" class="menu-description">
                {{ menu.description }}
              </p>
              
              <div class="menu-tags">
                <span class="tags-label">タグ:</span>
                <span
                  v-for="tag in menu.tags"
                  :key="tag.tag_id"
                  class="tag-badge"
                >
                  {{ tag.tag_name }}
                </span>
                <span v-if="!menu.tags || menu.tags.length === 0" class="no-tags">
                  タグなし
                </span>
              </div>
              
              <div class="menu-meta">
                <span class="created-at">
                  <Icon name="mdi:calendar" size="14" />
                  {{ formatDate(menu.created_at) }}
                </span>
              </div>
            </div>
          </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- メニュー作成モーダル -->
    <div v-if="showCreateModal" class="modal-overlay" @click="closeCreateModal">
      <div class="modal-container" @click.stop>
        <div class="modal-header">
          <h3>新規メニュー追加</h3>
          <button @click="closeCreateModal" class="close-btn">
            <Icon name="mdi:close" size="20" />
          </button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="createMenu">
            <div class="form-group">
              <label class="form-label">メニューID <span class="required">*</span></label>
              <input
                v-model="newMenu.menuId"
                type="text"
                class="form-input"
                required
                maxlength="64"
                placeholder="英数字でIDを入力"
              />
            </div>
            
            <div class="form-group">
              <label class="form-label">メニュー名 <span class="required">*</span></label>
              <input
                v-model="newMenu.menuName"
                type="text"
                class="form-input"
                required
                maxlength="100"
                placeholder="メニュー名を入力"
              />
            </div>
            
            <div class="form-group">
              <label class="form-label">英語名</label>
              <input
                v-model="newMenu.englishName"
                type="text"
                class="form-input"
                maxlength="100"
                placeholder="English name (optional)"
              />
            </div>
            
            <div class="form-group">
              <label class="form-label">説明</label>
              <textarea
                v-model="newMenu.description"
                class="form-input"
                rows="3"
                maxlength="500"
                placeholder="メニューの説明を入力"
              ></textarea>
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

    <!-- メニュー編集モーダル -->
    <div v-if="showEditModal" class="modal-overlay" @click="closeEditModal">
      <div class="modal-container" @click.stop>
        <div class="modal-header">
          <h3>メニュー編集</h3>
          <button @click="closeEditModal" class="close-btn">
            <Icon name="mdi:close" size="20" />
          </button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="updateMenu">
            <div class="form-group">
              <label class="form-label">メニューID</label>
              <input
                :value="editingMenu.menu_id"
                type="text"
                class="form-input"
                disabled
              />
            </div>
            
            <div class="form-group">
              <label class="form-label">メニュー名 <span class="required">*</span></label>
              <input
                v-model="editForm.menuName"
                type="text"
                class="form-input"
                required
                maxlength="100"
                placeholder="メニュー名を入力"
              />
            </div>
            
            <div class="form-group">
              <label class="form-label">英語名</label>
              <input
                v-model="editForm.englishName"
                type="text"
                class="form-input"
                maxlength="100"
                placeholder="English name (optional)"
              />
            </div>
            
            <div class="form-group">
              <label class="form-label">説明</label>
              <textarea
                v-model="editForm.description"
                class="form-input"
                rows="3"
                maxlength="500"
                placeholder="メニューの説明を入力"
              ></textarea>
            </div>
            
            <div v-if="editError" class="error-message">
              {{ editError }}
            </div>
            
            <div class="modal-actions">
              <button
                type="button"
                @click="closeEditModal"
                class="btn btn-outline"
                :disabled="updating"
              >
                キャンセル
              </button>
              <button
                type="submit"
                class="btn btn-primary"
                :disabled="updating"
              >
                <Icon v-if="updating" name="mdi:loading" size="16" class="spin" />
                {{ updating ? '更新中...' : '更新' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- タグ編集モーダル -->
    <div v-if="showTagsModal" class="modal-overlay" @click="closeTagsModal">
      <div class="modal-container" @click.stop>
        <div class="modal-header">
          <h3>「{{ editingMenuForTags.menu_name }}」のタグ編集</h3>
          <button @click="closeTagsModal" class="close-btn">
            <Icon name="mdi:close" size="20" />
          </button>
        </div>
        <div class="modal-body">
          <div v-if="loadingTags" class="loading-container">
            <Icon name="mdi:loading" size="24" class="spin" />
            <p>タグ一覧を読み込み中...</p>
          </div>
          
          <form v-else @submit.prevent="updateMenuTags">
            <div class="form-group">
              <label class="form-label">利用可能なタグ</label>
              <div class="tags-selection">
                <div
                  v-for="tag in availableTags"
                  :key="tag.tag_id"
                  class="tag-option"
                >
                  <label class="tag-checkbox">
                    <input
                      type="checkbox"
                      :value="tag.tag_id"
                      v-model="selectedTagIds"
                      :disabled="updatingTags"
                    />
                    <span class="checkmark"></span>
                    <span class="tag-name">{{ tag.tag_name }}</span>
                    <span class="tag-id-small">{{ tag.tag_id }}</span>
                  </label>
                </div>
              </div>
              
              <div v-if="availableTags.length === 0" class="no-tags-available">
                <Icon name="mdi:information" size="24" />
                <p>利用可能なタグがありません。</p>
                <NuxtLink to="/admin/tags" class="btn btn-sm btn-outline">
                  <Icon name="mdi:tag-plus" size="16" />
                  タグを追加
                </NuxtLink>
              </div>
            </div>
            
            <div class="current-selection">
              <h4>選択中のタグ ({{ selectedTagIds.length }}個)</h4>
              <div class="selected-tags">
                <span
                  v-for="tagId in selectedTagIds"
                  :key="tagId"
                  class="selected-tag-badge"
                >
                  {{ getTagName(tagId) }}
                </span>
                <span v-if="selectedTagIds.length === 0" class="no-selection">
                  タグが選択されていません
                </span>
              </div>
            </div>
            
            <div v-if="tagsError" class="error-message">
              {{ tagsError }}
            </div>
            
            <div class="modal-actions">
              <button
                type="button"
                @click="closeTagsModal"
                class="btn btn-outline"
                :disabled="updatingTags"
              >
                キャンセル
              </button>
              <button
                type="submit"
                class="btn btn-primary"
                :disabled="updatingTags"
              >
                <Icon v-if="updatingTags" name="mdi:loading" size="16" class="spin" />
                {{ updatingTags ? '更新中...' : 'タグを更新' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- 削除確認モーダル -->
    <div v-if="showDeleteModal" class="modal-overlay" @click="closeDeleteModal">
      <div class="modal-container" @click.stop>
        <div class="modal-header">
          <h3>メニュー削除確認</h3>
          <button @click="closeDeleteModal" class="close-btn">
            <Icon name="mdi:close" size="20" />
          </button>
        </div>
        <div class="modal-body">
          <div class="delete-warning">
            <Icon name="mdi:alert-circle" size="48" class="warning-icon" />
            <h4>本当に削除しますか？</h4>
            <p>
              「{{ deletingMenu.menu_name }}」を削除します。<br>
              この操作は取り消せません。
            </p>
          </div>
          
          <div v-if="deleteError" class="error-message">
            {{ deleteError }}
          </div>
          
          <div class="modal-actions">
            <button
              @click="closeDeleteModal"
              class="btn btn-outline"
              :disabled="deleting"
            >
              キャンセル
            </button>
            <button
              @click="confirmDelete"
              class="btn btn-danger"
              :disabled="deleting"
            >
              <Icon v-if="deleting" name="mdi:loading" size="16" class="spin" />
              {{ deleting ? '削除中...' : '削除' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useAuthStore } from '~/stores/auth'
import { apiClient } from '~/utils/api-client'

// 管理者権限が必要なページ
definePageMeta({
  middleware: 'admin'
})

const authStore = useAuthStore()

// 状態管理
const loading = ref(false)
const error = ref('')
const menus = ref([])

// 部位ごとのグループ分け
const menuGroups = computed(() => {
  const groups = {
    chest: { name: '胸', icon: 'mdi:arm-flex', menus: [] },
    back: { name: '背中', icon: 'mdi:human-handsup', menus: [] },
    shoulders: { name: '肩', icon: 'mdi:weight-lifter', menus: [] },
    arms: { name: '腕', icon: 'mdi:arm-flex-outline', menus: [] },
    legs: { name: '脚', icon: 'mdi:run', menus: [] },
    abs: { name: '腹筋', icon: 'mdi:ab-testing', menus: [] },
    others: { name: 'その他', icon: 'mdi:dots-horizontal', menus: [] }
  }
  
  menus.value.forEach(menu => {
    const menuId = menu.menu_id.toLowerCase()
    const menuName = menu.menu_name.toLowerCase()
    
    // メニューIDやメニュー名から部位を判定
    if (menuId.includes('chest') || menuId.includes('bench') || menuName.includes('胸') || menuName.includes('ベンチ')) {
      groups.chest.menus.push(menu)
    } else if (menuId.includes('back') || menuId.includes('lat') || menuId.includes('row') || menuName.includes('背') || menuName.includes('ラット')) {
      groups.back.menus.push(menu)
    } else if (menuId.includes('shoulder') || menuId.includes('delt') || menuName.includes('肩') || menuName.includes('ショルダー')) {
      groups.shoulders.menus.push(menu)
    } else if (menuId.includes('arm') || menuId.includes('bicep') || menuId.includes('tricep') || menuId.includes('curl') || menuName.includes('腕') || menuName.includes('カール')) {
      groups.arms.menus.push(menu)
    } else if (menuId.includes('leg') || menuId.includes('squat') || menuId.includes('calf') || menuName.includes('脚') || menuName.includes('スクワット')) {
      groups.legs.menus.push(menu)
    } else if (menuId.includes('ab') || menuId.includes('core') || menuName.includes('腹') || menuName.includes('コア')) {
      groups.abs.menus.push(menu)
    } else {
      groups.others.menus.push(menu)
    }
  })
  
  // 空のグループを除外
  return Object.entries(groups)
    .filter(([key, group]) => group.menus.length > 0)
    .reduce((acc, [key, group]) => {
      acc[key] = group
      return acc
    }, {})
})

// モーダル状態
const showCreateModal = ref(false)
const showEditModal = ref(false)
const showDeleteModal = ref(false)
const showTagsModal = ref(false)

// メニュー作成
const newMenu = ref({
  menuId: '',
  menuName: '',
  englishName: '',
  description: ''
})
const creating = ref(false)
const createError = ref('')

// メニュー編集
const editingMenu = ref({})
const editForm = ref({
  menuName: '',
  englishName: '',
  description: ''
})
const updating = ref(false)
const editError = ref('')

// メニュー削除
const deletingMenu = ref({})
const deleting = ref(false)
const deleteError = ref('')

// タグ編集
const editingMenuForTags = ref({})
const availableTags = ref([])
const selectedTagIds = ref([])
const loadingTags = ref(false)
const updatingTags = ref(false)
const tagsError = ref('')

// メニュー一覧の読み込み
const loadMenus = async () => {
  loading.value = true
  error.value = ''
  
  try {
    const response = await apiClient.get('/admin/menus')
    menus.value = response.menus || []
  } catch (err) {
    console.error('メニュー読み込みエラー:', err)
    error.value = err.response?.data?.userMessage || 'メニューの読み込みに失敗しました'
  } finally {
    loading.value = false
  }
}

// メニュー作成モーダル
const closeCreateModal = () => {
  showCreateModal.value = false
  newMenu.value = {
    menuId: '',
    menuName: '',
    englishName: '',
    description: ''
  }
  createError.value = ''
}

const createMenu = async () => {
  creating.value = true
  createError.value = ''
  
  try {
    const response = await apiClient.post('/admin/menus', {
      menuId: newMenu.value.menuId,
      menuName: newMenu.value.menuName,
      englishName: newMenu.value.englishName || null,
      description: newMenu.value.description || null
    })
    
    // キャッシュを無効化してから再読み込み
    apiClient.invalidateCache('/admin/menus')
    
    closeCreateModal()
    console.log('メニュー作成成功:', response)
    await loadMenus() // メニューリストを再読み込み
  } catch (err) {
    console.error('メニュー作成エラー:', err)
    createError.value = err.response?.data?.userMessage || 'メニューの作成に失敗しました'
  } finally {
    creating.value = false
  }
}

// メニュー編集モーダル
const editMenu = (menu) => {
  editingMenu.value = menu
  editForm.value = {
    menuName: menu.menu_name,
    englishName: menu.english_name || '',
    description: menu.description || ''
  }
  showEditModal.value = true
}

const closeEditModal = () => {
  showEditModal.value = false
  editingMenu.value = {}
  editForm.value = {
    menuName: '',
    englishName: '',
    description: ''
  }
  editError.value = ''
}

const updateMenu = async () => {
  updating.value = true
  editError.value = ''
  
  try {
    const response = await apiClient.put(`/admin/menus/${editingMenu.value.menu_id}`, {
      menuName: editForm.value.menuName,
      englishName: editForm.value.englishName || null,
      description: editForm.value.description || null
    })
    
    // キャッシュを無効化してから再読み込み
    apiClient.invalidateCache('/admin/menus')
    
    await loadMenus() // メニューリストを再読み込み
    closeEditModal()
  } catch (err) {
    console.error('メニュー更新エラー:', err)
    editError.value = err.response?.data?.userMessage || 'メニューの更新に失敗しました'
  } finally {
    updating.value = false
  }
}

// メニュー削除モーダル
const deleteMenu = (menu) => {
  deletingMenu.value = menu
  showDeleteModal.value = true
}

const closeDeleteModal = () => {
  showDeleteModal.value = false
  deletingMenu.value = {}
  deleteError.value = ''
}

const confirmDelete = async () => {
  deleting.value = true
  deleteError.value = ''
  
  try {
    await apiClient.delete(`/admin/menus/${deletingMenu.value.menu_id}`)
    
    // キャッシュを無効化してから再読み込み
    apiClient.invalidateCache('/admin/menus')
    
    await loadMenus() // メニューリストを再読み込み
    closeDeleteModal()
  } catch (err) {
    console.error('メニュー削除エラー:', err)
    deleteError.value = err.response?.data?.userMessage || 'メニューの削除に失敗しました'
  } finally {
    deleting.value = false
  }
}

// タグ編集モーダル
const editMenuTags = async (menu) => {
  editingMenuForTags.value = menu
  selectedTagIds.value = menu.tags ? menu.tags.map(tag => tag.tag_id) : []
  showTagsModal.value = true
  await loadAvailableTags()
}

const closeTagsModal = () => {
  showTagsModal.value = false
  editingMenuForTags.value = {}
  selectedTagIds.value = []
  availableTags.value = []
  tagsError.value = ''
}

const loadAvailableTags = async () => {
  loadingTags.value = true
  tagsError.value = ''
  
  try {
    const response = await apiClient.get('/admin/tags')
    availableTags.value = response.tags || []
  } catch (err) {
    console.error('タグ読み込みエラー:', err)
    tagsError.value = err.response?.data?.userMessage || 'タグの読み込みに失敗しました'
  } finally {
    loadingTags.value = false
  }
}

const updateMenuTags = async () => {
  updatingTags.value = true
  tagsError.value = ''
  
  try {
    await apiClient.put(`/admin/menus/${editingMenuForTags.value.menu_id}/tags`, {
      tagIds: selectedTagIds.value
    })
    
    // キャッシュを無効化してから再読み込み
    apiClient.invalidateCache('/admin/menus')
    
    await loadMenus() // メニューリストを再読み込み
    closeTagsModal()
  } catch (err) {
    console.error('タグ更新エラー:', err)
    tagsError.value = err.response?.data?.userMessage || 'タグの更新に失敗しました'
  } finally {
    updatingTags.value = false
  }
}

const getTagName = (tagId) => {
  const tag = availableTags.value.find(t => t.tag_id === tagId)
  return tag ? tag.tag_name : tagId
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
    loadMenus()
  }
})

// SEO設定
useHead({
  title: 'メニュー管理 - 管理者',
  meta: [
    { name: 'description', content: 'トレーニングメニューの管理画面。メニューの追加・編集・削除を行います。' },
    { name: 'robots', content: 'noindex, nofollow' }
  ]
})
</script>

<style scoped>
.admin-menus {
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
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
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

/* メニューセクション */
.menus-section {
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

/* メニューグループ */
.menus-groups {
  display: flex;
  flex-direction: column;
  gap: 30px;
}

.menu-group {
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  overflow: hidden;
  background: #fafafa;
}

.group-header {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 20px 24px;
  background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
  border-bottom: 1px solid #e5e7eb;
}

.group-header h3 {
  margin: 0;
  color: #1f2937;
  font-size: 1.25rem;
  font-weight: 600;
  flex: 1;
}

.group-count {
  background: #3b82f6;
  color: white;
  padding: 4px 12px;
  border-radius: 16px;
  font-size: 0.75rem;
  font-weight: 500;
}

/* メニューグリッド */
.menus-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 20px;
  padding: 20px;
  background: white;
}

.menu-card {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 20px;
  transition: all 0.2s ease;
}

.menu-card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  border-color: #cbd5e1;
}

.menu-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 16px;
}

.menu-header h3 {
  margin: 0;
  color: #1f2937;
  font-size: 1.25rem;
  font-weight: 600;
}

.menu-actions {
  display: flex;
  gap: 8px;
}

.menu-details {
  color: #6b7280;
  font-size: 0.875rem;
}

.menu-id {
  font-family: monospace;
  background: #e5e7eb;
  padding: 4px 8px;
  border-radius: 4px;
  display: inline-block;
  margin-bottom: 8px;
}

.menu-description {
  margin-bottom: 12px;
  line-height: 1.5;
}

.menu-tags {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 4px;
  margin-bottom: 12px;
}

.tags-label {
  font-weight: 500;
  color: #374151;
}

.tag-badge {
  background: #ddd6fe;
  color: #5b21b6;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 500;
}

.no-tags {
  color: #9ca3af;
  font-style: italic;
}

.menu-meta {
  display: flex;
  align-items: center;
  gap: 8px;
}

.created-at {
  display: flex;
  align-items: center;
  gap: 4px;
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

.form-input textarea {
  resize: vertical;
  min-height: 80px;
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

/* 削除警告 */
.delete-warning {
  text-align: center;
  padding: 20px;
}

.warning-icon {
  color: #f59e0b;
  margin-bottom: 16px;
}

.delete-warning h4 {
  margin: 0 0 12px 0;
  color: #1f2937;
}

.delete-warning p {
  color: #6b7280;
  line-height: 1.5;
  margin: 0;
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

.btn-danger {
  background: #ef4444;
  color: white;
}

.btn-danger:hover:not(:disabled) {
  background: #dc2626;
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

/* タグ選択 */
.tags-selection {
  max-height: 300px;
  overflow-y: auto;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 16px;
  background: #f9fafb;
}

.tag-option {
  margin-bottom: 12px;
}

.tag-checkbox {
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
  padding: 8px;
  border-radius: 6px;
  transition: all 0.2s;
}

.tag-checkbox:hover {
  background: #f3f4f6;
}

.tag-checkbox input[type="checkbox"] {
  display: none;
}

.checkmark {
  width: 20px;
  height: 20px;
  border: 2px solid #d1d5db;
  border-radius: 4px;
  position: relative;
  background: white;
  transition: all 0.2s;
}

.tag-checkbox input[type="checkbox"]:checked + .checkmark {
  background: #3b82f6;
  border-color: #3b82f6;
}

.tag-checkbox input[type="checkbox"]:checked + .checkmark::after {
  content: '';
  position: absolute;
  left: 6px;
  top: 2px;
  width: 6px;
  height: 10px;
  border: solid white;
  border-width: 0 2px 2px 0;
  transform: rotate(45deg);
}

.tag-name {
  font-weight: 500;
  color: #374151;
}

.tag-id-small {
  font-family: monospace;
  font-size: 0.75rem;
  color: #6b7280;
  background: #e5e7eb;
  padding: 2px 6px;
  border-radius: 4px;
}

.no-tags-available {
  text-align: center;
  padding: 40px 20px;
  color: #6b7280;
}

.no-tags-available p {
  margin: 16px 0;
}

.current-selection {
  margin-top: 24px;
  padding: 16px;
  background: #f8fafc;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
}

.current-selection h4 {
  margin: 0 0 12px 0;
  color: #374151;
  font-size: 1rem;
}

.selected-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.selected-tag-badge {
  background: #dbeafe;
  color: #1e40af;
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 0.875rem;
  font-weight: 500;
}

.no-selection {
  color: #9ca3af;
  font-style: italic;
  font-size: 0.875rem;
}

/* レスポンシブ */
@media (max-width: 768px) {
  .admin-menus {
    padding: 10px;
  }
  
  .page-header {
    flex-direction: column;
    gap: 20px;
    text-align: center;
  }
  
  .menus-grid {
    grid-template-columns: 1fr;
  }
  
  .menu-header {
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