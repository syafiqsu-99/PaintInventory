<script setup>
import { formatNumber } from '@/utils/format'

defineProps({
  title: { type: String, required: true },
  value: { type: [Number, String], default: 0 },
  icon: { type: String, default: 'mdi-information' },
  color: { type: String, default: 'primary' }
})
</script>

<template>
  <v-card>
    <v-card-text class="d-flex align-center">
      <v-avatar :color="color" size="48" class="mr-4">
        <v-icon :icon="icon" color="white" />
      </v-avatar>
      <div>
        <div class="text-caption text-medium-emphasis">{{ title }}</div>
        <div class="text-h5">{{ formatNumber(value) }}</div>
      </div>
    </v-card-text>
  </v-card>
</template>
