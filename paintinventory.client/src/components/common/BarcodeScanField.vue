<template>
  <div class="d-flex align-center ga-2">
    <v-text-field ref="field"
                  :model-value="modelValue"
                  :label="label"
                  :autofocus="autofocus"
                  variant="outlined"
                  density="comfortable"
                  hide-details
                  class="flex-grow-1"
                  @update:model-value="emit('update:modelValue', $event)"
                  @keyup.enter="onEnter" />
    <v-btn color="primary" size="large" height="56" @click="focus">Scan</v-btn>
  </div>
</template>

<script setup>
    import { ref } from 'vue'

    const props = defineProps({
        modelValue: { type: String, default: '' },
        label: { type: String, default: 'Scan barcode' },
        clearOnScan: { type: Boolean, default: false },
        autofocus: { type: Boolean, default: false }
    })
    const emit = defineEmits(['update:modelValue', 'scan'])

    const field = ref(null)

    function onEnter() {
        const code = (props.modelValue || '').trim()
        if (!code) return
        emit('scan', code)
        if (props.clearOnScan) emit('update:modelValue', '')
    }

    function focus() {
        field.value?.focus()
    }

    defineExpose({ focus })
</script>
