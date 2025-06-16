<template>
  <Transition name="modal">
    <div v-if="isOpen" class="modal-overlay" @click="closeModal">
      <div class="modal-container" @click.stop>
        <div class="modal-header">
          <h2 class="modal-title">ログイン</h2>
          <button @click="closeModal" class="close-button">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
            </svg>
          </button>
        </div>

        <div class="modal-body">
          <form @submit.prevent="handleLogin">
            <div class="form-group">
              <label for="modal-login-id" class="form-label">ログインID</label>
              <input
                id="modal-login-id"
                v-model="loginId"
                type="text"
                required
                class="form-input"
                placeholder="ログインIDを入力"
                :disabled="isLoading"
              >
            </div>

            <div class="form-group">
              <label for="modal-password" class="form-label">パスワード</label>
              <input
                id="modal-password"
                v-model="password"
                type="password"
                required
                class="form-input"
                placeholder="パスワードを入力"
                :disabled="isLoading"
              >
            </div>

            <div v-if="error" class="error-message">
              <div class="error-icon">
                <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
                </svg>
              </div>
              <span>{{ error }}</span>
            </div>

            <div class="modal-actions">
              <button
                type="button"
                @click="closeModal"
                class="cancel-button"
                :disabled="isLoading"
              >
                キャンセル
              </button>
              <button
                type="submit"
                class="login-button"
                :disabled="isLoading"
              >
                <span v-if="isLoading" class="loading-spinner">
                  <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                </span>
                {{ isLoading ? 'ログイン中...' : 'ログイン' }}
              </button>
            </div>
          </form>

          <div class="register-link">
            <p class="register-text">
              アカウントをお持ちでない場合は
              <button @click="goToRegister" class="register-button">
                新規登録
              </button>
            </p>
          </div>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useAuthStore } from '~/stores/auth'
import { apiClient } from '~/utils/api-client'
import { hashPassword } from '~/utils/crypto'

interface Props {
  isOpen: boolean
}

interface Emits {
  (e: 'close'): void
  (e: 'success'): void
  (e: 'register'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const authStore = useAuthStore()

const loginId = ref('')
const password = ref('')
const error = ref('')
const isLoading = ref(false)

// モーダルが開かれた時にフォームをリセット
watch(() => props.isOpen, (newValue) => {
  if (newValue) {
    resetForm()
  }
})

const resetForm = () => {
  loginId.value = ''
  password.value = ''
  error.value = ''
  isLoading.value = false
}

const closeModal = () => {
  if (!isLoading.value) {
    emit('close')
  }
}

const goToRegister = () => {
  emit('register')
}

const handleLogin = async () => {
  error.value = ''
  isLoading.value = true

  try {
    // パスワードをSHA256でハッシュ化
    const hashedPassword = await hashPassword(password.value)
    
    const response = await apiClient.post('/api/auth/login', {
      loginId: loginId.value,
      password: hashedPassword
    })

    // APIレスポンス形式を確認
    // APIクライアントがすでにdata部分を展開済み
    if (response && response.token && response.user) {
      await authStore.setAuth(response.token, response.user)
      
      console.log('🚀 Login successful, admin status:', authStore.isAdmin)
      
      emit('success')
      emit('close')
    } else if (response && response.userMessage) {
      error.value = response.userMessage || 'ログインに失敗しました'
    } else {
      error.value = 'ログインに失敗しました'
    }
  } catch (err: any) {
    console.error('Login error details:', err)
    
    if (err.message && err.message.includes('ハッシュ化に失敗')) {
      error.value = 'パスワードの処理でエラーが発生しました。ページを更新して再度お試しください。'
    } else if (err.response?.data?.userMessage) {
      error.value = err.response.data.userMessage
    } else if (err.response?.data?.message) {
      error.value = err.response.data.message
    } else if (err.message) {
      error.value = `エラー: ${err.message}`
    } else {
      error.value = 'ネットワークエラーが発生しました。後でもう一度お試しください。'
    }
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 16px;
}

.modal-container {
  background: white;
  border-radius: 12px;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  max-width: 420px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px 24px 16px;
  border-bottom: 1px solid #e5e7eb;
}

.modal-title {
  font-size: 1.5rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0;
}

.close-button {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: none;
  background: none;
  color: #6b7280;
  cursor: pointer;
  border-radius: 6px;
  transition: all 0.2s;
}

.close-button:hover {
  background-color: #f3f4f6;
  color: #374151;
}

.modal-body {
  padding: 24px;
}

.form-group {
  margin-bottom: 20px;
}

.form-label {
  display: block;
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
  margin-bottom: 6px;
}

.form-input {
  width: 100%;
  padding: 12px 16px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 1rem;
  transition: all 0.2s;
  background-color: white;
}

.form-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.form-input:disabled {
  background-color: #f9fafb;
  color: #6b7280;
  cursor: not-allowed;
}

.error-message {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px;
  background-color: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 8px;
  color: #dc2626;
  font-size: 0.875rem;
  margin-bottom: 20px;
}

.error-icon {
  flex-shrink: 0;
}

.modal-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  margin-top: 24px;
}

.cancel-button {
  padding: 12px 24px;
  border: 1px solid #d1d5db;
  background-color: white;
  color: #374151;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.cancel-button:hover:not(:disabled) {
  background-color: #f9fafb;
  border-color: #9ca3af;
}

.cancel-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.login-button {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
  background-color: #3b82f6;
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.login-button:hover:not(:disabled) {
  background-color: #2563eb;
}

.login-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.loading-spinner {
  display: flex;
  align-items: center;
}

.register-link {
  margin-top: 24px;
  padding-top: 20px;
  border-top: 1px solid #e5e7eb;
  text-align: center;
}

.register-text {
  color: #6b7280;
  font-size: 0.875rem;
  margin: 0;
}

.register-button {
  color: #3b82f6;
  background: none;
  border: none;
  font-weight: 500;
  cursor: pointer;
  text-decoration: underline;
  font-size: 0.875rem;
}

.register-button:hover {
  color: #2563eb;
}

/* Transition animations */
.modal-enter-active, .modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-from, .modal-leave-to {
  opacity: 0;
}

.modal-enter-active .modal-container,
.modal-leave-active .modal-container {
  transition: transform 0.3s ease;
}

.modal-enter-from .modal-container,
.modal-leave-to .modal-container {
  transform: scale(0.9) translateY(-20px);
}

@media (max-width: 480px) {
  .modal-container {
    margin: 16px;
    max-width: none;
  }
  
  .modal-header,
  .modal-body {
    padding: 20px;
  }
  
  .modal-actions {
    flex-direction: column;
  }
  
  .cancel-button,
  .login-button {
    width: 100%;
  }
}
</style>