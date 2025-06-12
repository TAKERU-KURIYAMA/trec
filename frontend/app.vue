<template>
  <div class="app-container">
    <nav class="main-navigation">
      <div class="nav-brand">
        <h1>💪 Message</h1>
        <span class="nav-subtitle">トレーニング記録アプリ</span>
      </div>
      <div class="nav-links">
        <NuxtLink to="/" class="nav-link" :class="{ active: $route.path === '/' }">
          📋 メニュー一覧
        </NuxtLink>
        <NuxtLink to="/dashboard" class="nav-link" :class="{ active: $route.path === '/dashboard' }">
          📊 ダッシュボード
        </NuxtLink>
        <NuxtLink to="/training/history" class="nav-link" :class="{ active: $route.path.includes('/training') }">
          📈 履歴
        </NuxtLink>
      </div>
    </nav>
    <main class="main-content">
      <NuxtPage />
    </main>
  </div>
</template>

<script setup>
useHead({
  title: 'Message - トレーニング記録アプリ',
  meta: [
    { name: 'description', content: 'あなたのトレーニングを記録・分析する高機能アプリ' },
    { name: 'viewport', content: 'width=device-width, initial-scale=1' }
  ]
})
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: 'Helvetica Neue', 'Arial', 'Hiragino Sans', 'Hiragino Kaku Gothic ProN', 'Meiryo', sans-serif;
  background: #f5f7fa;
  color: #2c3e50;
}

.app-container {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.main-navigation {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 15px 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
  position: sticky;
  top: 0;
  z-index: 100;
}

.nav-brand h1 {
  font-size: 1.8rem;
  margin-bottom: 2px;
}

.nav-subtitle {
  font-size: 0.9rem;
  opacity: 0.9;
}

.nav-links {
  display: flex;
  gap: 20px;
}

.nav-link {
  color: white;
  text-decoration: none;
  padding: 10px 15px;
  border-radius: 8px;
  transition: all 0.2s;
  font-weight: 500;
  white-space: nowrap;
}

.nav-link:hover {
  background: rgba(255, 255, 255, 0.15);
  transform: translateY(-1px);
}

.nav-link.active {
  background: rgba(255, 255, 255, 0.25);
  box-shadow: 0 2px 8px rgba(0,0,0,0.2);
}

.main-content {
  flex: 1;
  padding: 20px;
  max-width: 100%;
}

@media (max-width: 768px) {
  .main-navigation {
    flex-direction: column;
    gap: 15px;
    padding: 15px;
  }
  
  .nav-links {
    gap: 10px;
    flex-wrap: wrap;
    justify-content: center;
  }
  
  .nav-link {
    padding: 8px 12px;
    font-size: 0.9rem;
  }
  
  .main-content {
    padding: 15px;
  }
}

/* Global utility classes */
.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.2s;
  text-decoration: none;
  display: inline-block;
  text-align: center;
}

.btn-primary {
  background: linear-gradient(135deg, #3498db, #2980b9);
  color: white;
}

.btn-primary:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(52, 152, 219, 0.3);
}

.btn-success {
  background: linear-gradient(135deg, #27ae60, #229954);
  color: white;
}

.btn-success:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(39, 174, 96, 0.3);
}

.btn-warning {
  background: linear-gradient(135deg, #f39c12, #e67e22);
  color: white;
}

.btn-warning:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(243, 156, 18, 0.3);
}

.card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
  border: 1px solid #e9ecef;
}

.text-center {
  text-align: center;
}

.mt-1 { margin-top: 0.5rem; }
.mt-2 { margin-top: 1rem; }
.mt-3 { margin-top: 1.5rem; }
.mb-1 { margin-bottom: 0.5rem; }
.mb-2 { margin-bottom: 1rem; }
.mb-3 { margin-bottom: 1.5rem; }
</style>
