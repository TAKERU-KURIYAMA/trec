import { ref } from 'vue'

export type AuthModalType = 'login' | 'register' | null

export const useAuthModal = () => {
  const isOpen = ref(false)
  const modalType = ref<AuthModalType>(null)

  const openLogin = () => {
    modalType.value = 'login'
    isOpen.value = true
  }

  const openRegister = () => {
    modalType.value = 'register'
    isOpen.value = true
  }

  const switchToLogin = () => {
    modalType.value = 'login'
  }

  const switchToRegister = () => {
    modalType.value = 'register'
  }

  const close = () => {
    isOpen.value = false
    // モーダルのアニメーションが完了してからタイプをリセット
    setTimeout(() => {
      modalType.value = null
    }, 300)
  }

  return {
    isOpen,
    modalType,
    openLogin,
    openRegister,
    switchToLogin,
    switchToRegister,
    close
  }
}

// グローバルな認証モーダル状態
export const globalAuthModal = useAuthModal()