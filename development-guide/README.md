# Message システム開発ガイド

🎯 **役割分担に基づく効率的な開発体制**

## 👥 開発体制

### 🏗️ バックエンド開発（手動）
- **担当**: 開発者自身
- **技術**: ASP.NET Core 8.0, PostgreSQL, Entity Framework Core
- **責任範囲**: API設計・実装、データベース設計、認証・認可、ビジネスロジック

### 📱 クライアント開発（Claude Code）
- **担当**: Claude Code
- **技術**: Vue 3/Nuxt 3 (Web), React Native (Mobile)
- **責任範囲**: UI/UX実装、状態管理、API連携、レスポンシブ対応

## 📚 ガイド構成

| ガイド | 対象 | 目的 |
|--------|------|------|
| **[BACKEND_GUIDE.md](./BACKEND_GUIDE.md)** | 開発者 | バックエンド開発の完全手順 |
| **[FRONTEND_GUIDE.md](./FRONTEND_GUIDE.md)** | Claude Code | Webクライアント開発指示 |
| **[MOBILE_GUIDE.md](./MOBILE_GUIDE.md)** | Claude Code | モバイルアプリ開発指示 |
| **[API_CONTRACT.md](./API_CONTRACT.md)** | 両者 | API仕様の合意事項 |
| **[INTEGRATION_GUIDE.md](./INTEGRATION_GUIDE.md)** | 両者 | 統合テスト・デプロイ手順 |

## 🔄 開発フロー

### Phase 1: 基盤構築
1. **Backend**: データベース設計・基本API実装
2. **Frontend**: Claude Codeによる基本UI実装
3. **Integration**: 基本的な連携テスト

### Phase 2: 機能実装
1. **Backend**: 各機能のAPI実装
2. **Frontend**: Claude Codeによる機能UI実装
3. **Integration**: 機能別統合テスト

### Phase 3: 最適化・デプロイ
1. **Backend**: パフォーマンス最適化
2. **Frontend**: Claude CodeによるUX改善
3. **Integration**: 本番環境デプロイ

## 🎯 成功のポイント

### 明確な責任分離
- **API仕様**: 事前に詳細合意
- **データ形式**: 標準化されたJSON形式
- **エラーハンドリング**: 統一されたエラーコード

### 効率的なコミュニケーション
- **API Contract**: 変更時は必ず更新
- **進捗共有**: 各Phase完了時に確認
- **問題解決**: 統合時の問題は速やかに共有

---

**🚀 各ガイドを参照して、効率的な開発を進めてください！**