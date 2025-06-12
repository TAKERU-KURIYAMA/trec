import React, { useCallback, useEffect, useMemo, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  Dimensions,
  ActivityIndicator,
  RefreshControl,
  TouchableOpacity,
  Pressable,
  Platform,
  StatusBar,
} from 'react-native';
import Animated, {
  useSharedValue,
  useAnimatedStyle,
  withSpring,
  withTiming,
  FadeIn,
  SlideInDown,
  ZoomIn,
  interpolate,
  runOnJS,
} from 'react-native-reanimated';
import { LineChart, BarChart, PieChart } from 'react-native-chart-kit';
import { format, subDays, isToday, formatDistanceToNow } from 'date-fns';
import { ja } from 'date-fns/locale';
import Icon from 'react-native-vector-icons/MaterialIcons';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useDashboard } from '../hooks/useTrainingStore';
import { useNotifications } from '../hooks/useNotifications';
import { DashboardStats, Goal, Achievement } from '../types/enhanced';

const { width } = Dimensions.get('window');
const cardWidth = (width - 48) / 2;

/**
 * 超高度なダッシュボードスクリーン
 * 包括的な統計、分析、ビジュアライゼーション、アニメーション対応
 */
const DashboardScreen: React.FC = () => {
  const insets = useSafeAreaInsets();
  const notifications = useNotifications();
  
  // Store hooks
  const {
    dashboardStats,
    loading,
    error,
    fetchDashboardStats,
    getPersonalRecords,
  } = useDashboard();

  // Local state
  const [refreshing, setRefreshing] = useState(false);
  const [selectedChartType, setSelectedChartType] = useState<'line' | 'bar' | 'pie'>('line');
  const [selectedTimeRange, setSelectedTimeRange] = useState<'7d' | '30d' | '90d'>('7d');
  const [expandedCard, setExpandedCard] = useState<string | null>(null);

  // Animation values
  const headerOpacity = useSharedValue(1);
  const statsScale = useSharedValue(1);
  const chartRotation = useSharedValue(0);

  // ===================================
  // Computed Values
  // ===================================

  const timeRangeOptions = useMemo(() => [
    { key: '7d', label: '7日', days: 7 },
    { key: '30d', label: '30日', days: 30 },
    { key: '90d', label: '90日', days: 90 },
  ], []);

  const chartTypeOptions = useMemo(() => [
    { key: 'line', label: 'ライン', icon: 'timeline' },
    { key: 'bar', label: 'バー', icon: 'bar-chart' },
    { key: 'pie', label: 'パイ', icon: 'pie-chart' },
  ], []);

  const personalRecords = useMemo(() => {
    return getPersonalRecords().slice(0, 5); // Top 5 records
  }, [getPersonalRecords]);

  const weeklyProgress = useMemo(() => {
    if (!dashboardStats?.weeklyVolume) return [];
    
    const days = Array.from({ length: 7 }, (_, i) => {
      const date = subDays(new Date(), 6 - i);
      const dayData = dashboardStats.weeklyVolume.find(d => 
        format(new Date(d.date), 'yyyy-MM-dd') === format(date, 'yyyy-MM-dd')
      );
      
      return {
        date,
        volume: dayData?.volume || 0,
        workouts: dayData?.workoutCount || 0,
        isToday: isToday(date),
      };
    });
    
    return days;
  }, [dashboardStats]);

  const achievementBadges = useMemo(() => {
    if (!dashboardStats?.recentAchievements) return [];
    return dashboardStats.recentAchievements.slice(0, 3);
  }, [dashboardStats]);

  const streakInfo = useMemo(() => {
    if (!dashboardStats) return { current: 0, best: 0, isActive: false };
    
    return {
      current: dashboardStats.currentStreak,
      best: dashboardStats.bestStreak || dashboardStats.currentStreak,
      isActive: dashboardStats.lastWorkoutDate ? 
        isToday(new Date(dashboardStats.lastWorkoutDate)) : false,
    };
  }, [dashboardStats]);

  // ===================================
  // Animation Styles
  // ===================================

  const headerAnimatedStyle = useAnimatedStyle(() => ({
    opacity: headerOpacity.value,
    transform: [{ translateY: withSpring(headerOpacity.value === 1 ? 0 : -10) }],
  }));

  const statsAnimatedStyle = useAnimatedStyle(() => ({
    transform: [{ scale: statsScale.value }],
  }));

  const chartAnimatedStyle = useAnimatedStyle(() => ({
    transform: [{ rotateY: `${chartRotation.value}deg` }],
  }));

  // ===================================
  // Effects
  // ===================================

  useEffect(() => {
    fetchDashboardStats().catch((err) => {
      notifications.fromApiError(err);
    });
  }, [fetchDashboardStats, notifications]);

  useEffect(() => {
    if (error.hasError) {
      notifications.error(error.message, {
        actions: error.retryable ? [
          {
            text: '再試行',
            onPress: () => {
              fetchDashboardStats();
            },
          },
        ] : undefined,
      });
    }
  }, [error, notifications, fetchDashboardStats]);

  // ===================================
  // Event Handlers
  // ===================================

  const handleRefresh = useCallback(async () => {
    setRefreshing(true);
    try {
      await fetchDashboardStats();
      notifications.success('データを更新しました');
    } catch (err) {
      notifications.fromApiError(err);
    } finally {
      setRefreshing(false);
    }
  }, [fetchDashboardStats, notifications]);

  const handleChartTypeChange = useCallback((type: 'line' | 'bar' | 'pie') => {
    if (type === selectedChartType) return;
    
    chartRotation.value = withTiming(180, { duration: 300 }, () => {
      runOnJS(setSelectedChartType)(type);
      chartRotation.value = withTiming(0, { duration: 300 });
    });
  }, [selectedChartType, chartRotation]);

  const handleTimeRangeChange = useCallback((range: '7d' | '30d' | '90d') => {
    setSelectedTimeRange(range);
    fetchDashboardStats();
  }, [fetchDashboardStats]);

  const handleCardExpand = useCallback((cardId: string) => {
    setExpandedCard(expandedCard === cardId ? null : cardId);
    statsScale.value = withSpring(expandedCard === cardId ? 1 : 1.05, { damping: 15 });
  }, [expandedCard, statsScale]);

  const handleGoalPress = useCallback((goal: Goal) => {
    notifications.info(`目標: ${goal.title}`, {
      message: `進捗: ${Math.round(goal.progress * 100)}%\n期限: ${format(new Date(goal.targetDate), 'MM/dd', { locale: ja })}`,
      duration: 4000,
    });
  }, [notifications]);

  const handleAchievementPress = useCallback((achievement: Achievement) => {
    notifications.success(`達成バッジ: ${achievement.title}`, {
      message: achievement.description,
      duration: 3000,
    });
  }, [notifications]);

  // ===================================
  // Render Functions
  // ===================================

  const renderHeader = () => (
    <Animated.View style={[styles.header, { paddingTop: insets.top }, headerAnimatedStyle]}>
      <View style={styles.headerContent}>
        <View style={styles.titleContainer}>
          <Text style={styles.headerTitle}>ダッシュボード</Text>
          <Text style={styles.headerSubtitle}>
            {dashboardStats?.lastSyncAt ? 
              `最終更新: ${formatDistanceToNow(new Date(dashboardStats.lastSyncAt), { 
                addSuffix: true, 
                locale: ja 
              })}` : '統計を読み込み中...'
            }
          </Text>
        </View>
        
        <TouchableOpacity 
          style={styles.settingsButton}
          onPress={() => notifications.info('設定', { message: 'この機能は開発中です' })}
          activeOpacity={0.7}
        >
          <Icon name="settings" size={24} color="#007AFF" />
        </TouchableOpacity>
      </View>
    </Animated.View>
  );

  const renderTimeRangeSelector = () => (
    <View style={styles.timeRangeContainer}>
      {timeRangeOptions.map((option, index) => (
        <Animated.View key={option.key} entering={SlideInDown.delay(index * 50)}>
          <Pressable
            style={[
              styles.timeRangeButton,
              selectedTimeRange === option.key && styles.timeRangeButtonActive,
            ]}
            onPress={() => handleTimeRangeChange(option.key as any)}
          >
            <Text 
              style={[
                styles.timeRangeText,
                selectedTimeRange === option.key && styles.timeRangeTextActive,
              ]}
            >
              {option.label}
            </Text>
          </Pressable>
        </Animated.View>
      ))}
    </View>
  );

  const renderStatsCards = () => {
    if (!dashboardStats) return null;

    const stats = [
      {
        id: 'workouts',
        title: 'ワークアウト',
        value: dashboardStats.totalWorkouts.toString(),
        change: dashboardStats.workoutTrend,
        icon: 'fitness-center',
        color: '#FF6B6B',
        suffix: '回',
      },
      {
        id: 'streak',
        title: '連続記録',
        value: streakInfo.current.toString(),
        change: streakInfo.isActive ? '+1' : '0',
        icon: streakInfo.isActive ? 'local-fire-department' : 'flash-off',
        color: streakInfo.isActive ? '#4ECDC4' : '#95A5A6',
        suffix: '日',
      },
      {
        id: 'volume',
        title: '総ボリューム',
        value: Math.round(dashboardStats.totalVolume / 1000).toString(),
        change: dashboardStats.volumeTrend,
        icon: 'trending-up',
        color: '#45B7D1',
        suffix: 't',
      },
      {
        id: 'pr',
        title: '新記録',
        value: personalRecords.length.toString(),
        change: '+' + personalRecords.length,
        icon: 'emoji-events',
        color: '#F9CA24',
        suffix: '個',
      },
    ];

    return (
      <Animated.View style={[styles.statsContainer, statsAnimatedStyle]}>
        {stats.map((stat, index) => (
          <Animated.View key={stat.id} entering={ZoomIn.delay(index * 100)}>
            <TouchableOpacity
              style={[
                styles.statCard,
                { backgroundColor: stat.color },
                expandedCard === stat.id && styles.statCardExpanded,
              ]}
              onPress={() => handleCardExpand(stat.id)}
              activeOpacity={0.8}
            >
              <View style={styles.statHeader}>
                <Icon name={stat.icon} size={28} color="white" />
                {stat.change && (
                  <View style={styles.changeBadge}>
                    <Text style={styles.changeText}>{stat.change}</Text>
                  </View>
                )}
              </View>
              
              <View style={styles.statContent}>
                <Text style={styles.statValue}>
                  {stat.value}
                  <Text style={styles.statSuffix}>{stat.suffix}</Text>
                </Text>
                <Text style={styles.statLabel}>{stat.title}</Text>
              </View>
              
              {expandedCard === stat.id && (
                <Animated.View 
                  style={styles.statDetails}
                  entering={FadeIn.duration(200)}
                >
                  <Text style={styles.statDetailText}>
                    {stat.id === 'streak' && `最高記録: ${streakInfo.best}日`}
                    {stat.id === 'workouts' && '今週の目標まであと3回'}
                    {stat.id === 'volume' && '先週比 +15%'}
                    {stat.id === 'pr' && '今月の新記録達成'}
                  </Text>
                </Animated.View>
              )}
            </TouchableOpacity>
          </Animated.View>
        ))}
      </Animated.View>
    );
  };

  const renderChartSelector = () => (
    <View style={styles.chartSelector}>
      {chartTypeOptions.map((option, index) => (
        <Animated.View key={option.key} entering={SlideInDown.delay(index * 30)}>
          <Pressable
            style={[
              styles.chartButton,
              selectedChartType === option.key && styles.chartButtonActive,
            ]}
            onPress={() => handleChartTypeChange(option.key as any)}
          >
            <Icon 
              name={option.icon} 
              size={20} 
              color={selectedChartType === option.key ? 'white' : '#007AFF'} 
            />
            <Text 
              style={[
                styles.chartButtonText,
                selectedChartType === option.key && styles.chartButtonTextActive,
              ]}
            >
              {option.label}
            </Text>
          </Pressable>
        </Animated.View>
      ))}
    </View>
  );

  const renderChart = () => {
    if (!weeklyProgress.length) {
      return (
        <View style={styles.noDataContainer}>
          <Icon name="insert-chart" size={64} color="#ddd" />
          <Text style={styles.noDataText}>データがありません</Text>
          <Text style={styles.noDataSubtext}>ワークアウトを記録して統計を確認しましょう</Text>
        </View>
      );
    }

    const chartData = {
      labels: weeklyProgress.map(d => format(d.date, 'MM/dd')),
      datasets: [{
        data: weeklyProgress.map(d => d.volume / 1000), // Convert to tons
        color: (opacity = 1) => `rgba(0, 122, 255, ${opacity})`,
        strokeWidth: 3,
      }],
    };

    const barData = {
      labels: weeklyProgress.map(d => format(d.date, 'MM/dd')),
      datasets: [{
        data: weeklyProgress.map(d => d.workouts),
      }],
    };

    const pieData = weeklyProgress.filter(d => d.volume > 0).map((d, index) => ({
      name: format(d.date, 'MM/dd'),
      volume: d.volume,
      color: ['#FF6B6B', '#4ECDC4', '#45B7D1', '#F9CA24', '#A55EEA', '#26DE81', '#FD79A8'][index % 7],
      legendFontColor: '#333',
      legendFontSize: 12,
    }));

    const chartConfig = {
      backgroundColor: '#ffffff',
      backgroundGradientFrom: '#ffffff',
      backgroundGradientTo: '#ffffff',
      decimalPlaces: 1,
      color: (opacity = 1) => `rgba(0, 122, 255, ${opacity})`,
      labelColor: (opacity = 1) => `rgba(0, 0, 0, ${opacity})`,
      style: { borderRadius: 16 },
      propsForDots: {
        r: '6',
        strokeWidth: '2',
        stroke: '#007AFF',
      },
    };

    return (
      <Animated.View style={[styles.chartContent, chartAnimatedStyle]}>
        {selectedChartType === 'line' && (
          <LineChart
            data={chartData}
            width={width - 64}
            height={220}
            chartConfig={chartConfig}
            bezier
            style={styles.chart}
          />
        )}
        
        {selectedChartType === 'bar' && (
          <BarChart
            data={barData}
            width={width - 64}
            height={220}
            chartConfig={chartConfig}
            style={styles.chart}
          />
        )}
        
        {selectedChartType === 'pie' && pieData.length > 0 && (
          <PieChart
            data={pieData}
            width={width - 64}
            height={220}
            chartConfig={chartConfig}
            accessor="volume"
            backgroundColor="transparent"
            paddingLeft="15"
            center={[10, 0]}
            style={styles.chart}
          />
        )}
      </Animated.View>
    );
  };

  const renderAchievements = () => {
    if (!achievementBadges.length) return null;

    return (
      <View style={styles.achievementsContainer}>
        <Text style={styles.sectionTitle}>最近の達成バッジ</Text>
        <ScrollView horizontal showsHorizontalScrollIndicator={false}>
          {achievementBadges.map((achievement, index) => (
            <Animated.View key={achievement.id} entering={ZoomIn.delay(index * 100)}>
              <TouchableOpacity
                style={styles.achievementBadge}
                onPress={() => handleAchievementPress(achievement)}
                activeOpacity={0.8}
              >
                <View style={styles.achievementIcon}>
                  <Icon name={achievement.icon} size={24} color="#FFD700" />
                </View>
                <Text style={styles.achievementTitle} numberOfLines={2}>
                  {achievement.title}
                </Text>
                <Text style={styles.achievementDate}>
                  {formatDistanceToNow(new Date(achievement.unlockedAt), { 
                    addSuffix: true, 
                    locale: ja 
                  })}
                </Text>
              </TouchableOpacity>
            </Animated.View>
          ))}
        </ScrollView>
      </View>
    );
  };

  const renderPersonalRecords = () => {
    if (!personalRecords.length) return null;

    return (
      <View style={styles.recordsContainer}>
        <Text style={styles.sectionTitle}>パーソナルレコード</Text>
        {personalRecords.map((record, index) => (
          <Animated.View key={`${record.menuId}-${record.type}`} entering={FadeIn.delay(index * 50)}>
            <View style={styles.recordItem}>
              <View style={styles.recordIcon}>
                <Icon 
                  name={record.type === 'maxWeight' ? 'fitness-center' : 'trending-up'} 
                  size={20} 
                  color="#007AFF" 
                />
              </View>
              <View style={styles.recordContent}>
                <Text style={styles.recordTitle}>
                  {record.type === 'maxWeight' ? '最大重量' : '最大ボリューム'}
                </Text>
                <Text style={styles.recordSubtitle}>{record.menuId}</Text>
              </View>
              <View style={styles.recordValue}>
                <Text style={styles.recordValueText}>
                  {record.value}{record.type === 'maxWeight' ? 'kg' : 'kg'}
                </Text>
                <Text style={styles.recordDate}>
                  {format(record.date, 'MM/dd', { locale: ja })}
                </Text>
              </View>
            </View>
          </Animated.View>
        ))}
      </View>
    );
  };

  const renderGoals = () => {
    if (!dashboardStats?.activeGoals?.length) return null;

    return (
      <View style={styles.goalsContainer}>
        <Text style={styles.sectionTitle}>アクティブな目標</Text>
        {dashboardStats.activeGoals.slice(0, 3).map((goal, index) => (
          <Animated.View key={goal.goalId} entering={SlideInDown.delay(index * 100)}>
            <TouchableOpacity
              style={styles.goalItem}
              onPress={() => handleGoalPress(goal)}
              activeOpacity={0.7}
            >
              <View style={styles.goalHeader}>
                <Text style={styles.goalTitle}>{goal.title}</Text>
                <Text style={styles.goalProgress}>
                  {Math.round(goal.progress * 100)}%
                </Text>
              </View>
              
              <View style={styles.goalProgressBar}>
                <Animated.View 
                  style={[
                    styles.goalProgressFill,
                    { width: `${goal.progress * 100}%` }
                  ]}
                />
              </View>
              
              <Text style={styles.goalDeadline}>
                期限: {format(new Date(goal.targetDate), 'yyyy/MM/dd', { locale: ja })}
              </Text>
            </TouchableOpacity>
          </Animated.View>
        ))}
      </View>
    );
  };

  const renderLoading = () => (
    <View style={styles.loadingContainer}>
      <ActivityIndicator size="large" color="#007AFF" />
      <Text style={styles.loadingText}>
        {loading.message || 'ダッシュボードを読み込み中...'}
      </Text>
    </View>
  );

  // ===================================
  // Main Render
  // ===================================

  return (
    <View style={styles.container}>
      <StatusBar barStyle="dark-content" backgroundColor="white" />
      
      {renderHeader()}
      
      {loading.isLoading ? renderLoading() : (
        <ScrollView
          style={styles.scrollView}
          showsVerticalScrollIndicator={false}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={handleRefresh}
              colors={['#007AFF']}
              tintColor="#007AFF"
            />
          }
        >
          {renderTimeRangeSelector()}
          
          {renderStatsCards()}
          
          <View style={styles.chartContainer}>
            <View style={styles.chartHeader}>
              <Text style={styles.chartTitle}>ワークアウト統計</Text>
              {renderChartSelector()}
            </View>
            {renderChart()}
          </View>
          
          {renderAchievements()}
          
          {renderGoals()}
          
          {renderPersonalRecords()}
          
          <View style={styles.bottomSpacing} />
        </ScrollView>
      )}
    </View>
  );
};

