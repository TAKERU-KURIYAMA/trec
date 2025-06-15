<template>
  <div class="rm-calculator">
    <!-- Header -->
    <div class="page-header">
      <div class="header-content">
        <h1>1RM計算機</h1>
        <p>最大重量を推定・トレーニング重量を計算できます</p>
      </div>
      <div class="header-actions">
        <button @click="clearAll" class="btn btn-outline">
          <Icon name="mdi:refresh" />
          リセット
        </button>
      </div>
    </div>

    <!-- Calculator Modes -->
    <div class="calculator-modes">
      <div class="mode-tabs">
        <button 
          @click="activeMode = 'estimate'"
          :class="['mode-tab', { active: activeMode === 'estimate' }]"
        >
          <Icon name="mdi:calculator" />
          1RM推定
        </button>
        <button 
          @click="activeMode = 'percentage'"
          :class="['mode-tab', { active: activeMode === 'percentage' }]"
        >
          <Icon name="mdi:percent" />
          重量計算
        </button>
      </div>
    </div>

    <!-- 1RM Estimation Mode -->
    <div v-if="activeMode === 'estimate'" class="calculator-section">
      <div class="calculation-card">
        <div class="card-header">
          <h2>🏋️‍♂️ 1RM推定計算</h2>
          <p>現在挙げられる重量と回数から最大重量を推定します</p>
        </div>

        <div class="input-section">
          <div class="input-group">
            <label for="weight-input">重量 (kg)</label>
            <input 
              id="weight-input"
              v-model.number="estimationInputs.weight"
              type="number"
              min="1"
              step="0.5"
              class="input-field"
              placeholder="例: 80"
              @input="calculateOneRM"
            />
          </div>
          
          <div class="input-group">
            <label for="reps-input">回数</label>
            <input 
              id="reps-input"
              v-model.number="estimationInputs.reps"
              type="number"
              min="1"
              max="20"
              class="input-field"
              placeholder="例: 8"
              @input="calculateOneRM"
            />
          </div>
        </div>

        <!-- Formula Selection -->
        <div class="formula-selection">
          <h3>計算式</h3>
          <div class="formula-grid">
            <label 
              v-for="formula in formulaOptions" 
              :key="formula.name"
              class="formula-option"
              :class="{ active: selectedFormula === formula.name }"
            >
              <input 
                type="radio" 
                :value="formula.name" 
                v-model="selectedFormula"
                @change="calculateOneRM"
              />
              <div class="formula-info">
                <span class="formula-name">{{ formula.name }}</span>
                <span class="formula-description">{{ formula.description }}</span>
              </div>
            </label>
          </div>
        </div>

        <!-- Results -->
        <div v-if="oneRMResults.length > 0" class="results-section">
          <h3>推定結果</h3>
          <div class="results-grid">
            <div 
              v-for="result in oneRMResults" 
              :key="result.formula"
              class="result-card"
              :class="{ primary: result.formula === selectedFormula }"
            >
              <div class="result-formula">{{ result.formula }}</div>
              <div class="result-value">{{ Math.round(result.value * 10) / 10 }} kg</div>
            </div>
          </div>
          
          <div class="average-result">
            <div class="average-label">平均値</div>
            <div class="average-value">{{ averageOneRM }} kg</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Percentage Calculation Mode -->
    <div v-if="activeMode === 'percentage'" class="calculator-section">
      <div class="calculation-card">
        <div class="card-header">
          <h2>💪 トレーニング重量計算</h2>
          <p>1RMの割合に基づいてトレーニング重量を計算します</p>
        </div>

        <div class="input-section">
          <div class="input-group">
            <label for="max-weight">1RM (kg)</label>
            <input 
              id="max-weight"
              v-model.number="percentageInputs.maxWeight"
              type="number"
              min="1"
              step="0.5"
              class="input-field"
              placeholder="例: 100"
              @input="calculatePercentages"
            />
          </div>
        </div>

        <!-- Percentage Table -->
        <div v-if="percentageInputs.maxWeight > 0" class="percentage-table">
          <h3>トレーニング重量表</h3>
          <div class="table-container">
            <table class="weight-table">
              <thead>
                <tr>
                  <th>%</th>
                  <th>重量 (kg)</th>
                  <th>目安回数</th>
                  <th>用途</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="row in percentageTable" :key="row.percentage" class="table-row">
                  <td class="percentage-cell">{{ row.percentage }}%</td>
                  <td class="weight-cell">{{ row.weight }} kg</td>
                  <td class="reps-cell">{{ row.expectedReps }}</td>
                  <td class="purpose-cell">{{ row.purpose }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Quick Calculator -->
        <div class="quick-calculator">
          <h3>クイック計算</h3>
          <div class="quick-inputs">
            <div class="quick-input-group">
              <label>重量</label>
              <input 
                v-model.number="customPercentage"
                type="number"
                min="50"
                max="100"
                step="5"
                class="input-field small"
              />
              <span>%</span>
            </div>
            <div class="quick-result">
              <span class="quick-label">= </span>
              <span class="quick-value">{{ calculateCustomWeight() }} kg</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Info Section -->
    <div class="info-section">
      <div class="info-card">
        <h3>💡 使い方のコツ</h3>
        <div class="tips-grid">
          <div class="tip-item">
            <Icon name="mdi:information-outline" />
            <div class="tip-content">
              <h4>正確な測定のために</h4>
              <p>しっかりとしたフォームで、限界まで挙げた回数を入力してください</p>
            </div>
          </div>
          <div class="tip-item">
            <Icon name="mdi:chart-line" />
            <div class="tip-content">
              <h4>複数の計算式</h4>
              <p>Brzycki式が最も一般的ですが、複数の結果を参考にしてください</p>
            </div>
          </div>
          <div class="tip-item">
            <Icon name="mdi:shield-check" />
            <div class="tip-content">
              <h4>安全第一</h4>
              <p>計算結果は推定値です。実際の1RMテストは十分な準備とスポッターと行ってください</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive } from 'vue'

