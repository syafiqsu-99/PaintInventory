<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useReportStore } from '@/store/report'
import { useProductStore } from '@/store/product'
import { useVendorStore } from '@/store/vendor'
import { useUiStore } from '@/store/ui'
import ReportHeaderForm from '@/components/ReportHeaderForm.vue'
import ReportItemEditor from '@/components/ReportItemEditor.vue'
import { blankReport, blankItem, fromDto } from '@/utils/reportModel'

const route = useRoute()
const router = useRouter()
const reportStore = useReportStore()
const productStore = useProductStore()
const vendorStore = useVendorStore()
const ui = useUiStore()

const id = computed(() => (route.params.id ? Number(route.params.id) : null))
const form = ref(blankReport())
const saving = ref(false)
const loading = ref(false)

const productOptions = computed(() =>
  productStore.products.map((p) => ({
    title: p.component === 'Single' ? p.productName : `${p.productName} (${p.component === 'PartA' ? 'A' : 'B'})`,
    value: p.id
  })))
const blastVendors = computed(() => vendorStore.vendors.filter((v) => v.doesBlasting).map((v) => ({ title: v.name, value: v.id })))
const paintingVendors = computed(() => vendorStore.vendors.filter((v) => v.doesPainting).map((v) => ({ title: v.name, value: v.id })))
const stockLocations = computed(() => vendorStore.vendors.filter((v) => v.storesStock).map((v) => ({ title: v.name, value: v.id })))

function addItem() {
  form.value.items.push(blankItem(form.value.items.length + 1))
}
function removeItem(i) {
  form.value.items.splice(i, 1)
}

function clean(value) {
  if (Array.isArray(value)) return value.map(clean)
  if (value && typeof value === 'object') {
    const out = {}
    for (const k in value) out[k] = value[k] === '' ? null : clean(value[k])
    return out
  }
  return value
}

async function save() {
  saving.value = true
  try {
    const payload = clean(JSON.parse(JSON.stringify(form.value)))
    const result = id.value
      ? await reportStore.update(id.value, payload)
      : await reportStore.create(payload)
    ;(result.warnings ?? []).forEach((w) => ui.notify(w, 'warning'))
    ui.notify('Report saved.')
    router.push('/reports')
  } catch (e) {
    ui.error(e.message)
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  loading.value = true
  try {
    if (!productStore.products.length) await productStore.load()
    if (!vendorStore.vendors.length) await vendorStore.load()
    if (id.value) form.value = fromDto(await reportStore.get(id.value))
  } catch (e) {
    ui.error(e.message)
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <v-container>
    <div class="d-flex align-center mb-3">
      <h2 class="text-h6">{{ id ? 'Edit report' : 'New report' }}</h2>
      <v-spacer />
      <v-btn variant="text" to="/reports">Cancel</v-btn>
      <v-btn color="primary" :loading="saving" @click="save">Save</v-btn>
    </div>

    <ReportHeaderForm :model="form" />

    <ReportItemEditor v-for="(item, i) in form.items"
                      :key="i"
                      :model="item"
                      :product-options="productOptions"
                      :blast-vendors="blastVendors"
                      :painting-vendors="paintingVendors"
                      :stock-locations="stockLocations"
                      @remove="removeItem(i)" />

    <v-btn variant="tonal" prepend-icon="mdi-plus" @click="addItem">Add item</v-btn>
  </v-container>
</template>
