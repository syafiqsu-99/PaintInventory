import { defineStore } from 'pinia'
import { ref } from 'vue'
import http from '@/utils/http'

export const useProductStore = defineStore('product', () => {
  const products = ref([])
  const loading = ref(false)

  async function load(includeInactive = false) {
    loading.value = true
    try {
      products.value = await http.get(`/products?includeInactive=${includeInactive}`)
    } finally {
      loading.value = false
    }
  }

  async function lookup(gtin) {
    try {
      return await http.get(`/products/${encodeURIComponent(gtin)}`)
    } catch (e) {
      if (e.status === 404) return null
      throw e
    }
  }

  function create(payload) {
    return http.post('/products', payload)
  }

  function update(id, payload) {
    return http.put(`/products/${id}`, payload)
  }

  function deactivate(id) {
    return http.del(`/products/${id}`)
  }

  return { products, loading, load, lookup, create, update, deactivate }
})
