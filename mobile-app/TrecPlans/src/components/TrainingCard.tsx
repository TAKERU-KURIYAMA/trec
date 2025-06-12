import React, { useCallback, useMemo } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  Dimensions,
  Platform,
  Pressable,
} from 'react-native';
import Animated, {
  useSharedValue,
  useAnimatedStyle,
  withSpring,
  withTiming,
  runOnJS,
  interpolate,
} from 'react-native-reanimated';
import { Gesture, GestureDetector } from 'react-native-gesture-handler';
import Icon from 'react-native-vector-icons/MaterialIcons';
import { TrainingMenu, TrainingCardProps } from '../types/enhanced';
import { useNotifications } from '../hooks/useNotifications';

const { width } = Dimensions.get('window');
const cardWidth = (width - 48) / 2;

/**
 * 高度なトレーニングカードコンポーネント
 * アニメーション、ジェスチャー、リッチなUI対応
 */
const TrainingCard: React.FC<TrainingCardProps> = ({ 
  menu, 
  onPress, 
  onFavoritePress,
  size = 'medium',
  showStats = true,
  disabled = false 
}) => {
  const notifications = useNotifications();

  // アニメーション用の値
  const scale = useSharedValue(1);
  const opacity = useSharedValue(1);
  const favoriteScale = useSharedValue(1);
  const progressOpacity = useSharedValue(0);

  // ===================================
  // Computed Values
  // ===================================

  const cardDimensions = useMemo(() => {
    const baseWidth = cardWidth;
    const sizeMultiplier = size === 'small' ? 0.8 : size === 'large' ? 1.2 : 1;
    return {
      width: baseWidth * sizeMultiplier,
      height: baseWidth * sizeMultiplier,
    };
  }, [size]);

  const cardColor = useMemo(() => {
    return getCardColor(menu.targetAreas[0] || 'default');
  }, [menu.targetAreas]);

  const iconName = useMemo(() => {
    return getIconName(menu.targetAreas[0] || 'default');
  }, [menu.targetAreas]);

  const difficultyInfo = useMemo(() => {
    const difficultyMap = {
      beginner: { label: '初級', color: '#4CAF50', icon: 'star-border' },
      intermediate: { label: '中級', color: '#FF9800', icon: 'star-half' },
      advanced: { label: '上級', color: '#F44336', icon: 'star' },
    };
    return difficultyMap[menu.difficulty];
  }, [menu.difficulty]);

  // ===================================
  // Animation Styles
  // ===================================

  const cardAnimatedStyle = useAnimatedStyle(() => ({
    transform: [{ scale: scale.value }],
    opacity: opacity.value,
  }));

  const favoriteAnimatedStyle = useAnimatedStyle(() => ({
    transform: [{ scale: favoriteScale.value }],
  }));

  const progressAnimatedStyle = useAnimatedStyle(() => ({
    opacity: progressOpacity.value,
    transform: [{
      translateY: interpolate(progressOpacity.value, [0, 1], [20, 0])
    }],
  }));

  // ===================================
  // Gesture Handlers
  // ===================================

  const tapGesture = Gesture.Tap()
    .enabled(!disabled)
    .onBegin(() => {
      scale.value = withSpring(0.95, { damping: 15 });
    })
    .onFinalize((event) => {
      scale.value = withSpring(1, { damping: 15 });
      if (event.state === 4) { // GESTURE_STATE.ENDED
        runOnJS(handlePress)();
      }
    });

  const longPressGesture = Gesture.LongPress()
    .enabled(!disabled)
    .minDuration(500)
    .onStart(() => {
      scale.value = withSpring(0.9, { damping: 10 });
      runOnJS(handleLongPress)();
    })
    .onFinalize(() => {
      scale.value = withSpring(1, { damping: 15 });
    });

  // ===================================
  // Event Handlers
  // ===================================

  const handlePress = useCallback(() => {
    if (disabled) return;
    
    // アニメーション付きでコールバック実行
    progressOpacity.value = withTiming(1, { duration: 200 }, () => {
      progressOpacity.value = withTiming(0, { duration: 300 });
    });
    
    onPress(menu);
  }, [disabled, menu, onPress, progressOpacity]);

  const handleLongPress = useCallback(() => {
    if (disabled) return;
    
    notifications.info(`${menu.jpName}の詳細`, {
      message: `難易度: ${difficultyInfo.label}\n目安時間: ${menu.estimatedDuration}分`,
      duration: 3000,
    });
  }, [disabled, menu, difficultyInfo, notifications]);

  const handleFavoritePress = useCallback(() => {
    if (!onFavoritePress || disabled) return;
    
    // ハート形状のアニメーション
    favoriteScale.value = withSpring(1.3, { damping: 10 }, () => {
      favoriteScale.value = withSpring(1, { damping: 15 });
    });
    
    onFavoritePress(menu);
    
    const message = menu.isFavorite 
      ? 'お気に入りから削除しました' 
      : 'お気に入りに追加しました';
    
    notifications.success(message, {
      duration: 2000,
    });
  }, [onFavoritePress, disabled, menu, favoriteScale, notifications]);

  // ===================================
  // Render
  // ===================================

  if (disabled) {
    opacity.value = withTiming(0.5, { duration: 200 });
  } else {
    opacity.value = withTiming(1, { duration: 200 });
  }

  return (
    <GestureDetector gesture={Gesture.Exclusive(longPressGesture, tapGesture)}>
      <Animated.View style={[styles.container, cardAnimatedStyle]}>
        <View 
          style={[
            styles.card, 
            { 
              backgroundColor: cardColor,
              width: cardDimensions.width,
              height: cardDimensions.height,
            },
            disabled && styles.disabledCard
          ]}
        >
          {/* Background Pattern */}
          <View style={styles.backgroundPattern} />
          
          {/* Header */}
          <View style={styles.header}>
            <View style={styles.difficultyBadge}>
              <Icon name={difficultyInfo.icon} size={12} color="white" />
              <Text style={styles.difficultyText}>{difficultyInfo.label}</Text>
            </View>
            
            {onFavoritePress && (
              <Pressable onPress={handleFavoritePress} style={styles.favoriteButton}>
                <Animated.View style={favoriteAnimatedStyle}>
                  <Icon 
                    name={menu.isFavorite ? 'favorite' : 'favorite-border'} 
                    size={20} 
                    color="white" 
                  />
                </Animated.View>
              </Pressable>
            )}
          </View>

          {/* Main Content */}
          <View style={styles.content}>
            <View style={styles.iconContainer}>
              <Icon name={iconName} size={40} color="white" />
            </View>
            
            <Text style={styles.menuName} numberOfLines={2}>
              {menu.jpName}
            </Text>
            
            <Text style={styles.targetAreas} numberOfLines={1}>
              {menu.targetAreas.join(' • ')}
            </Text>
            
            {showStats && (
              <View style={styles.statsContainer}>
                <View style={styles.statItem}>
                  <Icon name="schedule" size={14} color="rgba(255,255,255,0.8)" />
                  <Text style={styles.statText}>{menu.estimatedDuration}分</Text>
                </View>
                
                {menu.popularity && (
                  <View style={styles.statItem}>
                    <Icon name="trending-up" size={14} color="rgba(255,255,255,0.8)" />
                    <Text style={styles.statText}>{menu.popularity}%</Text>
                  </View>
                )}
              </View>
            )}
          </View>

          {/* Footer */}
          <View style={styles.footer}>
            {menu.lastPerformed && (
              <Text style={styles.lastPerformed}>
                最終: {formatDate(menu.lastPerformed)}
              </Text>
            )}
            
            {menu.personalBest && (
              <View style={styles.personalBest}>
                <Icon name="emoji-events" size={12} color="#FFD700" />
                <Text style={styles.personalBestText}>
                  {menu.personalBest.maxWeight}kg
                </Text>
              </View>
            )}
          </View>

          {/* Progress Overlay */}
          <Animated.View style={[styles.progressOverlay, progressAnimatedStyle]}>
            <Icon name="play-arrow" size={24} color="white" />
          </Animated.View>
        </View>
      </Animated.View>
    </GestureDetector>
  );
};

