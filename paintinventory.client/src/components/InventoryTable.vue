<script setup>
import { ref } from 'vue'
import { formatNumber, formatDateTime } from '@/utils/format'
import QrDialog from '@/components/QrDialog.vue'

defineProps({
  items: { type: Array, default: () => [] },
  loading: { type: Boolean, default: false }
})
const emit = defineEmits(['add', 'edit'])

const headers = [
  { title: 'Name', key: 'name' },
  { title: 'Barcode', key: 'barcode' },
  { title: 'Colour', key: 'colorCode' },
  { title: 'On hand', key: 'onHand', align: 'end' },
  { title: 'Reorder', key: 'reorderLevel', align: 'end' },
  { title: 'Updated', key: 'updatedAt' },
  { title: '', key: 'actions', sortable: false, align: 'end' }
]

const qr = ref({ open: false, barcode: null })

function showQr(barcode) {
  qr.value = { open: true, barcode }
}

const rowProps = ({ item }) => ({ class: item.isLowStock ? 'bg-red-lighten-5' : '' })
</script>

<template>
  <v-card>
    <v-toolbar color="surface" flat>
      <v-toolbar-title>Inventory</v-toolbar-title>
      <v-spacer />
      <v-btn color="secondary"
             variant="text"
             prepend-icon="mdi-file-excel"
             href="/api/inventory/export">
        Export
      </v-btn>
      <v-btn color="primary" prepend-icon="mdi-plus" class="ml-2" @click="emit('add')">
        Add item
      </v-btn>
    </v-toolbar>

    <v-data-table :headers="headers"
                  :items="items"
                  :loading="loading"
                  :row-props="rowProps"
                  density="comfortable">
      <template #[`item.onHand`]="{ item }">
        {{ formatNumber(item.onHand) }} {{ item.unit }}
        <v-icon v-if="item.isLowStock" icon="mdi-alert" color="error" size="small" class="ml-1" />
      </template>

      <template #[`item.reorderLevel`]="{ item }">
        {{ item.reorderLevel != null ? formatNumber(item.reorderLevel) : '—' }}
      </template>

      <template #[`item.updatedAt`]="{ item }">
        {{ formatDateTime(item.updatedAt) }}
      </template>

      <template #[`item.actions`]="{ item }">
        <v-btn icon="mdi-qrcode" variant="text" size="small" @click="showQr(item.barcode)" />
        <v-btn icon="mdi-pencil" variant="text" size="small" @click="emit('edit', item)" />
      </template>
    </v-data-table>

    <QrDialog v-model="qr.open" :barcode="qr.barcode" />
  </v-card>
</template>
