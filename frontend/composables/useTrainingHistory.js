import { ref } from 'vue'
import axios from 'axios'

const records = ref([])
const menuName = ref('')

export default function useTrainingHistory() {
  const fetchHistory = async (menuId) => {
    if (!menuId) {
      alert('メニューIDが指定されていません')
      return
    }

    try {
      const res = await axios.get(`http://localhost:7204/api/1.0/training/daily`, {
        params: { menuId }
      })

      if (res.data.code !== '0000') {
        alert('履歴取得に失敗しました')
        return
      }

      menuName.value = res.data.menuName ?? 'メニュー名不明'
      records.value = res.data.records ?? []
    } catch (err) {
      console.error(err)
      alert('トレーニング履歴の取得に失敗しました')
    }
  }

  return { records, menuName, fetchHistory }
}
