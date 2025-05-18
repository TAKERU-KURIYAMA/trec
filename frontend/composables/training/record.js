import { ref, onMounted } from 'vue'
import { useRuntimeConfig, useCookie } from '#imports'
import { getTrainingMenus, postTrainingRecords } from '~/components/api'

export const menus = ref([])
export const menuId = ref('')
export const date = ref('')
export const weight = ref(null)
export const reps = ref(null)

export const message = ref('')
export const error = ref('')

const config = useRuntimeConfig()
const token = useCookie('auth_token').value

export const fetchMenus = async () => {
  try {
    const res = await getTrainingMenus(config.public.apiBaseUrl, token)
    menus.value = res
  } catch {
    error.value = 'メニューの取得に失敗しました'
  }
}

export const submit = async () => {
  try {
    await postTrainingRecords(config.public.apiBaseUrl, token, [
      {
        menuId: menuId.value,
        date: date.value,
        weight: weight.value,
        reps: reps.value,
      }
    ])
    message.value = '登録に成功しました'
    error.value = ''
  } catch {
    error.value = '登録に失敗しました'
    message.value = ''
  }
}
