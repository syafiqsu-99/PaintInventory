<template>
  <v-card class="mb-4">
    <v-card-title class="text-subtitle-1 d-flex align-center">
      <v-icon icon="mdi-truck-delivery-outline" class="me-2" />
      <span>Incoming</span>
      <v-spacer />
      <v-chip v-if="items.length" size="small" color="warning" variant="flat">{{ items.length }}</v-chip>
      <v-btn icon="mdi-refresh" variant="text" :loading="loading" title="Refresh" @click="emit('refresh')" />
    </v-card-title>
    <v-divider />
    <v-list v-if="items.length" lines="two">
      <v-list-item v-for="t in items"
                   :key="t.transactionId"
                   min-height="64"
                   @click="emit('select', t)">
        <v-list-item-title class="text-body-1 font-weight-medium text-wrap">
          {{ t.productName }}
          <span v-if="t.component !== 'Single'" class="text-medium-emphasis">({{ t.component === 'PartA' ? 'A' : 'B' }})</span>
        </v-list-item-title>
        <v-list-item-subtitle class="text-body-2">
          {{ t.quantity }} cans · from {{ t.fromVendorName }}<span v-if="t.batch"> · {{ t.batch }}</span>
        </v-list-item-subtitle>
        <template #append>
          <div class="d-flex flex-column align-end">
            <span class="text-caption text-medium-emphasis">{{ formatDisplayDate(t.sentAt?.slice(0, 10)) }}</span>
            <v-icon icon="mdi-chevron-right" />
          </div>
        </template>
      </v-list-item>
    </v-list>
    <v-card-text v-else class="text-center text-body-2 text-medium-emphasis py-6">
      No paint on the way to this site.
    </v-card-text>
  </v-card>
</template>

<script setup>
  import { formatDisplayDate } from '@/utils/scanParse'

  defineProps({
    items: { type: Array, default: () => [] },
    loading: { type: Boolean, default: false }
  })
  const emit = defineEmits(['select', 'refresh'])
</script>
