# フロントエンド開発ガイド（Claude Code向け）

🎨 **Vue 3/Nuxt 3 による Message システム Web クライアント開発**

## 🎯 開発概要

### 技術スタック
- **Framework**: Nuxt 3 (Vue 3ベース)
- **UI Framework**: Tailwind CSS
- **State Management**: Pinia
- **TypeScript**: 完全型安全
- **Testing**: Vitest, Testing Library
- **Build Tool**: Vite

### 開発責任範囲
- UI/UX実装
- レスポンシブデザイン
- API連携（認証含む）
- 状態管理
- フォームバリデーション
- パフォーマンス最適化

## 📋 実装計画

### Phase 1: 基盤構築（2-3日）
- [x] Nuxt 3プロジェクト設定
- [ ] 認証システム基盤
- [ ] API クライアント設定
- [ ] 基本レイアウト作成
- [ ] ルーティング設定

### Phase 2: 認証機能（1-2日）
- [ ] ログイン/登録画面
- [ ] JWT トークン管理
- [ ] 認証ミドルウェア
- [ ] ユーザー状態管理

### Phase 3: トレーニング機能（3-4日）
- [ ] トレーニングメニュー表示
- [ ] 記録入力フォーム
- [ ] 進捗可視化
- [ ] 履歴管理

### Phase 4: サプリメント機能（2-3日）
- [ ] サプリメント登録
- [ ] 摂取記録機能
- [ ] スケジュール管理
- [ ] 統計表示

### Phase 5: 最適化・デプロイ（1-2日）
- [ ] パフォーマンス最適化
- [ ] モバイル対応強化
- [ ] SEO対応
- [ ] エラーハンドリング改善

## 🏗️ プロジェクト構造

```
frontend/
├── components/           # Vue コンポーネント
│   ├── auth/            # 認証関連
│   ├── training/        # トレーニング機能
│   ├── supplement/      # サプリメント機能
│   ├── ui/              # 共通UIコンポーネント
│   └── layout/          # レイアウトコンポーネント
├── composables/         # Vue Composables
│   ├── useAuth.ts
│   ├── useTraining.ts
│   ├── useSupplement.ts
│   └── useApi.ts
├── pages/               # ページコンポーネント
│   ├── index.vue        # ホーム
│   ├── login.vue        # ログイン
│   ├── dashboard.vue    # ダッシュボード
│   └── training/        # トレーニング関連ページ
├── stores/              # Pinia ストア
│   ├── auth.ts
│   ├── training.ts
│   └── supplement.ts
├── types/               # TypeScript型定義
├── utils/               # ユーティリティ関数
├── middleware/          # ルートミドルウェア
└── assets/              # 静的アセット
```

## 🔐 認証システム実装

### composables/useAuth.ts
```typescript
export const useAuth = () => {
  const user = ref<User | null>(null)
  const token = useCookie('auth-token', {
    secure: true,
    sameSite: 'strict',
    maxAge: 60 * 60 * 24 * 7 // 7日
  })

  const login = async (credentials: LoginCredentials) => {
    try {
      const { data } = await $fetch('/api/auth/login', {
        method: 'POST',
        body: credentials
      })
      
      token.value = data.accessToken
      user.value = data.user
      
      await navigateTo('/dashboard')
    } catch (error) {
      throw new Error('ログインに失敗しました')
    }
  }

  const logout = async () => {
    try {
      await $fetch('/api/auth/logout', {
        method: 'POST',
        headers: { Authorization: `Bearer ${token.value}` }
      })
    } finally {
      token.value = null
      user.value = null
      await navigateTo('/login')
    }
  }

  const refreshToken = async () => {
    try {
      const { data } = await $fetch('/api/auth/refresh', {
        method: 'POST',
        headers: { Authorization: `Bearer ${token.value}` }
      })
      
      token.value = data.accessToken
      return data.accessToken
    } catch (error) {
      await logout()
      throw error
    }
  }

  return {
    user,
    token,
    login,
    logout,
    refreshToken,
    isAuthenticated: computed(() => !!token.value)
  }
}
```

### middleware/auth.ts
```typescript
export default defineNuxtRouteMiddleware((to, from) => {
  const { isAuthenticated } = useAuth()
  
  if (!isAuthenticated.value) {
    return navigateTo('/login')
  }
})
```

## 🏋️ トレーニング機能実装

