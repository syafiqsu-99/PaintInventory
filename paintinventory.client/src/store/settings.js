import { defineStore } from 'pinia'
import { reactive, watch } from 'vue'

const KEY = 'paint-inventory.settings'

const defaults = {
  expiryWarningDays: 60,
  defaultLocationId: null,
  scanBeep: true,
  scanDefaultDirection: 'out'
}

function load() {
  try {
    const raw = localStorage.getItem(KEY)
    return raw ? { ...defaults, ...JSON.parse(raw) } : { ...defaults }
  } catch {
    return { ...defaults }
  }
}

export const useSettingsStore = defineStore('settings', () => {
  const prefs = reactive(load())

  watch(prefs, (value) => {
    try { localStorage.setItem(KEY, JSON.stringify(value)) } catch { /* storage unavailable */ }
  }, { deep: true })

  function reset() {
    Object.assign(prefs, defaults)
  }

  return { prefs, reset }
})
