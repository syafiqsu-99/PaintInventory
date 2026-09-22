<template>
  <div style="height: 300px">
    <Bar v-if="items.length" :data="chartData" :options="chartOptions" />
    <div v-else class="text-medium-emphasis text-center py-8">No stock on hand yet.</div>
  </div>
</template>

<script setup>
  import { computed } from 'vue'
  import { Bar } from 'vue-chartjs'
  import {
    Chart as ChartJS,
    Title, Tooltip, Legend, BarElement, LinearScale, CategoryScale
  } from 'chart.js'

  ChartJS.register(Title, Tooltip, Legend, BarElement, LinearScale, CategoryScale)

  const props = defineProps({
    items: { type: Array, default: () => [] }
  })

  const chartData = computed(() => ({
    labels: props.items.map((i) => i.vendorName),
    datasets: [
      {
        label: 'On hand',
        data: props.items.map((i) => i.onHand),
        backgroundColor: '#1565C0',
        borderRadius: 4,
        maxBarThickness: 28
      }
    ]
  }))

  const chartOptions = {
    indexAxis: 'y',
    responsive: true,
    maintainAspectRatio: false,
    scales: { x: { beginAtZero: true } },
    plugins: { legend: { display: false } }
  }
</script>
