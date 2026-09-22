<template>
  <v-card>
    <v-card-title class="text-subtitle-1">Stock Out</v-card-title>
    <v-card-text>
      <BarcodeScanField v-model="barcode"
                        label="Scan paint barcode (GTIN)"
                        autofocus
                        clear-on-scan
                        class="mb-4"
                        @scan="onScan" />

      <v-alert v-if="resolved" type="info" variant="tonal" class="mb-4" density="comfortable">
        <div class="font-weight-medium">{{ resolved.productName }}</div>
        <div class="text-body-2">GTIN {{ resolved.gtin }}</div>
      </v-alert>

      <v-form v-model="valid" :disabled="!resolved">
        <v-row dense>
          <v-col cols="12" sm="6">
            <v-select v-model="form.vendorId" :items="stockLocations" label="Location" :rules="[rules.required]" variant="outlined" density="comfortable" />
          </v-col>
          <v-col cols="6" sm="3">
            <v-text-field v-model.number="form.quantity" label="Qty (cans)" type="number" :rules="[rules.required, rules.positive]" variant="outlined" density="comfortable" />
          </v-col>
          <v-col cols="6" sm="3">
            <v-text-field v-model="form.batch" label="Batch / lot" variant="outlined" density="comfortable" />
          </v-col>
          <v-col cols="12">
            <v-text-field v-model="form.notes" label="Notes" variant="outlined" density="comfortable" />
          </v-col>
        </v-row>
      </v-form>
    </v-card-text>
    <v-card-actions>
      <v-btn variant="text" @click="clear">Clear</v-btn>
      <v-spacer />
      <v-btn color="primary" :loading="saving" :disabled="!resolved || !valid" @click="submit">Issue stock</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup>
  import { ref, onMounted, computed } from 'vue'
  import { storeToRefs } from 'pinia'
  import { useProductStore } from '@/store/product'
  import { useVendorStore } from '@/store/vendor'
  import { useStockStore } from '@/store/stock'
  import { useUiStore } from '@/store/ui'
  import BarcodeScanField from '@/components/common/BarcodeScanField.vue'

  const products = useProductStore()
  const vendorStore = useVendorStore()
  const stock = useStockStore()
  const ui = useUiStore()
  const { vendors } = storeToRefs(vendorStore)

  const barcode = ref('')
  const resolved = ref(null)
  const saving = ref(false)
  const valid = ref(false)
  const form = ref(blankForm())

  function blankForm() {
      return { vendorId: null, quantity: null, batch: null, shade: null, operator: null, notes: null }
  }

  const stockLocations = computed(() =>
      vendors.value.filter((v) => v.storesStock).map((v) => ({ title: v.name, value: v.id })))

  onMounted(() => { if (!vendors.value.length) vendorStore.load() })

  const rules = {
      required: (v) => (v !== null && v !== undefined && String(v).trim() !== '') || 'Required',
      positive: (v) => (Number(v) > 0) || 'Must be greater than 0'
  }

  async function onScan(code) {
      const product = await products.lookup(code)
      if (product) {
        resolved.value = product
        form.value.shade = product.defaultShade ?? product.ralCode ?? null
      } else {
        resolved.value = null
        ui.error(`GTIN ${code} is not registered. Register it via Stock In first.`)
      }
  }

  function clear() {
      barcode.value = ''
      resolved.value = null
      form.value = blankForm()
  }

  async function submit() {
      if (!resolved.value || !valid.value) return
      saving.value = true
      try {
        const payload = { productId: resolved.value.id, ...form.value }
        Object.keys(payload).forEach((k) => { if (payload[k] === '') payload[k] = null })
        const result = await stock.stockOut(payload)
        ui.notify(`Stock out: ${resolved.value.productName} — on hand ${result.onHandQty}.`)
        clear()
      } catch (e) {
        ui.error(e.message)
      } finally {
        saving.value = false
      }
  }
</script>
