<script setup>
const token = useState('accessToken', () => localStorage.getItem('access_token'))
</script>

<template>
  <div>
    <nav class="bg-gray-800 text-white px-4 py-2 flex gap-4">
      <NuxtLink to="/">Home</NuxtLink>
      <NuxtLink to="/register">登録</NuxtLink>
      <NuxtLink v-if="!token" to="/login">ログイン</NuxtLink>
      <NuxtLink v-if="token" to="/training/record">記録</NuxtLink>
      <NuxtLink v-if="token" to="/training/history">履歴</NuxtLink>
    </nav>
    <main class="p-4">
      <slot />
    </main>
  </div>
</template>
