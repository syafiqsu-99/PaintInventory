import { inject, provide, reactive } from 'vue'

const CAPTURE_KEY = Symbol('scan-capture')

// Collects the raw scanner/OCR read per field so the trial can compare it with the saved value.
export function useScanCapture() {
  const raw = reactive({})

  provide(CAPTURE_KEY, (key, value) => {
    if (key) raw[key] = value
  })

  function payload() {
    return Object.keys(raw).length ? { ...raw } : null
  }

  function reset() {
    Object.keys(raw).forEach((k) => delete raw[k])
  }

  return { raw, payload, reset }
}

export function useScanRecorder() {
  return inject(CAPTURE_KEY, () => {})
}

export function focusNextScanField(fromEl) {
  if (!fromEl) return false
  const scope = fromEl.closest('form, .v-overlay__content, .v-card') ?? document
  const fields = [...scope.querySelectorAll('[data-scan-field] input:not([disabled]):not([readonly])')]
  const next = fields[fields.indexOf(fromEl) + 1]
  if (!next) return false
  next.focus()
  return true
}
