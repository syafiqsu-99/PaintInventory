<template>
  <v-container fluid>
    <div class="d-flex align-center mb-3">
      <h2 class="text-h6">Products</h2>
      <v-spacer />
      <v-btn color="primary" prepend-icon="mdi-plus" @click="add">Add</v-btn>
    </div>
    <v-card>
      <v-data-table-virtual :headers="headers" :items="products" :loading="loading" density="comfortable">
        <template #[`item.component`]="{ item }">
          {{ componentLabel(item.component) }}
        </template>
        <template #[`item.packVolume`]="{ item }">
          {{ item.packVolume != null ? `${item.packVolume} ${item.unit || ''}` : '—' }}
        </template>
        <template #[`item.actions`]="{ item }">
          <v-btn size="small" variant="text" icon="mdi-tune" title="Reorder level" @click="reorder(item)" />
          <v-btn size="small" variant="text" icon="mdi-pencil" @click="edit(item)" />
          <v-btn size="small" variant="text" icon="mdi-archive-arrow-down" color="error" @click="deactivate(item)" />
        </template>
      </v-data-table-virtual>
    </v-card>

    <ProductForm v-model="showForm" :product="selected" @saved="reload" />
    <ReorderDialog v-model="showReorder" :product="selected" />
  </v-container>
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

const showForm = ref(false)
const showReorder = ref(false)
const selected = ref(null)

const headers = [
    { title: 'GTIN', key: 'gtin' },
    { title: 'Product', key: 'productName' },
    { title: 'Component', key: 'component' },
    { title: 'Pack', key: 'packVolume', align: 'end' },
    { title: 'Manufacturer', key: 'manufacturer' },
    { title: '', key: 'actions', sortable: false, align: 'end' }
]

const componentLabel = (c) => (c === 'PartA' ? 'Part A' : c === 'PartB' ? 'Part B' : 'Single')

function add() {
    selected.value = null
    showForm.value = true
}

function edit(p) {
    selected.value = p
    showForm.value = true
}

function reorder(p) {
    selected.value = p
    showReorder.value = true
}

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

function reload() {
    productStore.load()
}

onMounted(reload)
</script>
