<template>
  <div class="fixed inset-0 z-50 overflow-y-auto">
    <div class="flex items-center justify-center min-h-screen px-4">
      <div class="fixed inset-0 bg-black opacity-50" @click="$emit('close')"></div>
      
      <div class="relative bg-white dark:bg-gray-800 rounded-lg max-w-md w-full p-6">
        <h2 class="text-xl font-bold mb-4">摂取記録を追加</h2>
        
        <form @submit.prevent="handleSubmit">
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium mb-1">サプリメント <span class="text-red-500">*</span></label>
              <select
                v-model.number="form.supplementId"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-md dark:bg-gray-700 dark:border-gray-600"
              >
                <option value="">選択してください</option>
                <option
                  v-for="supplement in supplements"
                  :key="supplement.supplementId"
                  :value="supplement.supplementId"
                >
                  {{ supplement.supplementName }} ({{ supplement.unit }})
                </option>
              </select>
            </div>
            
            <div>
              <label class="block text-sm font-medium mb-1">時間 <span class="text-red-500">*</span></label>
              <input
                v-model="form.intakeTime"
                type="time"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-md dark:bg-gray-700 dark:border-gray-600"
              />
            </div>
            
            <div>
              <label class="block text-sm font-medium mb-1">
                量 
                <span v-if="selectedSupplement">({{ selectedSupplement.unit }})</span>
                <span class="text-red-500">*</span>
              </label>
              <input
                v-model.number="form.amount"
                type="number"
                step="0.1"
                min="0"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-md dark:bg-gray-700 dark:border-gray-600"
              />
            </div>
            
            <div>
              <label class="block text-sm font-medium mb-1">タイミング</label>
              <select
                v-model="form.timingType"
                class="w-full px-3 py-2 border border-gray-300 rounded-md dark:bg-gray-700 dark:border-gray-600"
              >
                <option value="">選択してください</option>
                <option v-for="timing in timingTypes" :key="timing.value" :value="timing.value">
                  {{ timing.label }}
                </option>
              </select>
            </div>
            
            <div>
              <label class="block text-sm font-medium mb-1">メモ</label>
              <textarea
                v-model="form.memo"
                rows="2"
                class="w-full px-3 py-2 border border-gray-300 rounded-md dark:bg-gray-700 dark:border-gray-600"
                placeholder="例: 朝食後に摂取"
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
              記録する
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useSupplement } from '~/composables/useSupplement'

const props = defineProps<{
  supplements: Array<{
    supplementId: number
    supplementName: string
    unit: string
  }>
  date: string
}>()

const emit = defineEmits<{
  close: []
  save: [data: {
    supplementId: number
    intakeTime: string
    amount: number
    timingType?: string
    memo?: string
  }]
}>()

const { timingTypes } = useSupplement()

const form = ref({
  supplementId: null as number | null,
  intakeTime: new Date().toTimeString().slice(0, 5),
  amount: 0,
  timingType: '',
  memo: ''
})

const selectedSupplement = computed(() => {
  if (!form.value.supplementId) return null
  return props.supplements.find(s => s.supplementId === form.value.supplementId)
})

const handleSubmit = () => {
  if (!form.value.supplementId) return
  
  emit('save', {
    supplementId: form.value.supplementId,
    intakeTime: form.value.intakeTime,
    amount: form.value.amount,
    timingType: form.value.timingType || undefined,
    memo: form.value.memo || undefined
  })
}
</script>