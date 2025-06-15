import { ref, computed } from 'vue'

// ===================================
// Types & State
// ===================================

const goals = ref([])
const loading = ref(false)
const error = ref(null)

// ===================================
// Goal Management
// ===================================

export function useGoals() {
  
  // Computed properties
  const totalGoals = computed(() => goals.value.length)
  const completedGoals = computed(() => goals.value.filter(g => g.status === 'completed').length)
  const activeGoals = computed(() => goals.value.filter(g => g.status === 'active').length)
  const averageProgress = computed(() => {
    if (goals.value.length === 0) return 0
    const sum = goals.value.reduce((acc, goal) => acc + goal.progress, 0)
    return Math.round(sum / goals.value.length)
  })
  
  const upcomingDeadlines = computed(() => {
    const now = new Date()
    const oneWeek = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000)
    
    return goals.value
      .filter(goal => {
        if (goal.status !== 'active') return false
        const endDate = new Date(goal.endDate)
        return endDate >= now && endDate <= oneWeek
      })
      .sort((a, b) => new Date(a.endDate) - new Date(b.endDate))
  })

  // Load goals from API
  async function fetchGoals() {
    loading.value = true
    error.value = null
    
    try {
      // Mock API call - replace with real API
      await new Promise(resolve => setTimeout(resolve, 500))
      
      // Mock data
      goals.value = [
        {
          id: '1',
          title: 'ベンチプレス100kg達成',
          description: '安全なフォームで100kgを1回挙げる',
          category: 'weight',
          currentValue: 85,
          targetValue: 100,
          unit: 'kg',
          progress: 85,
          status: 'active',
          startDate: '2024-01-01',
          endDate: '2024-06-30',
          milestones: [
            { id: 'm1', title: '90kg達成', targetValue: 90, completed: true, completedAt: new Date('2024-05-01') },
            { id: 'm2', title: '95kg達成', targetValue: 95, completed: false },
            { id: 'm3', title: '100kg達成', targetValue: 100, completed: false }
          ],
          createdAt: new Date('2024-01-01'),
          updatedAt: new Date()
        },
        {
          id: '2',
          title: '週3回のトレーニング継続',
          description: '3ヶ月間週3回のペースでトレーニングを継続',
          category: 'frequency',
          currentValue: 8,
          targetValue: 12,
          unit: '週',
          progress: 67,
          status: 'active',
          startDate: '2024-04-01',
          endDate: '2024-06-30',
          createdAt: new Date('2024-04-01'),
          updatedAt: new Date()
        },
        {
          id: '3',
          title: 'スクワット150kg達成',
          description: '正しいフォームでスクワット150kg',
          category: 'weight',
          currentValue: 150,
          targetValue: 150,
          unit: 'kg',
          progress: 100,
          status: 'completed',
          startDate: '2024-01-01',
          endDate: '2024-05-31',
          createdAt: new Date('2024-01-01'),
          updatedAt: new Date('2024-05-15')
        },
        {
          id: '4',
          title: '体重を75kgに減量',
          description: '健康的なペースで目標体重まで減量',
          category: 'bodyweight',
          currentValue: 78,
          targetValue: 75,
          unit: 'kg',
          progress: 60,
          status: 'active',
          startDate: '2024-05-01',
          endDate: '2024-08-31',
          milestones: [
            { id: 'm4', title: '77kg達成', targetValue: 77, completed: true, completedAt: new Date('2024-05-20') },
            { id: 'm5', title: '76kg達成', targetValue: 76, completed: false },
            { id: 'm6', title: '75kg達成', targetValue: 75, completed: false }
          ],
          createdAt: new Date('2024-05-01'),
          updatedAt: new Date()
        }
      ]
      
    } catch (err) {
      error.value = 'Failed to load goals'
      console.error('Error loading goals:', err)
    } finally {
      loading.value = false
    }
  }

  // Create new goal
  async function createGoal(goalData) {
    loading.value = true
    error.value = null
    
    try {
      // Mock API call
      await new Promise(resolve => setTimeout(resolve, 500))
      
      const newGoal = {
        id: Date.now().toString(),
        ...goalData,
        progress: (goalData.currentValue / goalData.targetValue) * 100,
        status: goalData.currentValue >= goalData.targetValue ? 'completed' : 'active',
        createdAt: new Date(),
        updatedAt: new Date()
      }
      
      goals.value.push(newGoal)
      return newGoal
      
    } catch (err) {
      error.value = 'Failed to create goal'
      throw err
    } finally {
      loading.value = false
    }
  }

  // Update existing goal
  async function updateGoal(goalId, updates) {
    loading.value = true
    error.value = null
    
    try {
      // Mock API call
      await new Promise(resolve => setTimeout(resolve, 300))
      
      const index = goals.value.findIndex(g => g.id === goalId)
      if (index !== -1) {
        goals.value[index] = {
          ...goals.value[index],
          ...updates,
          progress: updates.currentValue ? (updates.currentValue / goals.value[index].targetValue) * 100 : goals.value[index].progress,
          status: updates.currentValue && updates.currentValue >= goals.value[index].targetValue ? 'completed' : goals.value[index].status,
          updatedAt: new Date()
        }
        return goals.value[index]
      }
      
    } catch (err) {
      error.value = 'Failed to update goal'
      throw err
    } finally {
      loading.value = false
    }
  }

  // Update goal progress
  async function updateProgress(goalId, newValue, note = '') {
    loading.value = true
    error.value = null
    
    try {
      // Mock API call
      await new Promise(resolve => setTimeout(resolve, 300))
      
      const goal = goals.value.find(g => g.id === goalId)
      if (!goal) throw new Error('Goal not found')
      
      const newProgress = (newValue / goal.targetValue) * 100
      
      // Update goal
      const index = goals.value.findIndex(g => g.id === goalId)
      goals.value[index] = {
        ...goal,
        currentValue: newValue,
        progress: newProgress,
        status: newProgress >= 100 ? 'completed' : 'active',
        updatedAt: new Date()
      }
      
      // Check and update milestones
      if (goal.milestones) {
        goal.milestones.forEach(milestone => {
          if (!milestone.completed && newValue >= milestone.targetValue) {
            milestone.completed = true
            milestone.completedAt = new Date()
          }
        })
      }
      
      // Return completion status for notifications
      return {
        goal: goals.value[index],
        isCompleted: newProgress >= 100,
        milestonesAchieved: goal.milestones ? goal.milestones.filter(m => m.completed && !m.previouslyCompleted) : []
      }
      
    } catch (err) {
      error.value = 'Failed to update progress'
      throw err
    } finally {
      loading.value = false
    }
  }

  // Delete goal
  async function deleteGoal(goalId) {
    loading.value = true
    error.value = null
    
    try {
      // Mock API call
      await new Promise(resolve => setTimeout(resolve, 300))
      
      goals.value = goals.value.filter(g => g.id !== goalId)
      
    } catch (err) {
      error.value = 'Failed to delete goal'
      throw err
    } finally {
      loading.value = false
    }
  }

  // Get goal by ID
  function getGoalById(goalId) {
    return goals.value.find(g => g.id === goalId)
  }

  // Filter goals
  function getGoalsByCategory(category) {
    return goals.value.filter(g => g.category === category)
  }

  function getGoalsByStatus(status) {
    return goals.value.filter(g => g.status === status)
  }

  // Utility functions
  function calculateRemainingDays(endDate) {
    const end = new Date(endDate)
    const now = new Date()
    const diffTime = end.getTime() - now.getTime()
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
    return Math.max(0, diffDays)
  }

  function formatGoalValue(value, unit) {
    return `${value}${unit}`
  }

  function getCategoryText(category) {
    const categories = {
      weight: '重量向上',
      reps: '回数向上',
      frequency: '頻度目標',
      bodyweight: '体重管理',
      endurance: '持久力向上',
      custom: 'カスタム'
    }
    return categories[category] || category
  }

  function getStatusText(status) {
    const statuses = {
      active: '進行中',
      completed: '達成済み',
      paused: '一時停止',
      cancelled: 'キャンセル'
    }
    return statuses[status] || status
  }

  // Analytics
  function getGoalAnalytics() {
    const analytics = {
      totalGoals: totalGoals.value,
      completedGoals: completedGoals.value,
      activeGoals: activeGoals.value,
      completionRate: totalGoals.value > 0 ? Math.round((completedGoals.value / totalGoals.value) * 100) : 0,
      averageProgress: averageProgress.value,
      upcomingDeadlines: upcomingDeadlines.value,
      categoriesBreakdown: {}
    }
    
    // Calculate category breakdown
    const categories = ['weight', 'reps', 'frequency', 'bodyweight', 'endurance', 'custom']
    categories.forEach(category => {
      const categoryGoals = getGoalsByCategory(category)
      analytics.categoriesBreakdown[category] = {
        total: categoryGoals.length,
        completed: categoryGoals.filter(g => g.status === 'completed').length,
        active: categoryGoals.filter(g => g.status === 'active').length
      }
    })
    
    return analytics
  }

  return {
    // State
    goals: computed(() => goals.value),
    loading: computed(() => loading.value),
    error: computed(() => error.value),
    
    // Computed
    totalGoals,
    completedGoals,
    activeGoals,
    averageProgress,
    upcomingDeadlines,
    
    // Methods
    fetchGoals,
    createGoal,
    updateGoal,
    updateProgress,
    deleteGoal,
    getGoalById,
    getGoalsByCategory,
    getGoalsByStatus,
    
    // Utilities
    calculateRemainingDays,
    formatGoalValue,
    getCategoryText,
    getStatusText,
    getGoalAnalytics
  }
}

// Global state for shared usage
const globalGoalsState = {
  goals,
  loading,
  error
}

export { globalGoalsState }