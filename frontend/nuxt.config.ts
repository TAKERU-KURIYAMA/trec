export default defineNuxtConfig({
  app: {
    head: {
      title: 'トレーニング記録',
    },
  },
  runtimeConfig: {
    public: {
      apiBaseUrl: '/api', // ← Nginxで/api/にプロキシしてるのでこれだけでOK
    },
  },
})
