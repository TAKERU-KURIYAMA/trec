import React, { useCallback, useEffect, useMemo, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  TouchableOpacity,
  RefreshControl,
  ActivityIndicator,
  TextInput,
  Pressable,
  StatusBar,
  Platform,
} from 'react-native';
import Animated, {
  useSharedValue,
  useAnimatedStyle,
  withSpring,
  withTiming,
  FadeIn,
  SlideInDown,
} from 'react-native-reanimated';
import Icon from 'react-native-vector-icons/MaterialIcons';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { TrainingMenu } from '../types/enhanced';
import { useMenus, useWorkoutSession } from '../hooks/useTrainingStore';
import { useNotifications } from '../hooks/useNotifications';
import TrainingCard from '../components/TrainingCard';

const AnimatedFlatList = Animated.createAnimatedComponent(FlatList);

/**
 * 高度なホームスクリーン
 * 検索、フィルタリング、アニメーション、状態管理対応
 */
const HomeScreen: React.FC = () => {
  const insets = useSafeAreaInsets();
  const notifications = useNotifications();
  
  // Workout session management
  const { startWorkoutSession } = useWorkoutSession();
  
  // State from advanced store
  const {
    menus,
    allMenus,
    loading,
    error,
    searchQuery,
    filters,
    favoriteMenuIds,
    fetchMenus,
    setSearchQuery,
    updateFilters,
    clearFilters,
    toggleFavorite,
    getMenuById,
  } = useMenus();

  // Local UI state
  const [showSearch, setShowSearch] = useState(false);
  const [selectedFilter, setSelectedFilter] = useState<string>('all');
  const [refreshing, setRefreshing] = useState(false);

  // Animation values
  const searchHeight = useSharedValue(0);
  const headerOpacity = useSharedValue(1);

  // ===================================
  // Computed Values
  // ===================================

  const filteredMenus = useMemo(() => {
    let filtered = menus;

    // Filter by category
    if (selectedFilter !== 'all') {
      if (selectedFilter === 'favorites') {
        filtered = filtered.filter(menu => favoriteMenuIds.has(menu.menuId));
      } else {
        filtered = filtered.filter(menu => 
          menu.targetAreas.includes(selectedFilter) || 
          menu.difficulty === selectedFilter
        );
      }
    }

    return filtered;
  }, [menus, selectedFilter, favoriteMenuIds]);

  const quickFilters = useMemo(() => [
    { id: 'all', label: 'すべて', icon: 'apps', count: allMenus.length },
    { id: 'favorites', label: 'お気に入り', icon: 'favorite', count: favoriteMenuIds.size },
    { id: '胸', label: '胸', icon: 'fitness-center', count: allMenus.filter(m => m.targetAreas.includes('胸')).length },
    { id: '背中', label: '背中', icon: 'accessibility-new', count: allMenus.filter(m => m.targetAreas.includes('背中')).length },
    { id: '脚', label: '脚', icon: 'directions-run', count: allMenus.filter(m => m.targetAreas.includes('脚')).length },
    { id: 'beginner', label: '初級', icon: 'star-border', count: allMenus.filter(m => m.difficulty === 'beginner').length },
  ], [allMenus, favoriteMenuIds]);

  const stats = useMemo(() => ({
    totalMenus: allMenus.length,
    filteredCount: filteredMenus.length,
    favoriteCount: favoriteMenuIds.size,
  }), [allMenus, filteredMenus, favoriteMenuIds]);

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

  // ===================================
  // Effects
  // ===================================

  useEffect(() => {
    fetchMenus().catch((err) => {
      notifications.fromApiError(err);
    });
  }, [fetchMenus, notifications]);

  useEffect(() => {
    if (error.hasError) {
      notifications.error(error.message, {
        actions: error.retryable ? [
          {
            text: '再試行',
            onPress: () => {
              fetchMenus();
            },
          },
        ] : undefined,
      });
    }
  }, [error, notifications, fetchMenus]);

  // ===================================
  // Event Handlers
  // ===================================

  const handleRefresh = useCallback(async () => {
    setRefreshing(true);
    try {
      await fetchMenus();
      notifications.success('データを更新しました');
    } catch (err) {
      notifications.fromApiError(err);
    } finally {
      setRefreshing(false);
    }
  }, [fetchMenus, notifications]);

  const handleMenuPress = useCallback((menu: TrainingMenu) => {
    notifications.info(`${menu.jpName}を選択`, {
      message: 'ワークアウトを開始しますか？',
      actions: [
        {
          text: '開始',
          onPress: async () => {
            try {
              const sessionId = await startWorkoutSession(menu.menuId);
              notifications.workoutNotifications.sessionStarted(menu.jpName);
              notifications.success('ワークアウト開始', `${menu.jpName}のセッションを開始しました`);
              // Note: In a full implementation, this would navigate to a WorkoutSession screen
              // For now, the session is started and can be managed through the store
            } catch (error) {
              notifications.error('開始エラー', 'ワークアウトセッションの開始に失敗しました');
            }
          },
        },
        {
          text: 'キャンセル',
          onPress: () => {},
          style: 'cancel',
        },
      ],
    });
  }, [notifications]);

  const handleFavoritePress = useCallback(async (menu: TrainingMenu) => {
    await toggleFavorite(menu.menuId);
  }, [toggleFavorite]);

  const handleSearchToggle = useCallback(() => {
    const newShowSearch = !showSearch;
    setShowSearch(newShowSearch);
    
    searchHeight.value = withSpring(newShowSearch ? 60 : 0, { damping: 15 });
    headerOpacity.value = withTiming(newShowSearch ? 0.7 : 1, { duration: 200 });
  }, [showSearch, searchHeight, headerOpacity]);

  const handleFilterPress = useCallback((filterId: string) => {
    setSelectedFilter(filterId);
    
    // Haptic feedback
    if (Platform.OS === 'ios') {
      // Add haptic feedback for iOS
    }
  }, []);

  const handleSearchChange = useCallback((text: string) => {
    setSearchQuery(text);
  }, [setSearchQuery]);

  const handleClearFilters = useCallback(() => {
    setSelectedFilter('all');
    clearFilters();
    setShowSearch(false);
    searchHeight.value = withSpring(0);
    headerOpacity.value = withTiming(1);
  }, [clearFilters, searchHeight, headerOpacity]);

  // ===================================
  // Render Functions
  // ===================================

  const renderHeader = () => (
    <Animated.View style={[styles.header, { paddingTop: insets.top }, headerAnimatedStyle]}>
      <View style={styles.headerContent}>
        <View style={styles.titleContainer}>
          <Text style={styles.headerTitle}>トレーニングメニュー</Text>
          <Text style={styles.headerSubtitle}>
            {stats.filteredCount}件のメニュー
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
          
          <TouchableOpacity 
            style={styles.actionButton}
            onPress={() => notifications.info('開発中', { message: 'この機能は開発中です' })}
            activeOpacity={0.7}
          >
            <Icon name="add" size={24} color="#007AFF" />
          </TouchableOpacity>
        </View>
      </View>

      {/* Search Input */}
      <Animated.View style={[styles.searchContainer, searchAnimatedStyle]}>
        <View style={styles.searchInputContainer}>
          <Icon name="search" size={20} color="#666" style={styles.searchIcon} />
          <TextInput
            style={styles.searchInput}
            placeholder="メニューを検索..."
            value={searchQuery}
            onChangeText={handleSearchChange}
            placeholderTextColor="#999"
            returnKeyType="search"
          />
          {searchQuery.length > 0 && (
            <TouchableOpacity onPress={() => handleSearchChange('')}>
              <Icon name="close" size={20} color="#666" />
            </TouchableOpacity>
          )}
        </View>
      </Animated.View>
    </Animated.View>
  );

  const renderQuickFilters = () => (
    <Animated.View 
      style={styles.filtersContainer}
      entering={SlideInDown.delay(100)}
    >
      <FlatList
        horizontal
        showsHorizontalScrollIndicator={false}
        data={quickFilters}
        keyExtractor={(item) => item.id}
        renderItem={({ item, index }) => (
          <Animated.View entering={SlideInDown.delay(index * 50)}>
            <Pressable
              style={[
                styles.filterChip,
                selectedFilter === item.id && styles.filterChipActive,
              ]}
              onPress={() => handleFilterPress(item.id)}
            >
              <Icon 
                name={item.icon} 
                size={16} 
                color={selectedFilter === item.id ? 'white' : '#007AFF'} 
              />
              <Text 
                style={[
                  styles.filterChipText,
                  selectedFilter === item.id && styles.filterChipTextActive,
                ]}
              >
                {item.label}
              </Text>
              <Text 
                style={[
                  styles.filterChipCount,
                  selectedFilter === item.id && styles.filterChipCountActive,
                ]}
              >
                {item.count}
              </Text>
            </Pressable>
          </Animated.View>
        )}
        contentContainerStyle={styles.filtersContent}
      />
      
      {(selectedFilter !== 'all' || searchQuery.length > 0) && (
        <TouchableOpacity 
          style={styles.clearFiltersButton}
          onPress={handleClearFilters}
        >
          <Text style={styles.clearFiltersText}>クリア</Text>
        </TouchableOpacity>
      )}
    </Animated.View>
  );

  const renderMenu = ({ item, index }: { item: TrainingMenu; index: number }) => (
    <Animated.View entering={FadeIn.delay(index * 50)}>
      <TrainingCard
        menu={item}
        onPress={handleMenuPress}
        onFavoritePress={handleFavoritePress}
        showStats={true}
      />
    </Animated.View>
  );

  const renderEmptyState = () => (
    <Animated.View style={styles.emptyContainer} entering={FadeIn}>
      <Icon name="search-off" size={64} color="#ccc" />
      <Text style={styles.emptyTitle}>メニューが見つかりません</Text>
      <Text style={styles.emptySubtitle}>
        {searchQuery.length > 0 
          ? '検索条件を変更してみてください' 
          : 'フィルターを変更してみてください'
        }
      </Text>
      <TouchableOpacity style={styles.emptyButton} onPress={handleClearFilters}>
        <Text style={styles.emptyButtonText}>フィルターをリセット</Text>
      </TouchableOpacity>
    </Animated.View>
  );

  const renderLoading = () => (
    <View style={styles.loadingContainer}>
      <ActivityIndicator size="large" color="#007AFF" />
      <Text style={styles.loadingText}>
        {loading.message || 'メニューを読み込み中...'}
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
      
      {!loading.isLoading && renderQuickFilters()}
      
      {loading.isLoading ? renderLoading() : (
        <AnimatedFlatList
          data={filteredMenus}
          keyExtractor={(item) => item.menuId}
          renderItem={renderMenu}
          numColumns={2}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={handleRefresh}
              colors={['#007AFF']}
              tintColor="#007AFF"
            />
          }
          contentContainerStyle={[
            styles.listContent,
            { paddingTop: 10 }
          ]}
          showsVerticalScrollIndicator={false}
          ListEmptyComponent={renderEmptyState}
          // Performance optimizations
          removeClippedSubviews={true}
          maxToRenderPerBatch={10}
          updateCellsBatchingPeriod={50}
          windowSize={10}
        />
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
  filterChipCount: {
    fontSize: 12,
    color: '#6c757d',
    backgroundColor: 'white',
    paddingHorizontal: 6,
    paddingVertical: 2,
    borderRadius: 8,
    minWidth: 20,
    textAlign: 'center',
  },
  filterChipCountActive: {
    backgroundColor: 'rgba(255,255,255,0.2)',
    color: 'white',
  },
  clearFiltersButton: {
    position: 'absolute',
    right: 20,
    top: '50%',
    backgroundColor: '#dc3545',
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 12,
    transform: [{ translateY: -12 }],
  },
  clearFiltersText: {
    color: 'white',
    fontSize: 12,
    fontWeight: '600',
  },
  listContent: {
    paddingHorizontal: 12,
    paddingBottom: 20,
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
});

export default HomeScreen;