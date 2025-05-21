export default defineNuxtConfig({
  app: {
    head: {
      title: 'トレーニング記録',
    },
  },
  runtimeConfig: {
    public: {
      apiBaseUrl: process.env.API_BASE_URL || 'http://localhost:7204/api/1.0',
    },
  },
})
