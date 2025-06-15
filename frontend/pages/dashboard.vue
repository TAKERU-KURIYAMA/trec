<template>
  <div class="dashboard">
    <!-- ログイン済みユーザー向けダッシュボード -->
    <div v-if="authStore.isAuthenticated">
      <header class="dashboard-header">
        <div class="header-top">
          <h1>🏋️ トレーニングダッシュボード</h1>
          <div class="header-actions">
            <span v-if="authStore.user" class="welcome-text">{{ authStore.user.displayName }}さん</span>
            <button @click="authStore.logout" class="logout-btn">ログアウト</button>
          </div>
        </div>
        <div class="header-stats">
          <div class="stat-card">
            <span class="stat-number">{{ totalWorkouts }}</span>
            <span class="stat-label">総ワークアウト数</span>
          </div>
          <div class="stat-card">
            <span class="stat-number">{{ currentStreak }}</span>
            <span class="stat-label">連続日数</span>
          </div>
          <div class="stat-card">
            <span class="stat-number">{{ weeklyProgress }}%</span>
            <span class="stat-label">週間達成率</span>
          </div>
        </div>
      </header>

      <div class="dashboard-grid">
        <!-- Progress Chart Section -->
        <section class="chart-section">
          <h2>📊 プログレス分析</h2>
          <ProgressChart :data="progressData" />
        </section>

        <!-- Recent Activities -->
        <section class="activities-section">
          <h2>🚀 最近のアクティビティ</h2>
          <RecentActivities :activities="recentActivities" />
        </section>

        <!-- Achievement System -->
        <section class="achievements-section">
          <h2>🏆 アチーブメント</h2>
          <AchievementBadges :achievements="achievements" />
        </section>

        <!-- Personal Records -->
        <section class="records-section">
          <h2>⭐ パーソナルレコード</h2>
          <PersonalRecords :records="personalRecords" />
        </section>

        <!-- Weekly Calendar -->
        <section class="calendar-section">
          <h2>📅 トレーニングカレンダー</h2>
          <TrainingCalendar :training-data="trainingCalendarData" />
        </section>

        <!-- Smart Recommendations -->
        <section class="recommendations-section">
          <h2>🧠 スマート推奨</h2>
          <SmartRecommendations :recommendations="smartRecommendations" />
        </section>
      </div>
    </div>

    <!-- 未ログインユーザー向けデモダッシュボード -->
    <div v-else>
      <header class="demo-dashboard-header">
        <div class="header-top">
          <h1>🏋️ TrecPlans ダッシュボード</h1>
          <div class="header-actions">
            <button @click="openLoginModal" class="login-btn">ログイン</button>
            <button @click="openRegisterModal" class="register-btn">新規登録</button>
          </div>
        </div>
        <div class="demo-banner">
          <div class="demo-content">
            <h2>🌟 TrecPlansにようこそ！</h2>
            <p>ログインして、あなた専用のダッシュボードでトレーニング進捗を詳しく確認しましょう</p>
            <button @click="openLoginModal" class="cta-login-btn">
              <Icon name="mdi:login" size="20" />
              <span>ログインして始める</span>
            </button>
          </div>
        </div>
        <div class="header-stats">
          <div class="stat-card demo">
            <span class="stat-number">{{ demoStats.totalWorkouts }}</span>
            <span class="stat-label">利用可能メニュー</span>
          </div>
          <div class="stat-card demo">
            <span class="stat-number">{{ demoStats.activeUsers }}</span>
            <span class="stat-label">アクティブユーザー</span>
          </div>
          <div class="stat-card demo">
            <span class="stat-number">{{ demoStats.completedGoals }}%</span>
            <span class="stat-label">目標達成率</span>
          </div>
        </div>
      </header>

      <div class="dashboard-grid">
        <!-- デモ機能紹介セクション -->
        <section class="demo-features-section">
          <h2>✨ 主な機能</h2>
          <div class="features-grid">
            <div class="feature-card">
              <div class="feature-icon">📊</div>
              <h3>詳細な進捗分析</h3>
              <p>トレーニングデータを視覚的に分析し、パフォーマンスの向上を確認できます</p>
            </div>
            <div class="feature-card">
              <div class="feature-icon">🎯</div>
              <h3>パーソナライズされた目標</h3>
              <p>あなたの目標に合わせたカスタムトレーニングプランを作成できます</p>
            </div>
            <div class="feature-card">
              <div class="feature-icon">📅</div>
              <h3>スマートなスケジュール</h3>
              <p>効率的なトレーニングスケジュールを自動で提案します</p>
            </div>
            <div class="feature-card">
              <div class="feature-icon">🏆</div>
              <h3>アチーブメント</h3>
              <p>目標達成でバッジを獲得し、モチベーションを維持できます</p>
            </div>
          </div>
        </section>

        <!-- デモチャート -->
        <section class="demo-chart-section">
          <h2>📈 プログレス例</h2>
          <div class="demo-chart">
            <div class="chart-placeholder">
              <div class="chart-bars">
                <div class="bar" style="height: 60%"></div>
                <div class="bar" style="height: 75%"></div>
                <div class="bar" style="height: 45%"></div>
                <div class="bar" style="height: 90%"></div>
                <div class="bar" style="height: 80%"></div>
                <div class="bar" style="height: 95%"></div>
                <div class="bar" style="height: 85%"></div>
              </div>
              <p class="chart-description">過去7日間のトレーニング強度の例</p>
            </div>
          </div>
        </section>

        <!-- クイックスタート -->
        <section class="quick-start-section">
          <h2>🚀 今すぐ始める</h2>
          <div class="quick-start-grid">
            <NuxtLink to="/" class="quick-start-card">
              <div class="start-icon">💪</div>
              <h3>トレーニングメニューを見る</h3>
              <p>豊富なメニューから選択してトレーニングを開始</p>
            </NuxtLink>
            <button @click="openRegisterModal" class="quick-start-card">
              <div class="start-icon">✨</div>
              <h3>無料で始める</h3>
              <p>今すぐアカウントを作成して全機能を利用</p>
            </button>
          </div>
        </section>

        <!-- 推奨メニュー -->
        <section class="demo-recommendations-section">
          <h2>🔥 人気のトレーニング</h2>
          <div class="recommendations-grid">
            <div class="recommendation-card">
              <div class="rec-icon">💪</div>
              <h4>初心者向け全身トレーニング</h4>
              <p>基本的な動作で全身をバランスよく鍛える</p>
            </div>
            <div class="recommendation-card">
              <div class="rec-icon">🏋️</div>
              <h4>上半身集中ワークアウト</h4>
              <p>胸、背中、腕を効率的に強化</p>
            </div>
            <div class="recommendation-card">
              <div class="rec-icon">🦵</div>
              <h4>下半身パワーアップ</h4>
              <p>脚とお尻の筋力向上にフォーカス</p>
            </div>
          </div>
        </section>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import ProgressChart from '~/components/dashboard/ProgressChart.vue'
