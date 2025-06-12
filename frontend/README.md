# Frontend - Nuxt 3 プロジェクト

## 概要
トレーニング記録管理システムのフロントエンドアプリケーションです。Nuxt 3を使用し、レスポンシブなUIでトレーニングの記録と可視化を提供します。

## 必要要件
- Node.js 18.0以上
- npm または yarn

## プロジェクト構成
```
frontend/
├── pages/              # ページコンポーネント
│   ├── index.vue         # ホームページ
│   ├── dashboard.vue     # ダッシュボード
│   └── training/
│       └── history.vue   # トレーニング履歴
├── components/         # 再利用可能コンポーネント
│   ├── TrainingCard.vue  # トレーニングカード
│   ├── EnhancedTrainingCard.vue # 拡張トレーニングカード
│   └── dashboard/        # ダッシュボード用コンポーネント
│       ├── AchievementBadges.vue
│       ├── PersonalRecords.vue
│       ├── ProgressChart.vue
│       ├── RecentActivities.vue
│       ├── SmartRecommendations.vue
│       └── TrainingCalendar.vue
├── composables/        # Composition API
│   ├── useFetchMenus.js    # メニュー取得
│   └── useTrainingHistory.js # 履歴取得
├── assets/             # 静的リソース
├── public/             # 公開ファイル
└── nuxt.config.ts      # Nuxt設定
```

## セットアップ

### 1. 依存関係のインストール
```bash
cd frontend
npm install
```

### 2. 環境設定
`nuxt.config.ts`でAPIのベースURLを設定（デフォルトは`/api`）：

```typescript
export default defineNuxtConfig({
  runtimeConfig: {
    public: {
      apiBaseUrl: '/api', // Nginxプロキシ経由
    },
  },
})
```

### 3. 開発サーバーの起動
```bash
npm run dev
```
アプリケーションは `http://localhost:3000` で起動します。

## 主な機能

### ホームページ (`/`)
- トレーニングメニューの一覧表示
- メニューの検索とフィルタリング
- タグによる分類

### ダッシュボード (`/dashboard`)
- トレーニング進捗の可視化
- 個人記録の表示
- 達成バッジシステム
- カレンダービュー
- スマートレコメンデーション

### トレーニング履歴 (`/training/history`)
- 過去のトレーニング記録一覧
- 詳細な記録の表示
- 統計情報

## コンポーネント

### TrainingCard
基本的なトレーニングカードコンポーネント
```vue
<TrainingCard 
  :menu="menuData"
  :tags="tagData"
/>
```

### EnhancedTrainingCard
アニメーション付きの拡張トレーニングカード
```vue
<EnhancedTrainingCard
  :menu="menuData"
  :tags="tagData"
  @click="handleCardClick"
/>
```

## Composables

### useFetchMenus
トレーニングメニューを取得
```javascript
const { menus, tags, loading, error } = await useFetchMenus()
```

### useTrainingHistory
トレーニング履歴を取得
```javascript
const { history, loading, error } = await useTrainingHistory()
```

## ビルドとデプロイ

### プロダクションビルド
```bash
npm run build
```

### プレビュー
```bash
npm run preview
```

### Dockerビルド
```bash
docker build -t training-frontend .
docker run -p 3000:3000 training-frontend
```

## 開発ガイドライン

### 新しいページの追加
1. `pages/`ディレクトリに新しい`.vue`ファイルを作成
2. Nuxtの自動ルーティングが適用されます

### 新しいコンポーネントの作成
1. `components/`ディレクトリに作成
2. 自動インポートが有効なので、importは不要

### APIの呼び出し
```javascript
const { data } = await $fetch('/api/training/menu', {
  baseURL: useRuntimeConfig().public.apiBaseUrl
})
```

## スタイリング
- Tailwind CSS（設定されている場合）
- Scoped CSS in Vue components
- グローバルスタイルは`assets/`に配置

## パフォーマンス最適化
- 自動的なコード分割
- 遅延ローディング
- 画像の最適化
- SSR/SSGのサポート

## トラブルシューティング

### APIエラー
- ブラウザの開発者ツールでネットワークタブを確認
- CORSエラーの場合は、バックエンドの設定を確認

### ビルドエラー
```bash
# キャッシュのクリア
rm -rf .nuxt node_modules
npm install
npm run dev
```

### ポート競合
```bash
# 別のポートで起動
PORT=3001 npm run dev
```

## テスト
```bash
# ユニットテスト（設定されている場合）
npm run test

# E2Eテスト（設定されている場合）
npm run test:e2e
```
