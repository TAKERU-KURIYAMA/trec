import { ref, computed } from 'vue'
import type { Ref } from 'vue'
import { apiClient } from '~/utils/api-client'

interface Supplement {
  supplementId: number
  supplementName: string
  unit: string
  description?: string
}

interface IntakeRecord {
  recordId: number
  supplementId: number
  supplementName: string
  unit: string
  intakeDate: string
  intakeTime: string
  amount: number
  timingType?: string
  memo?: string
}

interface Schedule {
  scheduleId: number
  supplementId: number
  supplementName: string
  unit: string
  scheduleTime: string
  amount: number
  timingType?: string
  daysOfWeek: string
  memo?: string
}

export const useSupplement = () => {
  const supplements = ref<Supplement[]>([])
  const intakeRecords = ref<IntakeRecord[]>([])
  const schedules = ref<Schedule[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  // サプリメント一覧を取得
  const fetchSupplements = async () => {
    loading.value = true
    error.value = null
    try {
      const response = await apiClient.get('/api/supplement/supplements')
      supplements.value = response.data
    } catch (e) {
      error.value = 'サプリメント一覧の取得に失敗しました'
      console.error(e)
    } finally {
      loading.value = false
    }
  }

  // サプリメントを作成
  const createSupplement = async (data: {
    supplementName: string
    unit: string
    description?: string
  }) => {
    loading.value = true
    error.value = null
    try {
      const response = await apiClient.post('/api/supplement/supplements', data)
      await fetchSupplements() // 一覧を再取得
      return response.data
    } catch (e) {
      error.value = 'サプリメントの登録に失敗しました'
      console.error(e)
      throw e
    } finally {
      loading.value = false
    }
  }

  // サプリメントを更新
  const updateSupplement = async (id: number, data: {
    supplementName: string
    unit: string
    description?: string
  }) => {
    loading.value = true
    error.value = null
    try {
      await apiClient.put(`/api/supplement/supplements/${id}`, data)
      await fetchSupplements() // 一覧を再取得
    } catch (e) {
      error.value = 'サプリメントの更新に失敗しました'
      console.error(e)
      throw e
    } finally {
      loading.value = false
    }
  }

  // サプリメントを削除
  const deleteSupplement = async (id: number) => {
    loading.value = true
    error.value = null
    try {
      await apiClient.delete(`/api/supplement/supplements/${id}`)
      await fetchSupplements() // 一覧を再取得
    } catch (e) {
      error.value = 'サプリメントの削除に失敗しました'
      console.error(e)
      throw e
    } finally {
      loading.value = false
    }
  }

  // 摂取記録を取得
  const fetchIntakeRecords = async (date?: Date) => {
    loading.value = true
    error.value = null
    try {
      const params = date ? { date: date.toISOString().split('T')[0] } : {}
      const response = await apiClient.get('/api/supplement/intakes', { params })
      intakeRecords.value = response.data
    } catch (e) {
      error.value = '摂取記録の取得に失敗しました'
      console.error(e)
    } finally {
      loading.value = false
    }
  }

  // 摂取記録を作成
  const recordIntake = async (data: {
    supplementId: number
    intakeDate: Date
    intakeTime: string
    amount: number
    timingType?: string
    memo?: string
  }) => {
    loading.value = true
    error.value = null
    try {
      const response = await apiClient.post('/api/supplement/intakes', {
        ...data,
        intakeDate: data.intakeDate.toISOString(),
        intakeTime: data.intakeTime + ':00' // HH:mm to HH:mm:ss
      })
      await fetchIntakeRecords(data.intakeDate) // 記録を再取得
      return response.data
    } catch (e) {
      error.value = '摂取記録の保存に失敗しました'
      console.error(e)
      throw e
    } finally {
      loading.value = false
    }
  }

  // 摂取記録を削除
  const deleteIntakeRecord = async (id: number) => {
    loading.value = true
    error.value = null
    try {
      await apiClient.delete(`/api/supplement/intakes/${id}`)
      await fetchIntakeRecords() // 記録を再取得
    } catch (e) {
      error.value = '摂取記録の削除に失敗しました'
      console.error(e)
      throw e
    } finally {
      loading.value = false
    }
  }

  // スケジュールを取得
  const fetchSchedules = async () => {
    loading.value = true
    error.value = null
    try {
      const response = await apiClient.get('/api/supplement/schedules')
      schedules.value = response.data
    } catch (e) {
      error.value = 'スケジュールの取得に失敗しました'
      console.error(e)
    } finally {
      loading.value = false
    }
  }

  // スケジュールを作成
  const createSchedule = async (data: {
    supplementId: number
    scheduleTime: string
    amount: number
    timingType?: string
    daysOfWeek?: string
    memo?: string
  }) => {
    loading.value = true
    error.value = null
    try {
      const response = await apiClient.post('/api/supplement/schedules', {
        ...data,
        scheduleTime: data.scheduleTime + ':00' // HH:mm to HH:mm:ss
      })
      await fetchSchedules() // スケジュールを再取得
      return response.data
    } catch (e) {
      error.value = 'スケジュールの作成に失敗しました'
      console.error(e)
      throw e
    } finally {
      loading.value = false
    }
  }

  // スケジュールを削除
  const deleteSchedule = async (id: number) => {
    loading.value = true
    error.value = null
    try {
      await apiClient.delete(`/api/supplement/schedules/${id}`)
      await fetchSchedules() // スケジュールを再取得
    } catch (e) {
      error.value = 'スケジュールの削除に失敗しました'
      console.error(e)
      throw e
    } finally {
      loading.value = false
    }
  }

  // タイミングタイプの選択肢
  const timingTypes = [
    { value: '朝', label: '朝' },
    { value: '昼', label: '昼' },
    { value: '夜', label: '夜' },
    { value: 'トレーニング前', label: 'トレーニング前' },
    { value: 'トレーニング後', label: 'トレーニング後' },
    { value: '食前', label: '食前' },
    { value: '食後', label: '食後' },
    { value: '就寝前', label: '就寝前' }
  ]

  // 曜日の選択肢
  const daysOfWeekOptions = [
    { value: 'ALL', label: '毎日' },
    { value: '1,2,3,4,5', label: '平日' },
    { value: '6,7', label: '週末' },
    { value: '1,3,5', label: '月水金' },
    { value: '2,4', label: '火木' }
  ]

  return {
    supplements: readonly(supplements),
    intakeRecords: readonly(intakeRecords),
    schedules: readonly(schedules),
    loading: readonly(loading),
    error: readonly(error),
    fetchSupplements,
    createSupplement,
    updateSupplement,
    deleteSupplement,
    fetchIntakeRecords,
    recordIntake,
    deleteIntakeRecord,
    fetchSchedules,
    createSchedule,
    deleteSchedule,
    timingTypes,
    daysOfWeekOptions
  }
}