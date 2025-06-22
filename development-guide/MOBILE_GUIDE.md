# モバイルアプリ開発ガイド（Claude Code向け）

📱 **React Native による Message システム モバイルアプリ開発**

## 🎯 開発概要

### 技術スタック
- **Framework**: React Native 0.72+
- **Navigation**: React Navigation 6
- **State Management**: Redux Toolkit + RTK Query
- **UI Library**: React Native Elements + Tamagui
- **TypeScript**: 完全型安全
- **Testing**: Jest, Detox
- **Push Notifications**: React Native Firebase

### 開発責任範囲
- ネイティブアプリ実装
- プッシュ通知
- オフライン対応
- デバイス機能統合
- パフォーマンス最適化
- アプリストア公開準備

## 📋 実装計画

### Phase 1: 基盤構築（2-3日）
- [ ] React Native環境構築
- [ ] 認証システム基盤
- [ ] API クライアント設定
- [ ] 基本ナビゲーション設定
- [ ] 状態管理設定

### Phase 2: 認証・ユーザー管理（1-2日）
- [ ] ログイン/登録画面
- [ ] JWT トークン管理
- [ ] 生体認証統合
- [ ] セキュアストレージ

### Phase 3: トレーニング機能（3-4日）
- [ ] トレーニングメニュー表示
- [ ] 記録入力（音声入力対応）
- [ ] タイマー機能
- [ ] カメラ統合（フォーム撮影）
- [ ] オフライン記録対応

### Phase 4: サプリメント機能（2-3日）
- [ ] サプリメント登録（バーコードスキャン）
- [ ] 摂取記録とリマインダー
- [ ] プッシュ通知設定
- [ ] カレンダー統合

### Phase 5: 高度機能・リリース準備（2-3日）
- [ ] データ同期機能
- [ ] バックアップ・復元
- [ ] ウィジェット対応
- [ ] アプリアイコン・スプラッシュ
- [ ] ストア公開準備

## 🏗️ プロジェクト構造

```
mobile-app/
├── src/
│   ├── components/          # 共通コンポーネント
│   │   ├── common/          # 汎用UI
│   │   ├── training/        # トレーニング関連
│   │   ├── supplement/      # サプリメント関連
│   │   └── forms/           # フォーム関連
│   ├── screens/             # 画面コンポーネント
│   │   ├── auth/           # 認証画面
│   │   ├── dashboard/      # ダッシュボード
│   │   ├── training/       # トレーニング画面
│   │   └── supplement/     # サプリメント画面
│   ├── navigation/          # ナビゲーション設定
│   ├── services/           # API サービス
│   │   ├── api.ts          # API クライアント
│   │   ├── auth.ts         # 認証サービス
│   │   ├── storage.ts      # ローカルストレージ
│   │   └── notifications.ts # 通知サービス
│   ├── store/              # Redux ストア
│   │   ├── slices/         # Redux スライス
│   │   └── middleware/     # カスタムミドルウェア
│   ├── hooks/              # カスタムフック
│   ├── types/              # TypeScript型定義
│   ├── utils/              # ユーティリティ
│   └── constants/          # 定数
├── android/                # Android固有設定
├── ios/                    # iOS固有設定
└── __tests__/              # テストファイル
```

## 🔐 認証システム実装

### services/auth.ts
```typescript
import AsyncStorage from '@react-native-async-storage/async-storage'
import * as Keychain from 'react-native-keychain'
import TouchID from 'react-native-touch-id'

class AuthService {
  private readonly TOKEN_KEY = 'auth_token'
  private readonly REFRESH_TOKEN_KEY = 'refresh_token'
  
  async login(credentials: LoginCredentials): Promise<AuthResult> {
    try {
      const response = await api.post('/auth/login', credentials)
      
      // セキュアストレージに保存
      await this.storeTokens(response.data.accessToken, response.data.refreshToken)
      
      return {
        success: true,
        user: response.data.user,
        accessToken: response.data.accessToken
      }
    } catch (error) {
      throw new Error('ログインに失敗しました')
    }
  }

  async logout(): Promise<void> {
    try {
      await api.post('/auth/logout')
    } finally {
      await this.clearTokens()
    }
  }

  private async storeTokens(accessToken: string, refreshToken: string): Promise<void> {
    // 通常ストレージにアクセストークン
    await AsyncStorage.setItem(this.TOKEN_KEY, accessToken)
    
    // セキュアストレージにリフレッシュトークン
    await Keychain.setInternetCredentials(
      this.REFRESH_TOKEN_KEY,
      'user',
      refreshToken
    )
  }

  async enableBiometric(): Promise<void> {
    const biometryType = await TouchID.isSupported()
    if (biometryType) {
      const currentToken = await this.getAccessToken()
      if (currentToken) {
        await Keychain.setInternetCredentials(
          'biometric_auth',
          'user',
          currentToken,
          { accessControl: Keychain.ACCESS_CONTROL.BIOMETRY_ANY }
        )
      }
    }
  }

  async authenticateWithBiometric(): Promise<string | null> {
    try {
      const credentials = await Keychain.getInternetCredentials('biometric_auth')
      if (credentials) {
        return credentials.password
      }
      return null
    } catch (error) {
      return null
    }
  }
}

export const authService = new AuthService()
```

