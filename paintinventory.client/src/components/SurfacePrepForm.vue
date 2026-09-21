<script setup>
import EnvironmentFields from '@/components/EnvironmentFields.vue'

defineProps({
  model: { type: Object, required: true }
})
</script>

<template>
  <div>
    <v-row density="compact">
      <v-col cols="12" sm="4">
        <v-text-field v-model="model.gradeOfCleanliness" label="Grade of cleanliness" variant="outlined" density="comfortable" />
      </v-col>
      <v-col cols="6" sm="4">
        <v-text-field v-model="model.requiredRoughness" label="Req'd roughness" variant="outlined" density="comfortable" />
      </v-col>
      <v-col cols="6" sm="4">
        <v-text-field v-model.number="model.measuredRoughness" label="Measured roughness" type="number" variant="outlined" density="comfortable" />
      </v-col>
    </v-row>
    <EnvironmentFields :model="model" />
    <v-row density="compact">
      <v-col cols="6"><v-text-field v-model="model.operator" label="Operator" variant="outlined" density="comfortable" /></v-col>
      <v-col cols="6"><v-text-field v-model="model.date" label="Date" type="date" variant="outlined" density="comfortable" /></v-col>
    </v-row>
  </div>
</template>
