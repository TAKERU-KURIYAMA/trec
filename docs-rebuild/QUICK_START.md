# クイックスタートガイド

🚀 **今すぐ始めるための最短手順**

## 📋 事前準備（5分）

### 必要な環境確認
```bash
# Node.js バージョン確認（18+ 必須）
node --version

# npm バージョン確認
npm --version

# Git 確認
git --version
```

### ディレクトリ準備
```bash
# 作業ディレクトリに移動
cd /mnt/d/develop/Message/Message

# 新しいディレクトリ作成
mkdir docs-rebuild
cd docs-rebuild
```

## ⚡ Phase 1: 最初の30分で動作サイトを作成

### Step 1: プロジェクト初期化（5分）
```bash
# package.json 作成
cat > package.json << 'EOF'
{
  "name": "message-docs-rebuild",
  "version": "1.0.0",
  "type": "module",
  "scripts": {
    "docs:dev": "vitepress dev",
    "docs:build": "vitepress build",
    "docs:preview": "vitepress preview"
  },
  "devDependencies": {
    "vitepress": "^1.0.0-rc.31"
  }
}
EOF

# 依存関係インストール
npm install
```

### Step 2: 最小限設定（5分）
```bash
# ディレクトリ作成
mkdir -p .vitepress public

# 設定ファイル作成
cat > .vitepress/config.js << 'EOF'
import { defineConfig } from 'vitepress'

export default defineConfig({
  title: 'Message システム設計書',
  description: 'トレーニング管理システムの設計ドキュメント',
  lang: 'ja-JP',
  
  themeConfig: {
    nav: [
      { text: 'ホーム', link: '/' },
      { text: '機能一覧', link: '/features/' }
    ]
  }
})
EOF
```

### Step 3: ホームページ作成（10分）
```bash
# ホームページ作成
cat > index.md << 'EOF'
---
layout: home

hero:
  name: "Message"
  text: "システム設計書"
  tagline: トレーニング管理システムの設計ドキュメント
  actions:
    - theme: brand
      text: 機能一覧
      link: /features/

features:
  - icon: 🔐
    title: 認証・ユーザー管理
    details: JWT認証、ユーザー登録・ログイン、権限管理
    
  - icon: 🏋️‍♂️
    title: トレーニング管理
    details: ワークアウトセッション、セット記録、履歴管理
    
  - icon: 💊
    title: サプリメント管理
    details: サプリメント登録、摂取記録、スケジュール管理
---

## プロジェクト概要

**Message** は、フィットネス愛好者とアスリートのための包括的なトレーニング管理システムです。

### 主要特徴
- 📊 包括的な記録管理
- 🔒 企業級セキュリティ
- 📱 クロスプラットフォーム対応
- ⚡ 高性能API

### 技術スタック
- **フロントエンド**: Vue 3, Nuxt 3, TypeScript
- **バックエンド**: ASP.NET Core 8.0
- **データベース**: PostgreSQL
- **モバイル**: React Native
EOF
```

### Step 4: サンプルページ作成（5分）
```bash
# ディレクトリ作成
mkdir features

# 機能一覧ページ作成
cat > features/index.md << 'EOF'
# 機能一覧

Message システムの全機能を紹介します。

## 実装状況

| 機能 | 状況 | 説明 |
|------|------|------|
| ユーザー認証 | ✅ 完了 | JWT認証、登録・ログイン |
| トレーニング記録 | ✅ 完了 | ワークアウト記録・履歴 |
| サプリメント管理 | 🔄 開発中 | 摂取記録・スケジュール |
| ダッシュボード | ✅ 完了 | 統計・進捗表示 |
| モバイルアプリ | 🔄 開発中 | React Native版 |

## 認証・ユーザー管理

### ✅ 実装済み
- ユーザー登録・ログイン
- JWT トークン管理
- パスワードセキュリティ
- 管理者機能

### 🔄 開発中
- プロフィール管理
- アカウント設定

## トレーニング管理

### ✅ 実装済み
- メニュー表示・検索
- ワークアウトセッション
- セット記録・履歴
- ダッシュボード統計
- プリセット管理

## サプリメント管理

### ✅ 実装済み
- サプリメント登録・管理
- 摂取記録
- スケジュール管理

### 🔄 開発中
- リマインダー通知
- 摂取分析グラフ
EOF
```

### Step 5: 動作確認（5分）
```bash
# 開発サーバー起動
npm run docs:dev
```

ブラウザで `http://localhost:5173` にアクセスして動作確認

## 🎯 30分後の確認項目

- [ ] サイトが正常に表示される
- [ ] ホームページのヒーローセクションが表示される
- [ ] 機能一覧ページにアクセスできる
- [ ] ナビゲーションが機能する
- [ ] モバイルで適切に表示される

## 📝 次のステップ（1時間目）

### Phase 1 残りタスク
1. **T1-01 ✅**: プロジェクト初期化（完了）
2. **T1-02 ✅**: VitePress設定（完了）
3. **T1-03 ✅**: ホームページ作成（完了）
4. **T1-04 ✅**: サンプルページ作成（完了）

### Phase 2 開始準備
次に実行するタスク：
- **T2-01**: ナビゲーション設定
- **T2-02**: 機能一覧ページ拡張
- **T2-03**: API仕様ページ作成

## 🛠️ よく使うコマンド

```bash
# 開発サーバー起動
npm run docs:dev

# 本番ビルド
npm run docs:build

# ビルド結果プレビュー
npm run docs:preview

# 依存関係リセット
rm -rf node_modules package-lock.json
npm install
```

## 🆘 トラブルシューティング

### サーバーが起動しない
```bash
# Node.js バージョン確認
node --version  # 18+ であることを確認

# キャッシュクリア
rm -rf node_modules .vitepress/cache
npm install
```

### ページが表示されない
1. ファイル名・パスを確認
2. マークダウン記法を確認
3. ブラウザのDevToolsでエラー確認

### スタイルが崩れる
1. ブラウザキャッシュをクリア
2. モバイルビューを確認
3. CSS記法を確認

## 📋 チェックリスト（毎作業時）

### 作業開始前
- [ ] 前回の作業が正常終了している
- [ ] 作業時間を確保できている
- [ ] 必要なツールが起動している

### 作業中
- [ ] こまめにローカルテスト
- [ ] エラーメッセージを確認
- [ ] モバイル表示も確認

### 作業終了時
- [ ] 全ての変更をテスト
- [ ] 次回作業の準備
- [ ] 進捗を記録

---

**🎉 これで最初の動作するサイトが完成です！**

次は `TASK_LIST.md` を参照して、Phase 2 のタスクを順番に実行してください。