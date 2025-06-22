# Message システム設計書ドキュメントサイト

## 概要

このディレクトリには、VitePressを使用したMessage システム設計書の静的サイトが含まれています。

## セットアップ

### 前提条件
- Node.js 18.0.0 以上
- npm または yarn

### インストール

```bash
# 依存関係のインストール
npm install

# 開発サーバーの起動
npm run docs:dev

# 本番ビルド
npm run docs:build

# ビルド結果のプレビュー
npm run docs:preview
```

### 開発サーバー

```bash
npm run docs:dev
```

ローカル開発サーバーが `http://localhost:5173` で起動します。

## 構成

### ディレクトリ構造

```
docs/
├── .vitepress/
│   ├── config.js          # VitePress設定
│   └── theme/
│       ├── index.js       # カスタムテーマ
│       └── style.css      # カスタムスタイル
├── features/              # 機能一覧
├── api/                   # API仕様
├── system/                # システム設計
├── screens/               # 画面設計
├── testing/               # テスト設計
├── sequences/             # シーケンス図
├── index.md               # ホームページ
└── package.json           # 依存関係
```

### 設定ファイル

#### `.vitepress/config.js`
- サイト設定
- ナビゲーション構造
- サイドバー設定
- SEO設定

#### `.vitepress/theme/style.css`
- カスタムスタイル
- ブランドカラー
- レスポンシブデザイン

## ページ作成

### 新しいページの追加

1. 適切なディレクトリに `.md` ファイルを作成
2. Front Matter を設定
3. `.vitepress/config.js` のサイドバーに追加

### Front Matter 例

```yaml
---
title: ページタイトル
description: ページの説明
outline: deep
---
```

### マークダウン拡張

VitePressは以下のマークダウン拡張をサポートしています：

- コードブロックのハイライト
- カスタムコンテナ（tip, warning, danger）
- Mermaid図表
- 数式表示（LaTeX）

## デプロイ

### GitHub Pages

```bash
# ビルド
npm run docs:build

# デプロイ
# .vitepress/dist/ ディレクトリの内容をGitHub Pagesに配置
```

### Netlify

1. GitHubリポジトリをNetlifyに接続
2. ビルドコマンド: `npm run docs:build`
3. 公開ディレクトリ: `docs/.vitepress/dist`

### その他のホスティング

生成された `docs/.vitepress/dist/` ディレクトリを任意の静的サイトホスティングサービスに配置できます。

## カスタマイズ

### ブランドカラーの変更

`docs/.vitepress/theme/style.css` の `:root` セクションでカラー変数を変更：

```css
:root {
  --vp-c-brand-1: #3eaf7c;
  --vp-c-brand-2: #369870;
  --vp-c-brand-3: #2d8063;
}
```

### ロゴの変更

1. `docs/public/` にロゴファイルを配置
2. `docs/.vitepress/config.js` でロゴパスを更新

### ナビゲーションの変更

`docs/.vitepress/config.js` の `themeConfig.nav` と `themeConfig.sidebar` を編集

## 参考リンク

- [VitePress 公式ドキュメント](https://vitepress.dev/)
- [Vue.js 3](https://vuejs.org/)
- [Markdown Guide](https://www.markdownguide.org/)
- [Mermaid 図表](https://mermaid.js.org/)

## トラブルシューティング

### よくある問題

1. **ビルドエラー**: Node.js のバージョンを確認
2. **画像が表示されない**: `docs/public/` 配下に配置しているか確認
3. **リンクが動作しない**: 相対パスが正しいか確認

### サポート

問題が発生した場合は、[GitHub Issues](https://github.com/Message-Team/Message/issues) にお寄せください。