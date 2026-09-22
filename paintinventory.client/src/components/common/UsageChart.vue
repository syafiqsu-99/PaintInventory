<template>
  <div style="height: 300px">
    <Line v-if="points.length" :data="chartData" :options="chartOptions" />
    <div v-else class="text-medium-emphasis text-center py-8">No usage recorded yet.</div>
  </div>
</template>

<script setup>
    import { computed } from 'vue'
    import { Line } from 'vue-chartjs'
    import {
      Chart as ChartJS,
      Title, Tooltip, Legend,
      LineElement, PointElement, LinearScale, CategoryScale, Filler
    } from 'chart.js'
    import { formatDate } from '@/utils/format'

    ChartJS.register(Title, Tooltip, Legend, LineElement, PointElement, LinearScale, CategoryScale, Filler)

    const props = defineProps({
      points: { type: Array, default: () => [] }
    })

    const chartData = computed(() => ({
      labels: props.points.map((p) => formatDate(p.date)),
      datasets: [
        {
          label: 'Used',
          data: props.points.map((p) => p.quantity),
          borderColor: '#1565C0',
          backgroundColor: 'rgba(21, 101, 192, 0.15)',
          fill: true,
          tension: 0.3
        }
      ]
    }))

    const chartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      scales: { y: { beginAtZero: true } },
      plugins: { legend: { display: false } }
    }
</script>
