<script setup>
import { computed } from 'vue'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  barcode: { type: String, default: null }
})
const emit = defineEmits(['update:modelValue'])

const qrUrl = computed(() =>
  props.barcode ? `/api/paint/${encodeURIComponent(props.barcode)}/qr` : null
)

function close() {
  emit('update:modelValue', false)
}

function printLabel() {
  const win = window.open('', '_blank', 'width=400,height=500')
  if (!win) return
  win.document.write(
    `<html><head><title>${props.barcode}</title></head>` +
    `<body style="text-align:center;font-family:sans-serif;margin-top:40px">` +
    `<img src="${qrUrl.value}" style="width:240px;height:240px" />` +
    `<p style="font-size:18px">${props.barcode}</p>` +
    `</body></html>`
  )
  win.document.close()
  win.focus()
  win.onload = () => win.print()
}
</script>

<template>
  <v-dialog :model-value="modelValue" max-width="360" @update:model-value="emit('update:modelValue', $event)">
    <v-card>
      <v-card-title>QR label</v-card-title>
      <v-card-text class="text-center">
        <img v-if="qrUrl" :src="qrUrl" alt="QR code" width="220" height="220" />
        <div class="text-body-2 mt-2">{{ barcode }}</div>
      </v-card-text>
      <v-card-actions>
        <v-spacer />
        <v-btn variant="text" @click="close">Close</v-btn>
        <v-btn color="primary" prepend-icon="mdi-printer" @click="printLabel">Print</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>
