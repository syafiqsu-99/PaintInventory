<script setup>
import { formatNumber } from '@/utils/format'

defineProps({
  item: { type: Object, required: true }
})
</script>

<template>
  <v-card variant="tonal" color="primary">
    <v-card-text>
      <div class="d-flex justify-space-between align-center mb-1">
        <span class="text-subtitle-1 font-weight-medium">{{ item.name || 'Unnamed' }}</span>
        <v-chip v-if="item.isLowStock" color="error" size="small" label>Low stock</v-chip>
      </div>

      <div class="text-body-2">Barcode: {{ item.barcode }}</div>
      <div class="text-body-2" v-if="item.sku">SKU: {{ item.sku }}</div>
      <div class="text-body-2" v-if="item.colorCode">Colour: {{ item.colorCode }}</div>

      <div class="text-h5 mt-3">
        {{ formatNumber(item.onHand) }} <span class="text-body-2">{{ item.unit }}</span>
        <span class="text-body-2 text-medium-emphasis"> on hand</span>
      </div>
      <div class="text-caption" v-if="item.reorderLevel != null">
        Reorder level: {{ formatNumber(item.reorderLevel) }} {{ item.unit }}
      </div>
    </v-card-text>
  </v-card>
</template>
