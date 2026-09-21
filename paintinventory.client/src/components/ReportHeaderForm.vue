<script setup>
defineProps({
  model: { type: Object, required: true }
})
</script>

<template>
  <v-card class="mb-4">
    <v-card-title class="text-subtitle-1">Report</v-card-title>
    <v-card-text>
      <v-row density="compact">
        <v-col cols="12" sm="3"><v-text-field v-model="model.ipo" label="IPO" variant="outlined" density="comfortable" /></v-col>
        <v-col cols="12" sm="3"><v-text-field v-model="model.customer" label="Customer" variant="outlined" density="comfortable" /></v-col>
        <v-col cols="12" sm="3"><v-text-field v-model="model.project" label="Project" variant="outlined" density="comfortable" /></v-col>
        <v-col cols="6" sm="2"><v-text-field v-model="model.preparedBy" label="Prepared by" variant="outlined" density="comfortable" /></v-col>
        <v-col cols="6" sm="1"><v-text-field v-model="model.preparedDate" label="Date" type="date" variant="outlined" density="comfortable" /></v-col>
      </v-row>
    </v-card-text>
  </v-card>
</template>