// ===================================
// Types & Interfaces
// ===================================

interface FormulaOption {
  name: string
  description: string
  calculate: (weight: number, reps: number) => number
}

interface OneRMResult {
  formula: string
  value: number
}

interface PercentageRow {
  percentage: number
  weight: number
  expectedReps: string
  purpose: string
}

// ===================================
// State
// ===================================

const activeMode = ref<'estimate' | 'percentage'>('estimate')

const estimationInputs = reactive({
  weight: null as number | null,
  reps: null as number | null
})

const percentageInputs = reactive({
  maxWeight: null as number | null
})

const selectedFormula = ref('Brzycki')
const customPercentage = ref(80)
const oneRMResults = ref<OneRMResult[]>([])

// ===================================
// Formula Definitions
// ===================================

const formulaOptions: FormulaOption[] = [
  {
    name: 'Brzycki',
    description: '最も一般的な計算式',
    calculate: (weight: number, reps: number) => weight * (36 / (37 - reps))
  },
  {
    name: 'Epley',
    description: 'NSCAで使用される計算式',
    calculate: (weight: number, reps: number) => weight * (1 + 0.0333 * reps)
  },
  {
    name: 'Lander',
    description: '中程度の回数に適している',
    calculate: (weight: number, reps: number) => (100 * weight) / (101.3 - 2.67123 * reps)
  },
  {
    name: 'Lombardi',
    description: 'シンプルな計算式',
    calculate: (weight: number, reps: number) => weight * Math.pow(reps, 0.1)
  },
  {
    name: "O'Conner",
    description: '高回数に適している',
    calculate: (weight: number, reps: number) => weight * (1 + 0.025 * reps)
  }
]

// ===================================
// Computed Properties
// ===================================

const averageOneRM = computed(() => {
  if (oneRMResults.value.length === 0) return 0
  const sum = oneRMResults.value.reduce((acc, result) => acc + result.value, 0)
  return Math.round((sum / oneRMResults.value.length) * 10) / 10
})

