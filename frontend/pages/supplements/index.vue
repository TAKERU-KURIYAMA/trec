<template>
  <div class="container mx-auto px-4 py-8 max-w-6xl">
    <h1 class="text-3xl font-bold mb-6">サプリメント管理</h1>

    <!-- タブナビゲーション -->
    <div class="border-b border-gray-200 dark:border-gray-700">
      <nav class="-mb-px flex space-x-8">
        <button
          v-for="tab in tabs"
          :key="tab.id"
          @click="activeTab = tab.id"
          :class="[
            'py-2 px-1 border-b-2 font-medium text-sm',
            activeTab === tab.id
              ? 'border-blue-500 text-blue-600 dark:text-blue-400'
              : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400'
          ]"
        >
          {{ tab.label }}
        </button>
      </nav>
    </div>

    <!-- タブコンテンツ -->
    <div class="mt-6">
      <!-- 摂取記録タブ -->
      <div v-if="activeTab === 'intake'" class="space-y-6">
        <!-- 日付選択 -->
        <div class="flex items-center space-x-4">
          <input
            type="date"
            v-model="selectedDate"
            @change="onDateChange"
            class="px-3 py-2 border border-gray-300 rounded-md dark:bg-gray-800 dark:border-gray-600"
          />
          <button
            @click="recordIntakeModal = true"
            class="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700"
          >
            摂取記録を追加
          </button>
        </div>

        <!-- 摂取記録リスト -->
        <div v-if="loading" class="text-center py-8">
          <div class="inline-flex items-center">
            <svg class="animate-spin h-5 w-5 mr-3" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            読み込み中...
          </div>
        </div>

        <div v-else-if="intakeRecords.length === 0" class="text-center py-8 text-gray-500">
          この日の摂取記録はありません
        </div>

        <div v-else class="space-y-4">
          <div
            v-for="record in sortedIntakeRecords"
            :key="record.recordId"
            class="bg-white dark:bg-gray-800 rounded-lg shadow p-4 flex items-center justify-between"
          >
            <div>
              <h3 class="font-semibold">{{ record.supplementName }}</h3>
              <p class="text-sm text-gray-600 dark:text-gray-400">
                {{ formatTime(record.intakeTime) }} - {{ record.amount }}{{ record.unit }}
                <span v-if="record.timingType" class="ml-2 text-xs bg-gray-100 dark:bg-gray-700 px-2 py-1 rounded">
                  {{ record.timingType }}
                </span>
              </p>
              <p v-if="record.memo" class="text-sm text-gray-500 mt-1">{{ record.memo }}</p>
            </div>
            <button
              @click="deleteRecord(record.recordId)"
              class="text-red-600 hover:text-red-800"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path>
              </svg>
            </button>
          </div>
        </div>
      </div>

      <!-- サプリメント一覧タブ -->
      <div v-if="activeTab === 'supplements'" class="space-y-6">
        <div class="flex justify-end">
          <button
            @click="supplementModal = true"
            class="bg-green-600 text-white px-4 py-2 rounded-md hover:bg-green-700"
          >
            サプリメントを追加
          </button>
        </div>

        <div v-if="loading" class="text-center py-8">
          <div class="inline-flex items-center">
            <svg class="animate-spin h-5 w-5 mr-3" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            読み込み中...
          </div>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="supplement in supplements"
            :key="supplement.supplementId"
            class="bg-white dark:bg-gray-800 rounded-lg shadow p-4"
          >
            <div class="flex items-start justify-between">
              <div class="flex-1">
                <h3 class="font-semibold">{{ supplement.supplementName }}</h3>
                <p class="text-sm text-gray-600 dark:text-gray-400">単位: {{ supplement.unit }}</p>
                <p v-if="supplement.description" class="text-sm text-gray-500 mt-2">
                  {{ supplement.description }}
                </p>
              </div>
              <div class="flex space-x-2 ml-4">
                <button
                  @click="editSupplement(supplement)"
                  class="text-blue-600 hover:text-blue-800"
                >
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path>
                  </svg>
                </button>
                <button
                  @click="deleteSupplement(supplement.supplementId)"
                  class="text-red-600 hover:text-red-800"
                >
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path>
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- スケジュールタブ -->
      <div v-if="activeTab === 'schedules'" class="space-y-6">
        <div class="flex justify-end">
          <button
            @click="scheduleModal = true"
            class="bg-purple-600 text-white px-4 py-2 rounded-md hover:bg-purple-700"
          >
            スケジュールを追加
          </button>
        </div>

        <div v-if="loading" class="text-center py-8">
          <div class="inline-flex items-center">
            <svg class="animate-spin h-5 w-5 mr-3" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            読み込み中...
          </div>
        </div>

        <div v-else-if="schedules.length === 0" class="text-center py-8 text-gray-500">
          スケジュールが設定されていません
        </div>

        <div v-else class="space-y-4">
          <div
            v-for="schedule in sortedSchedules"
            :key="schedule.scheduleId"
            class="bg-white dark:bg-gray-800 rounded-lg shadow p-4 flex items-center justify-between"
          >
            <div>
              <h3 class="font-semibold">{{ schedule.supplementName }}</h3>
              <p class="text-sm text-gray-600 dark:text-gray-400">
                {{ formatTime(schedule.scheduleTime) }} - {{ schedule.amount }}{{ schedule.unit }}
                <span v-if="schedule.timingType" class="ml-2 text-xs bg-gray-100 dark:bg-gray-700 px-2 py-1 rounded">
                  {{ schedule.timingType }}
                </span>
                <span class="ml-2 text-xs bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200 px-2 py-1 rounded">
                  {{ formatDaysOfWeek(schedule.daysOfWeek) }}
                </span>
              </p>
              <p v-if="schedule.memo" class="text-sm text-gray-500 mt-1">{{ schedule.memo }}</p>
            </div>
            <button
              @click="deleteSchedule(schedule.scheduleId)"
              class="text-red-600 hover:text-red-800"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path>
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- モーダル -->
    <SupplementModal
      v-if="supplementModal"
      :supplement="editingItem"
      @close="closeSuppplementModal"
      @save="saveSupplement"
    />

    <IntakeRecordModal
      v-if="recordIntakeModal"
      :supplements="supplements"
      :date="selectedDate"
      @close="recordIntakeModal = false"
      @save="saveIntakeRecord"
    />

    <ScheduleModal
      v-if="scheduleModal"
      :supplements="supplements"
      @close="scheduleModal = false"
      @save="saveSchedule"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useSupplement } from '~/composables/useSupplement'