### composables/useTraining.ts
```typescript
export const useTraining = () => {
  const { $fetch } = useNuxtApp()
  const { token } = useAuth()
  
  const trainingMenus = ref<TrainingMenu[]>([])
  const trainingRecords = ref<TrainingRecord[]>([])
  const isLoading = ref(false)

  const fetchMenus = async () => {
    isLoading.value = true
    try {
      const { data } = await $fetch('/api/training/menus', {
        headers: { Authorization: `Bearer ${token.value}` }
      })
      trainingMenus.value = data
    } finally {
      isLoading.value = false
    }
  }

  const createRecord = async (record: CreateTrainingRecord) => {
    const { data } = await $fetch('/api/training/records', {
      method: 'POST',
      headers: { Authorization: `Bearer ${token.value}` },
      body: record
    })
    
    trainingRecords.value.push(data)
    return data
  }

  const fetchRecords = async (dateRange?: DateRange) => {
    const query = dateRange ? {
      fromDate: dateRange.from,
      toDate: dateRange.to
    } : {}
    
    const { data } = await $fetch('/api/training/records', {
      headers: { Authorization: `Bearer ${token.value}` },
      query
    })
    
    trainingRecords.value = data
  }

  return {
    trainingMenus,
    trainingRecords,
    isLoading,
    fetchMenus,
    createRecord,
    fetchRecords
  }
}
```

### components/training/TrainingForm.vue
```vue
<template>
  <form @submit.prevent="handleSubmit" class="space-y-4">
    <div>
      <label class="block text-sm font-medium mb-2">
        トレーニングメニュー
      </label>
      <select 
        v-model="form.menuId" 
        required
        class="w-full p-3 border rounded-lg focus:ring-2 focus:ring-blue-500"
      >
        <option value="">選択してください</option>
        <option 
          v-for="menu in trainingMenus" 
          :key="menu.menuId"
          :value="menu.menuId"
        >
          {{ menu.jpName }}
        </option>
      </select>
    </div>

    <div class="grid grid-cols-2 gap-4">
      <div>
        <label class="block text-sm font-medium mb-2">重量 (kg)</label>
        <input 
          v-model.number="form.weight"
          type="number"
          step="0.5"
          class="w-full p-3 border rounded-lg focus:ring-2 focus:ring-blue-500"
          placeholder="0.0"
        />
      </div>
      
      <div>
        <label class="block text-sm font-medium mb-2">回数</label>
        <input 
          v-model.number="form.reps"
          type="number"
          required
          class="w-full p-3 border rounded-lg focus:ring-2 focus:ring-blue-500"
          placeholder="0"
        />
      </div>
    </div>

    <div>
      <label class="block text-sm font-medium mb-2">メモ</label>
      <textarea 
        v-model="form.memo"
        rows="3"
        class="w-full p-3 border rounded-lg focus:ring-2 focus:ring-blue-500"
        placeholder="記録メモ（任意）"
      />
    </div>

    <button 
      type="submit"
      :disabled="isSubmitting"
      class="w-full py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
    >
      <span v-if="isSubmitting">保存中...</span>
      <span v-else>記録を保存</span>
    </button>
  </form>
</template>

<script setup lang="ts">
interface TrainingFormData {
  menuId: string
  weight: number
  reps: number
  setNumber: number
  memo: string
}

const { createRecord } = useTraining()
const { trainingMenus } = storeToRefs(useTrainingStore())

const form = reactive<TrainingFormData>({
  menuId: '',
  weight: 0,
  reps: 0,
  setNumber: 1,
  memo: ''
})

const isSubmitting = ref(false)

const handleSubmit = async () => {
  isSubmitting.value = true
  try {
    await createRecord({
      ...form,
      trainingDate: new Date().toISOString().split('T')[0]
    })
    
    // フォームリセット
    Object.assign(form, {
      menuId: '',
      weight: 0,
      reps: 0,
      setNumber: 1,
      memo: ''
    })
    
    useToast().success('記録を保存しました')
  } catch (error) {
    useToast().error('保存に失敗しました')
  } finally {
    isSubmitting.value = false
  }
}
</script>
```

## 💊 サプリメント機能実装

