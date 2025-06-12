<template>
  <div class="progress-chart">
    <div class="chart-controls">
      <select v-model="selectedMetric" class="metric-selector">
        <option value="weight">重量</option>
        <option value="reps">レップ数</option>
        <option value="volume">ボリューム</option>
      </select>
      <select v-model="selectedPeriod" class="period-selector">
        <option value="7">過去7日</option>
        <option value="30">過去30日</option>
        <option value="90">過去90日</option>
      </select>
    </div>

    <div class="chart-container">
      <canvas ref="chartCanvas" width="800" height="400"></canvas>
    </div>

    <div class="chart-summary">
      <div class="summary-item">
        <span class="summary-label">最高記録:</span>
        <span class="summary-value">{{ maxValue }}{{ getUnit() }}</span>
      </div>
      <div class="summary-item">
        <span class="summary-label">平均:</span>
        <span class="summary-value">{{ averageValue }}{{ getUnit() }}</span>
      </div>
      <div class="summary-item">
        <span class="summary-label">改善:</span>
        <span class="summary-value" :class="{ positive: improvement > 0, negative: improvement < 0 }">
          {{ improvement > 0 ? '+' : '' }}{{ improvement }}%
        </span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, computed, nextTick } from 'vue'

const props = defineProps({
  data: {
    type: Array,
    default: () => []
  }
})

const chartCanvas = ref(null)
const selectedMetric = ref('weight')
const selectedPeriod = ref('30')

// Computed values
const filteredData = computed(() => {
  const days = parseInt(selectedPeriod.value)
  const cutoffDate = new Date()
  cutoffDate.setDate(cutoffDate.getDate() - days)
  
  return props.data.filter(item => new Date(item.date) >= cutoffDate)
})

const maxValue = computed(() => {
  if (!filteredData.value.length) return 0
  return Math.max(...filteredData.value.map(item => item[selectedMetric.value]))
})

const averageValue = computed(() => {
  if (!filteredData.value.length) return 0
  const sum = filteredData.value.reduce((acc, item) => acc + item[selectedMetric.value], 0)
  return Math.round(sum / filteredData.value.length * 10) / 10
})

const improvement = computed(() => {
  if (filteredData.value.length < 2) return 0
  const first = filteredData.value[0][selectedMetric.value]
  const last = filteredData.value[filteredData.value.length - 1][selectedMetric.value]
  return Math.round(((last - first) / first) * 100 * 10) / 10
})

function getUnit() {
  switch (selectedMetric.value) {
    case 'weight': return 'kg'
    case 'reps': return '回'
    case 'volume': return 'kg'
    default: return ''
  }
}

function drawChart() {
  if (!chartCanvas.value || !filteredData.value.length) return
  
  const canvas = chartCanvas.value
  const ctx = canvas.getContext('2d')
  const width = canvas.width
  const height = canvas.height
  
  // Clear canvas
  ctx.clearRect(0, 0, width, height)
  
  // Set up chart area
  const padding = 60
  const chartWidth = width - padding * 2
  const chartHeight = height - padding * 2
  
  // Get data values
  const values = filteredData.value.map(item => item[selectedMetric.value])
  const dates = filteredData.value.map(item => item.date)
  
  const minValue = Math.min(...values)
  const maxValue = Math.max(...values)
  const valueRange = maxValue - minValue || 1
  
  // Draw grid lines
  ctx.strokeStyle = '#e0e0e0'
  ctx.lineWidth = 1
  
  // Vertical grid lines
  for (let i = 0; i <= 5; i++) {
    const x = padding + (chartWidth / 5) * i
    ctx.beginPath()
    ctx.moveTo(x, padding)
    ctx.lineTo(x, padding + chartHeight)
    ctx.stroke()
  }
  
  // Horizontal grid lines
  for (let i = 0; i <= 4; i++) {
    const y = padding + (chartHeight / 4) * i
    ctx.beginPath()
    ctx.moveTo(padding, y)
    ctx.lineTo(padding + chartWidth, y)
    ctx.stroke()
  }
  
  // Draw data line
  if (values.length > 1) {
    ctx.strokeStyle = '#3498db'
    ctx.lineWidth = 3
    ctx.beginPath()
    
    values.forEach((value, index) => {
      const x = padding + (chartWidth / (values.length - 1)) * index
      const y = padding + chartHeight - ((value - minValue) / valueRange) * chartHeight
      
      if (index === 0) {
        ctx.moveTo(x, y)
      } else {
        ctx.lineTo(x, y)
      }
    })
    
    ctx.stroke()
    
    // Draw data points
    ctx.fillStyle = '#3498db'
    values.forEach((value, index) => {
      const x = padding + (chartWidth / (values.length - 1)) * index
      const y = padding + chartHeight - ((value - minValue) / valueRange) * chartHeight
      
      ctx.beginPath()
      ctx.arc(x, y, 5, 0, Math.PI * 2)
      ctx.fill()
    })
  }
  
  // Draw axes labels
  ctx.fillStyle = '#333'
  ctx.font = '12px Arial'
  ctx.textAlign = 'center'
  
  // X-axis labels (dates)
  dates.forEach((date, index) => {
    if (index % Math.ceil(dates.length / 5) === 0) {
      const x = padding + (chartWidth / (dates.length - 1)) * index
      const shortDate = new Date(date).toLocaleDateString('ja-JP', { month: 'short', day: 'numeric' })
      ctx.fillText(shortDate, x, height - 20)
    }
  })
  
  // Y-axis labels (values)
  ctx.textAlign = 'right'
  for (let i = 0; i <= 4; i++) {
    const value = minValue + (valueRange / 4) * (4 - i)
    const y = padding + (chartHeight / 4) * i + 5
    ctx.fillText(Math.round(value * 10) / 10 + getUnit(), padding - 10, y)
  }
}

// Watch for data changes
watch([filteredData, selectedMetric], () => {
  nextTick(() => {
    drawChart()
  })
})

onMounted(() => {
  nextTick(() => {
    drawChart()
  })
})
</script>

<style scoped>
.progress-chart {
  width: 100%;
}

.chart-controls {
  display: flex;
  gap: 15px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.metric-selector,
.period-selector {
  padding: 8px 12px;
  border: 2px solid #ddd;
  border-radius: 6px;
  background: white;
  font-size: 14px;
  cursor: pointer;
  transition: border-color 0.2s;
}

.metric-selector:focus,
.period-selector:focus {
  outline: none;
  border-color: #3498db;
}

.chart-container {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 20px;
  margin-bottom: 20px;
  overflow-x: auto;
}

.chart-summary {
  display: flex;
  justify-content: space-around;
  flex-wrap: wrap;
  gap: 20px;
}

.summary-item {
  text-align: center;
}

.summary-label {
  display: block;
  font-size: 0.9rem;
  color: #666;
  margin-bottom: 5px;
}

.summary-value {
  display: block;
  font-size: 1.5rem;
  font-weight: bold;
  color: #2c3e50;
}

.summary-value.positive {
  color: #27ae60;
}

.summary-value.negative {
  color: #e74c3c;
}

@media (max-width: 768px) {
  .chart-controls {
    justify-content: center;
  }
  
  .chart-container canvas {
    max-width: 100%;
    height: auto;
  }
}
</style>