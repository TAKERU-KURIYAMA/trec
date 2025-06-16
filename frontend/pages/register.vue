<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <div>
        <h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900">
          新規アカウント作成
        </h2>
        <p class="mt-2 text-center text-sm text-gray-600">
          または
          <NuxtLink to="/login" class="font-medium text-indigo-600 hover:text-indigo-500">
            既存のアカウントでログイン
          </NuxtLink>
        </p>
      </div>
      <form class="mt-8 space-y-6" @submit.prevent="handleRegister">
        <div class="rounded-md shadow-sm space-y-4">
          <div>
            <label for="login-id" class="block text-sm font-medium text-gray-700">ログインID</label>
            <input
              id="login-id"
              v-model="loginId"
              name="loginId"
              type="text"
              required
              class="mt-1 appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
              placeholder="英数字のみ、32文字以下"
              :disabled="isLoading"
            >
          </div>
          <div>
            <label for="display-name" class="block text-sm font-medium text-gray-700">表示名</label>
            <input
              id="display-name"
              v-model="displayName"
              name="displayName"
              type="text"
              required
              class="mt-1 appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
              placeholder="64文字以下"
              :disabled="isLoading"
            >
          </div>
          <div>
            <label for="password" class="block text-sm font-medium text-gray-700">パスワード</label>
            <input
              id="password"
              v-model="password"
              name="password"
              type="password"
              required
              class="mt-1 appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
              placeholder="8文字以上128文字以下"
              :disabled="isLoading"
            >
          </div>
          <div>
            <label for="password-confirm" class="block text-sm font-medium text-gray-700">パスワード確認</label>
            <input
              id="password-confirm"
              v-model="passwordConfirm"
              name="passwordConfirm"
              type="password"
              required
              class="mt-1 appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
              placeholder="パスワードを再入力"
              :disabled="isLoading"
            >
          </div>
        </div>

        <div v-if="error" class="rounded-md bg-red-50 p-4">
          <div class="flex">
            <div class="flex-shrink-0">
              <svg class="h-5 w-5 text-red-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
              </svg>
            </div>
            <div class="ml-3">
              <p class="text-sm text-red-800">{{ error }}</p>
            </div>
          </div>
        </div>

        <div v-if="success" class="rounded-md bg-green-50 p-4">
          <div class="flex">
            <div class="flex-shrink-0">
              <svg class="h-5 w-5 text-green-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" />
              </svg>
            </div>
            <div class="ml-3">
              <p class="text-sm text-green-800">{{ success }}</p>
            </div>
          </div>
        </div>

        <div>
          <button
            type="submit"
            :disabled="isLoading"
            class="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <span v-if="isLoading" class="absolute left-0 inset-y-0 flex items-center pl-3">
              <svg class="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
            </span>
            {{ isLoading ? '登録中...' : 'アカウント作成' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '~/stores/auth'
import { apiClient } from '~/utils/api-client'
import { hashPassword } from '~/utils/crypto'

definePageMeta({
  middleware: 'guest'
})

const router = useRouter()
const authStore = useAuthStore()

const loginId = ref('')
const displayName = ref('')
const password = ref('')
const passwordConfirm = ref('')
const error = ref('')
const success = ref('')
const isLoading = ref(false)

const validateForm = () => {
  if (!loginId.value || !displayName.value || !password.value || !passwordConfirm.value) {
    error.value = 'すべての項目を入力してください'
    return false
  }

  if (loginId.value.length > 32) {
    error.value = 'ログインIDは32文字以下で入力してください'
    return false
  }

  if (!/^[a-zA-Z0-9]+$/.test(loginId.value)) {
    error.value = 'ログインIDは英数字のみ使用可能です'
    return false
  }

  if (displayName.value.length > 64) {
    error.value = '表示名は64文字以下で入力してください'
    return false
  }

  if (password.value.length < 8 || password.value.length > 128) {
    error.value = 'パスワードは8文字以上128文字以下で入力してください'
    return false
  }

  if (password.value !== passwordConfirm.value) {
    error.value = 'パスワードと確認用パスワードが一致しません'
    return false
  }

  return true
}

const handleRegister = async () => {
  error.value = ''
  success.value = ''

  if (!validateForm()) {
    return
  }

  isLoading.value = true

  try {
    // パスワードをSHA256でハッシュ化
    const hashedPassword = await hashPassword(password.value)
    
    const response = await apiClient.post('/auth/register', {
      loginId: loginId.value,
      displayName: displayName.value,
      password: hashedPassword
    })

    if (response && response.token && response.user) {
      await authStore.setAuth(response.token, response.user)
      success.value = 'アカウントが作成されました。ダッシュボードに移動します...'
      
      console.log('🚀 Registration page successful, admin status:', authStore.isAdmin)
      
      setTimeout(async () => {
        await router.push('/dashboard')
      }, 2000)
    } else {
      error.value = response?.userMessage || 'アカウント作成に失敗しました'
    }
  } catch (err: any) {
    if (err.response?.data?.error?.message) {
      error.value = err.response.data.error.message
    } else {
      error.value = 'ネットワークエラーが発生しました。後でもう一度お試しください。'
    }
  } finally {
    isLoading.value = false
  }
}
</script>