// ===================================
// Styles
// ===================================

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f8f9fa',
  },
  scrollView: {
    flex: 1,
  },
  header: {
    backgroundColor: 'white',
    borderBottomWidth: 1,
    borderBottomColor: '#e9ecef',
    elevation: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.1,
    shadowRadius: 2,
  },
  headerContent: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 20,
    paddingVertical: 16,
  },
  titleContainer: {
    flex: 1,
  },
  headerTitle: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#212529',
  },
  headerSubtitle: {
    fontSize: 14,
    color: '#6c757d',
    marginTop: 2,
  },
  settingsButton: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: '#f8f9fa',
    justifyContent: 'center',
    alignItems: 'center',
  },
  timeRangeContainer: {
    flexDirection: 'row',
    backgroundColor: 'white',
    marginHorizontal: 16,
    marginTop: 16,
    borderRadius: 12,
    padding: 4,
    elevation: 1,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 1,
  },
  timeRangeButton: {
    flex: 1,
    paddingVertical: 8,
    paddingHorizontal: 12,
    borderRadius: 8,
    alignItems: 'center',
  },
  timeRangeButtonActive: {
    backgroundColor: '#007AFF',
  },
  timeRangeText: {
    fontSize: 14,
    fontWeight: '600',
    color: '#6c757d',
  },
  timeRangeTextActive: {
    color: 'white',
  },
  statsContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    paddingHorizontal: 8,
    marginTop: 16,
  },
  statCard: {
    width: cardWidth,
    margin: 8,
    padding: 16,
    borderRadius: 16,
    elevation: 4,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    overflow: 'hidden',
  },
  statCardExpanded: {
    minHeight: 140,
  },
  statHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: 12,
  },
  changeBadge: {
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    paddingHorizontal: 8,
    paddingVertical: 2,
    borderRadius: 10,
  },
  changeText: {
    fontSize: 12,
    fontWeight: '600',
    color: 'white',
  },
  statContent: {
    alignItems: 'center',
  },
  statValue: {
    fontSize: 32,
    fontWeight: 'bold',
    color: 'white',
    textAlign: 'center',
  },
  statSuffix: {
    fontSize: 18,
    fontWeight: '500',
  },
  statLabel: {
    fontSize: 14,
    color: 'rgba(255, 255, 255, 0.9)',
    marginTop: 4,
    textAlign: 'center',
  },
  statDetails: {
    marginTop: 12,
    padding: 8,
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    borderRadius: 8,
  },
  statDetailText: {
    fontSize: 12,
    color: 'rgba(255, 255, 255, 0.8)',
    textAlign: 'center',
  },
  chartContainer: {
    backgroundColor: 'white',
    margin: 16,
    borderRadius: 16,
    elevation: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 2,
    overflow: 'hidden',
  },
  chartHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: 16,
    borderBottomWidth: 1,
    borderBottomColor: '#f1f3f4',
  },
  chartTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#212529',
  },
  chartSelector: {
    flexDirection: 'row',
    backgroundColor: '#f8f9fa',
    borderRadius: 8,
    padding: 2,
  },
  chartButton: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 6,
    paddingHorizontal: 8,
    borderRadius: 6,
    gap: 4,
  },
  chartButtonActive: {
    backgroundColor: '#007AFF',
  },
  chartButtonText: {
    fontSize: 12,
    fontWeight: '600',
    color: '#007AFF',
  },
  chartButtonTextActive: {
    color: 'white',
  },
  chartContent: {
    padding: 16,
    alignItems: 'center',
  },
  chart: {
    borderRadius: 16,
  },
  noDataContainer: {
    height: 220,
    justifyContent: 'center',
    alignItems: 'center',
    gap: 8,
  },
  noDataText: {
    fontSize: 18,
    fontWeight: '600',
    color: '#6c757d',
  },
  noDataSubtext: {
    fontSize: 14,
    color: '#adb5bd',
    textAlign: 'center',
  },
  achievementsContainer: {
    backgroundColor: 'white',
    marginHorizontal: 16,
    marginBottom: 16,
    borderRadius: 16,
    padding: 16,
    elevation: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 2,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#212529',
    marginBottom: 16,
  },
  achievementBadge: {
    width: 120,
    padding: 12,
    marginRight: 12,
    backgroundColor: '#f8f9fa',
    borderRadius: 12,
    alignItems: 'center',
    borderWidth: 2,
    borderColor: '#FFD700',
  },
  achievementIcon: {
    width: 48,
    height: 48,
    borderRadius: 24,
    backgroundColor: '#FFF8DC',
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 8,
  },
  achievementTitle: {
    fontSize: 12,
    fontWeight: '600',
    color: '#212529',
    textAlign: 'center',
    marginBottom: 4,
  },
  achievementDate: {
    fontSize: 10,
    color: '#6c757d',
  },
  goalsContainer: {
    backgroundColor: 'white',
    marginHorizontal: 16,
    marginBottom: 16,
    borderRadius: 16,
    padding: 16,
    elevation: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 2,
  },
  goalItem: {
    padding: 12,
    backgroundColor: '#f8f9fa',
    borderRadius: 12,
    marginBottom: 12,
  },
  goalHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  goalTitle: {
    fontSize: 16,
    fontWeight: '600',
    color: '#212529',
    flex: 1,
  },
  goalProgress: {
    fontSize: 14,
    fontWeight: 'bold',
    color: '#007AFF',
  },
  goalProgressBar: {
    height: 6,
    backgroundColor: '#e9ecef',
    borderRadius: 3,
    marginBottom: 8,
    overflow: 'hidden',
  },
  goalProgressFill: {
    height: '100%',
    backgroundColor: '#007AFF',
    borderRadius: 3,
  },
  goalDeadline: {
    fontSize: 12,
    color: '#6c757d',
  },
  recordsContainer: {
    backgroundColor: 'white',
    marginHorizontal: 16,
    marginBottom: 16,
    borderRadius: 16,
    padding: 16,
    elevation: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 2,
  },
  recordItem: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: '#f1f3f4',
  },
  recordIcon: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: '#e3f2fd',
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: 12,
  },
  recordContent: {
    flex: 1,
  },
  recordTitle: {
    fontSize: 16,
    fontWeight: '600',
    color: '#212529',
  },
  recordSubtitle: {
    fontSize: 14,
    color: '#6c757d',
    marginTop: 2,
  },
  recordValue: {
    alignItems: 'flex-end',
  },
  recordValueText: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#007AFF',
  },
  recordDate: {
    fontSize: 12,
    color: '#6c757d',
    marginTop: 2,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    gap: 12,
  },
  loadingText: {
    fontSize: 16,
    color: '#6c757d',
  },
  bottomSpacing: {
    height: 20,
  },
});

export default DashboardScreen;