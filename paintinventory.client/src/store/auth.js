import { defineStore } from 'pinia'
import { ref } from 'vue'
import http from '@/utils/http'

export const useAuthStore = defineStore('auth', () => {
  const isStaff = ref(false)
  const passwordConfigured = ref(true)
  const loaded = ref(false)
  const unlockOpen = ref(false)
  const pendingPath = ref(null)

  async function load(force = false) {
    if (loaded.value && !force) return
    const me = await http.get('/auth/me')
    isStaff.value = me.isStaff
    passwordConfigured.value = me.passwordConfigured
    loaded.value = true
  }

  async function unlock(password) {
    await http.post('/auth/unlock', { password })
    isStaff.value = true
    unlockOpen.value = false
    const next = pendingPath.value
    pendingPath.value = null
    return next
  }

  async function lock() {
    try { await http.post('/auth/lock') } finally { isStaff.value = false }
  }

  function changePassword(currentPassword, newPassword) {
    return http.put('/auth/password', { currentPassword, newPassword })
  }

  function requestUnlock(path = null) {
    pendingPath.value = path
    unlockOpen.value = true
  }

  function cancelUnlock() {
    pendingPath.value = null
    unlockOpen.value = false
  }

  function onDenied() {
    isStaff.value = false
    unlockOpen.value = true
  }

  return {
    isStaff, passwordConfigured, loaded, unlockOpen, pendingPath,
    load, unlock, lock, changePassword, requestUnlock, cancelUnlock, onDenied
  }
})
