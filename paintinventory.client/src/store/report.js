import { defineStore } from 'pinia'
import { ref } from 'vue'
import http from '@/utils/http'

export const useReportStore = defineStore('report', () => {
  const reports = ref([])
  const loading = ref(false)

  async function load() {
    loading.value = true
    try {
      reports.value = await http.get('/reports')
    } finally {
      loading.value = false
    }
  }

  function get(id) {
    return http.get(`/reports/${id}`)
  }

  function create(payload) {
    return http.post('/reports', payload)
  }

  function update(id, payload) {
    return http.put(`/reports/${id}`, payload)
  }

  function remove(id) {
    return http.del(`/reports/${id}`)
  }

  return { reports, loading, load, get, create, update, remove }
})
