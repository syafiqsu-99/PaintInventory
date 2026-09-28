<template>
  <v-text-field v-bind="$attrs"
                :model-value="modelValue"
                :label="label"
                :inputmode="effectiveInputmode"
                :hint="scannedRaw ? `Scanned: ${scannedRaw}` : hint"
                :persistent-hint="!!scannedRaw || persistentHint"
                autocomplete="off"
                autocapitalize="characters"
                spellcheck="false"
                variant="outlined"
                density="comfortable"
                data-scan-field
                @update:model-value="onInput"
                @keydown.enter.prevent="onEnter">
    <template v-if="assist" #append-inner>
      <v-chip v-if="scannedRaw" size="x-small" color="warning" variant="flat" class="me-1">Check</v-chip>
      <v-btn :icon="typing ? 'mdi-keyboard-off-outline' : 'mdi-keyboard-outline'"
             :title="typing ? 'Hide keyboard' : 'Type manually'"
             variant="text"
             size="small"
             density="comfortable"
             tabindex="-1"
             @click="toggleTyping" />
    </template>
  </v-text-field>
</template>

<script setup>
  import { computed, ref } from 'vue'
  import { storeToRefs } from 'pinia'
  import { useSettingsStore } from '@/store/settings'
  import { cleanScannedText } from '@/utils/scanParse'
  import { focusNextScanField, useScanRecorder } from '@/composables/useScanFields'

  defineOptions({ inheritAttrs: false })

  const props = defineProps({
    modelValue: { type: [String, Number], default: null },
    label: { type: String, default: '' },
    scanKey: { type: String, default: '' },
    inputmode: { type: String, default: 'text' },
    clean: { type: Boolean, default: true },
    advance: { type: Boolean, default: true },
    hint: { type: String, default: '' },
    persistentHint: { type: Boolean, default: false }
  })
  const emit = defineEmits(['update:modelValue', 'scanned'])

  const { prefs } = storeToRefs(useSettingsStore())
  const record = useScanRecorder()

  const scannedRaw = ref('')
  const typing = ref(false)

  const assist = computed(() => prefs.value.ocrAssist)
  const effectiveInputmode = computed(() => (assist.value && !typing.value ? 'none' : props.inputmode))

  function onInput(value) {
    scannedRaw.value = ''
    emit('update:modelValue', value)
  }

  function onEnter(e) {
    const raw = String(props.modelValue ?? '')
    if (assist.value && raw.trim()) {
      const value = props.clean ? cleanScannedText(raw) : raw.trim()
      scannedRaw.value = value !== raw ? raw : ''
      record(props.scanKey, raw)
      emit('update:modelValue', value)
      emit('scanned', { raw, value })
    }
    if (assist.value && props.advance) focusNextScanField(e.target)
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