### composables/useSupplement.ts
```typescript
export const useSupplement = () => {
  const { $fetch } = useNuxtApp()
  const { token } = useAuth()
  
  const supplements = ref<Supplement[]>([])
  const intakeRecords = ref<SupplementIntakeRecord[]>([])
  const schedules = ref<SupplementSchedule[]>([])

  const fetchSupplements = async () => {
    const { data } = await $fetch('/api/supplement/supplements', {
      headers: { Authorization: `Bearer ${token.value}` }
    })
    supplements.value = data
  }

  const createSupplement = async (supplement: CreateSupplementRequest) => {
    const { data } = await $fetch('/api/supplement/supplements', {
      method: 'POST',
      headers: { Authorization: `Bearer ${token.value}` },
      body: supplement
    })
    
    supplements.value.push(data)
    return data
  }

  const recordIntake = async (intake: CreateIntakeRecord) => {
    const { data } = await $fetch('/api/supplement/intake-records', {
      method: 'POST',
      headers: { Authorization: `Bearer ${token.value}` },
      body: intake
    })
    
    intakeRecords.value.push(data)
    return data
  }

  const createSchedule = async (schedule: CreateScheduleRequest) => {
    const { data } = await $fetch('/api/supplement/schedules', {
      method: 'POST',
      headers: { Authorization: `Bearer ${token.value}` },
      body: schedule
    })
    
    schedules.value.push(data)
    return data
  }

  return {
    supplements,
    intakeRecords,
    schedules,
    fetchSupplements,
    createSupplement,
    recordIntake,
    createSchedule
  }
}
```

### components/supplement/SupplementCard.vue
```vue
<template>
  <div class="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow">
    <div class="flex justify-between items-start mb-4">
      <div>
        <h3 class="text-lg font-semibold text-gray-900">
          {{ supplement.supplementName }}
        </h3>
        <p class="text-sm text-gray-500">{{ supplement.unit }}</p>
      </div>
      
      <div class="flex space-x-2">
        <button 
          @click="openIntakeModal"
          class="px-3 py-1 bg-green-100 text-green-700 rounded-full text-sm hover:bg-green-200"
        >
          摂取記録
        </button>
        <button 
          @click="openEditModal"
          class="px-3 py-1 bg-blue-100 text-blue-700 rounded-full text-sm hover:bg-blue-200"
        >
          編集
        </button>
      </div>
    </div>

    <p v-if="supplement.description" class="text-gray-600 text-sm mb-4">
      {{ supplement.description }}
    </p>

    <div class="space-y-2">
      <div class="flex justify-between text-sm">
        <span class="text-gray-500">今日の摂取量</span>
        <span class="font-medium">{{ todayIntake }}{{ supplement.unit }}</span>
      </div>
      
      <div class="flex justify-between text-sm">
        <span class="text-gray-500">今週の摂取日数</span>
        <span class="font-medium">{{ weeklyIntakeDays }}/7日</span>
      </div>
    </div>

    <!-- 今日のスケジュール -->
    <div v-if="todaySchedules.length > 0" class="mt-4 pt-4 border-t">
      <h4 class="text-sm font-medium text-gray-700 mb-2">今日の予定</h4>
      <div class="space-y-1">
        <div 
          v-for="schedule in todaySchedules" 
          :key="schedule.scheduleId"
          class="flex justify-between items-center text-sm"
        >
          <span>{{ schedule.scheduledTime }}</span>
          <span class="text-gray-500">{{ schedule.dosage }}{{ supplement.unit }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  supplement: Supplement
  intakeRecords: SupplementIntakeRecord[]
  schedules: SupplementSchedule[]
}

const props = defineProps<Props>()
const emit = defineEmits<{
  openIntakeModal: [supplement: Supplement]
  openEditModal: [supplement: Supplement]
}>()

const todayIntake = computed(() => {
  const today = new Date().toISOString().split('T')[0]
  return props.intakeRecords
    .filter(record => 
      record.supplementId === props.supplement.supplementId &&
      record.intakeDate === today
    )
    .reduce((total, record) => total + record.amount, 0)
})

const weeklyIntakeDays = computed(() => {
  const oneWeekAgo = new Date()
  oneWeekAgo.setDate(oneWeekAgo.getDate() - 7)
  
  const intakeDates = new Set(
    props.intakeRecords
      .filter(record => 
        record.supplementId === props.supplement.supplementId &&
        new Date(record.intakeDate) >= oneWeekAgo
      )
      .map(record => record.intakeDate)
  )
  
  return intakeDates.size
})

const todaySchedules = computed(() => {
  const today = new Date().toISOString().split('T')[0]
  return props.schedules.filter(schedule => 
    schedule.supplementId === props.supplement.supplementId &&
    schedule.effectiveDate <= today &&
    (!schedule.expirationDate || schedule.expirationDate >= today)
  )
})

const openIntakeModal = () => emit('openIntakeModal', props.supplement)
const openEditModal = () => emit('openEditModal', props.supplement)
</script>
```