### components/auth/BiometricAuth.tsx
```typescript
import React, { useState, useEffect } from 'react'
import { View, Text, TouchableOpacity, Alert } from 'react-native'
import TouchID from 'react-native-touch-id'
import Icon from 'react-native-vector-icons/MaterialIcons'

interface BiometricAuthProps {
  onSuccess: (token: string) => void
  onSkip: () => void
}

export const BiometricAuth: React.FC<BiometricAuthProps> = ({ onSuccess, onSkip }) => {
  const [biometryType, setBiometryType] = useState<string | null>(null)

  useEffect(() => {
    checkBiometricSupport()
  }, [])

  const checkBiometricSupport = async () => {
    try {
      const type = await TouchID.isSupported()
      setBiometryType(type)
    } catch (error) {
      setBiometryType(null)
    }
  }

  const handleBiometricAuth = async () => {
    try {
      await TouchID.authenticate('ログインするために認証してください', {
        title: '生体認証',
        subtitle: 'Message アプリにアクセス',
        fallbackLabel: 'パスワードを使用',
        cancelLabel: 'キャンセル'
      })
      
      const token = await authService.authenticateWithBiometric()
      if (token) {
        onSuccess(token)
      }
    } catch (error) {
      Alert.alert('認証エラー', '生体認証に失敗しました')
    }
  }

  if (!biometryType) {
    return null
  }

  const getIconName = () => {
    switch (biometryType) {
      case 'FaceID': return 'face'
      case 'TouchID': return 'fingerprint'
      default: return 'security'
    }
  }

  return (
    <View className="items-center p-6">
      <TouchableOpacity
        onPress={handleBiometricAuth}
        className="bg-blue-600 rounded-full p-4 mb-4"
      >
        <Icon name={getIconName()} size={48} color="white" />
      </TouchableOpacity>
      
      <Text className="text-lg font-semibold mb-2">
        {biometryType} でログイン
      </Text>
      
      <Text className="text-gray-600 text-center mb-4">
        生体認証を使用してすばやくアクセス
      </Text>
      
      <TouchableOpacity onPress={onSkip} className="py-2">
        <Text className="text-blue-600">パスワードでログイン</Text>
      </TouchableOpacity>
    </View>
  )
}
```

## 🏋️ トレーニング機能実装

### screens/training/WorkoutScreen.tsx
```typescript
import React, { useState, useEffect } from 'react'
import { View, ScrollView, Alert } from 'react-native'
import { useSelector, useDispatch } from 'react-redux'
import Voice from '@react-native-voice/voice'
import { Camera } from 'react-native-camera'

import { TimerComponent } from '../../components/training/TimerComponent'
import { VoiceInput } from '../../components/training/VoiceInput'
import { WorkoutForm } from '../../components/training/WorkoutForm'
import { ExerciseCard } from '../../components/training/ExerciseCard'

export const WorkoutScreen: React.FC = () => {
  const dispatch = useDispatch()
  const { currentWorkout, exercises } = useSelector(state => state.training)
  
  const [isTimerRunning, setIsTimerRunning] = useState(false)
  const [currentSet, setCurrentSet] = useState(1)
  const [restTimer, setRestTimer] = useState(0)

  const handleSetComplete = async (exerciseId: string, setData: SetData) => {
    try {
      await dispatch(recordSet({
        exerciseId,
        setNumber: currentSet,
        ...setData,
        timestamp: new Date().toISOString()
      }))
      
      // レスト時間の開始
      if (setData.restTime > 0) {
        setRestTimer(setData.restTime)
        setIsTimerRunning(true)
      }
      
      setCurrentSet(prev => prev + 1)
    } catch (error) {
      Alert.alert('エラー', 'セットの記録に失敗しました')
    }
  }

  const handleVoiceCommand = (command: string) => {
    // 音声コマンドの処理
    const parsed = parseVoiceCommand(command)
    if (parsed) {
      handleSetComplete(parsed.exerciseId, parsed.setData)
    }
  }

  return (
    <ScrollView className="flex-1 bg-gray-50">
      <View className="p-4">
        {/* ワークアウトヘッダー */}
        <View className="bg-white rounded-lg p-4 mb-4">
          <Text className="text-xl font-bold">{currentWorkout?.name}</Text>
          <Text className="text-gray-600">セット {currentSet}</Text>
        </View>

        {/* タイマー */}
        <TimerComponent
          duration={restTimer}
          isRunning={isTimerRunning}
          onComplete={() => {
            setIsTimerRunning(false)
            setRestTimer(0)
          }}
        />

        {/* 音声入力 */}
        <VoiceInput
          onCommand={handleVoiceCommand}
          isListening={true}
        />

        {/* エクササイズリスト */}
        {exercises.map((exercise, index) => (
          <ExerciseCard
            key={exercise.id}
            exercise={exercise}
            currentSet={currentSet}
            onSetComplete={(setData) => handleSetComplete(exercise.id, setData)}
            isActive={index === 0} // 現在のエクササイズ
          />
        ))}
      </View>
    </ScrollView>
  )
}
```

