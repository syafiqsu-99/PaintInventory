<script setup>
import { formatNumber } from '@/utils/format'

defineProps({
  items: { type: Array, default: () => [] },
  loading: { type: Boolean, default: false }
})

const headers = [
  { title: 'Product', key: 'productName' },
  { title: 'Component', key: 'component' },
  { title: 'Shade', key: 'shade' },
  { title: 'Location', key: 'vendorName' },
  { title: 'On hand', key: 'onHandQty', align: 'end' },
  { title: 'Unit', key: 'unit' },
  { title: 'Reorder', key: 'reorderLevel', align: 'end' }
]

const componentLabel = (c) => (c === 'PartA' ? 'Part A' : c === 'PartB' ? 'Part B' : '—')
const rowProps = ({ item }) => ({ class: item.isLowStock ? 'bg-red-lighten-5' : '' })
</script>

<template>
  <v-card>
    <v-card-title class="text-subtitle-1">Stock level</v-card-title>
    <v-data-table :headers="headers"
                  :items="items"
                  :loading="loading"
                  :row-props="rowProps"
                  density="comfortable"
                  items-per-page="25">
      <template #[`item.component`]="{ item }">
        {{ componentLabel(item.component) }}
      </template>
      <template #[`item.shade`]="{ item }">
        {{ item.shade || '—' }}
      </template>
      <template #[`item.onHandQty`]="{ item }">
        {{ formatNumber(item.onHandQty) }}
        <v-chip v-if="item.isLowStock" color="error" size="x-small" label class="ms-2">Low</v-chip>
      </template>
      <template #[`item.reorderLevel`]="{ item }">
        {{ item.reorderLevel != null ? formatNumber(item.reorderLevel) : '—' }}
      </template>
    </v-data-table>
  </v-card>
</template>
