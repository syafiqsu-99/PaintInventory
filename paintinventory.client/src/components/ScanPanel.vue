<script setup>
import { ref } from 'vue'
import { useInventoryStore } from '@/store/inventory'
import { useScanner } from '@/utils/useScanner'
import ItemDetailsCard from '@/components/ItemDetailsCard.vue'
import ScanConfirmForm from '@/components/ScanConfirmForm.vue'

const store = useInventoryStore()

const item = ref(null)
const loading = ref(false)
const notFound = ref(false)
const feedback = ref(null) // { type, text }

async function handleScan(code) {
  loading.value = true
  notFound.value = false
  feedback.value = null
  item.value = null
  try {
    item.value = await store.lookup(code)
  } catch (err) {
    if (err.status === 404) {
      notFound.value = true
      feedback.value = { type: 'warning', text: `Barcode "${code}" is not registered yet.` }
    } else {
      feedback.value = { type: 'error', text: err.message }
    }
  } finally {
    loading.value = false
  }
}

const { value, inputRef, submit, focus } = useScanner(handleScan)

async function confirm(payload) {
  try {
    const result = await store.recordScan({ ...payload, Barcode: item.value.barcode })
    feedback.value = {
      type: result.isLowStock ? 'warning' : 'success',
      text: `${result.action}: on hand is now ${result.onHand}` +
        (result.isLowStock ? ' — at or below reorder level.' : '.')
    }
    item.value = null
    focus()
  } catch (err) {
    feedback.value = { type: 'error', text: err.message }
  }
}

function cancel() {
  item.value = null
  focus()
}
</script>

<template>
  <v-card>
    <v-card-title class="text-h6">Scan paint</v-card-title>
    <v-card-text>
      <v-text-field ref="inputRef"
                    v-model="value"
                    label="Barcode / QR"
                    placeholder="Scan or type, then Enter"
                    prepend-inner-icon="mdi-barcode-scan"
                    :loading="loading"
                    autofocus
                    clearable
                    @keyup.enter="submit" />

      <v-alert v-if="feedback"
               :type="feedback.type"
               variant="tonal"
               density="comfortable"
               class="mb-4">
        {{ feedback.text }}
      </v-alert>

      <div v-if="notFound" class="mb-2">
        <v-btn to="/inventory" color="primary" variant="tonal" prepend-icon="mdi-plus">
          Register this item
        </v-btn>
      </div>

      <template v-if="item">
        <ItemDetailsCard :item="item" class="mb-4" />
        <ScanConfirmForm :item="item" @confirm="confirm" @cancel="cancel" />
      </template>
    </v-card-text>
  </v-card>
</template>
