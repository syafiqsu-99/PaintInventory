import { onMounted, onUnmounted, ref, unref } from 'vue'

// Detects hardware barcode scanners acting as keyboard wedges: characters arrive
// in a fast burst and end with Enter. Human typing is slower, so a gap longer than
// `interKeyMs` between keystrokes resets the buffer. Manual typing in form fields is
// ignored unless the field opts in with a `data-scan-input` attribute.
export function useScanner(onScan, options = {}) {
  const { interKeyMs = 60, minLength = 3, enabled = true } = options

  const active = ref(unref(enabled))
  let buffer = ''
  let lastTime = 0

  function isEditableTarget(el) {
    if (!el) return false
    if (el.hasAttribute?.('data-scan-input')) return false
    const tag = el.tagName
    return tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT' || el.isContentEditable
  }

  function onKeydown(e) {
    if (!active.value) return
    if (e.ctrlKey || e.altKey || e.metaKey) return
    if (isEditableTarget(document.activeElement)) return

    const now = performance.now()
    if (now - lastTime > interKeyMs) buffer = ''
    lastTime = now

    if (e.key === 'Enter') {
      const code = buffer.trim()
      buffer = ''
      if (code.length >= minLength) {
        e.preventDefault()
        onScan(code)
      }
      return
    }

    if (e.key.length === 1) buffer += e.key
  }

  function pause() { active.value = false }
  function resume() { buffer = ''; active.value = true }

  onMounted(() => window.addEventListener('keydown', onKeydown, true))
  onUnmounted(() => window.removeEventListener('keydown', onKeydown, true))

  return { active, pause, resume }
}