### components/training/VoiceInput.tsx
```typescript
import React, { useState, useEffect } from 'react'
import { View, Text, TouchableOpacity } from 'react-native'
import Voice from '@react-native-voice/voice'
import Icon from 'react-native-vector-icons/MaterialIcons'

interface VoiceInputProps {
  onCommand: (command: string) => void
  isListening: boolean
}

export const VoiceInput: React.FC<VoiceInputProps> = ({ onCommand, isListening }) => {
  const [isRecording, setIsRecording] = useState(false)
  const [recognizedText, setRecognizedText] = useState('')

  useEffect(() => {
    Voice.onSpeechStart = onSpeechStart
    Voice.onSpeechRecognized = onSpeechRecognized
    Voice.onSpeechEnd = onSpeechEnd
    Voice.onSpeechError = onSpeechError
    Voice.onSpeechResults = onSpeechResults

    return () => {
      Voice.destroy().then(Voice.removeAllListeners)
    }
  }, [])

  const onSpeechStart = () => {
    setIsRecording(true)
  }

  const onSpeechRecognized = () => {
    // 音声認識中
  }

  const onSpeechEnd = () => {
    setIsRecording(false)
  }

  const onSpeechError = (error: any) => {
    console.error('音声認識エラー:', error)
    setIsRecording(false)
  }

  const onSpeechResults = (event: any) => {
    const text = event.value[0]
    setRecognizedText(text)
    onCommand(text)
  }

  const startListening = async () => {
    try {
      await Voice.start('ja-JP')
    } catch (error) {
      console.error('音声認識開始エラー:', error)
    }
  }

  const stopListening = async () => {
    try {
      await Voice.stop()
    } catch (error) {
      console.error('音声認識停止エラー:', error)
    }
  }

  return (
    <View className="bg-white rounded-lg p-4 mb-4">
      <View className="flex-row items-center justify-between">
        <View className="flex-1">
          <Text className="text-sm text-gray-600 mb-1">音声入力</Text>
          <Text className="text-lg">
            {recognizedText || '「ベンチプレス 70キロ 10回」と話してください'}
          </Text>
        </View>
        
        <TouchableOpacity
          onPress={isRecording ? stopListening : startListening}
          className={`p-3 rounded-full ${
            isRecording ? 'bg-red-500' : 'bg-blue-500'
          }`}
        >
          <Icon 
            name={isRecording ? 'mic-off' : 'mic'} 
            size={24} 
            color="white" 
          />
        </TouchableOpacity>
      </View>
      
      {isRecording && (
        <View className="mt-2">
          <Text className="text-sm text-blue-600 text-center">
            🎤 音声を認識中...
          </Text>
        </View>
      )}
    </View>
  )
}

// 音声コマンドのパース
export const parseVoiceCommand = (command: string): ParsedCommand | null => {
  // 「ベンチプレス 70キロ 10回」のようなパターンを解析
  const patterns = [
    /(.+?)\s*(\d+(?:\.\d+)?)\s*(?:キロ|kg)\s*(\d+)\s*(?:回|レップ)/i,
    /(.+?)\s*(\d+)\s*(?:回|レップ)\s*(\d+(?:\.\d+)?)\s*(?:キロ|kg)/i
  ]
  
  for (const pattern of patterns) {
    const match = command.match(pattern)
    if (match) {
      const [, exercise, weight, reps] = match
      return {
        exerciseId: getExerciseIdByName(exercise.trim()),
        setData: {
          weight: parseFloat(weight),
          reps: parseInt(reps),
          restTime: 60 // デフォルト1分
        }
      }
    }
  }
  
  return null
}
```

