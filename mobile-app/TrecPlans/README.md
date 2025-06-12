# TrecPlans - React Native モバイルアプリ

## 概要
トレーニング記録管理システムのモバイルアプリケーションです。React Native 0.72.6を使用し、iOS/Android両プラットフォームでトレーニングの記録と管理を行えます。

## 必要要件
- Node.js 16以上
- npm または yarn
- React Native開発環境
  - iOS: Xcode 14以上、macOS
  - Android: Android Studio、JDK 11

## プロジェクト構成
```
TrecPlans/
├── src/
│   ├── components/      # 再利用可能コンポーネント
│   │   └── TrainingCard.tsx
│   ├── contexts/        # Context API
│   │   └── AuthContext.tsx
│   ├── screens/         # 画面コンポーネント
│   │   ├── DashboardScreen.tsx
│   │   ├── HistoryScreen.tsx
│   │   └── HomeScreen.tsx
│   ├── services/        # APIサービス
│   │   └── api.ts
│   ├── types/           # TypeScript型定義
│   │   └── index.ts
│   └── utils/           # ユーティリティ関数
├── App.tsx              # アプリケーションエントリーポイント
├── index.js             # React Nativeエントリーポイント
├── package.json         # 依存関係
└── tsconfig.json        # TypeScript設定
```

## セットアップ

### 1. 依存関係のインストール
```bash
cd mobile-app/TrecPlans
npm install
```

### 2. iOS依存関係（macOSのみ）
```bash
cd ios
pod install
cd ..
```

### 3. 環境設定
`src/services/api.ts` でAPIのベースURLを設定：
```typescript
const API_BASE_URL = 'http://localhost:5000/api'; // 開発環境
// const API_BASE_URL = 'https://your-production-api.com/api'; // 本番環境
```

開発環境の設定:
- Android エミュレータ: `http://10.0.2.2:5000/api`
- iOS シミュレータ: `http://localhost:5000/api`
- 実機: PCのIPアドレスを使用 (例: `http://192.168.1.100:5000/api`)

## 実行方法

### メトロバンドラーの起動
```bash
npm start
```

### iOS実行（macOSのみ）
```bash
npm run ios
```

### Android実行
```bash
# エミュレーターまたは実機を接続後
npm run android
```

## 主な機能

### ホーム画面 (HomeScreen)
- ログイン/サインアップ
- メニュー一覧の表示
- メニューの検索

### ダッシュボード (DashboardScreen)
- トレーニング統計
- 進捗グラフ（react-native-chart-kit使用）
- 最近の活動

### 履歴画面 (HistoryScreen)
- 過去のトレーニング記録
- 日付でのフィルタリング
- 詳細表示

## 主要な依存関係

### ナビゲーション
- `@react-navigation/native`: 画面遷移
- `@react-navigation/native-stack`: スタックナビゲーション
- `@react-navigation/bottom-tabs`: タブナビゲーション

### UI/UX
- `react-native-vector-icons`: アイコン
- `react-native-safe-area-context`: セーフエリア対応
- `react-native-chart-kit`: グラフ表示

### データ管理
- `axios`: HTTPクライアント
- `@react-native-async-storage/async-storage`: ローカルストレージ
- `date-fns`: 日付操作

## コンポーネント

### TrainingCard
トレーニング情報を表示するカードコンポーネント
```tsx
<TrainingCard
  title="ベンチプレス"
  sets={3}
  reps={10}
  weight={60}
  onPress={handleCardPress}
/>
```

## Context

### AuthContext
認証状態の管理
```tsx
const { user, login, logout, isAuthenticated } = useAuth();
```

## APIサービス

### 基本的な使用方法
```typescript
import { api } from './services/api';

// メニュー取得
const menus = await api.getMenus();

// トレーニング記録送信
await api.postTrainingRecord(recordData);
```

## ビルドとリリース

### iOS（macOSのみ）
```bash
# リリースビルド
cd ios
xcodebuild -workspace TrecPlans.xcworkspace -scheme TrecPlans -configuration Release

# または Xcode で Archive
```

### Android
```bash
# APKビルド
cd android
./gradlew assembleRelease

# AAB（App Bundle）ビルド
./gradlew bundleRelease
```

## デバッグ

### React Native Debugger
```bash
# Cmd+D (iOS) または Cmd+M (Android) でデバッグメニューを開く
```

### ログの確認
```bash
# iOS
npx react-native log-ios

# Android
npx react-native log-android
```

## パフォーマンス最適化

### コンポーネントの最適化
- `React.memo` でコンポーネントをメモ化
- `useMemo` と `useCallback` で計算結果をキャッシュ

### 画像の最適化
- 適切なサイズの画像を使用
- FastImageライブラリの検討

## トラブルシューティング

### ビルドエラー
```bash
# キャッシュクリア
npx react-native start --reset-cache

# iOS: Podの再インストール
cd ios && pod deintegrate && pod install

# Android: Gradleキャッシュクリア
cd android && ./gradlew clean
```

### Metro バンドラーエラー
```bash
# node_modulesの再インストール
rm -rf node_modules
npm install
```

### デバイス接続エラー
- iOS: Xcodeでデバイスを信頼
- Android: 開発者オプションでUSBデバッグを有効化

## テスト

### ユニットテスト
```bash
npm test
```

### E2Eテスト（Detoxなど設定済みの場合）
```bash
npm run test:e2e
```

## コーディング規約

### TypeScript
- 厳格な型定義を使用
- `any` 型の使用を避ける

### コンポーネント
- 関数コンポーネントとHooksを使用
- PropsにはTypeScriptインターフェースを定義

### スタイリング
- StyleSheetを使用
- 共通スタイルは別ファイルで管理