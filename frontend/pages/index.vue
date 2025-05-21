<template>
  <div>
    <h1>トレーニングメニュー一覧</h1>
    <input v-model="tag" placeholder="タグで検索" />

    <div class="menu-list">
      <TrainingCard
        v-for="menu in filteredMenus"
        :key="menu.menuId"
        :menu="menu"
        @click="goToMenu(menu.menuId)"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useFetchMenus } from '~/composables/useFetchMenus'
import TrainingCard from '~/components/TrainingCard.vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const tag = ref('')
const menus = ref([])

onMounted(async () => {
  menus.value = await useFetchMenus()
})

const filteredMenus = computed(() => {
  if (!tag.value) return menus.value
  return menus.value.filter(menu =>
    menu.tags.some(tagObj => tagObj.tagId.includes(tag.value))
  )
})

function goToMenu(menuId) {
  router.push(`/training/history?menuId=${menuId}`)
}
</script>