### components/training/TimerComponent.tsx
```typescript
import React, { useState, useEffect } from 'react'
import { View, Text, TouchableOpacity } from 'react-native'
import { Animated, Easing } from 'react-native'
import Sound from 'react-native-sound'

interface TimerComponentProps {
  duration: number // 秒
  isRunning: boolean
  onComplete: () => void
}

export const TimerComponent: React.FC<TimerComponentProps> = ({
  duration,
  isRunning,
  onComplete
}) => {
  const [timeLeft, setTimeLeft] = useState(duration)
  const [progressAnim] = useState(new Animated.Value(1))

  useEffect(() => {
    if (isRunning && timeLeft > 0) {
      const timer = setTimeout(() => {
        setTimeLeft(prev => prev - 1)
        
        // プログレスバーのアニメーション
        Animated.timing(progressAnim, {
          toValue: (timeLeft - 1) / duration,
          duration: 1000,
          easing: Easing.linear,
          useNativeDriver: false
        }).start()
        
      }, 1000)

      return () => clearTimeout(timer)
    } else if (timeLeft === 0) {
      // タイマー完了
      playCompletionSound()
      onComplete()
    }
  }, [isRunning, timeLeft])

  useEffect(() => {
    setTimeLeft(duration)
    progressAnim.setValue(1)
  }, [duration])

  const playCompletionSound = () => {
    const sound = new Sound('timer_complete.mp3', Sound.MAIN_BUNDLE, () => {
      sound.play()
    })
  }

  const formatTime = (seconds: number): string => {
    const mins = Math.floor(seconds / 60)
    const secs = seconds % 60
    return `${mins}:${secs.toString().padStart(2, '0')}`
  }

  if (!isRunning && duration === 0) {
    return null
  }

  return (
    <View className="bg-blue-50 rounded-lg p-4 mb-4">
      <View className="items-center">
        <Text className="text-sm text-blue-600 mb-2">レストタイマー</Text>
        
        <View className="relative w-32 h-32 items-center justify-center">
          {/* 背景の円 */}
          <View className="absolute w-32 h-32 rounded-full border-4 border-blue-200" />
          
          {/* プログレス円 */}
          <Animated.View 
            className="absolute w-32 h-32 rounded-full border-4 border-blue-500"
            style={{
              transform: [{
                rotate: progressAnim.interpolate({
                  inputRange: [0, 1],
                  outputRange: ['0deg', '360deg']
                })
              }]
            }}
          />
          
          {/* 時間表示 */}
          <Text className="text-2xl font-bold text-blue-800">
            {formatTime(timeLeft)}
          </Text>
        </View>
        
        <TouchableOpacity 
          onPress={onComplete}
          className="mt-4 bg-blue-600 px-6 py-2 rounded-full"
        >
          <Text className="text-white font-medium">スキップ</Text>
        </TouchableOpacity>
      </div>
    </View>
  )
}
```

## 💊 サプリメント機能実装

### components/supplement/BarcodeScannerModal.tsx
```typescript
import React, { useState } from 'react'
import { View, Text, TouchableOpacity, Modal } from 'react-native'
import { RNCamera } from 'react-native-camera'
import Icon from 'react-native-vector-icons/MaterialIcons'

interface BarcodeScannerModalProps {
  visible: boolean
  onClose: () => void
  onBarcodeScanned: (barcode: string) => void
}

export const BarcodeScannerModal: React.FC<BarcodeScannerModalProps> = ({
  visible,
  onClose,
  onBarcodeScanned
}) => {
  const [scanned, setScanned] = useState(false)

  const handleBarCodeRead = (event: any) => {
    if (!scanned) {
      setScanned(true)
      onBarcodeScanned(event.data)
      setTimeout(() => {
        setScanned(false)
        onClose()
      }, 1000)
    }
  }

  return (
    <Modal visible={visible} animationType="slide">
      <View className="flex-1 bg-black">
        {/* ヘッダー */}
        <View className="flex-row items-center justify-between p-4 bg-black/80">
          <Text className="text-white text-lg font-semibold">
            バーコードをスキャン
          </Text>
          <TouchableOpacity onPress={onClose}>
            <Icon name="close" size={24} color="white" />
          </TouchableOpacity>
        </View>

        {/* カメラビュー */}
        <RNCamera
          style={{ flex: 1 }}
          onBarCodeRead={handleBarCodeRead}
          barCodeTypes={[RNCamera.Constants.BarCodeType.ean13]}
          captureAudio={false}
        >
          {/* スキャンエリア */}
          <View className="flex-1 items-center justify-center">
            <View className="w-64 h-64 border-2 border-white rounded-lg">
              <View className="absolute top-0 left-0 w-8 h-8 border-l-4 border-t-4 border-green-400" />
              <View className="absolute top-0 right-0 w-8 h-8 border-r-4 border-t-4 border-green-400" />
              <View className="absolute bottom-0 left-0 w-8 h-8 border-l-4 border-b-4 border-green-400" />
              <View className="absolute bottom-0 right-0 w-8 h-8 border-r-4 border-b-4 border-green-400" />
            </View>
            
            <Text className="text-white text-center mt-4 px-8">
              サプリメントのバーコードをスキャンしてください
            </Text>
          </View>
        </RNCamera>

        {/* フッター */}
        <View className="p-4 bg-black/80">
          <TouchableOpacity 
            onPress={onClose}
            className="bg-gray-600 py-3 px-6 rounded-lg"
          >
            <Text className="text-white text-center font-medium">
              手動で入力
            </Text>
          </TouchableOpacity>
        </View>
      </View>
    </Modal>
  )
}
```

