// ===================================
// Ultra Advanced Training Store with Zustand
// Comprehensive state management, offline support, real-time analytics
// ===================================

import { create } from 'zustand';
import { subscribeWithSelector } from 'zustand/middleware';
import { immer } from 'zustand/middleware/immer';
import AsyncStorage from '@react-native-async-storage/async-storage';
import {
  TrainingMenu,
  TrainingRecord,
  DailyRecord,
  DashboardStats,
  LoadingState,
  ErrorState,
  NetworkState,
  WorkoutSession,
  Goal,
  Achievement,
  FilterOptions,
  ProgressChartData,
} from '../types/enhanced';
import { apiClient, ApiError, NetworkError } from '../services/apiClient';

/**
 * Get current user ID from AsyncStorage
 */
async function getCurrentUserId(): Promise<string> {
  try {
    const storedUser = await AsyncStorage.getItem('@TrecPlans:user');
    if (storedUser) {
      const user = JSON.parse(storedUser);
      return user.userId || 'current-user';
    }
  } catch (error) {
    console.error('Error getting current user ID:', error);
  }
  return 'current-user'; // fallback
}

/**
 * トレーニングストアの状態インターface
 */
interface TrainingStoreState {
  // ===================================
  // Core Data
  // ===================================
  menus: TrainingMenu[];
  records: TrainingRecord[];
  dailyRecords: DailyRecord[];
  dashboardStats: DashboardStats | null;
  currentSession: WorkoutSession | null;
  goals: Goal[];
  achievements: Achievement[];

  // ===================================
  // UI State
  // ===================================
  loading: LoadingState;
  error: ErrorState;
  networkState: NetworkState;

  // ===================================
  // Filters & Search
  // ===================================
  filters: FilterOptions;
  searchQuery: string;
  favoriteMenuIds: Set<string>;

  // ===================================
  // Analytics
  // ===================================
  progressCharts: ProgressChartData[];
  lastSyncAt: Date | null;
  pendingSyncCount: number;

  // ===================================
  // Actions - Data Fetching
  // ===================================
  fetchMenus: (force?: boolean) => Promise<void>;
  fetchRecords: (params?: { menuId?: string; from?: string; to?: string }) => Promise<void>;
  fetchDashboardStats: (force?: boolean) => Promise<void>;
  fetchGoals: () => Promise<void>;
  fetchAchievements: () => Promise<void>;

  // ===================================
  // Actions - Data Manipulation
  // ===================================
  createRecord: (record: Omit<TrainingRecord, 'recordId' | 'createdAt' | 'updatedAt'>) => Promise<string>;
  updateRecord: (recordId: string, updates: Partial<TrainingRecord>) => Promise<void>;
  deleteRecord: (recordId: string) => Promise<void>;
  createGoal: (goal: Omit<Goal, 'goalId' | 'createdAt'>) => Promise<string>;
  updateGoal: (goalId: string, updates: Partial<Goal>) => Promise<void>;
  deleteGoal: (goalId: string) => Promise<void>;

  // ===================================
  // Actions - Workout Session
  // ===================================
  startWorkoutSession: (menuId: string) => Promise<string>;
  updateCurrentSession: (updates: Partial<WorkoutSession>) => void;
  pauseCurrentSession: () => void;
  resumeCurrentSession: () => void;
  completeCurrentSession: () => Promise<void>;
  cancelCurrentSession: () => void;

  // ===================================
  // Actions - Favorites & Filters
  // ===================================
  toggleFavorite: (menuId: string) => Promise<void>;
  updateFilters: (filters: Partial<FilterOptions>) => void;
  setSearchQuery: (query: string) => void;
  clearFilters: () => void;

  // ===================================
  // Actions - UI State
  // ===================================
  setLoading: (loading: Partial<LoadingState>) => void;
  setError: (error: Partial<ErrorState>) => void;
  clearError: () => void;
  updateNetworkState: (networkState: Partial<NetworkState>) => void;

