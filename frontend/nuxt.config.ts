export default defineNuxtConfig({
  app: {
    head: {
      title: 'TrecPlans - スマートなトレーニング管理',
      titleTemplate: '%s | TrecPlans',
      htmlAttrs: {
        lang: 'ja'
      },
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'description', content: 'TrecPlansは、あなたのフィットネス目標達成をサポートする次世代トレーニング管理アプリです。パーソナライズされたワークアウトプラン、詳細な進捗追跡、AI駆動の推奨機能で、効率的なトレーニングを実現します。' },
        { name: 'keywords', content: 'トレーニング記録,筋トレ,フィットネス,ワークアウト,進捗管理,体重管理,TrecPlans' },
        { name: 'author', content: 'TrecPlans' },
        { name: 'robots', content: 'index, follow' },
        
        // Open Graph / Facebook
        { property: 'og:type', content: 'website' },
        { property: 'og:title', content: 'TrecPlans - スマートなトレーニング管理' },
        { property: 'og:description', content: 'あなたのフィットネス目標達成をサポートする次世代トレーニング管理アプリ' },
        { property: 'og:image', content: '/images/og-image.jpg' },
        { property: 'og:url', content: process.env.NUXT_PUBLIC_SITE_URL || 'https://trecplans.com' },
        { property: 'og:site_name', content: 'TrecPlans' },
        { property: 'og:locale', content: 'ja_JP' },
        
        // Twitter
        { name: 'twitter:card', content: 'summary_large_image' },
        { name: 'twitter:title', content: 'TrecPlans - スマートなトレーニング管理' },
        { name: 'twitter:description', content: 'あなたのフィットネス目標達成をサポートする次世代トレーニング管理アプリ' },
        { name: 'twitter:image', content: '/images/og-image.jpg' },
        
        // PWA
        { name: 'theme-color', content: '#3b82f6' },
        { name: 'apple-mobile-web-app-capable', content: 'yes' },
        { name: 'apple-mobile-web-app-status-bar-style', content: 'default' },
        { name: 'apple-mobile-web-app-title', content: 'TrecPlans' },
        
        // Search Engine Optimization
        { name: 'google-site-verification', content: process.env.NUXT_GOOGLE_SITE_VERIFICATION || '' },
        { name: 'msvalidate.01', content: process.env.NUXT_BING_SITE_VERIFICATION || '' }
      ],
      link: [
        { rel: 'icon', type: 'image/x-icon', href: '/favicon.ico' },
        { rel: 'canonical', href: process.env.NUXT_PUBLIC_SITE_URL || 'https://trecplans.com' },
        { rel: 'apple-touch-icon', sizes: '180x180', href: '/apple-touch-icon.png' },
        { rel: 'manifest', href: '/manifest.json' }
      ]
    },
  },
  modules: [
    '@nuxt/icon',
    '@pinia/nuxt',
    '@nuxtjs/sitemap'
  ],
  site: {
    url: process.env.NUXT_PUBLIC_SITE_URL || 'http://localhost:3000'
  },
  sitemap: {
    hostname: process.env.NUXT_PUBLIC_SITE_URL || 'http://localhost:3000',
    gzip: true,
    routes: [
      '/',
      '/dashboard',
      '/training/history',
      '/goals',
      '/training/presets',
      '/training/schedule'
    ]
  },
  runtimeConfig: {
    public: {
      apiBaseUrl: '/api',
      siteUrl: process.env.NUXT_PUBLIC_SITE_URL || 'https://trecplans.com',
      googleAnalyticsId: process.env.NUXT_GOOGLE_ANALYTICS_ID || '',
      googleSiteVerification: process.env.NUXT_GOOGLE_SITE_VERIFICATION || '',
      bingSiteVerification: process.env.NUXT_BING_SITE_VERIFICATION || ''
    },
  },
  ssr: true,
  css: ['~/assets/css/main.css'],
  nitro: {
    prerender: {
      crawlLinks: true,
      routes: [
        '/',
        '/dashboard',
        '/training/history',
        '/training/presets',
        '/training/schedule',
        '/goals',
        '/tools/timer',
        '/tools/1rm-calculator',
        '/login',
        '/register'
      ]
    }
  }
})