### services/notifications.ts
```typescript
import PushNotification from 'react-native-push-notification'
import { Notifications } from 'react-native-notifications'

class NotificationService {
  constructor() {
    this.configure()
  }

  configure() {
    PushNotification.configure({
      onRegister: (token) => {
        console.log('FCM Token:', token)
        // サーバーにトークンを送信
        this.registerToken(token.token)
      },

      onNotification: (notification) => {
        console.log('通知受信:', notification)
        
        if (notification.userInteraction) {
          // ユーザーが通知をタップした場合
          this.handleNotificationTap(notification)
        }
      },

      permissions: {
        alert: true,
        badge: true,
        sound: true,
      },

      popInitialNotification: true,
      requestPermissions: true,
    })
  }

  // サプリメント摂取リマインダーをスケジュール
  scheduleSupplementReminder(supplement: Supplement, schedule: SupplementSchedule) {
    const notificationId = `supplement_${supplement.supplementId}_${schedule.scheduleId}`
    
    PushNotification.localNotificationSchedule({
      id: notificationId,
      title: 'サプリメント摂取時間',
      message: `${supplement.supplementName} を ${schedule.dosage}${supplement.unit} 摂取してください`,
      date: this.getNextScheduleDate(schedule),
      repeatType: this.getRepeatType(schedule.frequency),
      actions: ['摂取完了', '後で'],
      userInfo: {
        type: 'supplement_reminder',
        supplementId: supplement.supplementId,
        scheduleId: schedule.scheduleId
      }
    })
  }

  // トレーニングリマインダー
  scheduleWorkoutReminder(workoutTime: string, workoutDays: string[]) {
    workoutDays.forEach((day, index) => {
      PushNotification.localNotificationSchedule({
        id: `workout_${day}`,
        title: 'トレーニング時間',
        message: '今日のワークアウトを始めましょう！',
        date: this.getWorkoutDate(day, workoutTime),
        repeatType: 'week',
        userInfo: {
          type: 'workout_reminder',
          day: day
        }
      })
    })
  }

  // 休息日のリマインダー
  scheduleRestDayReminder() {
    PushNotification.localNotificationSchedule({
      id: 'rest_day_check',
      title: '体調チェック',
      message: '今日は調子はいかがですか？記録しておきましょう',
      date: new Date(Date.now() + 24 * 60 * 60 * 1000), // 明日
      repeatType: 'day',
      userInfo: {
        type: 'rest_day_check'
      }
    })
  }

  // 通知をクリア
  clearSupplementReminders(supplementId: string) {
    // 該当するサプリメントの通知をすべてキャンセル
    PushNotification.getScheduledLocalNotifications((notifications) => {
      notifications.forEach(notification => {
        if (notification.userInfo?.supplementId === supplementId) {
          PushNotification.cancelLocalNotifications({ id: notification.id })
        }
      })
    })
  }

  private async registerToken(token: string) {
    try {
      await api.post('/users/fcm-token', { token })
    } catch (error) {
      console.error('FCMトークン登録エラー:', error)
    }
  }

  private handleNotificationTap(notification: any) {
    const { type, supplementId, scheduleId } = notification.userInfo || {}
    
    switch (type) {
      case 'supplement_reminder':
        // サプリメント記録画面に遷移
        navigationRef.navigate('SupplementIntake', { supplementId, scheduleId })
        break
      case 'workout_reminder':
        // ワークアウト画面に遷移
        navigationRef.navigate('Workout')
        break
      default:
        // デフォルトのメイン画面
        navigationRef.navigate('Dashboard')
    }
  }

  private getNextScheduleDate(schedule: SupplementSchedule): Date {
    const now = new Date()
    const [hours, minutes] = schedule.scheduledTime.split(':').map(Number)
    
    const scheduleDate = new Date()
    scheduleDate.setHours(hours, minutes, 0, 0)
    
    // 今日の時間が過ぎている場合は明日に設定
    if (scheduleDate <= now) {
      scheduleDate.setDate(scheduleDate.getDate() + 1)
    }
    
    return scheduleDate
  }

  private getRepeatType(frequency: string): string {
    switch (frequency) {
      case 'daily': return 'day'
      case 'weekly': return 'week'
      case 'monthly': return 'month'
      default: return 'day'
    }
  }

  private getWorkoutDate(day: string, time: string): Date {
    // 指定された曜日と時間の次の日付を計算
    const dayMap = {
      'monday': 1, 'tuesday': 2, 'wednesday': 3, 'thursday': 4,
      'friday': 5, 'saturday': 6, 'sunday': 0
    }
    
    const targetDay = dayMap[day.toLowerCase()]
    const [hours, minutes] = time.split(':').map(Number)
    
    const now = new Date()
    const currentDay = now.getDay()
    
    let daysUntilTarget = (targetDay - currentDay + 7) % 7
    if (daysUntilTarget === 0) {
      // 今日が対象日の場合、時間をチェック
      const targetTime = new Date()
      targetTime.setHours(hours, minutes, 0, 0)
      
      if (targetTime <= now) {
        daysUntilTarget = 7 // 来週の同じ曜日
      }
    }
    
    const targetDate = new Date()
    targetDate.setDate(now.getDate() + daysUntilTarget)
    targetDate.setHours(hours, minutes, 0, 0)
    
    return targetDate
  }
}

export const notificationService = new NotificationService()
```

