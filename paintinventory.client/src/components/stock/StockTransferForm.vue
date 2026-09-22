<template>
  <v-card>
    <v-card-title class="text-subtitle-1">Stock Transfer</v-card-title>
    <v-card-text>
      <v-form v-model="valid">
        <v-row dense>
          <v-col cols="12">
            <v-select v-model="form.productId" :items="productOptions" label="Product" :rules="[rules.required]" variant="outlined" density="comfortable" />
          </v-col>
          <v-col cols="12" sm="6">
            <v-select v-model="form.fromVendorId" :items="locations" label="From location" :rules="[rules.required]" variant="outlined" density="comfortable" />
          </v-col>
          <v-col cols="12" sm="6">
            <v-select v-model="form.toVendorId" :items="locations" label="To location" :rules="[rules.required, rules.different]" variant="outlined" density="comfortable" />
          </v-col>
          <v-col cols="6" sm="3">
            <v-text-field v-model.number="form.quantity" label="Qty (cans)" type="number" :rules="[rules.required, rules.positive]" variant="outlined" density="comfortable" />
          </v-col>
          <v-col cols="6" sm="3">
            <v-text-field v-model="form.batch" label="Batch / lot" variant="outlined" density="comfortable" />
          </v-col>
          <v-col cols="12" sm="6">
            <v-text-field v-model="form.notes" label="Notes" variant="outlined" density="comfortable" />
          </v-col>
        </v-row>
      </v-form>
    </v-card-text>
    <v-card-actions>
      <v-spacer />
      <v-btn color="primary" :loading="saving" :disabled="!valid" @click="submit">Transfer</v-btn>
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

  const productStore = useProductStore()
  const vendorStore = useVendorStore()
  const stock = useStockStore()
  const ui = useUiStore()
  const { products } = storeToRefs(productStore)
  const { vendors } = storeToRefs(vendorStore)

  const saving = ref(false)
  const valid = ref(false)
  const form = ref(blankForm())

  function blankForm() {
        return { productId: null, fromVendorId: null, toVendorId: null, quantity: null, batch: null, notes: null }
  }

  const productOptions = computed(() =>
        products.value.map((p) => ({
          title: p.component === 'Single' ? p.productName : `${p.productName} (${p.component === 'PartA' ? 'A' : 'B'})`,
          value: p.id
        })))
  const locations = computed(() => vendors.value.filter((v) => v.storesStock).map((v) => ({ title: v.name, value: v.id })))

  onMounted(async () => {
        if (!products.value.length) await productStore.load()
        if (!vendors.value.length) await vendorStore.load()
  })

  const rules = {
        required: (v) => (v !== null && v !== undefined && String(v).trim() !== '') || 'Required',
        positive: (v) => (Number(v) > 0) || 'Must be greater than 0',
        different: () => (form.value.fromVendorId !== form.value.toVendorId) || 'Must differ from source'
  }

  async function submit() {
        if (!valid.value) return
        saving.value = true
        try {
          const payload = { ...form.value }
          Object.keys(payload).forEach((k) => { if (payload[k] === '') payload[k] = null })
          const result = await stock.transfer(payload)
          ui.notify(`Transferred ${result.from.quantityApplied * -1} — ${result.from.vendorName}: ${result.from.onHandQty}, ${result.to.vendorName}: ${result.to.onHandQty}.`)
          form.value = blankForm()
        } catch (e) {
          ui.error(e.message)
        } finally {
          saving.value = false
        }
  }
</script>