  // ===================================
  // Actions - Sync & Cache
  // ===================================
  syncData: () => Promise<void>;
  clearCache: () => Promise<void>;
  loadPersistedData: () => Promise<void>;
  persistData: () => Promise<void>;

  // ===================================
  // Selectors
  // ===================================
  getFilteredMenus: () => TrainingMenu[];
  getMenuById: (menuId: string) => TrainingMenu | undefined;
  getRecordsByMenuId: (menuId: string) => TrainingRecord[];
  getRecordsByDateRange: (from: Date, to: Date) => TrainingRecord[];
  getActiveGoals: () => Goal[];
  getRecentAchievements: (limit?: number) => Achievement[];
  getProgressForMenu: (menuId: string) => ProgressChartData | undefined;
  getPersonalRecords: () => Array<{ menuId: string; type: string; value: number; date: Date }>;
}

/**
 * 高度なトレーニングストア
 */
export const useTrainingStore = create<TrainingStoreState>()(
  subscribeWithSelector(
    immer((set, get) => ({
      // ===================================
      // Initial State
      // ===================================
      menus: [],
      records: [],
      dailyRecords: [],
      dashboardStats: null,
      currentSession: null,
      goals: [],
      achievements: [],

      loading: { isLoading: false },
      error: { hasError: false, message: '', retryable: false, timestamp: new Date() },
      networkState: { isConnected: false, isInternetReachable: false, type: 'unknown' },

      filters: {},
      searchQuery: '',
      favoriteMenuIds: new Set(),

      progressCharts: [],
      lastSyncAt: null,
      pendingSyncCount: 0,

      // ===================================
      // Data Fetching Actions
      // ===================================
      fetchMenus: async (force = false) => {
        try {
          set(state => {
            state.loading.isLoading = true;
            state.loading.message = 'メニューを読み込み中...';
            state.error.hasError = false;
          });

          const menus = await apiClient.get<TrainingMenu[]>('/training/menu', {
            useCache: !force,
            cacheLifetime: 600, // 10分
          });

          set(state => {
            state.menus = menus;
            state.loading.isLoading = false;
            state.loading.message = undefined;
          });

          // データを永続化
          await get().persistData();

        } catch (error) {
          const errorState = get().handleError(error as Error);
          set(state => {
            state.error = errorState;
            state.loading.isLoading = false;
          });
          throw error;
        }
      },

      fetchRecords: async (params = {}) => {
        try {
          set(state => {
            state.loading.isLoading = true;
            state.loading.message = '記録を読み込み中...';
          });

          const records = await apiClient.get<TrainingRecord[]>('/training/records', {
            params: {
              ...params,
              limit: params.limit || 50,
              offset: params.offset || 0,
            }
          });

          set(state => {
            state.records = records;
            state.loading.isLoading = false;
          });

          await get().persistData();

        } catch (error) {
          const errorState = get().handleError(error as Error);
          set(state => {
            state.error = errorState;
            state.loading.isLoading = false;
          });
          throw error;
        }
      },

      fetchDashboardStats: async (force = false) => {
        try {
          set(state => {
            state.loading.isLoading = true;
            state.loading.message = '統計を計算中...';
          });

          const stats = await apiClient.get<DashboardStats>('/dashboard/stats', {
            useCache: !force,
            cacheLifetime: 300, // 5分
          });

          set(state => {
            state.dashboardStats = stats;
            state.loading.isLoading = false;
          });

        } catch (error) {
          const errorState = get().handleError(error as Error);
          set(state => {
            state.error = errorState;
            state.loading.isLoading = false;
          });
        }
      },

      fetchGoals: async () => {
        try {
          const goals = await apiClient.get<Goal[]>('/goals');
          set(state => {
            state.goals = goals;
          });
        } catch (error) {
          console.warn('Failed to fetch goals:', error);
        }
      },

      fetchAchievements: async () => {
        try {
          const achievements = await apiClient.get<Achievement[]>('/achievements');
          set(state => {
            state.achievements = achievements;
          });
        } catch (error) {
          console.warn('Failed to fetch achievements:', error);
        }
      },

      // ===================================
      // Data Manipulation Actions
      // ===================================
      createRecord: async (recordData) => {
        try {
          const record = await apiClient.post<TrainingRecord>('/training/records', recordData);
          
          set(state => {
            state.records.push(record);
          });

          await get().persistData();
          
          // 統計を更新
          get().fetchDashboardStats(true);
          
          return record.recordId;
        } catch (error) {
          const errorState = get().handleError(error as Error);
          set(state => {
            state.error = errorState;
          });
          throw error;
        }
      },

      updateRecord: async (recordId, updates) => {
        try {
          const updatedRecord = await apiClient.put<TrainingRecord>(`/training/records/${recordId}`, updates);
          
          set(state => {
            const index = state.records.findIndex(r => r.recordId === recordId);
            if (index >= 0) {
              state.records[index] = updatedRecord;
            }
          });

          await get().persistData();
        } catch (error) {
          const errorState = get().handleError(error as Error);
          set(state => {
            state.error = errorState;
          });
          throw error;
        }
      },

      deleteRecord: async (recordId) => {
        try {
          await apiClient.delete(`/training/records/${recordId}`);
          
          set(state => {
            state.records = state.records.filter(r => r.recordId !== recordId);
          });

          await get().persistData();
          get().fetchDashboardStats(true);
        } catch (error) {
          const errorState = get().handleError(error as Error);
          set(state => {
            state.error = errorState;
          });
          throw error;
        }
      },

      createGoal: async (goalData) => {
        try {
          const goal = await apiClient.post<Goal>('/goals', goalData);
          
          set(state => {
            state.goals.push(goal);
          });

          return goal.goalId;
        } catch (error) {
          throw error;
        }
      },

      updateGoal: async (goalId, updates) => {
        try {
          const updatedGoal = await apiClient.put<Goal>(`/goals/${goalId}`, updates);
          
          set(state => {
            const index = state.goals.findIndex(g => g.goalId === goalId);
            if (index >= 0) {
              state.goals[index] = updatedGoal;
            }
          });
        } catch (error) {
          throw error;
        }
      },

      deleteGoal: async (goalId) => {
        try {
          await apiClient.delete(`/goals/${goalId}`);
          
          set(state => {
            state.goals = state.goals.filter(g => g.goalId !== goalId);
          });
        } catch (error) {
          throw error;
        }
      },

      // ===================================
      // Workout Session Actions
      // ===================================
      startWorkoutSession: async (menuId) => {
        const menu = get().getMenuById(menuId);
        if (!menu) throw new Error('Menu not found');

        const sessionId = Date.now().toString() + Math.random().toString(36).substr(2, 9);
        
        const session: WorkoutSession = {
          sessionId,
          menuId,
          userId: await getCurrentUserId(), // Get from auth context
          status: 'active',
          startTime: new Date(),
          currentSetIndex: 0,
          plannedSets: [],
          completedSets: [],
          isRestActive: false,
          timerState: {
            isRunning: true,
            startTime: new Date(),
            elapsedTime: 0,
            type: 'workout',
          },
        };

        set(state => {
          state.currentSession = session;
        });

        return sessionId;
      },

      updateCurrentSession: (updates) => {
        set(state => {
          if (state.currentSession) {
            Object.assign(state.currentSession, updates);
          }
        });
      },

      pauseCurrentSession: () => {
        set(state => {
          if (state.currentSession) {
            state.currentSession.status = 'paused';
            state.currentSession.timerState.isRunning = false;
          }
        });
      },

      resumeCurrentSession: () => {
        set(state => {
          if (state.currentSession) {
            state.currentSession.status = 'active';
            state.currentSession.timerState.isRunning = true;
          }
        });
      },

      completeCurrentSession: async () => {
        const session = get().currentSession;
        if (!session) return;

        try {
          // セッションをレコードとして保存
          const recordData = {
            menuId: session.menuId,
            userId: session.userId,
            sessionId: session.sessionId,
            date: session.startTime?.toISOString().split('T')[0] || new Date().toISOString().split('T')[0],
            sets: session.completedSets,
            totalVolume: session.completedSets.reduce((sum, set) => sum + (set.weight || 0) * set.reps, 0),
            totalDuration: Math.floor((new Date().getTime() - (session.startTime?.getTime() || 0)) / 1000),
            sessionNotes: session.notes,
            syncStatus: 'pending' as const,
          };

          await get().createRecord(recordData);

          set(state => {
            state.currentSession = null;
          });

        } catch (error) {
          throw error;
        }
      },

      cancelCurrentSession: () => {
        set(state => {
          state.currentSession = null;
        });
      },

      // ===================================
      // Favorites & Filters Actions
      // ===================================
      toggleFavorite: async (menuId) => {
        const favorites = get().favoriteMenuIds;
        const newFavorites = new Set(favorites);
        
        if (newFavorites.has(menuId)) {
          newFavorites.delete(menuId);
        } else {
          newFavorites.add(menuId);
        }

        set(state => {
          state.favoriteMenuIds = newFavorites;
        });

        // 永続化
        try {
          await AsyncStorage.setItem('@TrecPlans:favorites', JSON.stringify(Array.from(newFavorites)));
        } catch (error) {
          console.warn('Failed to persist favorites:', error);
        }
      },

      updateFilters: (newFilters) => {
        set(state => {
          state.filters = { ...state.filters, ...newFilters };
        });
      },

      setSearchQuery: (query) => {
        set(state => {
          state.searchQuery = query;
        });
      },

      clearFilters: () => {
        set(state => {
          state.filters = {};
          state.searchQuery = '';
        });
      },

      // ===================================
      // UI State Actions
      // ===================================
      setLoading: (loading) => {
        set(state => {
          state.loading = { ...state.loading, ...loading };
        });
      },

      setError: (error) => {
        set(state => {
          state.error = { ...state.error, ...error, timestamp: new Date() };
        });
      },

      clearError: () => {
        set(state => {
          state.error = { hasError: false, message: '', retryable: false, timestamp: new Date() };
        });
      },

      updateNetworkState: (networkState) => {
        set(state => {
          state.networkState = { ...state.networkState, ...networkState };
        });
      },

      // ===================================
      // Sync & Cache Actions
      // ===================================
      syncData: async () => {
        try {
          set(state => {
            state.loading.isLoading = true;
            state.loading.message = 'データを同期中...';
          });

          await apiClient.sync();
          
          // データを再取得
          await Promise.all([
            get().fetchMenus(true),
            get().fetchRecords(),
            get().fetchDashboardStats(true),
          ]);

          set(state => {
            state.lastSyncAt = new Date();
            state.pendingSyncCount = 0;
            state.loading.isLoading = false;
          });

        } catch (error) {
          const errorState = get().handleError(error as Error);
          set(state => {
            state.error = errorState;
            state.loading.isLoading = false;
          });
        }
      },

      clearCache: async () => {
        await apiClient.clearCache();
        set(state => {
          state.menus = [];
          state.records = [];
          state.dashboardStats = null;
        });
      },

      loadPersistedData: async () => {
        try {
          // お気に入りを読み込み
          const favoritesData = await AsyncStorage.getItem('@TrecPlans:favorites');
          if (favoritesData) {
            const favorites = JSON.parse(favoritesData);
            set(state => {
              state.favoriteMenuIds = new Set(favorites);
            });
          }

          // その他の永続化データを読み込み
          
          // Search history
          const searchHistoryData = await AsyncStorage.getItem('@TrecPlans:searchHistory');
          if (searchHistoryData) {
            const searchHistory = JSON.parse(searchHistoryData);
            set(state => {
              state.searchHistory = searchHistory.slice(0, 10); // Keep last 10 searches
            });
          }
          
          // Filter preferences
          const filterPrefsData = await AsyncStorage.getItem('@TrecPlans:filterPrefs');
          if (filterPrefsData) {
            const filterPrefs = JSON.parse(filterPrefsData);
            set(state => {
              state.filters = { ...state.filters, ...filterPrefs };
            });
          }
          
          // Last training session data
          const lastSessionData = await AsyncStorage.getItem('@TrecPlans:lastSession');
          if (lastSessionData) {
            const lastSession = JSON.parse(lastSessionData);
            set(state => {
              // Restore incomplete session if it exists and is recent (within 2 hours)
              if (lastSession.status === 'active' && 
                  Date.now() - new Date(lastSession.startTime).getTime() < 2 * 60 * 60 * 1000) {
                state.currentSession = lastSession;
              }
            });
          }

        } catch (error) {
          console.warn('Failed to load persisted data:', error);
        }
      },

      persistData: async () => {
        try {
          const state = get();
          
          // 重要なデータを永続化
          await AsyncStorage.multiSet([
            ['@TrecPlans:favorites', JSON.stringify(Array.from(state.favoriteMenuIds))],
            ['@TrecPlans:lastSync', state.lastSyncAt?.toISOString() || ''],
            ['@TrecPlans:searchHistory', JSON.stringify(state.searchHistory || [])],
            ['@TrecPlans:filterPrefs', JSON.stringify({
              sortBy: state.filters.sortBy,
              sortOrder: state.filters.sortOrder,
              bodyPart: state.filters.bodyPart,
              difficulty: state.filters.difficulty,
            })],
            ['@TrecPlans:lastSession', JSON.stringify(state.currentSession || {})],
          ]);

        } catch (error) {
          console.warn('Failed to persist data:', error);
        }
      },

      // ===================================
      // Selectors
      // ===================================
      getFilteredMenus: () => {
        const state = get();
        let filtered = state.menus;

        // 検索フィルター
        if (state.searchQuery) {
          const query = state.searchQuery.toLowerCase();
          filtered = filtered.filter(menu =>
            menu.jpName.toLowerCase().includes(query) ||
            menu.enName.toLowerCase().includes(query) ||
            menu.description?.toLowerCase().includes(query) ||
            menu.targetAreas.some(area => area.toLowerCase().includes(query))
          );
        }

        // その他のフィルター
        if (state.filters.difficulty?.length) {
          filtered = filtered.filter(menu => state.filters.difficulty!.includes(menu.difficulty));
        }

        if (state.filters.targetAreas?.length) {
          filtered = filtered.filter(menu =>
            menu.targetAreas.some(area => state.filters.targetAreas!.includes(area))
          );
        }

        // ソート
        if (state.filters.sortBy) {
          filtered.sort((a, b) => {
            let aValue: any, bValue: any;
            
            switch (state.filters.sortBy) {
              case 'name':
                aValue = a.jpName;
                bValue = b.jpName;
                break;
              case 'popularity':
                aValue = a.popularity;
                bValue = b.popularity;
                break;
              case 'difficulty':
                const difficultyOrder = { beginner: 1, intermediate: 2, advanced: 3 };
                aValue = difficultyOrder[a.difficulty];
                bValue = difficultyOrder[b.difficulty];
                break;
              default:
                return 0;
            }

            if (aValue < bValue) return state.filters.sortOrder === 'asc' ? -1 : 1;
            if (aValue > bValue) return state.filters.sortOrder === 'asc' ? 1 : -1;
            return 0;
          });
        }

        return filtered;
      },

      getMenuById: (menuId) => {
        return get().menus.find(menu => menu.menuId === menuId);
      },

      getRecordsByMenuId: (menuId) => {
        return get().records.filter(record => record.menuId === menuId);
      },

      getRecordsByDateRange: (from, to) => {
        return get().records.filter(record => {
          const recordDate = new Date(record.date);
          return recordDate >= from && recordDate <= to;
        });
      },

      getActiveGoals: () => {
        return get().goals.filter(goal => goal.status === 'active');
      },

      getRecentAchievements: (limit = 10) => {
        return get().achievements
          .sort((a, b) => b.unlockedAt.getTime() - a.unlockedAt.getTime())
          .slice(0, limit);
      },

      getProgressForMenu: (menuId) => {
        return get().progressCharts.find(chart => chart.title.includes(menuId));
      },

      getPersonalRecords: () => {
        const records = get().records;
        const personalRecords: Array<{ menuId: string; type: string; value: number; date: Date }> = [];

        // メニューごとに最大重量、最大回数、最大ボリュームを計算
        const menuGroups = records.reduce((acc, record) => {
          if (!acc[record.menuId]) acc[record.menuId] = [];
          acc[record.menuId].push(record);
          return acc;
        }, {} as Record<string, TrainingRecord[]>);

        Object.entries(menuGroups).forEach(([menuId, menuRecords]) => {
          // 最大重量
          const maxWeightRecord = menuRecords.reduce((max, record) => {
            const recordMaxWeight = Math.max(...record.sets.map(set => set.weight || 0));
            const currentMaxWeight = Math.max(...max.sets.map(set => set.weight || 0));
            return recordMaxWeight > currentMaxWeight ? record : max;
          });
          
          personalRecords.push({
            menuId,
            type: 'maxWeight',
            value: Math.max(...maxWeightRecord.sets.map(set => set.weight || 0)),
            date: new Date(maxWeightRecord.date),
          });

          // 最大ボリューム
          const maxVolumeRecord = menuRecords.reduce((max, record) => {
            return record.totalVolume > max.totalVolume ? record : max;
          });
          
          personalRecords.push({
            menuId,
            type: 'maxVolume',
            value: maxVolumeRecord.totalVolume,
            date: new Date(maxVolumeRecord.date),
          });
        });

        return personalRecords;
      },

      // ===================================
      // Helper Methods
      // ===================================
      handleError: (error: Error) => {
        console.error('[TrainingStore] Error:', error);

        if (error instanceof ApiError) {
          return {
            hasError: true,
            message: error.userMessage,
            code: error.code,
            retryable: error.isRetryable,
            timestamp: new Date(),
          };
        } else if (error instanceof NetworkError) {
          return {
            hasError: true,
            message: error.userMessage,
            code: 'NETWORK_ERROR',
            retryable: error.isRetryable,
            timestamp: new Date(),
          };
        } else {
          return {
            hasError: true,
            message: '予期しないエラーが発生しました',
            code: 'UNKNOWN_ERROR',
            retryable: true,
            timestamp: new Date(),
          };
        }
      },
    }))
  )
);

