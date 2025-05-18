<template>
  <div class="p-6 max-w-xl mx-auto">
    <h2 class="text-xl font-bold mb-4">ログイン</h2>
    <form @submit.prevent="login" class="space-y-4">
      <input v-model="form.email" type="email" placeholder="メールアドレス" class="border p-2 w-full" />
      <input v-model="form.password" type="password" placeholder="パスワード" class="border p-2 w-full" />
      <button class="bg-blue-600 text-white px-4 py-2 rounded">ログイン</button>
    </form>
    <p class="text-green-600 mt-2" v-if="message">{{ message }}</p>
    <p class="text-red-600 mt-2" v-if="error">{{ error }}</p>
  </div>
</template>
<script setup>
import { useLogin } from '~/composables/login.js'
const { form, message, error, login } = useLogin()
console.log('form at mount', form)
</script>