<template>
  <v-card>
    <v-card-title class="text-subtitle-1 d-flex align-center">
      <span>This session</span>
      <v-spacer />
      <v-chip v-if="items.length" size="small" variant="tonal">{{ items.length }}</v-chip>
    </v-card-title>
    <v-divider />
    <v-list v-if="items.length" lines="two" density="comfortable">
      <v-list-item v-for="(s, i) in items" :key="i">
        <template #prepend>
          <v-avatar :color="isIn(s) ? 'success' : 'primary'" size="36" variant="tonal">
            <v-icon :icon="isIn(s) ? 'mdi-tray-arrow-down' : 'mdi-tray-arrow-up'" size="20" />
          </v-avatar>
        </template>
        <v-list-item-title class="text-body-1 font-weight-medium text-wrap">{{ s.productName }}</v-list-item-title>
        <v-list-item-subtitle class="text-body-2">
          {{ isIn(s) ? '+' : '−' }}{{ s.quantity }} · {{ s.vendorName }} · on hand {{ s.onHandQty }}
        </v-list-item-subtitle>
        <template #append>
          <span class="text-caption text-medium-emphasis">{{ time(s.time) }}</span>
        </template>
      </v-list-item>
    </v-list>
    <v-card-text v-else class="text-center text-medium-emphasis py-8">
      Scanned movements will appear here.
    </v-card-text>
  </v-card>
</template>

<script setup>
  defineProps({
    items: { type: Array, default: () => [] }
  })

  const isIn = (s) => s.direction === 'in' || s.direction === 'receive'

  function time(d) {
    return new Date(d).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  }
</script>
