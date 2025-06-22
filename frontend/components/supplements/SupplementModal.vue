<template>
  <div class="fixed inset-0 z-50 overflow-y-auto">
    <div class="flex items-center justify-center min-h-screen px-4">
      <div class="fixed inset-0 bg-black opacity-50" @click="$emit('close')"></div>
      
      <div class="relative bg-white dark:bg-gray-800 rounded-lg max-w-md w-full p-6">
        <h2 class="text-xl font-bold mb-4">
          {{ supplement ? 'サプリメント編集' : 'サプリメント追加' }}
        </h2>
        
        <form @submit.prevent="handleSubmit">
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium mb-1">サプリメント名 <span class="text-red-500">*</span></label>
              <input
                v-model="form.supplementName"
                type="text"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-md dark:bg-gray-700 dark:border-gray-600"
                placeholder="例: プロテイン"
              />
            </div>
            
            <div>
              <label class="block text-sm font-medium mb-1">単位 <span class="text-red-500">*</span></label>
              <input
                v-model="form.unit"
                type="text"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-md dark:bg-gray-700 dark:border-gray-600"
                placeholder="例: g, 錠, ml"
              />
            </div>
            
            <div>
              <label class="block text-sm font-medium mb-1">説明</label>
              <textarea
                v-model="form.description"
                rows="3"
                class="w-full px-3 py-2 border border-gray-300 rounded-md dark:bg-gray-700 dark:border-gray-600"
                placeholder="例: ホエイプロテイン（バニラ味）"
              ></textarea>
            </div>
          </div>
          
          <div class="mt-6 flex justify-end space-x-3">
            <button
              type="button"
              @click="$emit('close')"
              class="px-4 py-2 text-gray-700 bg-gray-200 rounded-md hover:bg-gray-300 dark:bg-gray-700 dark:text-gray-300 dark:hover:bg-gray-600"
            >
              キャンセル
            </button>
            <button
              type="submit"
              class="px-4 py-2 text-white bg-blue-600 rounded-md hover:bg-blue-700"
            >
              保存
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps<{
  supplement?: {
    supplementId: number
    supplementName: string
    unit: string
    description?: string
  }
}>()

const emit = defineEmits<{
  close: []
  save: [data: {
    supplementName: string
    unit: string
    description?: string
  }]
}>()

const form = ref({
  supplementName: '',
  unit: '',
  description: ''
})

// 編集時は初期値を設定
watch(() => props.supplement, (newVal) => {
  if (newVal) {
    form.value = {
      supplementName: newVal.supplementName,
      unit: newVal.unit,
      description: newVal.description || ''
    }
  }
}, { immediate: true })

const handleSubmit = () => {
  emit('save', {
    supplementName: form.value.supplementName,
    unit: form.value.unit,
    description: form.value.description || undefined
  })
}
</script>