<template>
  <div class="p-6 max-w-xl mx-auto">
    <h2 class="text-xl font-bold mb-4">トレーニング記録</h2>

    <label class="block mb-1 font-semibold">メニュー</label>
    <select v-model="menuId" class="border p-2 w-full mb-4">
      <option disabled value="">選択してください</option>
      <option v-for="menu in menus" :key="menu.id" :value="menu.id">{{ menu.name }}</option>
    </select>

    <label class="block mb-1 font-semibold">日付</label>
    <input type="date" v-model="date" class="border p-2 w-full mb-4" />

    <label class="block mb-1 font-semibold">重量 (kg)</label>
    <input type="number" v-model="weight" class="border p-2 w-full mb-4" />

    <label class="block mb-1 font-semibold">回数</label>
    <input type="number" v-model="reps" class="border p-2 w-full mb-4" />

    <button @click="submit" class="bg-blue-600 text-white px-4 py-2 rounded">登録</button>

    <p v-if="message" class="text-green-600 mt-2">{{ message }}</p>
    <p v-if="error" class="text-red-600 mt-2">{{ error }}</p>
  </div>
</template>

<script setup>
import {
  menus, menuId, date, weight, reps,
  message, error, submit, fetchMenus
} from '~/composables/training/record.js'

onMounted(fetchMenus)
</script>