const {
  supplements,
  intakeRecords,
  schedules,
  loading,
  error,
  fetchSupplements,
  createSupplement,
  updateSupplement,
  deleteSupplement: deleteSupplementApi,
  fetchIntakeRecords,
  recordIntake,
  deleteIntakeRecord,
  fetchSchedules,
  createSchedule,
  deleteSchedule: deleteScheduleApi,
  timingTypes,
  daysOfWeekOptions
} = useSupplement()

// タブ管理
const tabs = [
  { id: 'intake', label: '摂取記録' },
  { id: 'supplements', label: 'サプリメント一覧' },
  { id: 'schedules', label: 'スケジュール' }
]
const activeTab = ref('intake')

// モーダル管理
const supplementModal = ref(false)
const recordIntakeModal = ref(false)
const scheduleModal = ref(false)
const editingItem = ref(null)

// 日付選択
const selectedDate = ref(new Date().toISOString().split('T')[0])

// ソートされたデータ
const sortedIntakeRecords = computed(() => {
  return [...intakeRecords.value].sort((a, b) => {
    return a.intakeTime.localeCompare(b.intakeTime)
  })
})

const sortedSchedules = computed(() => {
  return [...schedules.value].sort((a, b) => {
    return a.scheduleTime.localeCompare(b.scheduleTime)
  })
})

// 初期化
onMounted(async () => {
  await Promise.all([
    fetchSupplements(),
    fetchIntakeRecords(new Date(selectedDate.value)),
    fetchSchedules()
  ])
})

// 日付変更時の処理
const onDateChange = async () => {
  await fetchIntakeRecords(new Date(selectedDate.value))
}

// サプリメント編集
const editSupplement = (supplement: any) => {
  editingItem.value = supplement
  supplementModal.value = true
}

const closeSuppplementModal = () => {
  supplementModal.value = false
  editingItem.value = null
}

const saveSupplement = async (data: any) => {
  try {
    if (editingItem.value) {
      await updateSupplement(editingItem.value.supplementId, data)
    } else {
      await createSupplement(data)
    }
    closeSuppplementModal()
  } catch (e) {
    // エラーは composable で処理済み
  }
}

const deleteSupplement = async (id: number) => {
  if (confirm('このサプリメントを削除しますか？')) {
    try {
      await deleteSupplementApi(id)
    } catch (e) {
      // エラーは composable で処理済み
    }
  }
}

// 摂取記録の保存
const saveIntakeRecord = async (data: any) => {
  try {
    await recordIntake({
      ...data,
      intakeDate: new Date(selectedDate.value)
    })
    recordIntakeModal.value = false
  } catch (e) {
    // エラーは composable で処理済み
  }
}

const deleteRecord = async (id: number) => {
  if (confirm('この摂取記録を削除しますか？')) {
    try {
      await deleteIntakeRecord(id)
    } catch (e) {
      // エラーは composable で処理済み
    }
  }
}

// スケジュールの保存
const saveSchedule = async (data: any) => {
  try {
    await createSchedule(data)
    scheduleModal.value = false
  } catch (e) {
    // エラーは composable で処理済み
  }
}

const deleteSchedule = async (id: number) => {
  if (confirm('このスケジュールを削除しますか？')) {
    try {
      await deleteScheduleApi(id)
    } catch (e) {
      // エラーは composable で処理済み
    }
  }
}

// ユーティリティ関数
const formatTime = (time: string) => {
  return time.substring(0, 5) // HH:mm:ss -> HH:mm
}

const formatDaysOfWeek = (days: string) => {
  const mapping: Record<string, string> = {
    'ALL': '毎日',
    '1,2,3,4,5': '平日',
    '6,7': '週末',
    '1,3,5': '月水金',
    '2,4': '火木'
  }
  return mapping[days] || days
}
</script>