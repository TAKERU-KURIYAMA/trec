<template>
  <div class="training-presets">
    <!-- Header -->
    <div class="page-header">
      <div class="header-content">
        <h1>マイセット</h1>
        <p>よく使うトレーニングセットを保存・管理できます</p>
      </div>
      <div class="header-actions">
        <button @click="exportPresets" class="btn btn-outline">
          <Icon name="mdi:download" />
          データエクスポート
        </button>
        <button @click="showCreateModal = true" class="btn btn-primary">
          <Icon name="mdi:plus" />
          新しいマイセット
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading.isLoading" class="loading-state">
      <div class="loading-spinner">⚡</div>
      <p>{{ loading.message || 'マイセットを読み込み中...' }}</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error.hasError" class="error-state">
      <div class="error-icon">⚠️</div>
      <h3>エラーが発生しました</h3>
      <p>{{ error.message }}</p>
      <button @click="loadPresets" class="btn btn-primary">再試行</button>
    </div>

    <!-- Presets Content -->
    <div v-else class="presets-content">
      <!-- Empty State -->
      <div v-if="presets.length === 0" class="empty-state">
        <div class="empty-icon">📋</div>
        <h3>まだマイセットがありません</h3>
        <p>よく使うトレーニングセットを保存して、素早くワークアウトを開始しましょう！</p>
        <button @click="showCreateModal = true" class="btn btn-primary">
          <Icon name="mdi:plus" />
          最初のマイセットを作成
        </button>
      </div>

      <!-- Presets Grid -->
      <div v-else class="presets-grid">
        <div 
          v-for="preset in presets" 
          :key="preset.presetId"
          class="preset-card"
          :class="{ 'is-default': preset.isDefault }"
        >
          <!-- Preset Header -->
          <div class="preset-header">
            <div class="preset-info">
              <h3>{{ preset.name }}</h3>
              <p class="preset-menu">{{ preset.menuName }}</p>
              <p v-if="preset.description" class="preset-description">{{ preset.description }}</p>
            </div>
            <div class="preset-badge" v-if="preset.isDefault">
              <Icon name="mdi:star" />
              標準
            </div>
          </div>

          <!-- Preset Sets Preview -->
          <div class="preset-sets">
            <div class="sets-header">
              <h4>セット構成 ({{ preset.defaultSets.length }}セット)</h4>
            </div>
            <div class="sets-preview">
              <div 
                v-for="set in preset.defaultSets.slice(0, 3)" 
                :key="set.setNumber"
                class="set-preview"
              >
                <span class="set-number">{{ set.setNumber }}</span>
                <span class="set-details">{{ set.reps }}回</span>
                <span v-if="set.weight" class="set-weight">{{ set.weight }}kg</span>
              </div>
              <div v-if="preset.defaultSets.length > 3" class="more-sets">
                +{{ preset.defaultSets.length - 3 }}セット
              </div>
            </div>
          </div>

          <!-- Preset Actions -->
          <div class="preset-actions">
            <button 
              @click="startWorkoutFromPreset(preset)" 
              class="btn btn-primary btn-block"
            >
              <Icon name="mdi:play" />
              このセットで開始
            </button>
            <div class="secondary-actions">
              <button @click="viewPresetDetails(preset)" class="btn btn-outline btn-sm">
                <Icon name="mdi:eye" />
                詳細
              </button>
              <button @click="editPreset(preset)" class="btn btn-outline btn-sm">
                <Icon name="mdi:pencil" />
                編集
              </button>
              <button 
                v-if="!preset.isDefault"
                @click="deletePreset(preset)" 
                class="btn btn-danger btn-sm"
              >
                <Icon name="mdi:delete" />
                削除
              </button>
            </div>
          </div>

          <!-- Created Date -->
          <div class="preset-meta">
            <span class="created-date">{{ formatDate(preset.createdAt) }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Create/Edit Preset Modal -->
    <div v-if="showCreateModal || showEditModal" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>
            <Icon :name="showEditModal ? 'mdi:pencil' : 'mdi:plus'" />
            {{ showEditModal ? 'マイセット編集' : '新しいマイセット' }}
          </h2>
          <button @click="closeModal" class="modal-close-btn">
            <Icon name="mdi:close" size="24" />
          </button>
        </div>

        <div class="modal-body">
          <!-- Basic Info -->
          <div class="form-section">
            <h3>基本情報</h3>
            <div class="form-group">
              <label for="preset-name">マイセット名</label>
              <input 
                id="preset-name"
                v-model="formData.name" 
                type="text" 
                class="input-field"
                placeholder="例: 胸トレーニング基本"
                required
              />
            </div>
            
            <div class="form-group">
              <label for="preset-description">説明（任意）</label>
              <textarea 
                id="preset-description"
                v-model="formData.description" 
                class="input-field"
                rows="2"
                placeholder="このマイセットの説明"
              ></textarea>
            </div>
            
            <div class="form-group">
              <label for="preset-menu">トレーニングメニュー</label>
              <select 
                id="preset-menu"
                v-model="formData.menuId" 
                class="input-field"
                required
              >
                <option value="">メニューを選択</option>
                <option 
                  v-for="menu in menus" 
                  :key="menu.menuId" 
                  :value="menu.menuId"
                >
                  {{ menu.jpName }}
                </option>
              </select>
            </div>
          </div>

          <!-- Sets Configuration -->
          <div class="form-section">
            <div class="sets-header">
              <h3>セット構成</h3>
              <button @click="addSet" type="button" class="btn btn-secondary btn-sm">
                <Icon name="mdi:plus" />
                セット追加
              </button>
            </div>
            
            <div class="sets-form">
              <div 
                v-for="(set, index) in formData.defaultSets" 
                :key="set.setNumber"
                class="set-form-item"
              >
                <div class="set-number-badge">{{ set.setNumber }}</div>
                <div class="set-inputs">
                  <div class="input-group">
                    <label>回数</label>
                    <input 
                      v-model.number="set.reps" 
                      type="number" 
                      class="input-field small"
                      min="1"
                      max="100"
                      required
                    />
                  </div>
                  <div class="input-group">
                    <label>重量 (kg)</label>
                    <input 
                      v-model.number="set.weight" 
                      type="number" 
                      class="input-field small"
                      min="0"
                      step="0.5"
                      placeholder="任意"
                    />
                  </div>
                  <div class="input-group">
                    <label>メモ</label>
                    <input 
                      v-model="set.note" 
                      type="text" 
                      class="input-field small"
                      placeholder="任意"
                    />
                  </div>
                </div>
                <button 
                  @click="removeSet(index)" 
                  type="button" 
                  class="btn btn-danger btn-sm"
                  :disabled="formData.defaultSets.length <= 1"
                >
                  <Icon name="mdi:delete" size="16" />
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Modal Actions -->
        <div class="modal-actions">
          <button @click="closeModal" class="btn btn-outline">
            キャンセル
          </button>
          <button 
            @click="savePreset" 
            class="btn btn-primary"
            :disabled="!isFormValid || saving"
          >
            <Icon v-if="saving" name="mdi:loading" class="spin" />
            <Icon v-else :name="showEditModal ? 'mdi:content-save' : 'mdi:plus'" />
            {{ saving ? '保存中...' : (showEditModal ? '更新' : '作成') }}
          </button>
        </div>
      </div>
    </div>

    <!-- Detail Modal -->
    <div v-if="showDetailModal" class="modal-overlay" @click="closeDetailModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>
            <Icon name="mdi:clipboard-text" />
            マイセット詳細
          </h2>
          <button @click="closeDetailModal" class="modal-close-btn">
            <Icon name="mdi:close" size="24" />
          </button>
        </div>

        <div v-if="selectedPreset" class="modal-body">
          <!-- Preset Info -->
          <div class="detail-section">
            <h3>{{ selectedPreset.name }}</h3>
            <p class="detail-menu">{{ selectedPreset.menuName }}</p>
            <p v-if="selectedPreset.description" class="detail-description">
              {{ selectedPreset.description }}
            </p>
          </div>

          <!-- All Sets -->
          <div class="detail-section">
            <h4>全セット詳細</h4>
            <div class="detail-sets">
              <div 
                v-for="set in selectedPreset.defaultSets" 
                :key="set.setNumber"
                class="detail-set-item"
              >
                <div class="set-number">{{ set.setNumber }}</div>
                <div class="set-data">
                  <span class="set-reps">{{ set.reps }}回</span>
                  <span v-if="set.weight" class="set-weight">{{ set.weight }}kg</span>
                  <span v-if="set.note" class="set-note">{{ set.note }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Detail Modal Actions -->
        <div class="modal-actions">
          <button @click="startWorkoutFromPreset(selectedPreset)" class="btn btn-primary">
            <Icon name="mdi:play" />
            このセットで開始
          </button>
          <button @click="closeDetailModal" class="btn btn-outline">
            閉じる
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useTrainingMenus } from '~/composables/useTrainingMenus'
import { useApiClient } from '~/utils/api-client'
import { useGlobalNotifications } from '~/composables/useNotifications'

// ===================================
// Types
// ===================================

interface TrainingPreset {
  presetId: string
  name: string
  description?: string
  menuId: string
  menuName: string
  defaultSets: PresetSet[]
  createdAt?: Date
  isDefault: boolean
}

interface PresetSet {
  setNumber: number
  reps: number
  weight?: number
  note?: string
}

// ===================================
// Setup
// ===================================

const router = useRouter()
const { training } = useApiClient()
const notifications = useGlobalNotifications()
const { menus } = useTrainingMenus({ autoLoad: true })

// ===================================
// State
// ===================================

const presets = ref<TrainingPreset[]>([])
const loading = ref({ isLoading: false, message: '' })
const error = ref({ hasError: false, message: '' })
const saving = ref(false)

// Modal states
const showCreateModal = ref(false)
const showEditModal = ref(false)
const showDetailModal = ref(false)
const selectedPreset = ref<TrainingPreset | null>(null)

// Form data
const formData = ref({
  name: '',
  description: '',
  menuId: '',
  defaultSets: [
    { setNumber: 1, reps: 10, weight: undefined, note: '' }
  ] as PresetSet[]
})

// ===================================
// Computed
// ===================================

const isFormValid = computed(() => {
  return formData.value.name.trim() !== '' &&
         formData.value.menuId !== '' &&
         formData.value.defaultSets.length > 0 &&
         formData.value.defaultSets.every(s => s.reps > 0)
})

// ===================================
// Methods
// ===================================

async function loadPresets() {
  try {
    setLoading(true, 'マイセットを読み込み中...')
    clearError()

    const response = await training.get('/presets')
    
    if (response.isSuccess && response.data?.presets) {
      presets.value = response.data.presets
    } else {
      throw new Error(response.userMessage || 'データの取得に失敗しました')
    }

  } catch (err: any) {
    console.error('Presets load error:', err)
    setError('マイセットの取得中にエラーが発生しました: ' + (err.message || 'Unknown error'))
  } finally {
    setLoading(false)
  }
}

async function savePreset() {
  if (!isFormValid.value) return

  try {
    saving.value = true

    const requestData = {
      name: formData.value.name.trim(),
      description: formData.value.description?.trim() || undefined,
      menuId: formData.value.menuId,
      defaultSets: formData.value.defaultSets.map(set => ({
        setNumber: set.setNumber,
        reps: set.reps,
        weight: set.weight || undefined,
        note: set.note?.trim() || undefined
      }))
    }

    const response = await training.post('/presets', requestData)
    
    if (response.isSuccess) {
      notifications.success('保存完了', 'マイセットが正常に保存されました')
      closeModal()
      await loadPresets() // Reload presets
    } else {
      throw new Error(response.userMessage || '保存に失敗しました')
    }

  } catch (err: any) {
    console.error('Preset save error:', err)
    notifications.error('保存エラー', 'マイセットの保存に失敗しました: ' + (err.message || 'Unknown error'))
  } finally {
    saving.value = false
  }
}

function startWorkoutFromPreset(preset: TrainingPreset) {
  // Navigate to training session with preset data
  router.push({
    path: `/training/session/${preset.menuId}`,
    query: { presetId: preset.presetId }
  })
}

function viewPresetDetails(preset: TrainingPreset) {
  selectedPreset.value = preset
  showDetailModal.value = true
}

function editPreset(preset: TrainingPreset) {
  // Populate form with preset data
  formData.value = {
    name: preset.name,
    description: preset.description || '',
    menuId: preset.menuId,
    defaultSets: [...preset.defaultSets]
  }
  showEditModal.value = true
}

function deletePreset(preset: TrainingPreset) {
  if (confirm(`「${preset.name}」を削除しますか？この操作は取り消せません。`)) {
    // TODO: Implement delete functionality
    notifications.info('削除予定', 'プリセット削除機能は実装予定です')
  }
}

function addSet() {
  const newSetNumber = formData.value.defaultSets.length + 1
  formData.value.defaultSets.push({
    setNumber: newSetNumber,
    reps: 10,
    weight: undefined,
    note: ''
  })
}

function removeSet(index: number) {
  if (formData.value.defaultSets.length > 1) {
    formData.value.defaultSets.splice(index, 1)
    // Renumber sets
    formData.value.defaultSets.forEach((set, i) => {
      set.setNumber = i + 1
    })
  }
}

function closeModal() {
  showCreateModal.value = false
  showEditModal.value = false
  resetForm()
}

function closeDetailModal() {
  showDetailModal.value = false
  selectedPreset.value = null
}

function resetForm() {
  formData.value = {
    name: '',
    description: '',
    menuId: '',
    defaultSets: [
      { setNumber: 1, reps: 10, weight: undefined, note: '' }
    ]
  }
}

function formatDate(date?: Date): string {
  if (!date) return ''
  return new Date(date).toLocaleDateString('ja-JP', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

function setLoading(isLoading: boolean, message: string = '') {
  loading.value.isLoading = isLoading
  loading.value.message = message
}

function setError(message: string) {
  error.value.hasError = true
  error.value.message = message
}

function clearError() {
  error.value.hasError = false
  error.value.message = ''
}

// ===================================
// Export Functionality
// ===================================

async function exportPresets() {
  try {
    const exportData = {
      metadata: {
        exportType: 'training_presets',
        exportedAt: new Date().toISOString(),
        exportedBy: 'Message Training App',
        totalPresets: presets.value.length
      },
      menus: menus.value.map(menu => ({
        menuId: menu.menuId,
        jpName: menu.jpName,
        enName: menu.enName
      })),
      presets: presets.value.map(preset => ({
        ...preset,
        statistics: {
          totalSets: preset.defaultSets.length,
          totalReps: preset.defaultSets.reduce((sum, set) => sum + set.reps, 0),
          averageRepsPerSet: preset.defaultSets.length > 0 ? 
            Math.round((preset.defaultSets.reduce((sum, set) => sum + set.reps, 0) / preset.defaultSets.length) * 10) / 10 : 0,
          setsWithWeight: preset.defaultSets.filter(set => set.weight && set.weight > 0).length,
          averageWeight: calculateAverageWeight(preset.defaultSets)
        }
      })),
      summary: {
        totalPresets: presets.value.length,
        defaultPresets: presets.value.filter(p => p.isDefault).length,
        customPresets: presets.value.filter(p => !p.isDefault).length,
        mostUsedMenus: getMostUsedMenus(),
        totalSetsAcrossPresets: presets.value.reduce((sum, preset) => sum + preset.defaultSets.length, 0),
        averageSetsPerPreset: presets.value.length > 0 ? 
          Math.round((presets.value.reduce((sum, preset) => sum + preset.defaultSets.length, 0) / presets.value.length) * 10) / 10 : 0
      }
    }

    const dataStr = JSON.stringify(exportData, null, 2)
    const dataUri = 'data:application/json;charset=utf-8,'+ encodeURIComponent(dataStr)
    
    const exportFileDefaultName = `training_presets_${new Date().toISOString().split('T')[0]}.json`
    
    const linkElement = document.createElement('a')
    linkElement.setAttribute('href', dataUri)
    linkElement.setAttribute('download', exportFileDefaultName)
    linkElement.click()
    
    notifications.success('エクスポート完了', 'マイセットデータをダウンロードしました')
    
  } catch (err) {
    console.error('Export error:', err)
    notifications.error('エクスポートエラー', 'データのエクスポートに失敗しました')
  }
}

function calculateAverageWeight(sets: PresetSet[]): number {
  const setsWithWeight = sets.filter(set => set.weight && set.weight > 0)
  if (setsWithWeight.length === 0) return 0
  
  const totalWeight = setsWithWeight.reduce((sum, set) => sum + (set.weight || 0), 0)
  return Math.round((totalWeight / setsWithWeight.length) * 10) / 10
}

function getMostUsedMenus(): { menuId: string, menuName: string, count: number }[] {
  const menuCounts = presets.value.reduce((acc, preset) => {
    const menuName = preset.menuName || 'Unknown'
    acc[preset.menuId] = (acc[preset.menuId] || 0) + 1
    return acc
  }, {} as Record<string, number>)
  
  return Object.entries(menuCounts)
    .map(([menuId, count]) => ({
      menuId,
      menuName: menus.value.find(m => m.menuId === menuId)?.jpName || menuId,
      count
    }))
    .sort((a, b) => b.count - a.count)
    .slice(0, 5) // Top 5
}

// ===================================
// Lifecycle
// ===================================

onMounted(async () => {
  await loadPresets()
})

// ===================================
// Meta
// ===================================

useHead({
  title: 'マイセット - Message',
  meta: [
    { name: 'description', content: 'よく使うトレーニングセットを保存・管理' }
  ]
})
</script>

<style scoped>
.training-presets {
  max-width: 1200px;
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
}

.header-content p {
  margin: 0;
  color: var(--text-secondary);
}

/* Loading and Error States */
.loading-state,
.error-state,
.empty-state {
  text-align: center;
  padding: 60px 20px;
  color: var(--text-secondary);
}

.loading-spinner {
  font-size: 3rem;
  margin-bottom: 15px;
  animation: pulse 1.5s ease-in-out infinite;
}

.error-icon,
.empty-icon {
  font-size: 4rem;
  margin-bottom: 20px;
}

/* Presets Grid */
.presets-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 20px;
}

.preset-card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  border: 2px solid transparent;
  transition: all 0.3s ease;
}

.preset-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.15);
}

