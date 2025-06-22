import { defineConfig } from 'vitepress'

export default defineConfig({
  title: 'Message システム設計書',
  description: 'トレーニング管理システム Message の包括的な設計ドキュメント',
  lang: 'ja-JP',
  base: '/',
  
  // テーマ設定
  themeConfig: {
    // ナビゲーションメニュー
    nav: [
      { text: 'ホーム', link: '/' },
      { text: '機能一覧', link: '/features/' },
      { text: 'API仕様', link: '/api/' },
      { text: 'システム設計', link: '/system/' },
      { text: '画面設計', link: '/screens/' },
      { text: 'テスト設計', link: '/testing/' },
      {
        text: 'その他',
        items: [
          { text: 'シーケンス図', link: '/sequences/' },
          { text: 'セットアップ', link: '/setup/' }
        ]
      }
    ],

    // サイドバー設定
    sidebar: {
      '/features/': [
        {
          text: '機能概要',
          items: [
            { text: '機能一覧', link: '/features/' },
            { text: '実装済み機能', link: '/features/implemented' },
            { text: '開発予定機能', link: '/features/planned' }
          ]
        },
        {
          text: '主要機能',
          items: [
            { text: '認証・ユーザー管理', link: '/features/auth' },
            { text: 'トレーニング管理', link: '/features/training' },
            { text: 'サプリメント管理', link: '/features/supplement' },
            { text: 'ダッシュボード', link: '/features/dashboard' }
          ]
        }
      ],
      '/api/': [
        {
          text: 'API仕様',
          items: [
            { text: 'API概要', link: '/api/' },
            { text: '認証API', link: '/api/auth' },
            { text: 'トレーニングAPI', link: '/api/training' },
            { text: 'サプリメントAPI', link: '/api/supplement' },
            { text: '管理者API', link: '/api/admin' }
          ]
        },
        {
          text: 'API詳細',
          items: [
            { text: 'エラーコード', link: '/api/errors' },
            { text: 'レート制限', link: '/api/rate-limiting' },
            { text: 'WebSocket', link: '/api/websocket' }
          ]
        }
      ],
      '/system/': [
        {
          text: 'システム設計',
          items: [
            { text: 'システム概要', link: '/system/' },
            { text: 'アーキテクチャ', link: '/system/architecture' },
            { text: 'データベース設計', link: '/system/database' },
            { text: 'セキュリティ', link: '/system/security' }
          ]
        },
        {
          text: '技術詳細',
          items: [
            { text: 'テクノロジースタック', link: '/system/tech-stack' },
            { text: 'デプロイメント', link: '/system/deployment' },
            { text: '監視・運用', link: '/system/monitoring' }
          ]
        }
      ],
      '/screens/': [
        {
          text: '画面設計',
          items: [
            { text: '画面設計概要', link: '/screens/' },
            { text: 'Web画面', link: '/screens/web' },
            { text: 'モバイル画面', link: '/screens/mobile' },
            { text: 'レスポンシブ対応', link: '/screens/responsive' }
          ]
        },
        {
          text: 'UI/UX詳細',
          items: [
            { text: 'デザインシステム', link: '/screens/design-system' },
            { text: 'アクセシビリティ', link: '/screens/accessibility' },
            { text: 'パフォーマンス最適化', link: '/screens/performance' }
          ]
        }
      ],
      '/testing/': [
        {
          text: 'テスト設計',
          items: [
            { text: 'テスト概要', link: '/testing/' },
            { text: 'テスト戦略', link: '/testing/strategy' },
            { text: '機能テスト', link: '/testing/functional' },
            { text: '非機能テスト', link: '/testing/non-functional' }
          ]
        },
        {
          text: 'テスト実装',
          items: [
            { text: '単体テスト', link: '/testing/unit' },
            { text: '統合テスト', link: '/testing/integration' },
            { text: 'E2Eテスト', link: '/testing/e2e' },
            { text: 'テストデータ', link: '/testing/test-data' }
          ]
        }
      ],
      '/sequences/': [
        {
          text: 'シーケンス図',
          items: [
            { text: 'シーケンス図概要', link: '/sequences/' },
            { text: '認証フロー', link: '/sequences/auth' },
            { text: 'トレーニングフロー', link: '/sequences/training' },
            { text: 'サプリメントフロー', link: '/sequences/supplement' },
            { text: 'データ同期フロー', link: '/sequences/sync' }
          ]
        }
      ]
    },

    // ソーシャルリンク
    socialLinks: [
      { icon: 'github', link: 'https://github.com/Message-Team/Message' }
    ],

    // フッター
    footer: {
      message: 'Message システム設計書',
      copyright: 'Copyright © 2024 Message Team'
    },

    // 検索設定
    search: {
      provider: 'local',
      options: {
        locales: {
          ja: {
            translations: {
              button: {
                buttonText: '検索',
                buttonAriaLabel: '検索'
              },
              modal: {
                displayDetails: '詳細を表示',
                resetButtonTitle: 'リセット',
                backButtonTitle: '戻る',
                noResultsText: '検索結果が見つかりません',
                footer: {
                  selectText: '選択',
                  navigateText: '移動',
                  closeText: '閉じる'
                }
              }
            }
          }
        }
      }
    },

    // 編集リンク
    editLink: {
      pattern: 'https://github.com/Message-Team/Message/edit/main/docs/:path',
      text: 'このページを編集'
    },

    // 最終更新日
    lastUpdated: {
      text: '最終更新日',
      formatOptions: {
        dateStyle: 'short',
        timeStyle: 'medium'
      }
    },

    // アウトライン設定
    outline: {
      level: [2, 3],
      label: '目次'
    }
  },

  // マークダウン設定
  markdown: {
    lineNumbers: true,
    // Mermaidサポート（VitePress組み込み）
    mermaid: true
  },

  // Head設定（SEO、favicon等）
  head: [
    ['link', { rel: 'icon', href: '/favicon.ico' }],
    ['meta', { name: 'theme-color', content: '#3eaf7c' }],
    ['meta', { name: 'apple-mobile-web-app-capable', content: 'yes' }],
    ['meta', { name: 'apple-mobile-web-app-status-bar-style', content: 'black' }],
    ['meta', { name: 'description', content: 'Message トレーニング管理システムの設計ドキュメント' }],
    ['meta', { property: 'og:title', content: 'Message システム設計書' }],
    ['meta', { property: 'og:description', content: 'トレーニング管理システム Message の包括的な設計ドキュメント' }],
    ['meta', { property: 'og:type', content: 'website' }],
    ['meta', { property: 'og:locale', content: 'ja_JP' }]
  ],

  // ビルド設定
  buildEnd: async (config) => {
    // ビルド後の処理（必要に応じて）
  }
})