## 🎨 レイアウトとナビゲーション

### layouts/default.vue
```vue
<template>
  <div class="min-h-screen bg-gray-50">
    <!-- ヘッダー -->
    <header class="bg-white shadow-sm border-b">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center h-16">
          <div class="flex items-center">
            <NuxtLink to="/dashboard" class="text-xl font-bold text-blue-600">
              Message
            </NuxtLink>
          </div>
          
          <nav class="hidden md:flex space-x-8">
            <NuxtLink 
              to="/dashboard"
              class="text-gray-700 hover:text-blue-600 px-3 py-2 rounded-md text-sm font-medium"
            >
              ダッシュボード
            </NuxtLink>
            <NuxtLink 
              to="/training"
              class="text-gray-700 hover:text-blue-600 px-3 py-2 rounded-md text-sm font-medium"
            >
              トレーニング
            </NuxtLink>
            <NuxtLink 
              to="/supplement"
              class="text-gray-700 hover:text-blue-600 px-3 py-2 rounded-md text-sm font-medium"
            >
              サプリメント
            </NuxtLink>
          </nav>

          <div class="flex items-center space-x-4">
            <UserMenu />
          </div>
        </div>
      </div>
    </header>

    <!-- メインコンテンツ -->
    <main class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
      <slot />
    </main>

    <!-- モバイルナビゲーション -->
    <MobileNavigation class="md:hidden" />
  </div>
</template>
```

### components/layout/MobileNavigation.vue
```vue
<template>
  <nav class="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 px-4 py-2">
    <div class="flex justify-around">
      <NuxtLink 
        to="/dashboard"
        class="flex flex-col items-center py-2 px-3 rounded-lg"
        :class="{ 'text-blue-600 bg-blue-50': $route.path === '/dashboard' }"
      >
        <Icon name="home" class="w-6 h-6" />
        <span class="text-xs mt-1">ホーム</span>
      </NuxtLink>
      
      <NuxtLink 
        to="/training"
        class="flex flex-col items-center py-2 px-3 rounded-lg"
        :class="{ 'text-blue-600 bg-blue-50': $route.path.startsWith('/training') }"
      >
        <Icon name="dumbbell" class="w-6 h-6" />
        <span class="text-xs mt-1">運動</span>
      </NuxtLink>
      
      <NuxtLink 
        to="/supplement"
        class="flex flex-col items-center py-2 px-3 rounded-lg"
        :class="{ 'text-blue-600 bg-blue-50': $route.path.startsWith('/supplement') }"
      >
        <Icon name="pill" class="w-6 h-6" />
        <span class="text-xs mt-1">サプリ</span>
      </NuxtLink>
      
      <NuxtLink 
        to="/profile"
        class="flex flex-col items-center py-2 px-3 rounded-lg"
        :class="{ 'text-blue-600 bg-blue-50': $route.path === '/profile' }"
      >
        <Icon name="user" class="w-6 h-6" />
        <span class="text-xs mt-1">設定</span>
      </NuxtLink>
    </div>
  </nav>
</template>
```

## 📊 データ可視化

