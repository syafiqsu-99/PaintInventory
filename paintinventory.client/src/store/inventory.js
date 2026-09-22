import { defineStore } from 'pinia'
import { ref } from 'vue'
import http from '@/utils/http'

export const useInventoryStore = defineStore('inventory', () => {
  const levels = ref([])
  const lowStock = ref([])
  const dashboard = ref(null)
  const summary = ref(null)
  const loading = ref(false)
  const summaryLoading = ref(false)

  async function loadLevels(vendorId = null) {
    loading.value = true
    try {
      const q = vendorId ? `?vendorId=${vendorId}` : ''
      levels.value = await http.get(`/inventory${q}`)
    } finally {
      loading.value = false
    }
  }

  async function loadLowStock(vendorId = null) {
    const q = vendorId ? `?vendorId=${vendorId}` : ''
    lowStock.value = await http.get(`/inventory/low-stock${q}`)
  }

  async function loadDashboard() {
    dashboard.value = await http.get('/inventory/dashboard')
  }

  async function loadSummary(expiryDays = 60) {
    summaryLoading.value = true
    try {
      summary.value = await http.get(`/dashboard/summary?expiryDays=${expiryDays}`)
    } finally {
      summaryLoading.value = false
    }
  }

  function history(productId, vendorId = null) {
    const q = vendorId ? `?vendorId=${vendorId}` : ''
    return http.get(`/inventory/${productId}/history${q}`)
  }

  function setReorder(payload) {
    return http.put('/inventory/reorder', payload)
  }

  return {
    levels, lowStock, dashboard, summary, loading, summaryLoading,
    loadLevels, loadLowStock, loadDashboard, loadSummary, history, setReorder
  }
})
