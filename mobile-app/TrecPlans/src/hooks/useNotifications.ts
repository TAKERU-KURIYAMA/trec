// ===================================
// Ultra Advanced React Native Notification System
// Toast notifications, haptic feedback, sound alerts
// ===================================

import { useCallback } from 'react';
import Toast from 'react-native-toast-message';
import { Vibration, Platform } from 'react-native';
import { create } from 'zustand';

/**
 * 通知タイプ
 */
export type NotificationType = 'success' | 'error' | 'warning' | 'info';

/**
 * 通知設定
 */
interface NotificationConfig {
  enableVibration: boolean;
  enableSound: boolean;
  defaultDuration: number;
  position: 'top' | 'bottom';
}

/**
 * 通知アクション
 */
interface NotificationAction {
  text: string;
  onPress: () => void;
  style?: 'default' | 'destructive' | 'cancel';
}

/**
 * 通知オプション
 */
interface NotificationOptions {
  title?: string;
  message: string;
  duration?: number;
  actions?: NotificationAction[];
  onPress?: () => void;
  vibrationPattern?: number[];
  autoHide?: boolean;
  position?: 'top' | 'bottom';
}

/**
 * 通知ストアの状態
 */
interface NotificationStoreState {
  config: NotificationConfig;
  activeNotifications: string[];
  
  updateConfig: (config: Partial<NotificationConfig>) => void;
  addActiveNotification: (id: string) => void;
  removeActiveNotification: (id: string) => void;
  clearAllNotifications: () => void;
}

/**
 * 通知ストア
 */
const useNotificationStore = create<NotificationStoreState>((set, get) => ({
  config: {
    enableVibration: true,
    enableSound: true,
    defaultDuration: 4000,
    position: 'top',
  },
  activeNotifications: [],

  updateConfig: (newConfig) => {
    set((state) => ({
      config: { ...state.config, ...newConfig }
    }));
  },

  addActiveNotification: (id) => {
    set((state) => ({
      activeNotifications: [...state.activeNotifications, id]
    }));
  },

  removeActiveNotification: (id) => {
    set((state) => ({
      activeNotifications: state.activeNotifications.filter(notifId => notifId !== id)
    }));
  },

  clearAllNotifications: () => {
    Toast.hide();
    set({ activeNotifications: [] });
  },
}));

/**
 * 高度な通知システムフック
 */
