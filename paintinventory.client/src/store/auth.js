import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import http from '@/utils/http'

export const useAuthStore = defineStore('auth', () => {
  const me = ref(null)
  const loaded = ref(false)

  const isAuthenticated = computed(() => !!me.value)
  const isStaff = computed(() => me.value?.role === 'Staff')
  const vendorId = computed(() => me.value?.vendorId ?? null)

  async function load(force = false) {
    if (loaded.value && !force) return me.value
    try {
      me.value = await http.get('/auth/me')
    } catch (e) {
      if (e.status !== 401) throw e
      me.value = null
    }
    loaded.value = true
    return me.value
  }

  function sites() {
    return http.get('/auth/sites')
  }

  async function login(payload) {
    me.value = await http.post('/auth/login', payload)
    loaded.value = true
    return me.value
  }

  async function logout() {
    try { await http.post('/auth/logout') } finally { clear() }
  }

  function clear() {
    me.value = null
    loaded.value = true
  }

  return { me, loaded, isAuthenticated, isStaff, vendorId, load, sites, login, logout, clear }
})
