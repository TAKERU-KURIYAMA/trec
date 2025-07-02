// ===================================
// Training History Composable (ultrathink)
// Simple and reliable history management
// ===================================

import { ref, computed, reactive } from 'vue'
import { useApiClient } from '~/utils/api-client'

// Types
export interface TrainingHistoryRecord {
  recordId: string
  menuId: string
  menuName: string
  trainingDate: string
  setCount: number
  totalReps: number
  maxReps: number
  maxWeight: number
  maxRepsWeight: number
  maxWeightReps: number
  totalLoadAmount: number
  createdAt?: Date
  updatedAt?: Date
}

export interface TrainingSetDetail {
  setNumber: number
  reps: number
  weight?: number
  note?: string
  createdAt?: Date
}

export interface HistoryFilters {
  menuId?: string
  startDate?: string
  endDate?: string
  limit?: number
}

export interface HistoryDetailsData {
  menuId: string
  trainingDate: string
  sets: TrainingSetDetail[]
  summary: {
    totalSets: number
    totalReps: number
    maxWeight: number
    totalVolume: number
  }
}

// Simple loading and error states
interface LoadingState {
  isLoading: boolean
  message?: string
}

interface ErrorState {
  hasError: boolean
  message: string
}

/**
 * Simple training history composable
 * Focus on reliability and ease of use
 */
export function useTrainingHistory() {
  const { training } = useApiClient()

  // ===================================
  // State
  // ===================================

  const records = ref<TrainingHistoryRecord[]>([])
  const currentDetails = ref<HistoryDetailsData | null>(null)

  const loading = reactive<LoadingState>({
    isLoading: false,
    message: ''
  })

  const error = reactive<ErrorState>({
    hasError: false,
    message: ''
  })

  // ===================================
  // Simple Analytics
  // ===================================

  const stats = computed(() => {
    const totalWorkouts = records.value.length
    const totalSets = records.value.reduce((sum, r) => sum + r.setCount, 0)
    const totalReps = records.value.reduce((sum, r) => sum + r.totalReps, 0)
    const totalVolume = records.value.reduce((sum, r) => sum + r.totalLoadAmount, 0)
    const maxWeight = Math.max(...records.value.map(r => r.maxWeight), 0)

    // This week's stats (last 7 days)
    const oneWeekAgo = new Date()
    oneWeekAgo.setDate(oneWeekAgo.getDate() - 7)
    
    const thisWeek = records.value.filter(r => {
      const recordDate = new Date(r.trainingDate)
      return recordDate >= oneWeekAgo
    })

    return {
      totalWorkouts,
      totalSets,
      totalReps,
      totalVolume,
      maxWeight,
      thisWeekWorkouts: thisWeek.length,
      thisWeekSets: thisWeek.reduce((sum, r) => sum + r.setCount, 0)
    }
  })

  // ===================================
  // Actions
  // ===================================

  /**
   * Get training history with filters
   */
  async function fetchHistory(filters: HistoryFilters = {}) {
    try {
      setLoading(true, 'トレーニング履歴を読み込み中...')
      clearError()

      const params = new URLSearchParams()
      if (filters.menuId) params.append('menuId', filters.menuId)
      if (filters.startDate) params.append('startDate', filters.startDate)
      if (filters.endDate) params.append('endDate', filters.endDate)
      if (filters.limit) params.append('limit', filters.limit.toString())

      const response = await training.get(`/history?${params.toString()}`)
      
      if (response.records) {
        // 日付フィールドをDateオブジェクトに変換
        records.value = response.records.map((record: any) => ({
          ...record,
          createdAt: record.createdAt ? new Date(record.createdAt) : undefined,
          updatedAt: record.updatedAt ? new Date(record.updatedAt) : undefined
        }))
      } else {
        throw new Error('データの取得に失敗しました')
      }

    } catch (err: any) {
      console.error('History fetch error:', err)
      setError('履歴の取得中にエラーが発生しました: ' + (err.message || 'Unknown error'))
    } finally {
      setLoading(false)
    }
  }

  /**
   * Get detailed set information for a specific workout
   */
  async function fetchHistoryDetails(menuId: string, trainingDate: string) {
    try {
      setLoading(true, 'セット詳細を読み込み中...')
      clearError()

      const params = new URLSearchParams({
        menuId,
        trainingDate
      })

      const response = await training.get(`/history/details?${params.toString()}`)
      
      if (response.isSuccess && response.data) {
        currentDetails.value = response.data
      } else {
        throw new Error(response.userMessage || 'セット詳細の取得に失敗しました')
      }

    } catch (err: any) {
      console.error('History details fetch error:', err)
      setError('セット詳細の取得中にエラーが発生しました: ' + (err.message || 'Unknown error'))
    } finally {
      setLoading(false)
    }
  }

  /**
   * Refresh current history
   */
  async function refreshHistory() {
    await fetchHistory()
  }

  // ===================================
  // Helper Functions
  // ===================================

  function setLoading(isLoading: boolean, message: string = '') {
    loading.isLoading = isLoading
    loading.message = message
  }

  function setError(message: string) {
    error.hasError = true
    error.message = message
  }

  function clearError() {
    error.hasError = false
    error.message = ''
  }

  function formatDate(dateString: string): string {
    const date = new Date(dateString)
    return date.toLocaleDateString('ja-JP', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      weekday: 'short'
    })
  }

  function formatTime(date: Date | string): string {
    const dateObj = typeof date === 'string' ? new Date(date) : date
    return dateObj.toLocaleTimeString('ja-JP', {
      hour: '2-digit',
      minute: '2-digit'
    })
  }

  // ===================================
  // Return Public API
  // ===================================

  return {
    // State
    records: computed(() => records.value),
    currentDetails: computed(() => currentDetails.value),
    loading: computed(() => loading),
    error: computed(() => error),
    stats,

    // Actions
    fetchHistory,
    fetchHistoryDetails,
    refreshHistory,
    clearError,

    // Utilities
    formatDate,
    formatTime
  }
}