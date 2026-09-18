<script setup>
import { ref } from 'vue'
import { getPaintByBarcode, recordScan } from '../api'

const barcode = ref('')
const paint = ref(null)
const quantity = ref(1)
const unit = ref('')
const action = ref('Use')
const location = ref('')
const notes = ref('')
const message = ref('')
const loading = ref(false)

async function onScanEnter() {
  if (!barcode.value) return
  message.value = ''
  loading.value = true
  try {
    paint.value = await getPaintByBarcode(barcode.value)
  } catch (err) {
    console.error(err)
    message.value = 'Error looking up barcode'
    paint.value = null
  } finally {
    loading.value = false
  }
}

async function confirm() {
  message.value = ''
  loading.value = true
  try {
    const payload = {
      BarcodeScanned: barcode.value,
      Action: action.value,
      Quantity: Number(quantity.value),
      Unit: unit.value || (paint.value ? paint.value.unit : null),
      DeviceId: window.navigator.userAgent,
      Operator: null,
      Location: location.value,
      Notes: notes.value,
    }

    const result = await recordScan(payload)
    message.value = 'Scan recorded'
    // clear for next
    barcode.value = ''
    paint.value = null
    quantity.value = 1
    unit.value = ''
    location.value = ''
    notes.value = ''
  } catch (err) {
    console.error(err)
    message.value = 'Failed to record scan'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="scan-container">
    <h1>Paint Inventory - Scan</h1>

    <div class="field">
      <label>Barcode</label>
      <input v-model="barcode" @keyup.enter="onScanEnter" placeholder="Scan barcode or type and press Enter" autofocus />
      <button @click="onScanEnter">Lookup</button>
    </div>

    <div v-if="loading">Loading...</div>

    <div v-if="paint">
      <h2>Item</h2>
      <div>Barcode: {{ paint.barcode }}</div>
      <div>Name: {{ paint.name }}</div>
      <div>SKU: {{ paint.sku }}</div>
      <div>Color: {{ paint.colorCode }}</div>
      <div>Volume: {{ paint.volume }} {{ paint.unit }}</div>

      <h3>Confirm</h3>
      <div class="field">
        <label>Action</label>
        <select v-model="action">
          <option>Use</option>
          <option>Receive</option>
          <option>Adjust</option>
        </select>
      </div>

      <div class="field">
        <label>Quantity</label>
        <input type="number" v-model.number="quantity" />
      </div>

      <div class="field">
        <label>Unit</label>
        <input v-model="unit" />
      </div>

      <div class="field">
        <label>Location</label>
        <input v-model="location" />
      </div>

      <div class="field">
        <label>Notes</label>
        <input v-model="notes" />
      </div>

      <button @click="confirm">Confirm</button>
    </div>

    <div v-else>
      <p v-if="message">{{ message }}</p>
    </div>
  </div>
</template>

<style scoped>
.scan-container {
  max-width: 420px;
  margin: 0 auto;
  padding: 1rem;
}
.field { margin-bottom: 0.75rem }
label { display:block; font-weight:600; margin-bottom:0.25rem }
input, select { width:100%; padding:0.5rem; }
button { margin-top:0.5rem; padding:0.5rem 1rem }
</style>