const percentageTable = computed((): PercentageRow[] => {
  if (!percentageInputs.maxWeight) return []
  
  const rows = [
    { percentage: 100, expectedReps: '1', purpose: '1RM' },
    { percentage: 95, expectedReps: '2-3', purpose: '最大筋力' },
    { percentage: 90, expectedReps: '3-4', purpose: '最大筋力' },
    { percentage: 85, expectedReps: '5-6', purpose: '筋力・筋肥大' },
    { percentage: 80, expectedReps: '6-8', purpose: '筋肥大' },
    { percentage: 75, expectedReps: '8-10', purpose: '筋肥大' },
    { percentage: 70, expectedReps: '10-12', purpose: '筋肥大・筋持久力' },
    { percentage: 65, expectedReps: '12-15', purpose: '筋持久力' },
    { percentage: 60, expectedReps: '15-20', purpose: '筋持久力' },
    { percentage: 50, expectedReps: '20+', purpose: 'ウォームアップ' }
  ]
  
  return rows.map(row => ({
    ...row,
    weight: Math.round(percentageInputs.maxWeight! * (row.percentage / 100) * 2) / 2
  }))
})

// ===================================
// Methods
// ===================================

function calculateOneRM() {
  if (!estimationInputs.weight || !estimationInputs.reps) {
    oneRMResults.value = []
    return
  }
  
  if (estimationInputs.reps < 1 || estimationInputs.reps > 20) {
    oneRMResults.value = []
    return
  }
  
  oneRMResults.value = formulaOptions.map(formula => ({
    formula: formula.name,
    value: formula.calculate(estimationInputs.weight!, estimationInputs.reps!)
  }))
}

function calculatePercentages() {
  // This is automatically handled by the computed property
}

function calculateCustomWeight(): number {
  if (!percentageInputs.maxWeight || !customPercentage.value) return 0
  const result = percentageInputs.maxWeight * (customPercentage.value / 100)
  return Math.round(result * 2) / 2 // Round to nearest 0.5
}

function clearAll() {
  estimationInputs.weight = null
  estimationInputs.reps = null
  percentageInputs.maxWeight = null
  customPercentage.value = 80
  oneRMResults.value = []
  selectedFormula.value = 'Brzycki'
}

// ===================================
// Page Meta
// ===================================

useHead({
  title: '1RM計算機 - Message',
  meta: [
    { name: 'description', content: '最大重量推定とトレーニング重量計算ツール' }
  ]
})
</script>

<style scoped>
.rm-calculator {
  max-width: 1000px;
  margin: 0 auto;
  padding: 20px;
}

/* Header */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 30px;
  flex-wrap: wrap;
  gap: 20px;
}

.header-content h1 {
  margin: 0 0 5px 0;
  color: var(--text-primary);
  font-size: 2rem;
  font-weight: 700;
}

.header-content p {
  margin: 0;
  color: var(--text-secondary);
}

.header-actions {
  display: flex;
  gap: 10px;
}

/* Mode Tabs */
.calculator-modes {
  margin-bottom: 30px;
}

.mode-tabs {
  display: flex;
  background: var(--bg-secondary);
  border-radius: 12px;
  padding: 4px;
  gap: 4px;
}

.mode-tab {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 12px 20px;
  background: transparent;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  font-weight: 500;
  color: var(--text-secondary);
}

.mode-tab.active {
  background: var(--primary);
  color: white;
  box-shadow: 0 2px 8px rgba(0, 122, 255, 0.3);
}

.mode-tab:hover:not(.active) {
  background: var(--bg-tertiary);
  color: var(--text-primary);
}

/* Calculator Section */
.calculator-section {
  margin-bottom: 30px;
}

.calculation-card {
  background: var(--card-bg);
  border-radius: 16px;
  padding: 30px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  border: 2px solid var(--border);
}

.card-header {
  text-align: center;
  margin-bottom: 30px;
}