// ===================================
// Helper Functions
// ===================================

function getIconName(targetArea: string): string {
  const iconMap: Record<string, string> = {
    '胸': 'fitness-center',
    '背中': 'accessibility-new',
    '脚': 'directions-run',
    '肩': 'sports-martial-arts',
    '腕': 'sports-handball',
    '腹': 'self-improvement',
    '有酸素': 'favorite',
    'default': 'fitness-center',
  };
  return iconMap[targetArea] || iconMap.default;
}

function getCardColor(targetArea: string): string {
  const colorMap: Record<string, string> = {
    '胸': '#FF6B6B',
    '背中': '#4ECDC4',
    '脚': '#45B7D1',
    '肩': '#F9CA24',
    '腕': '#A55EEA',
    '腹': '#26DE81',
    '有酸素': '#FF7675',
    'default': '#007AFF',
  };
  return colorMap[targetArea] || colorMap.default;
}

function formatDate(date: Date): string {
  const now = new Date();
  const diffInDays = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24));
  
  if (diffInDays === 0) return '今日';
  if (diffInDays === 1) return '昨日';
  if (diffInDays <= 7) return `${diffInDays}日前`;
  
  return date.toLocaleDateString('ja-JP', { month: 'short', day: 'numeric' });
}

// ===================================
// Styles
// ===================================

const styles = StyleSheet.create({
  container: {
    margin: 8,
  },
  card: {
    borderRadius: 20,
    padding: 16,
    elevation: 8,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.15,
    shadowRadius: 8,
    overflow: 'hidden',
    position: 'relative',
  },
  disabledCard: {
    opacity: 0.6,
  },
  backgroundPattern: {
    position: 'absolute',
    top: 0,
    right: 0,
    width: '50%',
    height: '50%',
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    borderRadius: 100,
    transform: [{ translateX: 30 }, { translateY: -30 }],
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: 12,
  },
  difficultyBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 12,
  },
  difficultyText: {
    fontSize: 10,
    fontWeight: '600',
    color: 'white',
    marginLeft: 4,
  },
  favoriteButton: {
    padding: 4,
  },
  content: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  iconContainer: {
    width: 60,
    height: 60,
    borderRadius: 30,
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 12,
  },
  menuName: {
    fontSize: 16,
    fontWeight: 'bold',
    color: 'white',
    textAlign: 'center',
    marginBottom: 6,
    lineHeight: 20,
  },
  targetAreas: {
    fontSize: 12,
    color: 'rgba(255, 255, 255, 0.9)',
    textAlign: 'center',
    marginBottom: 8,
  },
  statsContainer: {
    flexDirection: 'row',
    justifyContent: 'center',
    gap: 12,
  },
  statItem: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
  },
  statText: {
    fontSize: 11,
    color: 'rgba(255, 255, 255, 0.8)',
    fontWeight: '500',
  },
  footer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  lastPerformed: {
    fontSize: 10,
    color: 'rgba(255, 255, 255, 0.7)',
  },
  personalBest: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
  },
  personalBestText: {
    fontSize: 10,
    color: '#FFD700',
    fontWeight: '600',
  },
  progressOverlay: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: 'rgba(0, 0, 0, 0.3)',
    justifyContent: 'center',
    alignItems: 'center',
    borderRadius: 20,
  },
});

export default TrainingCard;