import { reactive, ref } from 'vue'
import { useRuntimeConfig, navigateTo } from '#imports'
import { loginUser } from '~/components/api'

export function useLogin() {
  const form = reactive({
    email: '',
    password: ''
  })
  const message = ref('')
  const error = ref('')
  const config = useRuntimeConfig()

  const login = async () => {
    message.value = ''
    error.value = ''
    try {
      const res = await loginUser(config.public.apiBaseUrl, form)
      localStorage.setItem('access_token', res.access_token)
      message.value = 'ログイン成功'
      navigateTo('/training/record')
    } catch (e) {
      error.value = e?.data?.message || 'ログインに失敗しました'
    }
  }

  return { form, message, error, login }
}
