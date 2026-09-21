import { defineStore } from 'pinia'
import { ref } from 'vue'
import http from '@/utils/http'

export const useInventoryStore = defineStore('inventory', () => {
  const levels = ref([])
  const lowStock = ref([])
  const dashboard = ref(null)
  const loading = ref(false)

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

  function history(productId, vendorId = null) {
    const q = vendorId ? `?vendorId=${vendorId}` : ''
    return http.get(`/inventory/${productId}/history${q}`)
  }

  function setReorder(payload) {
    return http.put('/inventory/reorder', payload)
  }

  return { levels, lowStock, dashboard, loading, loadLevels, loadLowStock, loadDashboard, history, setReorder }
})