## 📊 オフライン対応とデータ同期

### services/offline.ts
```typescript
import NetInfo from '@react-native-netinfo'
import AsyncStorage from '@react-native-async-storage/async-storage'

class OfflineService {
  private readonly OFFLINE_QUEUE_KEY = 'offline_queue'
  private offlineQueue: OfflineAction[] = []

  constructor() {
    this.loadOfflineQueue()
    this.setupNetworkListener()
  }

  async setupNetworkListener() {
    NetInfo.addEventListener(state => {
      if (state.isConnected && this.offlineQueue.length > 0) {
        this.syncOfflineData()
      }
    })
  }

  // オフライン時のアクション記録
  async queueAction(action: OfflineAction) {
    this.offlineQueue.push({
      ...action,
      timestamp: new Date().toISOString(),
      id: this.generateId()
    })
    
    await this.saveOfflineQueue()
  }

  // オンライン復帰時のデータ同期
  async syncOfflineData() {
    const actions = [...this.offlineQueue]
    
    for (const action of actions) {
      try {
        await this.executeAction(action)
        
        // 成功した場合、キューから削除
        this.offlineQueue = this.offlineQueue.filter(a => a.id !== action.id)
      } catch (error) {
        console.error('オフライン同期エラー:', error, action)
        // エラーの場合、リトライ回数を増やす
        action.retryCount = (action.retryCount || 0) + 1
        
        // 最大リトライ回数を超えた場合は削除
        if (action.retryCount > 3) {
          this.offlineQueue = this.offlineQueue.filter(a => a.id !== action.id)
        }
      }
    }
    
    await this.saveOfflineQueue()
  }

  private async executeAction(action: OfflineAction) {
    switch (action.type) {
      case 'CREATE_TRAINING_RECORD':
        return await api.post('/training/records', action.data)
      
      case 'CREATE_SUPPLEMENT_INTAKE':
        return await api.post('/supplement/intake-records', action.data)
      
      case 'UPDATE_USER_PROFILE':
        return await api.put('/users/profile', action.data)
      
      default:
        throw new Error(`不明なアクションタイプ: ${action.type}`)
    }
  }

  private async loadOfflineQueue() {
    try {
      const stored = await AsyncStorage.getItem(this.OFFLINE_QUEUE_KEY)
      if (stored) {
        this.offlineQueue = JSON.parse(stored)
      }
    } catch (error) {
      console.error('オフラインキューの読み込みエラー:', error)
    }
  }

  private async saveOfflineQueue() {
    try {
      await AsyncStorage.setItem(
        this.OFFLINE_QUEUE_KEY,
        JSON.stringify(this.offlineQueue)
      )
    } catch (error) {
      console.error('オフラインキューの保存エラー:', error)
    }
  }

  private generateId(): string {
    return Date.now().toString() + Math.random().toString(36).substr(2, 9)
  }

  // オフライン状態の確認
  async isOnline(): Promise<boolean> {
    const state = await NetInfo.fetch()
    return state.isConnected === true
  }

  // キューの状態取得
  getQueueStatus() {
    return {
      total: this.offlineQueue.length,
      pending: this.offlineQueue.filter(a => !a.retryCount || a.retryCount < 3).length,
      failed: this.offlineQueue.filter(a => a.retryCount && a.retryCount >= 3).length
    }
  }
}

export const offlineService = new OfflineService()
```

## 🔧 パフォーマンス最適化

### hooks/useOptimizedList.ts
```typescript
import { useMemo, useState, useCallback } from 'react'

interface UseOptimizedListProps<T> {
  data: T[]
  searchFields: (keyof T)[]
  pageSize?: number
}

export function useOptimizedList<T>({ 
  data, 
  searchFields, 
  pageSize = 20 
}: UseOptimizedListProps<T>) {
  const [searchTerm, setSearchTerm] = useState('')
  const [currentPage, setCurrentPage] = useState(0)

  // 検索フィルタリング
  const filteredData = useMemo(() => {
    if (!searchTerm) return data
    
    return data.filter(item => 
      searchFields.some(field => {
        const value = item[field]
        if (typeof value === 'string') {
          return value.toLowerCase().includes(searchTerm.toLowerCase())
        }
        return false
      })
    )
  }, [data, searchTerm, searchFields])

  // ページネーション
  const paginatedData = useMemo(() => {
    const startIndex = currentPage * pageSize
    return filteredData.slice(startIndex, startIndex + pageSize)
  }, [filteredData, currentPage, pageSize])

  const totalPages = Math.ceil(filteredData.length / pageSize)
  const hasMore = currentPage < totalPages - 1

  const loadMore = useCallback(() => {
    if (hasMore) {
      setCurrentPage(prev => prev + 1)
    }
  }, [hasMore])

  const resetPagination = useCallback(() => {
    setCurrentPage(0)
  }, [])

  return {
    searchTerm,
    setSearchTerm,
    filteredData: paginatedData,
    totalItems: filteredData.length,
    hasMore,
    loadMore,
    resetPagination,
    currentPage,
    totalPages
  }
}
```