.card-header h2 {
  margin: 0 0 10px 0;
  color: var(--text-primary);
  font-size: 1.5rem;
  font-weight: 700;
}

.card-header p {
  margin: 0;
  color: var(--text-secondary);
  font-size: 1rem;
}

/* Input Section */
.input-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.input-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.input-group label {
  font-weight: 600;
  color: var(--text-primary);
  font-size: 1rem;
}

.input-field {
  padding: 12px 16px;
  border: 2px solid var(--border);
  border-radius: 8px;
  font-size: 1.1rem;
  background: var(--bg-primary);
  color: var(--text-primary);
  transition: all 0.2s;
}

.input-field:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: 0 0 0 3px rgba(0, 122, 255, 0.1);
}

.input-field.small {
  padding: 8px 12px;
  font-size: 1rem;
  width: 80px;
}

/* Formula Selection */
.formula-selection {
  margin-bottom: 30px;
}

.formula-selection h3 {
  margin: 0 0 15px 0;
  color: var(--text-primary);
  font-size: 1.2rem;
  font-weight: 600;
}

.formula-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 12px;
}

.formula-option {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 15px;
  background: var(--bg-secondary);
  border: 2px solid var(--border);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
}

.formula-option:hover {
  border-color: var(--primary);
}

.formula-option.active {
  border-color: var(--primary);
  background: linear-gradient(135deg, rgba(0, 122, 255, 0.1), rgba(0, 122, 255, 0.05));
}

.formula-option input[type="radio"] {
  margin: 0;
}

.formula-info {
  display: flex;
  flex-direction: column;
}

.formula-name {
  font-weight: 600;
  color: var(--text-primary);
}

.formula-description {
  font-size: 0.9rem;
  color: var(--text-secondary);
}

/* Results Section */
.results-section {
  border-top: 1px solid var(--border);
  padding-top: 30px;
}

.results-section h3 {
  margin: 0 0 20px 0;
  color: var(--text-primary);
  font-size: 1.2rem;
  font-weight: 600;
  text-align: center;
}

.results-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 15px;
  margin-bottom: 25px;
}

.result-card {
  background: var(--bg-secondary);
  padding: 20px;
  border-radius: 12px;
  text-align: center;
  border: 2px solid var(--border);
  transition: all 0.2s;
}

.result-card.primary {
  border-color: var(--primary);
  background: linear-gradient(135deg, rgba(0, 122, 255, 0.1), rgba(0, 122, 255, 0.05));
}

.result-formula {
  font-size: 0.9rem;
  color: var(--text-secondary);
  margin-bottom: 8px;
}

.result-value {
  font-size: 1.4rem;
  font-weight: 700;
  color: var(--primary);
}

.average-result {
  text-align: center;
  padding: 20px;
  background: linear-gradient(135deg, rgba(40, 167, 69, 0.1), rgba(40, 167, 69, 0.05));
  border: 2px solid #28A745;
  border-radius: 12px;
}

.average-label {
  font-size: 1rem;
  color: var(--text-secondary);
  margin-bottom: 8px;
}

.average-value {
  font-size: 1.8rem;
  font-weight: 700;
  color: #28A745;
}

/* Percentage Table */
.percentage-table {
  margin-bottom: 30px;
}

.percentage-table h3 {
  margin: 0 0 15px 0;
  color: var(--text-primary);
  font-size: 1.2rem;
  font-weight: 600;
}