import RecentActivities from '~/components/dashboard/RecentActivities.vue'
import AchievementBadges from '~/components/dashboard/AchievementBadges.vue'
import PersonalRecords from '~/components/dashboard/PersonalRecords.vue'
import TrainingCalendar from '~/components/dashboard/TrainingCalendar.vue'

// 認証ミドルウェアを削除（ログイン前でもアクセス可能にする）

import SmartRecommendations from '~/components/dashboard/SmartRecommendations.vue'

const authStore = useAuthStore()
const { openLogin, openRegister } = globalAuthModal

// 認証済みユーザー用データ
const totalWorkouts = ref(0)
const currentStreak = ref(0)
const weeklyProgress = ref(0)
const progressData = ref([])
const recentActivities = ref([])
const achievements = ref([])
const personalRecords = ref([])
const trainingCalendarData = ref([])
const smartRecommendations = ref([])

// デモ用統計データ
const demoStats = ref({
  totalWorkouts: 150,
  activeUsers: '2.5k',
  completedGoals: 85
})

// モーダル制御
function openLoginModal() {
  openLogin()
}

function openRegisterModal() {
  openRegister()
}

// Load dashboard data
onMounted(async () => {
  if (authStore.isAuthenticated) {
    await loadDashboardData()
  }
})

async function loadDashboardData() {
  // Load all dashboard data concurrently
  await Promise.all([
    loadStatistics(),
    loadProgressData(),
    loadRecentActivities(),
    loadAchievements(),
    loadPersonalRecords(),
    loadCalendarData(),
    loadSmartRecommendations()
  ])
}

async function loadStatistics() {
  // Mock data - replace with API calls
  totalWorkouts.value = 87
  currentStreak.value = 5
  weeklyProgress.value = 71
}

async function loadProgressData() {
  // Mock progress data
  progressData.value = [
    { date: '2024-06-01', weight: 60, reps: 10 },
    { date: '2024-06-02', weight: 62, reps: 10 },
    { date: '2024-06-03', weight: 62, reps: 12 },
    { date: '2024-06-04', weight: 65, reps: 10 },
    { date: '2024-06-05', weight: 65, reps: 12 },
  ]
}

async function loadRecentActivities() {
  recentActivities.value = [
    { id: 1, exercise: 'ベンチプレス', weight: 70, reps: 12, sets: 3, date: '2024-06-12' },
    { id: 2, exercise: 'スクワット', weight: 80, reps: 10, sets: 4, date: '2024-06-11' },
    { id: 3, exercise: 'デッドリフト', weight: 90, reps: 8, sets: 3, date: '2024-06-10' },
  ]
}