### components/dashboard/ProgressChart.vue
```vue
<template>
  <div class="bg-white rounded-lg shadow p-6">
    <h3 class="text-lg font-semibold mb-4">進捗チャート</h3>
    
    <div class="relative h-64">
      <canvas ref="chartCanvas" />
    </div>
    
    <div class="mt-4 flex justify-center space-x-4">
      <button 
        v-for="period in periods"
        :key="period.value"
        @click="selectedPeriod = period.value"
        class="px-3 py-1 rounded-full text-sm"
        :class="selectedPeriod === period.value 
          ? 'bg-blue-100 text-blue-700' 
          : 'text-gray-600 hover:bg-gray-100'"
      >
        {{ period.label }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Chart, registerables } from 'chart.js'

Chart.register(...registerables)

const chartCanvas = ref<HTMLCanvasElement | null>(null)
const chart = ref<Chart | null>(null)

const selectedPeriod = ref('7d')
const periods = [
  { value: '7d', label: '1週間' },
  { value: '30d', label: '1ヶ月' },
  { value: '90d', label: '3ヶ月' }
]

const { fetchProgressData } = useTraining()

const chartData = ref<ChartData>({
  labels: [],
  datasets: []
})

watch(selectedPeriod, async (newPeriod) => {
  const data = await fetchProgressData(newPeriod)
  updateChart(data)
})

const updateChart = (data: ProgressData) => {
  if (!chart.value) return
  
  chart.value.data = {
    labels: data.labels,
    datasets: [{
      label: 'トレーニング量',
      data: data.values,
      borderColor: 'rgb(59, 130, 246)',
      backgroundColor: 'rgba(59, 130, 246, 0.1)',
      tension: 0.4
    }]
  }
  
  chart.value.update()
}

onMounted(() => {
  if (!chartCanvas.value) return
  
  chart.value = new Chart(chartCanvas.value, {
    type: 'line',
    data: chartData.value,
    options: {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        y: {
          beginAtZero: true
        }
      }
    }
  })
})

onUnmounted(() => {
  chart.value?.destroy()
})
</script>
```

## 🔧 設定ファイル

### nuxt.config.ts
```typescript
export default defineNuxtConfig({
  devtools: { enabled: true },
  
  modules: [
    '@nuxtjs/tailwindcss',
    '@pinia/nuxt',
    '@vueuse/nuxt'
  ],
  
  css: [
    '~/assets/css/main.css'
  ],
  
  runtimeConfig: {
    public: {
      apiBaseUrl: process.env.API_BASE_URL || 'http://localhost:5000'
    }
  },
  
  ssr: false, // SPA mode for authentication
  
  router: {
    middleware: ['auth']
  },
  
  app: {
    head: {
      title: 'Message - フィットネス管理',
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'description', content: 'トレーニングとサプリメントの記録管理アプリ' }
      ]
    }
  }
})
```

### tailwind.config.js
```javascript
module.exports = {
  content: [
    "./components/**/*.{js,vue,ts}",
    "./layouts/**/*.vue",
    "./pages/**/*.vue",
    "./plugins/**/*.{js,ts}",
    "./nuxt.config.{js,ts}",
    "./app.vue"
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: '#eff6ff',
          500: '#3b82f6',
          600: '#2563eb',
          700: '#1d4ed8'
        }
      }
    }
  },
  plugins: [
    require('@tailwindcss/forms')
  ]
}
```

## 🧪 テスト実装

### tests/components/TrainingForm.test.ts
```typescript
import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import TrainingForm from '~/components/training/TrainingForm.vue'

describe('TrainingForm', () => {
  it('should render form fields correctly', () => {
    const wrapper = mount(TrainingForm, {
      global: {
        mocks: {
          useTraining: () => ({
            createRecord: vi.fn(),
            trainingMenus: []
          })
        }
      }
    })

    expect(wrapper.find('select').exists()).toBe(true)
    expect(wrapper.find('input[type="number"]').exists()).toBe(true)
    expect(wrapper.find('textarea').exists()).toBe(true)
  })

  it('should submit form with correct data', async () => {
    const createRecord = vi.fn()
    
    const wrapper = mount(TrainingForm, {
      global: {
        mocks: {
          useTraining: () => ({
            createRecord,
            trainingMenus: [
              { menuId: 'bench-press', jpName: 'ベンチプレス' }
            ]
          })
        }
      }
    })

    await wrapper.find('select').setValue('bench-press')
    await wrapper.find('input[type="number"]').setValue('70')
    await wrapper.find('form').trigger('submit')

    expect(createRecord).toHaveBeenCalledWith(
      expect.objectContaining({
        menuId: 'bench-press',
        weight: 70
      })
    )
  })
})
```

## 📱 レスポンシブ対応

### Breakpoint Strategy
```typescript
// composables/useBreakpoints.ts
export const useBreakpoints = () => {
  const { width } = useWindowSize()
  
  const isMobile = computed(() => width.value < 768)
  const isTablet = computed(() => width.value >= 768 && width.value < 1024)
  const isDesktop = computed(() => width.value >= 1024)
  
  return {
    isMobile,
    isTablet,
    isDesktop,
    width
  }
}
```

