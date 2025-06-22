---
layout: home

hero:
  name: "Message"
  text: "システム設計書"
  tagline: 包括的なトレーニング管理システムの設計ドキュメント
  image:
    src: /logo.png
    alt: Message Logo
  actions:
    - theme: brand
      text: 機能一覧
      link: /features/
    - theme: alt
      text: API仕様
      link: /api/
    - theme: alt
      text: システム設計
      link: /system/

features:
  - icon: 🔐
    title: 認証・ユーザー管理
    details: JWT認証、ユーザー登録・ログイン、権限管理、セキュアなトークン管理
    
  - icon: 🏋️‍♂️
    title: トレーニング管理
    details: ワークアウトセッション、セット記録、履歴管理、進捗追跡
    
  - icon: 💊
    title: サプリメント管理
    details: サプリメント登録、摂取記録、スケジュール管理、統計分析
    
  - icon: 📊
    title: ダッシュボード
    details: 進捗グラフ、統計サマリー、パーソナルレコード、アクティビティ履歴
    
  - icon: 📱
    title: マルチプラットフォーム
    details: Web（Vue 3 + Nuxt 3）、モバイル（React Native）、レスポンシブ対応
    
  - icon: 🚀
    title: 高性能
    details: ASP.NET Core 8.0、PostgreSQL、Redis キャッシュ、最適化されたAPI
---

## 📋 ドキュメント構成

このサイトでは、Messageシステムの設計に関する包括的な情報を提供しています：

### 🎯 [機能一覧](/features/)
- 実装済み機能の詳細
- 開発予定機能のロードマップ
- 各機能の技術仕様

### 🔌 [API仕様](/api/)
- RESTful API エンドポイント
- 認証・認可の仕組み
- リクエスト・レスポンス形式

### 🏗️ [システム設計](/system/)
- システムアーキテクチャ
- データベース設計（ER図）
- 技術スタック詳細

### 🎨 [画面設計](/screens/)
- UI/UX設計
- レスポンシブデザイン
- アクセシビリティ対応

### 🧪 [テスト設計](/testing/)
- テスト戦略
- テストケース詳細
- 品質保証プロセス

### 🔄 [シーケンス図](/sequences/)
- 業務フロー図
- システム間連携
- エラーハンドリング

## 🚀 クイックスタート

### 開発環境セットアップ

```bash
# フロントエンド（Web）
cd frontend
npm install
npm run dev

# API
cd api
dotnet restore
dotnet run

# モバイルアプリ
cd mobile-app/TrecPlans
npm install
npx react-native run-android
```

### ドキュメントサイトの開発

```bash
cd docs
npm install
npm run docs:dev
```

## 📈 システム概要

**Message** は、フィットネス愛好者とアスリートのための包括的なトレーニング管理システムです。

### 主要特徴

- **📊 包括的な記録管理**: トレーニングからサプリメント摂取まで一元管理
- **🎯 データ駆動型分析**: 進捗可視化と統計分析
- **🔒 企業級セキュリティ**: JWT認証、暗号化、権限管理
- **📱 クロスプラットフォーム**: Web、iOS、Androidに対応
- **⚡ 高性能**: 最新技術スタックによる高速レスポンス
- **🌐 オフライン対応**: ネットワーク環境に依存しない使用体験

### 技術スタック

| 分野 | 技術 |
|------|------|
| **フロントエンド** | Vue 3, Nuxt 3, TypeScript, TailwindCSS |
| **モバイル** | React Native, TypeScript, Zustand |
| **バックエンド** | ASP.NET Core 8.0, Entity Framework Core |
| **データベース** | PostgreSQL, Redis（キャッシュ） |
| **インフラ** | Docker, Nginx, CI/CD（GitHub Actions） |
| **監視** | Prometheus, Grafana, Loki |

### 対象ユーザー

- **一般ユーザー**: 日常的なトレーニング管理
- **フィットネス愛好者**: 詳細な進捗追跡と分析
- **アスリート**: 競技レベルのトレーニング管理
- **パーソナルトレーナー**: クライアント管理（将来実装予定）

---

## 📞 サポート・フィードバック

- **Issues**: [GitHub Issues](https://github.com/Message-Team/Message/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Message-Team/Message/discussions)
- **Documentation**: このサイト

---

<div style="text-align: center; margin-top: 2rem; padding: 1rem; background-color: var(--vp-c-bg-soft); border-radius: 8px;">
  <strong>🎯 継続的に更新中</strong><br>
  このドキュメントは開発の進行に合わせて継続的に更新されています。
</div>