### components/common/LazyImage.tsx
```typescript
import React, { useState, useRef } from 'react'
import { Image, View, Animated } from 'react-native'
import FastImage from 'react-native-fast-image'

interface LazyImageProps {
  source: { uri: string }
  style?: any
  placeholder?: React.ReactNode
  onLoad?: () => void
  onError?: () => void
}

export const LazyImage: React.FC<LazyImageProps> = ({
  source,
  style,
  placeholder,
  onLoad,
  onError
}) => {
  const [isLoaded, setIsLoaded] = useState(false)
  const [hasError, setHasError] = useState(false)
  const fadeAnim = useRef(new Animated.Value(0)).current

  const handleLoad = () => {
    setIsLoaded(true)
    Animated.timing(fadeAnim, {
      toValue: 1,
      duration: 300,
      useNativeDriver: true
    }).start()
    onLoad?.()
  }

  const handleError = () => {
    setHasError(true)
    onError?.()
  }

  if (hasError) {
    return (
      <View style={[style, { backgroundColor: '#f3f4f6', justifyContent: 'center', alignItems: 'center' }]}>
        <Text className="text-gray-500">画像を読み込めませんでした</Text>
      </View>
    )
  }

  return (
    <View style={style}>
      {!isLoaded && placeholder}
      
      <Animated.View style={{ opacity: fadeAnim }}>
        <FastImage
          source={source}
          style={style}
          onLoad={handleLoad}
          onError={handleError}
          resizeMode={FastImage.resizeMode.cover}
        />
      </Animated.View>
    </View>
  )
}
```

## 📱 ネイティブ機能統合

### services/device.ts
```typescript
import { Platform, PermissionsAndroid, Alert } from 'react-native'
import CameraRoll from '@react-native-community/cameraroll'
import Share from 'react-native-share'
import RNFS from 'react-native-fs'

class DeviceService {
  // カメラロールへの保存
  async saveImageToCameraRoll(imageUri: string): Promise<boolean> {
    try {
      if (Platform.OS === 'android') {
        const permission = await PermissionsAndroid.request(
          PermissionsAndroid.PERMISSIONS.WRITE_EXTERNAL_STORAGE
        )
        
        if (permission !== PermissionsAndroid.RESULTS.GRANTED) {
          Alert.alert('権限エラー', 'ストレージへの書き込み権限が必要です')
          return false
        }
      }

      await CameraRoll.save(imageUri, { type: 'photo' })
      Alert.alert('保存完了', '画像がカメラロールに保存されました')
      return true
    } catch (error) {
      console.error('画像保存エラー:', error)
      Alert.alert('エラー', '画像の保存に失敗しました')
      return false
    }
  }

  // データのシェア
  async shareWorkoutData(workoutData: WorkoutData): Promise<boolean> {
    try {
      const shareData = this.formatWorkoutForShare(workoutData)
      
      await Share.open({
        title: 'ワークアウト記録をシェア',
        message: shareData.text,
        filename: shareData.filename,
        type: 'text/plain'
      })
      
      return true
    } catch (error) {
      console.error('シェアエラー:', error)
      return false
    }
  }

  // バックアップファイルの作成
  async createBackup(): Promise<string | null> {
    try {
      const backupData = await this.gatherBackupData()
      const backupJson = JSON.stringify(backupData, null, 2)
      
      const fileName = `message_backup_${new Date().toISOString().split('T')[0]}.json`
      const filePath = `${RNFS.DocumentDirectoryPath}/${fileName}`
      
      await RNFS.writeFile(filePath, backupJson, 'utf8')
      
      return filePath
    } catch (error) {
      console.error('バックアップ作成エラー:', error)
      return null
    }
  }

  // バックアップファイルの復元
  async restoreFromBackup(filePath: string): Promise<boolean> {
    try {
      const fileExists = await RNFS.exists(filePath)
      if (!fileExists) {
        Alert.alert('エラー', 'バックアップファイルが見つかりません')
        return false
      }

      const backupData = await RNFS.readFile(filePath, 'utf8')
      const parsedData = JSON.parse(backupData)
      
      await this.restoreData(parsedData)
      
      Alert.alert('復元完了', 'バックアップから復元しました')
      return true
    } catch (error) {
      console.error('復元エラー:', error)
      Alert.alert('エラー', '復元に失敗しました')
      return false
    }
  }

  private formatWorkoutForShare(workoutData: WorkoutData): { text: string, filename: string } {
    const date = new Date(workoutData.date).toLocaleDateString('ja-JP')
    
    let text = `🏋️‍♂️ ${date} のワークアウト記録\n\n`
    
    workoutData.exercises.forEach(exercise => {
      text += `${exercise.name}:\n`
      exercise.sets.forEach((set, index) => {
        text += `  セット${index + 1}: ${set.weight}kg × ${set.reps}回\n`
      })
      text += '\n'
    })
    
    text += `合計時間: ${workoutData.duration}分\n`
    text += `#Message #ワークアウト記録\n`
    
    return {
      text,
      filename: `workout_${workoutData.date}.txt`
    }
  }

  private async gatherBackupData(): Promise<BackupData> {
    // すべてのローカルデータを収集
    const trainingData = await AsyncStorage.getItem('training_data')
    const supplementData = await AsyncStorage.getItem('supplement_data')
    const userSettings = await AsyncStorage.getItem('user_settings')
    
    return {
      version: '1.0',
      createdAt: new Date().toISOString(),
      trainingData: trainingData ? JSON.parse(trainingData) : [],
      supplementData: supplementData ? JSON.parse(supplementData) : [],
      userSettings: userSettings ? JSON.parse(userSettings) : {}
    }
  }

  private async restoreData(backupData: BackupData): Promise<void> {
    // バックアップデータから復元
    await AsyncStorage.setItem('training_data', JSON.stringify(backupData.trainingData))
    await AsyncStorage.setItem('supplement_data', JSON.stringify(backupData.supplementData))
    await AsyncStorage.setItem('user_settings', JSON.stringify(backupData.userSettings))
    
    // Redux ストアを更新
    store.dispatch(restoreTrainingData(backupData.trainingData))
    store.dispatch(restoreSupplementData(backupData.supplementData))
  }
}

