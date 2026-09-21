import { defineStore } from 'pinia'
import { ref } from 'vue'
import http from '@/utils/http'

export const useVendorStore = defineStore('vendor', () => {
  const vendors = ref([])
  const loading = ref(false)

  async function load(includeInactive = false) {
    loading.value = true
    try {
      vendors.value = await http.get(`/vendors?includeInactive=${includeInactive}`)
    } finally {
      loading.value = false
    }
  }

  function create(payload) {
    return http.post('/vendors', payload)
  }

  function update(id, payload) {
    return http.put(`/vendors/${id}`, payload)
  }

  function deactivate(id) {
    return http.del(`/vendors/${id}`)
  }

  return { vendors, loading, load, create, update, deactivate }
})
