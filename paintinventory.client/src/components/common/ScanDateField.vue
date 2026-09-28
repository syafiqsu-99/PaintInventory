<template>
  <div>
    <v-text-field v-bind="$attrs"
                  v-model="text"
                  :label="label"
                  :inputmode="effectiveInputmode"
                  :error-messages="error ? [error] : []"
                  :hint="scannedRaw ? `Scanned: ${scannedRaw}` : hint || 'DD.MM.YYYY'"
                  :persistent-hint="!!scannedRaw || persistentHint"
                  placeholder="DD.MM.YYYY"
                  autocomplete="off"
                  spellcheck="false"
                  variant="outlined"
                  density="comfortable"
                  data-scan-field
                  @keydown.enter.prevent="onEnter"
                  @blur="commit(false)">
      <template #append-inner>
        <v-chip v-if="scannedRaw" size="x-small" color="warning" variant="flat" class="me-1">Check</v-chip>
        <v-btn v-if="assist"
               :icon="typing ? 'mdi-keyboard-off-outline' : 'mdi-keyboard-outline'"
               :title="typing ? 'Hide keyboard' : 'Type manually'"
               variant="text"
               size="small"
               density="comfortable"
               tabindex="-1"
               @click="toggleTyping" />
        <v-btn icon="mdi-calendar"
               title="Pick from calendar"
               variant="text"
               size="small"
               density="comfortable"
               tabindex="-1"
               @click="pickerOpen = true" />
      </template>
    </v-text-field>

    <v-dialog v-model="pickerOpen" :fullscreen="xs" max-width="360">
      <v-card>
        <v-date-picker :model-value="pickerValue"
                       :min="min || undefined"
                       :max="max || undefined"
                       show-adjacent-months
                       width="100%"
                       @update:model-value="onPick" />
        <v-card-actions>
          <v-btn variant="text" size="large" @click="clear">Clear</v-btn>
          <v-spacer />
          <v-btn variant="text" size="large" @click="pickerOpen = false">Close</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup>
  import { computed, ref, watch } from 'vue'
  import { useDisplay } from 'vuetify'
  import { storeToRefs } from 'pinia'
  import { useSettingsStore } from '@/store/settings'
  import { formatDisplayDate, parseScannedDate } from '@/utils/scanParse'
  import { focusNextScanField, useScanRecorder } from '@/composables/useScanFields'

  defineOptions({ inheritAttrs: false })

  const props = defineProps({
    modelValue: { type: String, default: null },
    label: { type: String, default: 'Date' },
    scanKey: { type: String, default: '' },
    min: { type: String, default: null },
    max: { type: String, default: null },
    advance: { type: Boolean, default: true },
    hint: { type: String, default: '' },
    persistentHint: { type: Boolean, default: false }
  })
  const emit = defineEmits(['update:modelValue', 'scanned'])

  const { xs } = useDisplay()
  const { prefs } = storeToRefs(useSettingsStore())
  const record = useScanRecorder()

  const text = ref(formatDisplayDate(props.modelValue))
  const parseError = ref('')
  const scannedRaw = ref('')
  const typing = ref(false)
  const pickerOpen = ref(false)

  const assist = computed(() => prefs.value.ocrAssist)
  const effectiveInputmode = computed(() => (assist.value && !typing.value ? 'none' : 'numeric'))

  const rangeError = computed(() => {
    if (!props.modelValue) return ''
    if (props.min && props.modelValue < props.min) return `Must be on or after ${formatDisplayDate(props.min)}`
    if (props.max && props.modelValue > props.max) return `Must be on or before ${formatDisplayDate(props.max)}`
    return ''
  })
  const error = computed(() => parseError.value || rangeError.value)

  const pickerValue = computed(() => {
    const m = props.modelValue?.match(/^(\d{4})-(\d{2})-(\d{2})/)
    return m ? new Date(Number(m[1]), Number(m[2]) - 1, Number(m[3])) : null
  })

  watch(() => props.modelValue, (iso) => {
    if (iso !== parseScannedDate(text.value).iso) {
      text.value = formatDisplayDate(iso)
      parseError.value = ''
      scannedRaw.value = ''
    }
  })

  watch(text, () => { parseError.value = '' })

  function commit(fromEnter) {
    const raw = text.value ?? ''
    const { iso, error: err } = parseScannedDate(raw)

    if (err) {
      parseError.value = err
      if (props.modelValue !== null) emit('update:modelValue', null)
      return false
    }

    const display = formatDisplayDate(iso)
    if (fromEnter && assist.value && raw.trim()) {
      scannedRaw.value = display !== raw.trim() ? raw : ''
      record(props.scanKey, raw)
      emit('scanned', { raw, value: iso })
    }
    text.value = display
    parseError.value = ''
    if (iso !== props.modelValue) emit('update:modelValue', iso)
    return true
  }

  function onEnter(e) {
    const ok = commit(true)
    if (ok && assist.value && props.advance) focusNextScanField(e.target)
  }

  function onPick(date) {
    if (!date) return
    const d = new Date(date)
    const iso = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
    text.value = formatDisplayDate(iso)
    parseError.value = ''
    scannedRaw.value = ''
    emit('update:modelValue', iso)
    pickerOpen.value = false
  }

  function clear() {
    text.value = ''
    parseError.value = ''
    scannedRaw.value = ''
    emit('update:modelValue', null)
    pickerOpen.value = false
  }

  function toggleTyping(e) {
    typing.value = !typing.value
    const input = e.currentTarget.closest('.v-field')?.querySelector('input')
    if (input) {
      input.blur()
      requestAnimationFrame(() => input.focus())
    }
  }
</script>
