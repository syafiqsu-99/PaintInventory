import { ref, onMounted, nextTick } from 'vue'

export function useScanner(onScan) {
  const value = ref('')
  const inputRef = ref(null)

  function focus() {
    nextTick(() => inputRef.value?.focus?.())
  }

  function submit() {
    const code = value.value.trim()
    value.value = ''
    if (code) onScan(code)
    focus()
  }

  onMounted(focus)

  return { value, inputRef, focus, submit }
}
