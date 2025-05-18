<template>
  <div class="max-w-md mx-auto mt-10 p-6 border rounded-xl shadow">
    <h1 class="text-2xl font-bold mb-4">ユーザー登録</h1>

    <form @submit.prevent="register">
      <div class="mb-4">
        <label class="block mb-1">メールアドレス</label>
        <input v-model="form.email" type="email" required class="w-full border rounded px-3 py-2" />
      </div>

      <div class="mb-4">
        <label class="block mb-1">パスワード</label>
        <input v-model="form.password" type="password" required class="w-full border rounded px-3 py-2" />
      </div>

      <div class="mb-4">
        <label class="block mb-1">表示名</label>
        <input v-model="form.displayName" type="text" required class="w-full border rounded px-3 py-2" />
      </div>

      <button type="submit" class="w-full bg-blue-600 text-white py-2 rounded hover:bg-blue-700">
        登録する
      </button>
    </form>

    <p v-if="message" class="mt-4 text-green-600">{{ message }}</p>
    <p v-if="error" class="mt-4 text-red-600">{{ error }}</p>
  </div>
</template>

<script setup>
import { useRegister } from '~/composables/register.js'

const { form, message, error, register } = useRegister()
</script>
