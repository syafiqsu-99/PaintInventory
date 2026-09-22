<template>
  <div>
    <div class="d-flex align-center flex-wrap ga-2 mb-3">
      <v-text-field v-model="search"
                    placeholder="Search products"
                    prepend-inner-icon="mdi-magnify"
                    variant="outlined"
                    density="comfortable"
                    hide-details
                    clearable
                    class="flex-grow-1"
                    style="min-width: 200px" />
      <v-btn color="primary" prepend-icon="mdi-plus" @click="add">Add product</v-btn>
    </div>

    <v-card>
      <v-data-table :headers="headers"
                    :items="products"
                    :search="search"
                    :loading="loading"
                    density="comfortable"
                    items-per-page="25">
        <template #[`item.component`]="{ item }">
          {{ componentLabel(item.component) }}
        </template>
        <template #[`item.packVolume`]="{ item }">
          {{ item.packVolume != null ? `${item.packVolume} ${item.unit || ''}` : '—' }}
        </template>
        <template #[`item.tracksExpiry`]="{ item }">
          <v-icon v-if="item.tracksExpiry" icon="mdi-check" color="success" size="small" />
        </template>
        <template #[`item.actions`]="{ item }">
          <v-btn size="small" variant="text" icon="mdi-tune" title="Reorder level" @click="reorder(item)" />
          <v-btn size="small" variant="text" icon="mdi-pencil" title="Edit" @click="edit(item)" />
          <v-btn size="small" variant="text" icon="mdi-archive-arrow-down" color="error" title="Deactivate" @click="deactivate(item)" />
        </template>
        <template #no-data>
          <div class="text-medium-emphasis py-8">No products yet. Add one or import a CSV.</div>
        </template>
      </v-data-table>
    </v-card>

    <ProductForm v-model="showForm" :product="selected" @saved="reload" />
    <ReorderDialog v-model="showReorder" :product="selected" />
  </div>
</template>

<script setup>
  import { onMounted, ref } from 'vue'
  import { storeToRefs } from 'pinia'
  import { useProductStore } from '@/store/product'
  import { useUiStore } from '@/store/ui'
  import ProductForm from '@/components/products/ProductForm.vue'
  import ReorderDialog from '@/components/products/ReorderDialog.vue'

  const productStore = useProductStore()
  const ui = useUiStore()
  const { products, loading } = storeToRefs(productStore)

  const search = ref('')
  const showForm = ref(false)
  const showReorder = ref(false)
  const selected = ref(null)

  const headers = [
    { title: 'GTIN', key: 'gtin' },
    { title: 'Product', key: 'productName' },
    { title: 'Component', key: 'component' },
    { title: 'Pack', key: 'packVolume', align: 'end' },
    { title: 'Manufacturer', key: 'manufacturer' },
    { title: 'Expiry', key: 'tracksExpiry', align: 'center', sortable: false },
    { title: '', key: 'actions', sortable: false, align: 'end' }
  ]

  const componentLabel = (c) => (c === 'PartA' ? 'Part A' : c === 'PartB' ? 'Part B' : 'Single')

  function add() { selected.value = null; showForm.value = true }
  function edit(p) { selected.value = p; showForm.value = true }
  function reorder(p) { selected.value = p; showReorder.value = true }

  async function deactivate(p) {
    if (!confirm(`Deactivate ${p.productName}?`)) return
    try {
      await productStore.deactivate(p.id)
      ui.notify('Product deactivated.')
      reload()
    } catch (e) {
      ui.error(e.message)
    }
  }

  function reload() { productStore.load() }

  onMounted(reload)
  defineExpose({ reload })
</script>