async function loadAchievements() {
  achievements.value = [
    { id: 1, name: '初回達成', description: '初めてのワークアウト完了', earned: true, icon: '🎯' },
    { id: 2, name: '週間戦士', description: '7日連続でワークアウト', earned: true, icon: '🔥' },
    { id: 3, name: 'パワーリフター', description: '100kg以上のリフト達成', earned: false, icon: '💪' },
    { id: 4, name: '忍耐の達人', description: '30日連続でワークアウト', earned: false, icon: '🥋' },
  ]
}

async function loadPersonalRecords() {
  personalRecords.value = [
    { exercise: 'ベンチプレス', weight: 75, date: '2024-06-10' },
    { exercise: 'スクワット', weight: 85, date: '2024-06-09' },
    { exercise: 'デッドリフト', weight: 95, date: '2024-06-08' },
  ]
}

async function loadCalendarData() {
  // Mock calendar data
  trainingCalendarData.value = Array.from({ length: 30 }, (_, i) => ({
    date: new Date(2024, 5, i + 1).toISOString().split('T')[0],
    workouts: Math.random() > 0.7 ? Math.floor(Math.random() * 3) + 1 : 0
  }))
}

async function loadSmartRecommendations() {
  smartRecommendations.value = [
    {
      type: 'rest',
      title: '休息日の推奨',
      description: '4日連続でトレーニングしています。明日は休息日にしましょう。',
      priority: 'high'
    },
    {
      type: 'progressive_overload',
      title: 'プログレッシブオーバーロード',
      description: 'ベンチプレスの重量を2.5kg増やすことを推奨します。',
      priority: 'medium'
    },
    {
      type: 'new_exercise',
      title: '新しいエクササイズ',
      description: '肩のトレーニングを追加することで、バランスの良い体作りができます。',
      priority: 'low'
    }
  ]
}

// Page metadata
useHead({
  title: 'ダッシュボード',
  meta: [
    { name: 'description', content: 'TrecPlansダッシュボードであなたのトレーニング進捗を一目で確認。詳細な分析とパーソナライズされた推奨で効率的なフィットネス管理を実現します。' },
    { name: 'keywords', content: 'ダッシュボード,トレーニング進捗,統計,分析,TrecPlans' },
    { property: 'og:title', content: 'ダッシュボード - TrecPlans' },
    { property: 'og:description', content: 'あなたのトレーニング進捗を一目で確認できるダッシュボード' },
    { name: 'twitter:title', content: 'ダッシュボード - TrecPlans' },
    { name: 'twitter:description', content: 'あなたのトレーニング進捗を一目で確認できるダッシュボード' }
  ]
})
</script>

<style scoped>
.dashboard {
  padding: 20px;
  max-width: 1400px;
  margin: 0 auto;
}

.dashboard-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 30px;
  border-radius: 12px;
  margin-bottom: 30px;
  box-shadow: 0 4px 15px rgba(0,0,0,0.1);
}

.header-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 15px;
}

.welcome-text {
  font-size: 16px;
  opacity: 0.9;
}

.logout-btn {
  background: rgba(255, 255, 255, 0.2);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.3);
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 14px;
}

.logout-btn:hover {
  background: rgba(255, 255, 255, 0.3);
  border-color: rgba(255, 255, 255, 0.5);
}

.dashboard-header h1 {
  font-size: 2.5rem;
  color: white;
  margin: 0;
}

.header-stats {
  display: flex;
  justify-content: center;
  gap: 20px;
  flex-wrap: wrap;
}

.stat-card {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 20px;
  border-radius: 12px;
  text-align: center;
  min-width: 150px;
  box-shadow: 0 4px 15px rgba(0,0,0,0.1);
}

.stat-number {
  display: block;
  font-size: 2.5rem;
  font-weight: bold;
  margin-bottom: 5px;
}

.stat-label {
  font-size: 0.9rem;
  opacity: 0.9;
}

.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 25px;
  margin-top: 30px;
}

.dashboard-grid section {
  background: white;
  padding: 25px;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
  border: 1px solid #e9ecef;
}

.dashboard-grid h2 {
  margin-top: 0;
  margin-bottom: 20px;
  color: #2c3e50;
  font-size: 1.4rem;
  border-bottom: 2px solid #3498db;
  padding-bottom: 10px;
}

.chart-section {
  grid-column: span 2;
}

.calendar-section {
  grid-column: span 2;
}

@media (max-width: 768px) {
  .dashboard {
    padding: 10px;
  }
  
  .dashboard-grid {
    grid-template-columns: 1fr;
  }
  
  .chart-section,
  .calendar-section {
    grid-column: span 1;
  }
  
  .header-stats {
    flex-direction: column;
    align-items: center;
  }
}
</style>