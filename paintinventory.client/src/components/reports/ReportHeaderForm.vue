<template>
  <v-card class="mb-4">
    <v-card-title class="text-subtitle-1">Report</v-card-title>
    <v-card-text>
      <v-row dense>
        <v-col cols="12" sm="4"><v-text-field v-model="model.ipo" label="IPO" variant="outlined" density="comfortable" /></v-col>
        <v-col cols="12" sm="4"><v-text-field v-model="model.customer" label="Customer" variant="outlined" density="comfortable" /></v-col>
        <v-col cols="12" sm="4"><v-text-field v-model="model.project" label="Project" variant="outlined" density="comfortable" /></v-col>
        <v-col cols="12" sm="6"><v-text-field v-model="model.preparedBy" label="Prepared by" variant="outlined" density="comfortable" /></v-col>
        <v-col cols="12" sm="6"><ScanDateField v-model="model.preparedDate" label="Date" scan-key="preparedDate" /></v-col>
      </v-row>
    </v-card-text>
  </v-card>
</template>

<script setup>
  import ScanDateField from '@/components/common/ScanDateField.vue'

  defineProps({
      model: { type: Object, required: true }
  })
</script>
