<template>
  <div>
    <!-- Login Modal -->
    <LoginModal
      :is-open="modalType === 'login' && isOpen"
      @close="close"
      @success="handleSuccess"
      @register="switchToRegister"
    />

    <!-- Register Modal -->
    <RegisterModal
      :is-open="modalType === 'register' && isOpen"
      @close="close"
      @success="handleSuccess"
      @login="switchToLogin"
    />
  </div>
</template>

<script setup lang="ts">
import { globalAuthModal } from '~/composables/useAuthModal'
import LoginModal from './LoginModal.vue'
import RegisterModal from './RegisterModal.vue'

const { isOpen, modalType, switchToLogin, switchToRegister, close } = globalAuthModal

const handleSuccess = () => {
  // ログイン/登録成功時の処理
  // 必要に応じてページをリロードしたり、特定の処理を実行
}
</script>