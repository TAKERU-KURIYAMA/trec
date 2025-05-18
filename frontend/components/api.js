// components/api/index.js

export async function registerUser(apiBaseUrl, form) {
    return await $fetch('/api/auth/register', {
      method: 'POST',
      baseURL: apiBaseUrl,
      body: form
    })
  }
  
  // ここに今後の API 関数も追加
  // 例：ログイン
  export async function loginUser(apiBaseUrl, credentials) {
    return await $fetch('/api/auth/token', {
      method: 'POST',
      baseURL: apiBaseUrl,
      body: credentials
    })
  }
  
  // 例：トレーニングメニュー取得
  export async function getTrainingMenus(apiBaseUrl, token) {
    return await $fetch('/api/training/menus', {
      method: 'GET',
      baseURL: apiBaseUrl,
      headers: {
        Authorization: `Bearer ${token}`
      }
    })
  }
  
  // トレーニング記録登録（複数件）
export async function postTrainingRecords(apiBaseUrl, token, records) {
    return await $fetch('/api/training/record', {
      method: 'POST',
      baseURL: apiBaseUrl,
      headers: {
        Authorization: `Bearer ${token}`
      },
      body: records
    })
  }
  
  // トレーニング履歴取得
  export async function getTrainingHistory(apiBaseUrl, token, startDate, endDate) {
    return await $fetch('/api/training/history', {
      method: 'GET',
      baseURL: apiBaseUrl,
      headers: {
        Authorization: `Bearer ${token}`
      },
      query: {
        startDate,
        endDate
      }
    })
  }
  