// ===================================
// Convenience Hooks
// ===================================

/**
 * メニュー関連の状態と操作
 */
export const useMenus = () => {
  const store = useTrainingStore();
  return {
    menus: store.getFilteredMenus(),
    allMenus: store.menus,
    loading: store.loading,
    error: store.error,
    searchQuery: store.searchQuery,
    filters: store.filters,
    favoriteMenuIds: store.favoriteMenuIds,
    
    fetchMenus: store.fetchMenus,
    setSearchQuery: store.setSearchQuery,
    updateFilters: store.updateFilters,
    clearFilters: store.clearFilters,
    toggleFavorite: store.toggleFavorite,
    getMenuById: store.getMenuById,
  };
};

/**
 * ワークアウトセッション関連の状態と操作
 */
export const useWorkoutSession = () => {
  const store = useTrainingStore();
  return {
    currentSession: store.currentSession,
    
    startWorkoutSession: store.startWorkoutSession,
    updateCurrentSession: store.updateCurrentSession,
    pauseCurrentSession: store.pauseCurrentSession,
    resumeCurrentSession: store.resumeCurrentSession,
    completeCurrentSession: store.completeCurrentSession,
    cancelCurrentSession: store.cancelCurrentSession,
  };
};

/**
 * 統計とダッシュボード関連の状態
 */
export const useDashboard = () => {
  const store = useTrainingStore();
  return {
    dashboardStats: store.dashboardStats,
    loading: store.loading,
    error: store.error,
    
    fetchDashboardStats: store.fetchDashboardStats,
    getPersonalRecords: store.getPersonalRecords,
  };
};