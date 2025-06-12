import React, { useCallback, useEffect, useMemo, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  TouchableOpacity,
  ActivityIndicator,
  RefreshControl,
  Pressable,
  TextInput,
  StatusBar,
  Platform,
  SectionList,
  Dimensions,
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
import { Gesture, GestureDetector } from 'react-native-gesture-handler';
import Icon from 'react-native-vector-icons/MaterialIcons';
import { LineChart } from 'react-native-chart-kit';
import {
  format,
  parseISO,
  startOfWeek,
  endOfWeek,
  startOfMonth,
  endOfMonth,
  isWithinInterval,
  differenceInDays,
  formatDistanceToNow,
  isToday,
  isYesterday,
} from 'date-fns';
import { ja } from 'date-fns/locale';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { TrainingRecord, FilterOptions, LoadingState, ErrorState } from '../types/enhanced';
import { useTrainingStore } from '../hooks/useTrainingStore';
import { useNotifications } from '../hooks/useNotifications';

const { width } = Dimensions.get('window');
const AnimatedSectionList = Animated.createAnimatedComponent(SectionList);

/**
 * 超高度なトレーニング履歴スクリーン
 * 高度なフィルタリング、検索、分析、アニメーション対応
 */
const HistoryScreen: React.FC = () => {
  const insets = useSafeAreaInsets();
  const notifications = useNotifications();
  
  // Store hooks
  const {
    records,
    loading,
    error,
    fetchRecords,
    deleteRecord,
    getRecordsByMenuId,
    getRecordsByDateRange,
    menus,
  } = useTrainingStore();

  // Local state
  const [refreshing, setRefreshing] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedFilter, setSelectedFilter] = useState<'all' | 'week' | 'month' | 'menu'>('all');
  const [selectedMenuId, setSelectedMenuId] = useState<string | null>(null);
  const [viewMode, setViewMode] = useState<'list' | 'chart' | 'calendar'>('list');
  const [showSearch, setShowSearch] = useState(false);
  const [expandedRecordId, setExpandedRecordId] = useState<string | null>(null);
  const [selectedRecords, setSelectedRecords] = useState<Set<string>>(new Set());
  const [isSelectionMode, setIsSelectionMode] = useState(false);

  // Animation values
  const searchHeight = useSharedValue(0);
  const headerOpacity = useSharedValue(1);
  const listScale = useSharedValue(1);
  const chartRotation = useSharedValue(0);

  // ===================================
  // Computed Values
  // ===================================

  const filterOptions = useMemo(() => [
    { key: 'all', label: 'すべて', icon: 'all-inclusive' },
    { key: 'week', label: '今週', icon: 'date-range' },
    { key: 'month', label: '今月', icon: 'calendar-today' },
    { key: 'menu', label: 'メニュー別', icon: 'menu' },
  ], []);

  const viewModeOptions = useMemo(() => [
    { key: 'list', label: 'リスト', icon: 'list' },
    { key: 'chart', label: 'グラフ', icon: 'timeline' },
    { key: 'calendar', label: 'カレンダー', icon: 'calendar-view-month' },
  ], []);

  const filteredRecords = useMemo(() => {
    let filtered = records;

    // Filter by search query
    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      filtered = filtered.filter(record => {
        const menu = menus.find(m => m.menuId === record.menuId);
        return (
          menu?.jpName.toLowerCase().includes(query) ||
          menu?.enName.toLowerCase().includes(query) ||
          record.sessionNotes?.toLowerCase().includes(query)
        );
      });
    }

    // Filter by time range
    const now = new Date();
    switch (selectedFilter) {
      case 'week':
        const weekStart = startOfWeek(now, { weekStartsOn: 1 });
        const weekEnd = endOfWeek(now, { weekStartsOn: 1 });
        filtered = filtered.filter(record => 
          isWithinInterval(new Date(record.date), { start: weekStart, end: weekEnd })
        );
        break;
      case 'month':
        const monthStart = startOfMonth(now);
        const monthEnd = endOfMonth(now);
        filtered = filtered.filter(record => 
          isWithinInterval(new Date(record.date), { start: monthStart, end: monthEnd })
        );
        break;
      case 'menu':
        if (selectedMenuId) {
          filtered = filtered.filter(record => record.menuId === selectedMenuId);
        }
        break;
    }

    return filtered.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
  }, [records, searchQuery, selectedFilter, selectedMenuId, menus]);

  const groupedRecords = useMemo(() => {
    const groups: { [key: string]: TrainingRecord[] } = {};
    
    filteredRecords.forEach(record => {
      const date = new Date(record.date);
      let key: string;
      
      if (isToday(date)) {
        key = '今日';
      } else if (isYesterday(date)) {
        key = '昨日';
      } else if (differenceInDays(new Date(), date) <= 7) {
        key = format(date, 'EEEE', { locale: ja });
      } else if (differenceInDays(new Date(), date) <= 30) {
        key = format(date, 'MM/dd (EEEE)', { locale: ja });
      } else {
        key = format(date, 'yyyy/MM', { locale: ja });
      }
      
      if (!groups[key]) {
        groups[key] = [];
      }
      groups[key].push(record);
    });

    return Object.entries(groups).map(([title, data]) => ({ title, data }));
  }, [filteredRecords]);

  const chartData = useMemo(() => {
    if (filteredRecords.length === 0) return null;
    
    const last30Records = filteredRecords.slice(0, 30).reverse();
    const volumeData = last30Records.map(record => record.totalVolume / 1000); // Convert to tons
    const labels = last30Records.map(record => format(new Date(record.date), 'MM/dd'));
    
    return {
      labels: labels.length > 10 ? labels.filter((_, i) => i % Math.ceil(labels.length / 10) === 0) : labels,
      datasets: [{
        data: volumeData.length > 10 ? volumeData.filter((_, i) => i % Math.ceil(volumeData.length / 10) === 0) : volumeData,
        color: (opacity = 1) => `rgba(0, 122, 255, ${opacity})`,
        strokeWidth: 3,
      }],
    };
  }, [filteredRecords]);

  const statistics = useMemo(() => {
    if (!filteredRecords.length) return null;
    
    const totalWorkouts = filteredRecords.length;
    const totalVolume = filteredRecords.reduce((sum, r) => sum + r.totalVolume, 0);
    const totalSets = filteredRecords.reduce((sum, r) => sum + r.sets.length, 0);
    const avgVolumePerWorkout = totalVolume / totalWorkouts;
    
    const menuFrequency = filteredRecords.reduce((acc, record) => {
      acc[record.menuId] = (acc[record.menuId] || 0) + 1;
      return acc;
    }, {} as Record<string, number>);
    
    const mostFrequentMenuId = Object.entries(menuFrequency)
      .sort(([,a], [,b]) => b - a)[0]?.[0];
    const mostFrequentMenu = menus.find(m => m.menuId === mostFrequentMenuId);
    
    return {
      totalWorkouts,
      totalVolume,
      totalSets,
      avgVolumePerWorkout,
      mostFrequentMenu: mostFrequentMenu?.jpName || '不明',
    };
  }, [filteredRecords, menus]);

  // ===================================
  // Animation Styles
  // ===================================

  const searchAnimatedStyle = useAnimatedStyle(() => ({
    height: searchHeight.value,
    opacity: searchHeight.value > 0 ? 1 : 0,
  }));

  const headerAnimatedStyle = useAnimatedStyle(() => ({
    opacity: headerOpacity.value,
    transform: [{ translateY: withSpring(headerOpacity.value === 1 ? 0 : -10) }],
  }));

  const listAnimatedStyle = useAnimatedStyle(() => ({
    transform: [{ scale: listScale.value }],
  }));

  const chartAnimatedStyle = useAnimatedStyle(() => ({
    transform: [{ rotateY: `${chartRotation.value}deg` }],
  }));

  // ===================================
  // Effects
  // ===================================

  useEffect(() => {
    fetchRecords().catch((err) => {
      notifications.fromApiError(err);
    });
  }, [fetchRecords, notifications]);

  useEffect(() => {
    if (error.hasError) {
      notifications.error(error.message, {
        actions: error.retryable ? [
          {
            text: '再試行',
            onPress: () => {
              fetchRecords();
            },
          },
        ] : undefined,
      });
    }
  }, [error, notifications, fetchRecords]);

  // ===================================
  // Event Handlers
  // ===================================

  const handleRefresh = useCallback(async () => {
    setRefreshing(true);
    try {
      await fetchRecords();
      notifications.success('履歴を更新しました');
    } catch (err) {
      notifications.fromApiError(err);
    } finally {
      setRefreshing(false);
    }
  }, [fetchRecords, notifications]);

  const handleSearchToggle = useCallback(() => {
    const newShowSearch = !showSearch;
    setShowSearch(newShowSearch);
    
    searchHeight.value = withSpring(newShowSearch ? 60 : 0, { damping: 15 });
    headerOpacity.value = withTiming(newShowSearch ? 0.7 : 1, { duration: 200 });
  }, [showSearch, searchHeight, headerOpacity]);

  const handleFilterPress = useCallback((filterId: 'all' | 'week' | 'month' | 'menu') => {
    setSelectedFilter(filterId);
    
    if (filterId !== 'menu') {
      setSelectedMenuId(null);
    }
    
    // Haptic feedback
    if (Platform.OS === 'ios') {
      // Add haptic feedback for iOS
    }
  }, []);

  const handleViewModeChange = useCallback((mode: 'list' | 'chart' | 'calendar') => {
    if (mode === viewMode) return;
    
    if (mode === 'chart') {
      chartRotation.value = withTiming(180, { duration: 300 }, () => {
        runOnJS(setViewMode)(mode);
        chartRotation.value = withTiming(0, { duration: 300 });
      });
    } else {
      listScale.value = withSpring(0.9, { damping: 15 }, () => {
        runOnJS(setViewMode)(mode);
        listScale.value = withSpring(1, { damping: 15 });
      });
    }
  }, [viewMode, chartRotation, listScale]);

  const handleRecordPress = useCallback((record: TrainingRecord) => {
    if (isSelectionMode) {
      const newSelected = new Set(selectedRecords);
      if (newSelected.has(record.recordId)) {
        newSelected.delete(record.recordId);
      } else {
        newSelected.add(record.recordId);
      }
      setSelectedRecords(newSelected);
    } else {
      setExpandedRecordId(expandedRecordId === record.recordId ? null : record.recordId);
    }
  }, [isSelectionMode, selectedRecords, expandedRecordId]);

  const handleRecordLongPress = useCallback((record: TrainingRecord) => {
    if (!isSelectionMode) {
      setIsSelectionMode(true);
      setSelectedRecords(new Set([record.recordId]));
    }
  }, [isSelectionMode]);

  const handleDeleteSelected = useCallback(async () => {
    if (selectedRecords.size === 0) return;
    
    notifications.confirm(
      `選択した${selectedRecords.size}件の記録を削除しますか？`,
      async () => {
        try {
          for (const recordId of selectedRecords) {
            await deleteRecord(recordId);
          }
          notifications.success(`${selectedRecords.size}件の記録を削除しました`);
          setSelectedRecords(new Set());
          setIsSelectionMode(false);
        } catch (err) {
          notifications.fromApiError(err);
        }
      },
      () => {
        // Cancel
      }
    );
  }, [selectedRecords, deleteRecord, notifications]);

  const handleClearSelection = useCallback(() => {
    setSelectedRecords(new Set());
    setIsSelectionMode(false);
  }, []);

  const handleMenuSelect = useCallback((menuId: string) => {
    setSelectedMenuId(menuId);
  }, []);

  // ===================================
  // Render Functions
  // ===================================

  const renderHeader = () => (
    <Animated.View style={[styles.header, { paddingTop: insets.top }, headerAnimatedStyle]}>
      <View style={styles.headerContent}>
        <View style={styles.titleContainer}>
          <Text style={styles.headerTitle}>トレーニング履歴</Text>
          <Text style={styles.headerSubtitle}>
            {filteredRecords.length}件の記録
            {statistics && ` · 総ボリューム ${Math.round(statistics.totalVolume / 1000)}t`}
          </Text>
        </View>
        
        <View style={styles.headerActions}>
          <TouchableOpacity 
            style={styles.actionButton}
            onPress={handleSearchToggle}
            activeOpacity={0.7}
          >
            <Icon name={showSearch ? 'close' : 'search'} size={24} color="#007AFF" />
          </TouchableOpacity>
          
          {isSelectionMode ? (
            <TouchableOpacity 
              style={[styles.actionButton, { backgroundColor: '#dc3545' }]}
              onPress={handleDeleteSelected}
              activeOpacity={0.7}
            >
              <Icon name="delete" size={24} color="white" />
            </TouchableOpacity>
          ) : (
            <TouchableOpacity 
              style={styles.actionButton}
              onPress={() => notifications.info('エクスポート', { message: 'この機能は開発中です' })}
              activeOpacity={0.7}
            >
              <Icon name="ios-share" size={24} color="#007AFF" />
            </TouchableOpacity>
          )}
        </View>
      </View>

      {/* Search Input */}
      <Animated.View style={[styles.searchContainer, searchAnimatedStyle]}>
        <View style={styles.searchInputContainer}>
          <Icon name="search" size={20} color="#666" style={styles.searchIcon} />
          <TextInput
            style={styles.searchInput}
            placeholder="記録を検索..."
            value={searchQuery}
            onChangeText={setSearchQuery}
            placeholderTextColor="#999"
            returnKeyType="search"
          />
          {searchQuery.length > 0 && (
            <TouchableOpacity onPress={() => setSearchQuery('')}>
              <Icon name="close" size={20} color="#666" />
            </TouchableOpacity>
          )}
        </View>
      </Animated.View>
    </Animated.View>
  );

  const renderFilters = () => (
    <View style={styles.filtersContainer}>
      <FlatList
        horizontal
        showsHorizontalScrollIndicator={false}
        data={filterOptions}
        keyExtractor={(item) => item.key}
        renderItem={({ item, index }) => (
          <Animated.View entering={SlideInDown.delay(index * 50)}>
            <Pressable
              style={[
                styles.filterChip,
                selectedFilter === item.key && styles.filterChipActive,
              ]}
              onPress={() => handleFilterPress(item.key as any)}
            >
              <Icon 
                name={item.icon} 
                size={16} 
                color={selectedFilter === item.key ? 'white' : '#007AFF'} 
              />
              <Text 
                style={[
                  styles.filterChipText,
                  selectedFilter === item.key && styles.filterChipTextActive,
                ]}
              >
                {item.label}
              </Text>
            </Pressable>
          </Animated.View>
        )}
        contentContainerStyle={styles.filtersContent}
      />
      
      <View style={styles.viewModeSelector}>
        {viewModeOptions.map((option, index) => (
          <Animated.View key={option.key} entering={SlideInDown.delay(index * 30)}>
            <Pressable
              style={[
                styles.viewModeButton,
                viewMode === option.key && styles.viewModeButtonActive,
              ]}
              onPress={() => handleViewModeChange(option.key as any)}
            >
              <Icon 
                name={option.icon} 
                size={18} 
                color={viewMode === option.key ? 'white' : '#007AFF'} 
              />
            </Pressable>
          </Animated.View>
        ))}
      </View>
    </View>
  );

  const renderStatistics = () => {
    if (!statistics) return null;
    
    return (
      <View style={styles.statsContainer}>
        <Text style={styles.statsTitle}>統計情報</Text>
        <View style={styles.statsGrid}>
          <View style={styles.statItem}>
            <Icon name="fitness-center" size={20} color="#007AFF" />
            <Text style={styles.statValue}>{statistics.totalWorkouts}</Text>
            <Text style={styles.statLabel}>ワークアウト</Text>
          </View>
          <View style={styles.statItem}>
            <Icon name="trending-up" size={20} color="#28a745" />
            <Text style={styles.statValue}>{Math.round(statistics.totalVolume / 1000)}t</Text>
            <Text style={styles.statLabel}>総ボリューム</Text>
          </View>
          <View style={styles.statItem}>
            <Icon name="repeat" size={20} color="#ffc107" />
            <Text style={styles.statValue}>{statistics.totalSets}</Text>
            <Text style={styles.statLabel}>総セット数</Text>
          </View>
          <View style={styles.statItem}>
            <Icon name="star" size={20} color="#dc3545" />
            <Text style={styles.statValue} numberOfLines={1}>{statistics.mostFrequentMenu}</Text>
            <Text style={styles.statLabel}>人気メニュー</Text>
          </View>
        </View>
      </View>
    );
  };

  const renderChart = () => {
    if (!chartData) {
      return (
        <View style={styles.noDataContainer}>
          <Icon name="insert-chart" size={64} color="#ddd" />
          <Text style={styles.noDataText}>データが不十分です</Text>
        </View>
      );
    }

    return (
      <Animated.View style={[styles.chartContainer, chartAnimatedStyle]}>
        <Text style={styles.chartTitle}>ボリューム推移 (最近30回)</Text>
        <LineChart
          data={chartData}
          width={width - 32}
          height={220}
          chartConfig={{
            backgroundColor: '#ffffff',
            backgroundGradientFrom: '#ffffff',
            backgroundGradientTo: '#ffffff',
            decimalPlaces: 1,
            color: (opacity = 1) => `rgba(0, 122, 255, ${opacity})`,
            labelColor: (opacity = 1) => `rgba(0, 0, 0, ${opacity})`,
            style: { borderRadius: 16 },
            propsForDots: {
              r: '4',
              strokeWidth: '2',
              stroke: '#007AFF',
            },
          }}
          bezier
          style={styles.chart}
        />
      </Animated.View>
    );
  };

  const renderRecord = ({ item }: { item: TrainingRecord }) => {
    const menu = menus.find(m => m.menuId === item.menuId);
    const isSelected = selectedRecords.has(item.recordId);
    const isExpanded = expandedRecordId === item.recordId;
    
    const totalSets = item.sets.length;
    const totalReps = item.sets.reduce((sum, set) => sum + set.reps, 0);
    const maxWeight = Math.max(...item.sets.map(set => set.weight || 0));
    
    const longPressGesture = Gesture.LongPress()
      .minDuration(500)
      .onStart(() => {
        runOnJS(handleRecordLongPress)(item);
      });

    return (
      <GestureDetector gesture={longPressGesture}>
        <Animated.View entering={FadeIn}>
          <TouchableOpacity
            style={[
              styles.recordCard,
              isSelected && styles.recordCardSelected,
              isExpanded && styles.recordCardExpanded,
            ]}
            onPress={() => handleRecordPress(item)}
            activeOpacity={0.8}
          >
            {isSelectionMode && (
              <View style={styles.selectionIndicator}>
                <Icon 
                  name={isSelected ? 'check-circle' : 'radio-button-unchecked'} 
                  size={24} 
                  color={isSelected ? '#007AFF' : '#ccc'} 
                />
              </View>
            )}
            
            <View style={styles.recordHeader}>
              <View style={styles.recordTitleContainer}>
                <Text style={styles.menuName}>{menu?.jpName || 'Unknown Menu'}</Text>
                <Text style={styles.date}>
                  {format(new Date(item.date), 'MM/dd (E)', { locale: ja })}
                </Text>
              </View>
              
              <View style={styles.recordBadge}>
                <Text style={styles.recordBadgeText}>PR</Text>
              </View>
            </View>
            
            <View style={styles.recordStats}>
              <View style={styles.recordStatItem}>
                <Icon name="fitness-center" size={16} color="#666" />
                <Text style={styles.recordStatText}>{totalSets}セット</Text>
              </View>
              <View style={styles.recordStatItem}>
                <Icon name="repeat" size={16} color="#666" />
                <Text style={styles.recordStatText}>{totalReps}回</Text>
              </View>
              <View style={styles.recordStatItem}>
                <Icon name="trending-up" size={16} color="#666" />
                <Text style={styles.recordStatText}>{item.totalVolume}kg</Text>
              </View>
              <View style={styles.recordStatItem}>
                <Icon name="fitness-center" size={16} color="#666" />
                <Text style={styles.recordStatText}>最大{maxWeight}kg</Text>
              </View>
            </View>

            {isExpanded && (
              <Animated.View style={styles.recordDetails} entering={FadeIn.duration(200)}>
                <Text style={styles.setsTitle}>セット詳細</Text>
                {item.sets.map((set, index) => (
                  <View key={index} style={styles.setDetailItem}>
                    <View style={styles.setNumber}>
                      <Text style={styles.setNumberText}>{set.setNumber}</Text>
                    </View>
                    <View style={styles.setDetails}>
                      <Text style={styles.setDetailText}>
                        {set.weight}kg × {set.reps}回
                      </Text>
                      {set.restTime && (
                        <Text style={styles.setRestTime}>
                          休憩 {set.restTime}s
                        </Text>
                      )}
                    </View>
                    <Text style={styles.setVolume}>
                      {(set.weight || 0) * set.reps}kg
                    </Text>
                  </View>
                ))}
                
                {item.sessionNotes && (
                  <View style={styles.notesContainer}>
                    <Text style={styles.notesTitle}>メモ</Text>
                    <Text style={styles.notesText}>{item.sessionNotes}</Text>
                  </View>
                )}
                
                <View style={styles.recordActions}>
                  <TouchableOpacity style={styles.actionButton} onPress={() => {
                    notifications.info('編集', { message: 'この機能は開発中です' });
                  }}>
                    <Icon name="edit" size={16} color="#007AFF" />
                    <Text style={styles.actionButtonText}>編集</Text>
                  </TouchableOpacity>
                  
                  <TouchableOpacity style={styles.actionButton} onPress={() => {
                    notifications.info('コピー', { message: 'この機能は開発中です' });
                  }}>
                    <Icon name="content-copy" size={16} color="#28a745" />
                    <Text style={styles.actionButtonText}>コピー</Text>
                  </TouchableOpacity>
                  
                  <TouchableOpacity style={styles.actionButton} onPress={() => {
                    notifications.confirm(
                      'この記録を削除しますか？',
                      async () => {
                        try {
                          await deleteRecord(item.recordId);
                          notifications.success('記録を削除しました');
                        } catch (err) {
                          notifications.fromApiError(err);
                        }
                      }
                    );
                  }}>
                    <Icon name="delete" size={16} color="#dc3545" />
                    <Text style={[styles.actionButtonText, { color: '#dc3545' }]}>削除</Text>
                  </TouchableOpacity>
                </View>
              </Animated.View>
            )}
          </TouchableOpacity>
        </Animated.View>
      </GestureDetector>
    );
  };

  const renderSectionHeader = ({ section }: { section: { title: string } }) => (
    <Animated.View style={styles.sectionHeader} entering={SlideInDown}>
      <Text style={styles.sectionHeaderText}>{section.title}</Text>
    </Animated.View>
  );

  const renderEmptyState = () => (
    <Animated.View style={styles.emptyContainer} entering={FadeIn}>
      <Icon name="history" size={64} color="#ddd" />
      <Text style={styles.emptyTitle}>トレーニング履歴がありません</Text>
      <Text style={styles.emptySubtitle}>
        {searchQuery.length > 0 || selectedFilter !== 'all' 
          ? 'フィルターを変更してみてください'
          : 'ワークアウトを記録してみましょう'
        }
      </Text>
      <TouchableOpacity style={styles.emptyButton} onPress={() => {
        if (searchQuery || selectedFilter !== 'all') {
          setSearchQuery('');
          setSelectedFilter('all');
          setSelectedMenuId(null);
        }
      }}>
        <Text style={styles.emptyButtonText}>
          {searchQuery.length > 0 || selectedFilter !== 'all' ? 'フィルターをリセット' : 'ワークアウトを開始'}
        </Text>
      </TouchableOpacity>
    </Animated.View>
  );

  const renderLoading = () => (
    <View style={styles.loadingContainer}>
      <ActivityIndicator size="large" color="#007AFF" />
      <Text style={styles.loadingText}>
        {loading.message || '履歴を読み込み中...'}
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
      
      {!loading.isLoading && renderFilters()}
      
      {isSelectionMode && (
        <Animated.View style={styles.selectionBar} entering={SlideInDown}>
          <Text style={styles.selectionText}>
            {selectedRecords.size}件選択中
          </Text>
          <TouchableOpacity onPress={handleClearSelection}>
            <Text style={styles.selectionCancel}>キャンセル</Text>
          </TouchableOpacity>
        </Animated.View>
      )}
      
      {loading.isLoading ? renderLoading() : (
        <Animated.View style={[styles.content, listAnimatedStyle]}>
          {viewMode === 'chart' ? (
            <View style={styles.chartView}>
              {renderStatistics()}
              {renderChart()}
            </View>
          ) : (
            <AnimatedSectionList
              sections={groupedRecords}
              keyExtractor={(item) => item.recordId}
              renderItem={renderRecord}
              renderSectionHeader={renderSectionHeader}
              refreshControl={
                <RefreshControl
                  refreshing={refreshing}
                  onRefresh={handleRefresh}
                  colors={['#007AFF']}
                  tintColor="#007AFF"
                />
              }
              contentContainerStyle={styles.listContent}
              showsVerticalScrollIndicator={false}
              ListEmptyComponent={renderEmptyState}
              stickySectionHeadersEnabled={true}
              // Performance optimizations
              removeClippedSubviews={true}
              maxToRenderPerBatch={10}
              updateCellsBatchingPeriod={50}
              windowSize={10}
            />
          )}
        </Animated.View>
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
    fontSize: 24,
    fontWeight: 'bold',
    color: '#212529',
  },
  headerSubtitle: {
    fontSize: 14,
    color: '#6c757d',
    marginTop: 2,
  },
  headerActions: {
    flexDirection: 'row',
    gap: 8,
  },
  actionButton: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: '#f8f9fa',
    justifyContent: 'center',
    alignItems: 'center',
  },
  searchContainer: {
    paddingHorizontal: 20,
    paddingBottom: 16,
    overflow: 'hidden',
  },
  searchInputContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#f8f9fa',
    borderRadius: 12,
    paddingHorizontal: 12,
    height: 44,
  },
  searchIcon: {
    marginRight: 8,
  },
  searchInput: {
    flex: 1,
    fontSize: 16,
    color: '#212529',
  },
  filtersContainer: {
    backgroundColor: 'white',
    borderBottomWidth: 1,
    borderBottomColor: '#e9ecef',
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingRight: 16,
  },
  filtersContent: {
    paddingHorizontal: 20,
    paddingVertical: 12,
    gap: 8,
  },
  filterChip: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#f8f9fa',
    paddingHorizontal: 12,
    paddingVertical: 8,
    borderRadius: 20,
    marginRight: 8,
    gap: 6,
  },
  filterChipActive: {
    backgroundColor: '#007AFF',
  },
  filterChipText: {
    fontSize: 14,
    fontWeight: '500',
    color: '#007AFF',
  },
  filterChipTextActive: {
    color: 'white',
  },
  viewModeSelector: {
    flexDirection: 'row',
    backgroundColor: '#f8f9fa',
    borderRadius: 8,
    padding: 2,
  },
  viewModeButton: {
    width: 36,
    height: 36,
    borderRadius: 6,
    justifyContent: 'center',
    alignItems: 'center',
    marginLeft: 2,
  },
  viewModeButtonActive: {
    backgroundColor: '#007AFF',
  },
  selectionBar: {
    backgroundColor: '#007AFF',
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 20,
    paddingVertical: 12,
  },
  selectionText: {
    color: 'white',
    fontSize: 16,
    fontWeight: '600',
  },
  selectionCancel: {
    color: 'white',
    fontSize: 16,
    fontWeight: '500',
  },
  content: {
    flex: 1,
  },
  listContent: {
    paddingHorizontal: 16,
    paddingBottom: 20,
  },
  sectionHeader: {
    backgroundColor: '#f8f9fa',
    paddingVertical: 8,
    paddingHorizontal: 16,
    marginHorizontal: -16,
    marginTop: 16,
    marginBottom: 8,
  },
  sectionHeaderText: {
    fontSize: 16,
    fontWeight: '600',
    color: '#495057',
  },
  recordCard: {
    backgroundColor: 'white',
    borderRadius: 16,
    padding: 16,
    marginBottom: 12,
    elevation: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 2,
    position: 'relative',
  },
  recordCardSelected: {
    borderWidth: 2,
    borderColor: '#007AFF',
  },
  recordCardExpanded: {
    elevation: 4,
    shadowOpacity: 0.1,
  },
  selectionIndicator: {
    position: 'absolute',
    top: 12,
    left: 12,
    zIndex: 1,
  },
  recordHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: 12,
    marginLeft: 32, // Account for selection indicator
  },
  recordTitleContainer: {
    flex: 1,
  },
  menuName: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#212529',
  },
  date: {
    fontSize: 14,
    color: '#6c757d',
    marginTop: 2,
  },
  recordBadge: {
    backgroundColor: '#ffc107',
    paddingHorizontal: 8,
    paddingVertical: 2,
    borderRadius: 10,
  },
  recordBadgeText: {
    fontSize: 10,
    fontWeight: 'bold',
    color: 'white',
  },
  recordStats: {
    flexDirection: 'row',
    justifyContent: 'space-around',
    paddingVertical: 12,
    backgroundColor: '#f8f9fa',
    borderRadius: 12,
    marginBottom: 12,
  },
  recordStatItem: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
  },
  recordStatText: {
    fontSize: 12,
    color: '#6c757d',
    fontWeight: '500',
  },
  recordDetails: {
    marginTop: 16,
    paddingTop: 16,
    borderTopWidth: 1,
    borderTopColor: '#e9ecef',
  },
  setsTitle: {
    fontSize: 16,
    fontWeight: '600',
    color: '#212529',
    marginBottom: 12,
  },
  setDetailItem: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 8,
    paddingHorizontal: 12,
    backgroundColor: '#f8f9fa',
    borderRadius: 8,
    marginBottom: 8,
  },
  setNumber: {
    width: 32,
    height: 32,
    borderRadius: 16,
    backgroundColor: '#007AFF',
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: 12,
  },
  setNumberText: {
    fontSize: 14,
    fontWeight: 'bold',
    color: 'white',
  },
  setDetails: {
    flex: 1,
  },
  setDetailText: {
    fontSize: 16,
    fontWeight: '600',
    color: '#212529',
  },
  setRestTime: {
    fontSize: 12,
    color: '#6c757d',
    marginTop: 2,
  },
  setVolume: {
    fontSize: 14,
    fontWeight: '600',
    color: '#28a745',
  },
  notesContainer: {
    marginTop: 16,
    padding: 12,
    backgroundColor: '#e3f2fd',
    borderRadius: 8,
  },
  notesTitle: {
    fontSize: 14,
    fontWeight: '600',
    color: '#1976d2',
    marginBottom: 4,
  },
  notesText: {
    fontSize: 14,
    color: '#1976d2',
    lineHeight: 20,
  },
  recordActions: {
    flexDirection: 'row',
    justifyContent: 'space-around',
    marginTop: 16,
    paddingTop: 16,
    borderTopWidth: 1,
    borderTopColor: '#e9ecef',
  },
  actionButtonText: {
    fontSize: 12,
    fontWeight: '600',
    color: '#007AFF',
    marginTop: 4,
  },
  statsContainer: {
    backgroundColor: 'white',
    margin: 16,
    padding: 16,
    borderRadius: 16,
    elevation: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 2,
  },
  statsTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#212529',
    marginBottom: 16,
  },
  statsGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-between',
  },
  statItem: {
    width: '48%',
    alignItems: 'center',
    padding: 12,
    backgroundColor: '#f8f9fa',
    borderRadius: 12,
    marginBottom: 8,
  },
  statValue: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#212529',
    marginVertical: 4,
  },
  statLabel: {
    fontSize: 12,
    color: '#6c757d',
    textAlign: 'center',
  },
  chartView: {
    flex: 1,
  },
  chartContainer: {
    backgroundColor: 'white',
    margin: 16,
    padding: 16,
    borderRadius: 16,
    elevation: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 2,
    alignItems: 'center',
  },
  chartTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#212529',
    marginBottom: 16,
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
    fontSize: 16,
    color: '#6c757d',
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: 40,
    paddingVertical: 60,
  },
  emptyTitle: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#495057',
    marginTop: 16,
    marginBottom: 8,
  },
  emptySubtitle: {
    fontSize: 16,
    color: '#6c757d',
    textAlign: 'center',
    lineHeight: 24,
    marginBottom: 24,
  },
  emptyButton: {
    backgroundColor: '#007AFF',
    paddingHorizontal: 24,
    paddingVertical: 12,
    borderRadius: 12,
  },
  emptyButtonText: {
    color: 'white',
    fontSize: 16,
    fontWeight: '600',
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
});

export default HistoryScreen;