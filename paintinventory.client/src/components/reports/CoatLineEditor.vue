<template>
  <v-card variant="outlined" class="mb-3">
    <v-card-title class="d-flex align-center text-subtitle-2">
      {{ coatLabel(model.sequence) }}
      <v-spacer />
      <v-btn size="small" variant="text" icon="mdi-delete" @click="emit('remove')" />
    </v-card-title>
    <v-card-text>
      <v-row dense>
        <v-col cols="12" sm="6">
          <v-select v-model="model.partAProductId" :items="productOptions" label="Part A (paint)" clearable variant="outlined" density="comfortable" />
        </v-col>
        <v-col cols="6" sm="3">
          <v-text-field v-model="model.partABatch" label="Part A batch" variant="outlined" density="comfortable" />
        </v-col>
        <v-col cols="6" sm="3">
          <v-text-field v-model="model.shade" label="Shade" variant="outlined" density="comfortable" />
        </v-col>
        <v-col cols="12" sm="6">
          <v-select v-model="model.partBProductId" :items="productOptions" label="Part B (hardener)" clearable variant="outlined" density="comfortable" />
        </v-col>
        <v-col cols="6" sm="3">
          <v-text-field v-model="model.partBBatch" label="Part B batch" variant="outlined" density="comfortable" />
        </v-col>
        <v-col cols="6" sm="3">
          <v-text-field v-model="model.paintIdText" label="Paint ID (if not listed)" variant="outlined" density="comfortable" />
        </v-col>
        <v-col cols="6" sm="3">
          <v-text-field v-model.number="model.requiredThicknessUm" label="Req'd thk µm" type="number" variant="outlined" density="comfortable" />
        </v-col>
        <v-col cols="6" sm="3">
          <v-text-field v-model.number="model.measuredThicknessUm" label="Measured thk µm" type="number" variant="outlined" density="comfortable" />
        </v-col>
        <v-col cols="6" sm="3">
          <v-text-field v-model="model.operator" label="Operator" variant="outlined" density="comfortable" />
        </v-col>
        <v-col cols="6" sm="3">
          <v-text-field v-model="model.date" label="Date" type="date" variant="outlined" density="comfortable" />
        </v-col>
      </v-row>

      <EnvironmentFields :model="model" />

      <v-divider class="my-2" />
      <v-switch v-model="model.deductFromStock" label="Deduct consumed paint from stock" color="primary" density="comfortable" hide-details />
      <v-row v-if="model.deductFromStock" dense class="mt-1">
        <v-col cols="12" sm="4">
          <v-select v-model="model.stockLocationVendorId" :items="stockLocations" label="Stock location" variant="outlined" density="comfortable" />
        </v-col>
        <v-col cols="6" sm="4">
          <v-text-field v-model.number="model.partAQtyUsed" label="Part A used (cans)" type="number" variant="outlined" density="comfortable" />
        </v-col>
        <v-col cols="6" sm="4">
          <v-text-field v-model.number="model.partBQtyUsed" label="Part B used (cans)" type="number" variant="outlined" density="comfortable" />
        </v-col>
      </v-row>
    </v-card-text>
  </v-card>
</template>

<script setup>
  import EnvironmentFields from '@/components/reports/EnvironmentFields.vue'
  import { coatLabel } from '@/utils/reportModel'

  defineProps({
      model: { type: Object, required: true },
      productOptions: { type: Array, default: () => [] },
      stockLocations: { type: Array, default: () => [] }
  })
  const emit = defineEmits(['remove'])
</script>
