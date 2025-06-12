// ===================================
// Ultra Advanced Notification System
// Toast notifications, error handling, user feedback with animations
// ===================================

import { ref, reactive, computed, nextTick } from 'vue'
import type { NotificationMessage, NotificationAction } from '~/types'

/**
 * 通知の表示位置
 */
export type NotificationPosition = 'top-right' | 'top-left' | 'bottom-right' | 'bottom-left' | 'top-center' | 'bottom-center'

/**
 * 通知システムの設定
 */
interface NotificationConfig {
  /** デフォルトの表示位置 */
  position: NotificationPosition
  /** デフォルトの自動消去時間（ミリ秒） */
  defaultAutoClose: number
  /** 最大表示件数 */
  maxVisible: number
  /** アニメーション有効化 */
  enableAnimations: boolean
  /** 音声通知有効化 */
  enableSounds: boolean
}

/**
 * 内部通知状態
 */
interface InternalNotification extends NotificationMessage {
  /** 表示中フラグ */
  visible: boolean
  /** アニメーション状態 */
  animating: boolean
  /** タイマーID */
  timerId?: NodeJS.Timeout
}

/**
 * 高度な通知システムコンポーザブル
 * 
 * 機能:
 * - 複数タイプの通知（成功、情報、警告、エラー）
 * - 自動消去とマニュアル消去
 * - アクションボタン付き通知
 * - 位置指定可能
 * - アニメーション対応
 * - 音声通知（オプション）
 * - 最大表示件数制御
 * - キューイング機能
 */
