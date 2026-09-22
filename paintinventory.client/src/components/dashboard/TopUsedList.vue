<template>
  <v-card>
    <v-card-title class="text-subtitle-1">Most used — last 30 days</v-card-title>
    <v-divider />
    <v-list v-if="items.length" density="comfortable">
      <v-list-item v-for="(row, i) in items" :key="i">
        <v-list-item-title class="text-body-2 d-flex justify-space-between">
          <span>{{ row.productName }}</span>
          <span class="font-weight-medium">{{ row.quantity }}</span>
        </v-list-item-title>
        <v-progress-linear :model-value="pct(row.quantity)" color="primary" height="6" rounded class="mt-1" />
      </v-list-item>
    </v-list>
    <v-card-text v-else class="text-center text-medium-emphasis py-8">
      No usage recorded in the last 30 days.
    </v-card-text>
  </v-card>
</template>

<script setup>
  import { computed } from 'vue'

  const props = defineProps({
    items: { type: Array, default: () => [] }
  })

  const max = computed(() => Math.max(1, ...props.items.map((i) => i.quantity)))
  const pct = (q) => (q / max.value) * 100
</script>