export const useNotifications = () => {
  const {
    config,
    activeNotifications,
    updateConfig,
    addActiveNotification,
    removeActiveNotification,
    clearAllNotifications,
  } = useNotificationStore();

  /**
   * バイブレーションを実行
   */
  const triggerVibration = useCallback((pattern?: number[]) => {
    if (!config.enableVibration) return;

    if (Platform.OS === 'ios') {
      // iOS: 簡単なバイブレーション
      Vibration.vibrate();
    } else {
      // Android: カスタムパターン対応
      const vibrationPattern = pattern || [0, 200, 100, 200];
      Vibration.vibrate(vibrationPattern);
    }
  }, [config.enableVibration]);

  /**
   * 音声フィードバックを実行（今後の拡張用）
   */
  const triggerSound = useCallback((type: NotificationType) => {
    if (!config.enableSound) return;
    
    // TODO: 実装
    // - react-native-sound などを使用してカスタム音声を再生
    // - タイプに応じて異なる音声を再生
  }, [config.enableSound]);

  /**
   * 基本的な通知を表示
   */
  const showNotification = useCallback((
    type: NotificationType,
    options: NotificationOptions
  ) => {
    const {
      title,
      message,
      duration = config.defaultDuration,
      onPress,
      vibrationPattern,
      autoHide = true,
      position = config.position,
    } = options;

    const notificationId = Date.now().toString() + Math.random().toString(36).substr(2, 9);

    // バイブレーションとサウンドを実行
    triggerVibration(vibrationPattern);
    triggerSound(type);

    // Toast設定を準備
    const toastConfig = {
      type,
      text1: title || getDefaultTitle(type),
      text2: message,
      position: position as any,
      visibilityTime: duration,
      autoHide,
      onPress: onPress || (() => {}),
      onShow: () => addActiveNotification(notificationId),
      onHide: () => removeActiveNotification(notificationId),
    };

    // Toastを表示
    Toast.show(toastConfig);

    return notificationId;
  }, [config, triggerVibration, triggerSound, addActiveNotification, removeActiveNotification]);

  /**
   * 成功通知
   */
  const success = useCallback((
    message: string,
    options: Omit<NotificationOptions, 'message'> = {}
  ) => {
    return showNotification('success', {
      ...options,
      message,
      title: options.title || '成功',
      vibrationPattern: [0, 100], // 短い振動
    });
  }, [showNotification]);

  /**
   * エラー通知
   */
  const error = useCallback((
    message: string,
    options: Omit<NotificationOptions, 'message'> = {}
  ) => {
    return showNotification('error', {
      ...options,
      message,
      title: options.title || 'エラー',
      duration: options.duration || 6000, // エラーは長めに表示
      vibrationPattern: [0, 200, 100, 200], // 強めの振動
    });
  }, [showNotification]);

  /**
   * 警告通知
   */
  const warning = useCallback((
    message: string,
    options: Omit<NotificationOptions, 'message'> = {}
  ) => {
    return showNotification('warning', {
      ...options,
      message,
      title: options.title || '警告',
      duration: options.duration || 5000,
      vibrationPattern: [0, 150, 50, 150],
    });
  }, [showNotification]);

  /**
   * 情報通知
   */
  const info = useCallback((
    message: string,
    options: Omit<NotificationOptions, 'message'> = {}
  ) => {
    return showNotification('info', {
      ...options,
      message,
      title: options.title || '情報',
      vibrationPattern: [0, 50], // 軽い振動
    });
  }, [showNotification]);

  /**
   * 確認ダイアログ風の通知
   */
  const confirm = useCallback((
    message: string,
    onConfirm: () => void,
    onCancel?: () => void,
    options: Partial<NotificationOptions> = {}
  ) => {
    return showNotification('info', {
      ...options,
      message,
      title: options.title || '確認',
      autoHide: false, // 手動で閉じる
      actions: [
        {
          text: 'はい',
          onPress: () => {
            onConfirm();
            Toast.hide();
          },
        },
        {
          text: 'いいえ',
          onPress: () => {
            if (onCancel) onCancel();
            Toast.hide();
          },
          style: 'cancel',
        },
      ],
    });
  }, [showNotification]);

  /**
   * 進行状況通知
   */
  const progress = useCallback((
    message: string,
    progress: number,
    options: Partial<NotificationOptions> = {}
  ) => {
    const progressMessage = `${message} (${Math.round(progress * 100)}%)`;
    
    return showNotification('info', {
      ...options,
      message: progressMessage,
      title: options.title || '処理中',
      autoHide: progress >= 1, // 完了時に自動で閉じる
      duration: progress >= 1 ? 2000 : undefined,
    });
  }, [showNotification]);

  /**
   * APIエラーを通知として表示
   */
  const fromApiError = useCallback((error: any) => {
    let message = '予期しないエラーが発生しました';
    
    if (error?.userMessage) {
      message = error.userMessage;
    } else if (error?.message) {
      message = error.message;
    }

    return error('エラー', message, {
      actions: error?.isRetryable ? [
        {
          text: '再試行',
          onPress: () => {
            // リトライロジックは呼び出し元で実装
            Toast.hide();
          },
        },
        {
          text: '閉じる',
          onPress: () => Toast.hide(),
          style: 'cancel',
        },
      ] : undefined,
    });
  }, [error]);

  /**
   * ワークアウト関連の通知
   */
  const workoutNotifications = {
    sessionStarted: (menuName: string) => 
      success(`${menuName}のワークアウトを開始しました`, {
        vibrationPattern: [0, 100, 50, 100],
      }),

    sessionPaused: () => 
      warning('ワークアウトを一時停止しました'),

    sessionResumed: () => 
      info('ワークアウトを再開しました'),

    sessionCompleted: (menuName: string, duration: number) => 
      success(`${menuName}のワークアウトが完了しました！\n所要時間: ${Math.round(duration / 60)}分`, {
        duration: 6000,
        vibrationPattern: [0, 200, 100, 200, 100, 200],
      }),

    setCompleted: (setNumber: number, reps: number, weight?: number) => {
      const weightText = weight ? ` @ ${weight}kg` : '';
      return info(`セット${setNumber}: ${reps}回${weightText} 完了`, {
        duration: 2000,
        vibrationPattern: [0, 50],
      });
    },

    restTimeUp: () => 
      warning('休憩時間が終了しました', {
        title: '次のセットへ',
        vibrationPattern: [0, 300, 100, 300],
      }),

    goalAchieved: (goalName: string) => 
      success(`目標達成: ${goalName}`, {
        title: '🎉 おめでとうございます！',
        duration: 8000,
        vibrationPattern: [0, 200, 100, 200, 100, 200, 100, 200],
      }),

    personalRecord: (type: string, value: number) => 
      success(`新記録達成！\n${type}: ${value}`, {
        title: '🏆 パーソナルレコード',
        duration: 8000,
        vibrationPattern: [0, 300, 200, 300, 200, 300],
      }),
  };

  /**
   * システム通知
   */
  const systemNotifications = {
    dataSync: (status: 'start' | 'success' | 'error') => {
      switch (status) {
        case 'start':
          return info('データを同期しています...', { duration: 2000 });
        case 'success':
          return success('データの同期が完了しました', { duration: 3000 });
        case 'error':
          return error('データの同期に失敗しました', {
            actions: [
              {
                text: '再試行',
                onPress: () => {
                  // 再同期ロジックは呼び出し元で実装
                  Toast.hide();
                },
              },
            ],
          });
      }
    },

    networkStatus: (isOnline: boolean) => {
      if (isOnline) {
        return success('インターネットに接続されました', { duration: 2000 });
      } else {
        return warning('オフラインモードで動作しています', { duration: 4000 });
      }
    },

    updateAvailable: (version: string) => 
      info(`新しいバージョン ${version} が利用可能です`, {
        actions: [
          {
            text: '更新',
            onPress: () => {
              // アップデートロジックは呼び出し元で実装
              Toast.hide();
            },
          },
          {
            text: '後で',
            onPress: () => Toast.hide(),
            style: 'cancel',
          },
        ],
      }),
  };

  /**
   * 隠す
   */
  const hide = useCallback(() => {
    Toast.hide();
  }, []);

  /**
   * 通知設定を更新
   */
  const updateSettings = useCallback((newConfig: Partial<NotificationConfig>) => {
    updateConfig(newConfig);
  }, [updateConfig]);

  return {
    // 基本機能
    success,
    error,
    warning,
    info,
    confirm,
    progress,
    hide,
    clearAll: clearAllNotifications,

    // 特殊機能
    fromApiError,
    workoutNotifications,
    systemNotifications,

    // 設定
    config,
    updateSettings,
    activeNotifications,

    // 低レベルAPI
    showNotification,
  };
};

/**
 * デフォルトタイトルを取得
 */
function getDefaultTitle(type: NotificationType): string {
  switch (type) {
    case 'success': return '成功';
    case 'error': return 'エラー';
    case 'warning': return '警告';
    case 'info': return '情報';
    default: return '';
  }
}

/**
 * グローバル通知インスタンス（便利な使い方用）
 */
let globalNotifications: ReturnType<typeof useNotifications> | null = null;

export const getGlobalNotifications = () => {
  if (!globalNotifications) {
    // Note: This should be called within a React component
    throw new Error('Global notifications must be initialized within a React component');
  }
  return globalNotifications;
};

/**
 * グローバル通知を初期化するフック
 * App.tsxで一度だけ呼び出す
 */
export const useInitializeGlobalNotifications = () => {
  const notifications = useNotifications();
  globalNotifications = notifications;
  return notifications;
};

export default useNotifications;