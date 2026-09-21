import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useUiStore = defineStore('ui', () => {
  const snackbar = ref({ show: false, text: '', color: 'success' })

  function notify(text, color = 'success') {
    snackbar.value = { show: true, text, color }
  }

  function error(text) {
    notify(text, 'error')
  }

  return { snackbar, notify, error }
})
