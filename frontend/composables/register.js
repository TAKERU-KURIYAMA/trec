import { ref } from 'vue'
import { useRuntimeConfig } from '#imports'
import { registerUser } from '~/components/api'

export function useRegister() {
  const form = ref({
    email: '',
    password: '',
    displayName: ''
  })

  const message = ref('')
  const error = ref('')
  const config = useRuntimeConfig()

  const register = async () => {
    message.value = ''
    error.value = ''
    try {
      await registerUser(config.public.apiBaseUrl, form.value)
      message.value = '登録に成功しました'
    } catch (err) {
      error.value = err?.data?.message || '登録に失敗しました'
    }
  }

  return {
    form,
    message,
    error,
    register
  }
}