.preset-card.is-default {
  border-color: #007AFF;
  background: linear-gradient(135deg, #E6F2FF, #FFFFFF);
}

/* Preset Header */
.preset-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 15px;
}

.preset-info h3 {
  margin: 0 0 5px 0;
  color: var(--text-primary);
  font-size: 1.3rem;
}

.preset-menu {
  margin: 0 0 5px 0;
  color: #007AFF;
  font-weight: 600;
  font-size: 0.9rem;
}

.preset-description {
  margin: 0;
  color: var(--text-secondary);
  font-size: 0.85rem;
}

.preset-badge {
  background: linear-gradient(135deg, #007AFF, #0056CC);
  color: white;
  padding: 4px 8px;
  border-radius: 6px;
  font-size: 0.75rem;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 4px;
}

/* Preset Sets */
.preset-sets {
  margin-bottom: 15px;
}

.sets-header h4 {
  margin: 0 0 10px 0;
  color: var(--text-primary);
  font-size: 1rem;
}

.sets-preview {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.set-preview {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px;
  background: #F8F9FA;
  border-radius: 6px;
  font-size: 0.85rem;
}

.set-number {
  background: linear-gradient(135deg, #007AFF, #0056CC);
  color: white;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 0.8rem;
}

.set-details {
  color: var(--text-primary);
  font-weight: 600;
}

.set-weight {
  color: var(--text-secondary);
}

.more-sets {
  color: var(--text-secondary);
  font-size: 0.8rem;
  font-style: italic;
  text-align: center;
  padding: 4px;
}

/* Preset Actions */
.preset-actions {
  margin-bottom: 10px;
}

.btn-block {
  width: 100%;
  margin-bottom: 10px;
}

.secondary-actions {
  display: flex;
  gap: 8px;
  justify-content: center;
}

.preset-meta {
  text-align: center;
  padding-top: 10px;
  border-top: 1px solid #E0E0E0;
}

.created-date {
  color: var(--text-secondary);
  font-size: 0.8rem;
}

/* Modal Styles */
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
  z-index: 2000;
  padding: 20px;
}

.modal-content {
  background: white;
  border-radius: 16px;
  max-width: 600px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 25px 25px 0 25px;
  border-bottom: 1px solid #E0E0E0;
  margin-bottom: 20px;
}

.modal-header h2 {
  margin: 0;
  color: var(--text-primary);
  font-size: 1.4rem;
  display: flex;
  align-items: center;
  gap: 10px;
}

.modal-close-btn {
  background: none;
  border: none;
  color: var(--text-secondary);
  cursor: pointer;
  padding: 5px;
  border-radius: 4px;
  transition: all 0.2s;
}

.modal-close-btn:hover {
  background: #F0F0F0;
  color: var(--text-primary);
}

.modal-body {
  padding: 0 25px 25px 25px;
}

.modal-actions {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
  padding: 15px 25px 25px 25px;
  border-top: 1px solid #E0E0E0;
}

/* Form Styles */
.form-section {
  margin-bottom: 25px;
}

.form-section h3 {
  margin: 0 0 15px 0;
  color: var(--text-primary);
  font-size: 1.1rem;
}

.form-group {
  margin-bottom: 15px;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: 600;
  color: var(--text-primary);
}

.input-field {
  width: 100%;
  padding: 12px;
  border: 1px solid #E0E0E0;
  border-radius: 6px;
  font-size: 1rem;
  transition: border-color 0.2s;
}

.input-field:focus {
  outline: none;
  border-color: #007AFF;
}

.input-field.small {
  padding: 8px;
  font-size: 0.9rem;
}

/* Sets Form */
.sets-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.sets-form {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.set-form-item {
  display: flex;
  align-items: center;
  gap: 15px;
  padding: 15px;
  background: #F8F9FA;
  border-radius: 8px;
}

.set-number-badge {
  background: linear-gradient(135deg, #007AFF, #0056CC);
  color: white;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 0.9rem;
  flex-shrink: 0;
}

.set-inputs {
  flex: 1;
  display: grid;
  grid-template-columns: 1fr 1fr 2fr;
  gap: 10px;
}

.input-group {
  display: flex;
  flex-direction: column;
}

.input-group label {
  font-size: 0.8rem;
  margin-bottom: 4px;
  color: var(--text-secondary);
}

/* Detail Modal */
.detail-section {
  margin-bottom: 20px;
}

.detail-section h3 {
  margin: 0 0 5px 0;
  color: var(--text-primary);
}

.detail-menu {
  margin: 0 0 10px 0;
  color: #007AFF;
  font-weight: 600;
}

.detail-description {
  margin: 0 0 10px 0;
  color: var(--text-secondary);
}

.detail-sets {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.detail-set-item {
  display: flex;
  align-items: center;
  gap: 15px;
  padding: 12px;
  background: #F8F9FA;
  border-radius: 8px;
}

.set-data {
  display: flex;
  gap: 15px;
  align-items: center;
}

.set-reps {
  font-weight: 600;
  color: var(--text-primary);
}

.set-weight {
  color: #007AFF;
  font-weight: 500;
}

.set-note {
  color: var(--text-secondary);
  font-style: italic;
}

/* Button Styles */
.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 10px 16px;
  border: none;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  text-decoration: none;
  font-size: 0.9rem;
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

.btn-primary {
  background: linear-gradient(135deg, #007AFF, #0056CC) !important;
  color: white !important;
  border: 2px solid #0056CC !important;
}

.btn-outline {
  background: transparent !important;
  border: 1px solid #E0E0E0 !important;
  color: var(--text-primary) !important;
}

.btn-outline:hover {
  background: #F0F0F0 !important;
}

.btn-secondary {
  background: linear-gradient(135deg, #6C757D, #5A6268) !important;
  color: white !important;
  border: 2px solid #5A6268 !important;
}

.btn-danger {
  background: linear-gradient(135deg, #DC3545, #C82333) !important;
  color: white !important;
  border: 2px solid #C82333 !important;
}

.btn-sm {
  padding: 6px 12px;
  font-size: 0.8rem;
}

/* Animations */
@keyframes pulse {
  0%, 100% { opacity: 0.5; }
  50% { opacity: 1; }
}

.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* Responsive */
@media (max-width: 768px) {
  .training-presets {
    padding: 15px;
  }
  
  .page-header {
    flex-direction: column;
    align-items: stretch;
  }
  
  .presets-grid {
    grid-template-columns: 1fr;
  }
  
  .set-inputs {
    grid-template-columns: 1fr;
  }
  
  .secondary-actions {
    flex-wrap: wrap;
  }
  
  .modal-content {
    margin: 10px;
    max-height: 95vh;
  }
}

/* CSS Variables */
:root {
  --text-primary: #212121;
  --text-secondary: #757575;
}

html.dark {
  --text-primary: #ffffff;
  --text-secondary: #b0b0b0;
}
</style>