### Mobile-First CSS
```css
/* assets/css/main.css */
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer components {
  .container-responsive {
    @apply px-4 sm:px-6 lg:px-8;
  }
  
  .card {
    @apply bg-white rounded-lg shadow-md p-4 sm:p-6;
  }
  
  .btn-primary {
    @apply bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 
           disabled:opacity-50 transition-colors;
  }
  
  .form-input {
    @apply w-full p-3 border border-gray-300 rounded-lg 
           focus:ring-2 focus:ring-blue-500 focus:border-transparent;
  }
}

/* Custom responsive utilities */
@responsive {
  .grid-responsive {
    @apply grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4;
  }
}
```

## 🔒 セキュリティ対策

### XSS Protection
```typescript
// utils/sanitize.ts
import DOMPurify from 'dompurify'

export const sanitizeHTML = (dirty: string): string => {
  return DOMPurify.sanitize(dirty)
}

export const escapeHTML = (text: string): string => {
  const div = document.createElement('div')
  div.textContent = text
  return div.innerHTML
}
```

### CSRF Protection
```typescript
// plugins/csrf.client.ts
export default defineNuxtPlugin(() => {
  const { $fetch } = useNuxtApp()
  
  // CSRFトークンを自動で付与
  $fetch.defaults.onRequest = ({ options }) => {
    const csrfToken = useCookie('csrf-token')
    if (csrfToken.value) {
      options.headers = {
        ...options.headers,
        'X-CSRF-Token': csrfToken.value
      }
    }
  }
})
```

## 🚀 パフォーマンス最適化

### Lazy Loading
```vue
<template>
  <div>
    <!-- 重いコンポーネントの遅延読み込み -->
    <LazyProgressChart v-if="showChart" />
    
    <!-- 画像の遅延読み込み -->
    <img 
      v-lazy="imageUrl" 
      :alt="altText"
      class="w-full h-48 object-cover"
    />
  </div>
</template>

<script setup lang="ts">
// コンポーネントの動的インポート
const LazyProgressChart = defineAsyncComponent(() => 
  import('~/components/dashboard/ProgressChart.vue')
)
</script>
```

### Virtual Scrolling
```vue
<template>
  <div class="h-96 overflow-auto">
    <RecycleScroller
      v-slot="{ item }"
      :items="trainingRecords"
      :item-size="80"
      key-field="recordId"
    >
      <TrainingRecordItem :record="item" />
    </RecycleScroller>
  </div>
</template>
```

## 📋 実装チェックリスト

### Phase 1: 基盤構築
- [ ] Nuxt 3プロジェクト初期化
- [ ] TypeScript設定完了
- [ ] Tailwind CSS設定完了
- [ ] Pinia状態管理設定
- [ ] 基本ルーティング設定
- [ ] API クライアント設定

### Phase 2: 認証機能
- [ ] ログインページ作成
- [ ] 登録ページ作成
- [ ] JWT トークン管理実装
- [ ] 認証ミドルウェア実装
- [ ] ログアウト機能実装

### Phase 3: トレーニング機能
- [ ] メニュー一覧表示
- [ ] 記録入力フォーム
- [ ] 記録一覧表示
- [ ] 進捗チャート実装
- [ ] 統計表示機能

### Phase 4: サプリメント機能
- [ ] サプリメント登録フォーム
- [ ] 摂取記録機能
- [ ] スケジュール管理
- [ ] 摂取状況表示
- [ ] リマインダー機能

### Phase 5: 最適化・テスト
- [ ] パフォーマンス最適化
- [ ] 単体テスト実装
- [ ] E2Eテスト実装
- [ ] エラーハンドリング強化
- [ ] SEO対応

## 🎯 品質基準

### Performance
- [ ] First Contentful Paint < 2秒
- [ ] Largest Contentful Paint < 3秒
- [ ] Cumulative Layout Shift < 0.1

### Accessibility
- [ ] WCAG 2.1 AA準拠
- [ ] キーボードナビゲーション対応
- [ ] スクリーンリーダー対応

### Browser Support
- [ ] Chrome 90+
- [ ] Firefox 88+
- [ ] Safari 14+
- [ ] Edge 90+

---

## 🎯 次のステップ

1. **API Contract確認**: バックエンドチームと仕様合意
2. **デザインシステム**: UI/UXガイドライン策定
3. **プロトタイプ**: 主要機能のモックアップ作成
4. **段階的実装**: Phase順での着実な開発

**🚀 効率的で高品質なWebクライアントの構築を目指しましょう！**