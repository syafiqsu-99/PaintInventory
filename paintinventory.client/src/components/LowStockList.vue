<script setup>
import { formatNumber } from '@/utils/format'

defineProps({
    items: { type: Array, default: () => [] }
})
</script>

<template>
  <v-card>
    <v-card-title class="text-subtitle-1">Low stock</v-card-title>
    <v-list v-if="items.length" density="compact">
      <v-list-item v-for="i in items"
                   :key="i.balanceId"
                   :title="`${i.productName} · ${i.vendorName}`"
                   :subtitle="`${formatNumber(i.onHandQty)} ${i.unit || ''} — reorder at ${formatNumber(i.reorderLevel)}`">
        <template #prepend>
          <v-icon icon="mdi-alert" color="error" />
        </template>
      </v-list-item>
    </v-list>
    <v-card-text v-else class="text-medium-emphasis">
      Nothing below reorder level.
    </v-card-text>
  </v-card>
</template>