.table-container {
  overflow-x: auto;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.weight-table {
  width: 100%;
  border-collapse: collapse;
  background: var(--card-bg);
}

.weight-table th {
  background: var(--primary);
  color: white;
  padding: 15px;
  text-align: left;
  font-weight: 600;
}

.weight-table th:first-child {
  border-radius: 12px 0 0 0;
}

.weight-table th:last-child {
  border-radius: 0 12px 0 0;
}

.table-row {
  border-bottom: 1px solid var(--border);
  transition: background-color 0.2s;
}

.table-row:hover {
  background: var(--bg-secondary);
}

.table-row:last-child {
  border-bottom: none;
}

.weight-table td {
  padding: 12px 15px;
  color: var(--text-primary);
}

.percentage-cell {
  font-weight: 600;
  color: var(--primary);
}

.weight-cell {
  font-weight: 700;
  font-size: 1.1rem;
}

.reps-cell {
  color: var(--text-secondary);
}

.purpose-cell {
  color: var(--text-secondary);
  font-style: italic;
}

/* Quick Calculator */
.quick-calculator {
  border-top: 1px solid var(--border);
  padding-top: 20px;
}

.quick-calculator h3 {
  margin: 0 0 15px 0;
  color: var(--text-primary);
  font-size: 1.1rem;
  font-weight: 600;
}

.quick-inputs {
  display: flex;
  align-items: center;
  gap: 15px;
  flex-wrap: wrap;
}

.quick-input-group {
  display: flex;
  align-items: center;
  gap: 8px;
}

.quick-input-group label {
  font-weight: 600;
  color: var(--text-primary);
}

.quick-input-group span {
  color: var(--text-secondary);
  font-weight: 600;
}

.quick-result {
  display: flex;
  align-items: center;
  gap: 5px;
  padding: 8px 15px;
  background: var(--bg-secondary);
  border-radius: 8px;
  border: 2px solid var(--border);
}

.quick-label {
  color: var(--text-secondary);
  font-weight: 600;
}

.quick-value {
  font-size: 1.1rem;
  font-weight: 700;
  color: var(--primary);
}

/* Info Section */
.info-section {
  margin-top: 40px;
}

.info-card {
  background: var(--card-bg);
  border-radius: 16px;
  padding: 30px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
  border: 1px solid var(--border);
}

.info-card h3 {
  margin: 0 0 20px 0;
  color: var(--text-primary);
  font-size: 1.3rem;
  font-weight: 600;
  text-align: center;
}

.tips-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 20px;
}

.tip-item {
  display: flex;
  gap: 15px;
  padding: 20px;
  background: var(--bg-secondary);
  border-radius: 12px;
  border: 1px solid var(--border);
}

.tip-content h4 {
  margin: 0 0 8px 0;
  color: var(--text-primary);
  font-size: 1rem;
  font-weight: 600;
}

.tip-content p {
  margin: 0;
  color: var(--text-secondary);
  font-size: 0.9rem;
  line-height: 1.5;
}

/* Buttons */
.btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 0.9rem;
  text-decoration: none;
}

.btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.btn-outline {
  background: transparent;
  border: 2px solid var(--border);
  color: var(--text-primary);
}

.btn-outline:hover {
  background: var(--bg-secondary);
  border-color: var(--primary);
  color: var(--primary);
}

/* CSS Variables */
:root {
  --card-bg: #ffffff;
  --bg-secondary: #f8f9fa;
  --bg-tertiary: #e9ecef;
  --text-primary: #212529;
  --text-secondary: #6c757d;
  --border: #dee2e6;
  --primary: #007bff;
}

html.dark {
  --card-bg: #1e1e1e;
  --bg-secondary: #2d2d2d;
  --bg-tertiary: #3d3d3d;
  --text-primary: #ffffff;
  --text-secondary: #b0b0b0;
  --border: #444444;
  --primary: #0d6efd;
}

/* Responsive */
@media (max-width: 768px) {
  .rm-calculator {
    padding: 15px;
  }
  
  .page-header {
    flex-direction: column;
    align-items: stretch;
  }
  
  .calculation-card {
    padding: 20px;
  }
  
  .input-section {
    grid-template-columns: 1fr;
  }
  
  .formula-grid {
    grid-template-columns: 1fr;
  }
  
  .results-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .tips-grid {
    grid-template-columns: 1fr;
  }
  
  .quick-inputs {
    flex-direction: column;
    align-items: stretch;
  }
  
  .quick-result {
    justify-content: center;
  }
}
</style>