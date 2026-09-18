import { defineStore } from 'pinia'
import { ref } from 'vue'
import http from '@/utils/http'

export const useInventoryStore = defineStore('inventory', () => {
  const items = ref([])
  const lowStock = ref([])
  const dashboard = ref(null)
  const loading = ref(false)

  async function loadInventory() {
    loading.value = true
    try {
      items.value = await http.get('/inventory')
    } finally {
      loading.value = false
    }
  }

  async function loadLowStock() {
    lowStock.value = await http.get('/inventory/low-stock')
  }

  async function loadDashboard() {
    dashboard.value = await http.get('/inventory/dashboard')
  }

  function history(id) {
    return http.get(`/inventory/${id}/history`)
  }

  function lookup(barcode) {
    return http.get(`/paint/${encodeURIComponent(barcode)}`)
  }

  function recordScan(payload) {
    return http.post('/scan', payload)
  }

  function createItem(payload) {
    return http.post('/paint', payload)
  }

  function updateItem(id, payload) {
    return http.put(`/paint/${id}`, payload)
  }

  return {
    items, lowStock, dashboard, loading,
    loadInventory, loadLowStock, loadDashboard, history,
    lookup, recordScan, createItem, updateItem
  }
})