export function useNotifications(config: Partial<NotificationConfig> = {}) {
  // ===================================
  // Configuration
  // ===================================
  
  const notificationConfig = reactive<NotificationConfig>({
    position: 'top-right',
    defaultAutoClose: 5000,
    maxVisible: 5,
    enableAnimations: true,
    enableSounds: false,
    ...config
  })

  // ===================================
  // State
  // ===================================

  /** 通知リスト */
  const notifications = ref<InternalNotification[]>([])
  
  /** 通知カウンター（一意ID生成用） */
  let notificationCounter = 0

  // ===================================
  // Computed Properties
  // ===================================

  /** 表示中の通知のみ */
  const visibleNotifications = computed(() => 
    notifications.value.filter(n => n.visible)
  )

  /** 通知数の統計 */
  const stats = computed(() => ({
    total: notifications.value.length,
    visible: visibleNotifications.value.length,
    byType: {
      success: notifications.value.filter(n => n.type === 'success').length,
      info: notifications.value.filter(n => n.type === 'info').length,
      warning: notifications.value.filter(n => n.type === 'warning').length,
      error: notifications.value.filter(n => n.type === 'error').length,
    }
  }))

  // ===================================
  // Core Functions
  // ===================================

  /**
   * 通知を追加
   */
  function addNotification(notification: Omit<NotificationMessage, 'id'>): string {
    const id = `notification-${++notificationCounter}`
    
    const internalNotification: InternalNotification = {
      id,
      visible: false,
      animating: false,
      autoClose: notificationConfig.defaultAutoClose,
      ...notification
    }

    // 最大表示件数を超える場合は古い通知を削除
    if (visibleNotifications.value.length >= notificationConfig.maxVisible) {
      const oldestVisible = visibleNotifications.value[0]
      if (oldestVisible) {
        removeNotification(oldestVisible.id)
      }
    }

    notifications.value.push(internalNotification)

    // アニメーション付きで表示
    nextTick(() => {
      showNotification(id)
    })

    // 自動消去の設定
    if (internalNotification.autoClose && internalNotification.autoClose > 0) {
      internalNotification.timerId = setTimeout(() => {
        removeNotification(id)
      }, internalNotification.autoClose)
    }

    // 音声通知
    if (notificationConfig.enableSounds) {
      playNotificationSound(notification.type)
    }

    console.log(`[Notification] Added ${notification.type}: ${notification.title}`)
    return id
  }

  /**
   * 通知を表示状態にする（アニメーション付き）
   */
  function showNotification(id: string): void {
    const notification = notifications.value.find(n => n.id === id)
    if (!notification) return

    notification.animating = true
    
    if (notificationConfig.enableAnimations) {
      // アニメーション後に表示状態にする
      setTimeout(() => {
        notification.visible = true
        notification.animating = false
      }, 50)
    } else {
      notification.visible = true
      notification.animating = false
    }
  }

  /**
   * 通知を削除
   */
  function removeNotification(id: string): void {
    const index = notifications.value.findIndex(n => n.id === id)
    if (index === -1) return

    const notification = notifications.value[index]

    // タイマーをクリア
    if (notification.timerId) {
      clearTimeout(notification.timerId)
    }

    if (notificationConfig.enableAnimations) {
      // アニメーション付きで削除
      notification.animating = true
      notification.visible = false
      
      setTimeout(() => {
        notifications.value.splice(index, 1)
      }, 300) // アニメーション時間
    } else {
      // 即座に削除
      notifications.value.splice(index, 1)
    }

    console.log(`[Notification] Removed: ${id}`)
  }

  /**
   * すべての通知をクリア
   */
  function clearAll(): void {
    notifications.value.forEach(n => {
      if (n.timerId) clearTimeout(n.timerId)
    })
    notifications.value = []
    console.log('[Notification] Cleared all notifications')
  }

  /**
   * 特定タイプの通知をクリア
   */
  function clearByType(type: NotificationMessage['type']): void {
    const toRemove = notifications.value.filter(n => n.type === type)
    toRemove.forEach(n => removeNotification(n.id))
    console.log(`[Notification] Cleared ${toRemove.length} ${type} notifications`)
  }

  // ===================================
  // Convenience Methods
  // ===================================

  /**
   * 成功通知を表示
   */
  function success(title: string, message?: string, options: Partial<NotificationMessage> = {}): string {
    return addNotification({
      type: 'success',
      title,
      message: message || '',
      autoClose: 4000,
      ...options
    })
  }

  /**
   * 情報通知を表示
   */
  function info(title: string, message?: string, options: Partial<NotificationMessage> = {}): string {
    return addNotification({
      type: 'info',
      title,
      message: message || '',
      autoClose: 5000,
      ...options
    })
  }

  /**
   * 警告通知を表示
   */
  function warning(title: string, message?: string, options: Partial<NotificationMessage> = {}): string {
    return addNotification({
      type: 'warning',
      title,
      message: message || '',
      autoClose: 7000,
      ...options
    })
  }

  /**
   * エラー通知を表示
   */
  function error(title: string, message?: string, options: Partial<NotificationMessage> = {}): string {
    return addNotification({
      type: 'error',
      title,
      message: message || '',
      autoClose: 0, // エラーは手動で閉じる
      ...options
    })
  }

  /**
   * 確認ダイアログ風の通知を表示
   */
  function confirm(
    title: string, 
    message: string, 
    onConfirm: () => void, 
    onCancel?: () => void
  ): string {
    const actions: NotificationAction[] = [
      {
        label: 'はい',
        handler: () => {
          onConfirm()
          removeNotification(id)
        },
        style: 'primary'
      },
      {
        label: 'いいえ',
        handler: () => {
          if (onCancel) onCancel()
          removeNotification(id)
        },
        style: 'secondary'
      }
    ]

    const id = addNotification({
      type: 'info',
      title,
      message,
      autoClose: 0,
      actions
    })

    return id
  }

  /**
   * 進行状況通知を表示
   */
  function progress(title: string, message?: string): {
    id: string
    update: (progress: number, message?: string) => void
    complete: (message?: string) => void
    error: (message: string) => void
  } {
    const id = addNotification({
      type: 'info',
      title,
      message: message || '処理中...',
      autoClose: 0
    })

    return {
      id,
      update: (progress: number, newMessage?: string) => {
        const notification = notifications.value.find(n => n.id === id)
        if (notification) {
          notification.message = newMessage || `進行状況: ${Math.round(progress)}%`
        }
      },
      complete: (newMessage?: string) => {
        removeNotification(id)
        success('完了', newMessage || '処理が完了しました')
      },
      error: (errorMessage: string) => {
        removeNotification(id)
        error('エラー', errorMessage)
      }
    }
  }

  // ===================================
  // Sound Functions
  // ===================================

  /**
   * 通知音を再生
   */
  function playNotificationSound(type: NotificationMessage['type']): void {
    if (!notificationConfig.enableSounds) return

    try {
      // Web Audio APIを使用したビープ音の生成
      const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)()
      const oscillator = audioContext.createOscillator()
      const gainNode = audioContext.createGain()

      // タイプに応じて周波数を変更
      const frequencies = {
        success: 800,
        info: 600,
        warning: 400,
        error: 200
      }

      oscillator.connect(gainNode)
      gainNode.connect(audioContext.destination)

      oscillator.frequency.setValueAtTime(frequencies[type], audioContext.currentTime)
      gainNode.gain.setValueAtTime(0.1, audioContext.currentTime)
      gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.2)

      oscillator.start(audioContext.currentTime)
      oscillator.stop(audioContext.currentTime + 0.2)
    } catch (err) {
      console.warn('[Notification] Failed to play sound:', err)
    }
  }

  // ===================================
  // API Error Helper
  // ===================================

  /**
   * APIエラーを通知として表示
   */
  function fromApiError(error: any): string {
    if (error?.userMessage) {
      return this.error('エラー', error.userMessage)
    } else if (error?.message) {
      return this.error('エラー', error.message)
    } else {
      return this.error('エラー', '予期しないエラーが発生しました')
    }
  }

  /**
   * 通知がアクティブかどうかを確認
   */
  function isActive(id: string): boolean {
    return notifications.value.some(n => n.id === id && n.visible)
  }

  /**
   * 通知を取得
   */
  function getNotification(id: string): InternalNotification | undefined {
    return notifications.value.find(n => n.id === id)
  }

  // ===================================
  // Configuration Methods
  // ===================================

  /**
   * 設定を更新
   */
  function updateConfig(newConfig: Partial<NotificationConfig>): void {
    Object.assign(notificationConfig, newConfig)
  }

  /**
   * 音声通知を切り替え
   */
  function toggleSounds(): void {
    notificationConfig.enableSounds = !notificationConfig.enableSounds
  }

  /**
   * アニメーションを切り替え
   */
  function toggleAnimations(): void {
    notificationConfig.enableAnimations = !notificationConfig.enableAnimations
  }

  // ===================================
  // Return Public API
  // ===================================

  return {
    // State (read-only)
    notifications: computed(() => visibleNotifications.value),
    config: computed(() => notificationConfig),
    stats,

    // Core functions
    addNotification,
    removeNotification,
    clearAll,
    clearByType,

    // Convenience methods
    success,
    info,
    warning,
    error,
    confirm,
    progress,

    // Utilities
    fromApiError,
    isActive,
    getNotification,

    // Configuration
    updateConfig,
    toggleSounds,
    toggleAnimations,
  }
}

// ===================================
// Global Notification Instance
// ===================================

let globalNotifications: ReturnType<typeof useNotifications> | null = null

/**
 * グローバル通知インスタンスを取得
 * アプリケーション全体で通知を共有する場合に使用
 */
export function useGlobalNotifications() {
  if (!globalNotifications) {
    globalNotifications = useNotifications({
      position: 'top-right',
      defaultAutoClose: 5000,
      maxVisible: 5,
      enableAnimations: true,
      enableSounds: false
    })
  }
  return globalNotifications
}