export const deviceService = new DeviceService()
```

## 📋 実装チェックリスト

### Phase 1: 基盤構築
- [ ] React Native プロジェクト初期化
- [ ] TypeScript設定完了
- [ ] Redux Toolkit + RTK Query設定
- [ ] React Navigation設定
- [ ] スタイリングシステム（NativeWind）設定
- [ ] デバッグツール設定

### Phase 2: 認証・セキュリティ
- [ ] JWT認証実装
- [ ] 生体認証統合
- [ ] セキュアストレージ実装
- [ ] 認証ミドルウェア実装
- [ ] ログアウト・トークンリフレッシュ

### Phase 3: トレーニング機能
- [ ] ワークアウト画面実装
- [ ] 音声入力機能
- [ ] タイマー機能
- [ ] カメラ統合
- [ ] オフライン記録対応

### Phase 4: サプリメント機能
- [ ] バーコードスキャナー実装
- [ ] プッシュ通知設定
- [ ] 摂取記録機能
- [ ] カレンダー統合
- [ ] リマインダー機能

### Phase 5: 最適化・リリース
- [ ] パフォーマンス最適化
- [ ] オフライン対応強化
- [ ] ネイティブ機能統合
- [ ] アプリアイコン・スプラッシュ
- [ ] ストア公開準備

## 🚀 ビルド・デプロイ設定

### android/app/build.gradle (抜粋)
```gradle
android {
    compileSdkVersion 33
    buildToolsVersion "33.0.0"

    defaultConfig {
        applicationId "com.message.app"
        minSdkVersion 21
        targetSdkVersion 33
        versionCode 1
        versionName "1.0.0"
        multiDexEnabled true
    }

    signingConfigs {
        release {
            if (project.hasProperty('MYAPP_UPLOAD_STORE_FILE')) {
                storeFile file(MYAPP_UPLOAD_STORE_FILE)
                storePassword MYAPP_UPLOAD_STORE_PASSWORD
                keyAlias MYAPP_UPLOAD_KEY_ALIAS
                keyPassword MYAPP_UPLOAD_KEY_PASSWORD
            }
        }
    }

    buildTypes {
        release {
            minifyEnabled enableProguardInReleaseBuilds
            proguardFiles getDefaultProguardFile("proguard-android.txt"), "proguard-rules.pro"
            signingConfig signingConfigs.release
        }
    }
}
```

### ios/Message/Info.plist (抜粋)
```xml
<key>NSCameraUsageDescription</key>
<string>トレーニングフォームの撮影に使用します</string>

<key>NSMicrophoneUsageDescription</key>
<string>音声でのトレーニング記録入力に使用します</string>

<key>NSFaceIDUsageDescription</key>
<string>アプリの安全なログインに使用します</string>

<key>NSPhotoLibraryUsageDescription</key>
<string>トレーニング画像の保存に使用します</string>
```

---

## 🎯 次のステップ

1. **開発環境構築**: React Native CLI、Android Studio、Xcode
2. **API Contract確認**: バックエンドチームと連携
3. **デザインシステム**: UI/UXガイドラインの策定
4. **段階的実装**: Phase順での着実な開発

**📱 ユーザビリティと性能を重視した素晴らしいモバイルアプリを作りましょう！**