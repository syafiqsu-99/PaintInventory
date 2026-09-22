<template>
  <v-card>
    <v-card-title class="text-subtitle-1 d-flex align-center">
      <v-icon icon="mdi-clock-alert-outline" size="20" class="me-2" color="warning" />
      <span>Reaching expiry</span>
      <v-spacer />
      <v-chip v-if="items.length" size="small" color="warning" variant="tonal">{{ items.length }}</v-chip>
    </v-card-title>
    <v-divider />
    <v-list v-if="items.length" density="comfortable" lines="two">
      <v-list-item v-for="(lot, i) in items" :key="i">
        <v-list-item-title class="text-body-2 font-weight-medium">{{ lot.productName }}</v-list-item-title>
        <v-list-item-subtitle>
          {{ lot.vendorName }}<span v-if="lot.batch"> · batch {{ lot.batch }}</span> · {{ lot.quantity }} on hand
        </v-list-item-subtitle>
        <template #append>
          <div class="text-end">
            <v-chip :color="urgency(lot.daysToExpiry)" size="small" label variant="flat">
              {{ label(lot.daysToExpiry) }}
            </v-chip>
            <div class="text-caption text-medium-emphasis mt-1">{{ date(lot.bestBefore) }}</div>
          </div>
        </template>
      </v-list-item>
    </v-list>
    <v-card-text v-else class="text-center text-medium-emphasis py-8">
      No stock reaching expiry in this window.
    </v-card-text>
  </v-card>
</template>

<script setup>
  defineProps({
    items: { type: Array, default: () => [] }
  })

  function urgency(days) {
    if (days <= 0) return 'error'
    if (days <= 14) return 'error'
    if (days <= 30) return 'warning'
    return 'secondary'
  }

  function label(days) {
    if (days < 0) return `${Math.abs(days)}d overdue`
    if (days === 0) return 'Today'
    return `${days}d left`
  }

  function date(s) {
    return s ? new Date(s).toLocaleDateString() : ''
  }
</script>
