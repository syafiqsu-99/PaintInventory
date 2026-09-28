<template>
  <v-card>
    <v-card-title class="text-subtitle-1">Stock In</v-card-title>
    <v-card-text>
      <BarcodeScanField v-model="barcode"
                        label="Scan paint barcode (GTIN)"
                        autofocus
                        clear-on-scan
                        class="mb-4"
                        @scan="onScan" />

      <v-alert v-if="resolved" type="info" variant="tonal" class="mb-4" density="comfortable">
        <div class="font-weight-medium">
          {{ resolved.productName }}
          <span v-if="resolved.component !== 'Single'">— {{ resolved.component === 'PartA' ? 'Part A' : 'Part B' }}</span>
        </div>
        <div class="text-body-2">
          GTIN {{ resolved.gtin }}<span v-if="resolved.itemCode"> · {{ resolved.itemCode }}</span>
          <span v-if="resolved.packVolume"> · {{ resolved.packVolume }} {{ resolved.unit }}</span>
        </div>
      </v-alert>

      <v-form v-model="valid" :disabled="!resolved">
        <v-row dense>
          <v-col cols="12" sm="6">
            <v-select v-model="form.vendorId" :items="stockLocations" label="Location" :rules="[rules.required]" variant="outlined" density="comfortable" />
          </v-col>
          <v-col cols="12" sm="6">
            <v-text-field v-model="form.source" label="Source" variant="outlined" density="comfortable" />
          </v-col>
          <v-col cols="12" sm="3">
            <v-text-field v-model.number="form.quantity" label="Received qty (cans)" type="number" inputmode="decimal" :rules="[rules.required, rules.positive]" variant="outlined" density="comfortable" />
          </v-col>
          <v-col cols="12" sm="3">
            <ScanTextField v-model="form.batch" label="Batch / lot" scan-key="batch" />
          </v-col>
          <v-col cols="12" sm="3">
            <ScanDateField v-model="form.manufacturingDate" label="Mfg date" scan-key="manufacturingDate" />
          </v-col>
          <v-col cols="12" sm="3">
            <ScanDateField v-model="form.bestBefore" label="Best before" scan-key="bestBefore" :min="form.manufacturingDate" />
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
      <v-btn color="primary" size="large" :loading="saving" :disabled="!resolved || !valid" @click="submit">Add to stock</v-btn>
    </v-card-actions>

    <ProductRegisterDialog v-model="showRegister" :gtin="pendingGtin" @registered="onRegistered" />
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
  import ScanTextField from '@/components/common/ScanTextField.vue'
  import ScanDateField from '@/components/common/ScanDateField.vue'
  import { useScanCapture } from '@/composables/useScanFields'
  import ProductRegisterDialog from '@/components/products/ProductRegisterDialog.vue'

  const products = useProductStore()
  const vendorStore = useVendorStore()
  const stock = useStockStore()
  const ui = useUiStore()
  const capture = useScanCapture()
  const { vendors } = storeToRefs(vendorStore)

  const barcode = ref('')
  const resolved = ref(null)
  const showRegister = ref(false)
  const pendingGtin = ref('')
  const saving = ref(false)
  const valid = ref(false)
  const form = ref(blankForm())

  function blankForm() {
      return {
        vendorId: null, quantity: null, batch: null, shade: null, packVolume: null,
        manufacturingDate: null, bestBefore: null, source: null, operator: null, notes: null
      }
  }

  const stockLocations = computed(() =>
      vendors.value.filter((v) => v.storesStock).map((v) => ({ title: v.name, value: v.id })))

  onMounted(() => { if (!vendors.value.length) vendorStore.load() })

  const rules = {
      required: (v) => (v !== null && v !== undefined && String(v).trim() !== '') || 'Required',
      positive: (v) => (Number(v) > 0) || 'Must be greater than 0'
  }

  function applyProduct(product) {
      resolved.value = product
      form.value.shade = product.colour ?? product.ralCode ?? null
      form.value.packVolume = product.packVolume ?? null
  }

  async function onScan(code) {
      const product = await products.lookup(code)
      if (product) {
        applyProduct(product)
      } else {
        pendingGtin.value = code
        showRegister.value = true
      }
  }

  function onRegistered(product) {
      barcode.value = product.gtin
      applyProduct(product)
  }

  function clear() {
      barcode.value = ''
      resolved.value = null
      form.value = blankForm()
      capture.reset()
  }

  async function submit() {
      if (!resolved.value || !valid.value) return
      saving.value = true
      try {
        const payload = { productId: resolved.value.id, ...form.value }
        Object.keys(payload).forEach((k) => { if (payload[k] === '') payload[k] = null })
        const result = await stock.stockIn({ ...payload, scanRaw: capture.payload() })
        ui.notify(`Stock in: ${resolved.value.productName} — on hand ${result.onHandQty}.`)
        clear()
      } catch (e) {
        ui.error(e.message)
      } finally {
        saving.value = false
      }
  }
</script>
