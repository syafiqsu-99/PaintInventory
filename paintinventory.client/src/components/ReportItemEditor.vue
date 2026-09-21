<script setup>
import SurfacePrepForm from '@/components/SurfacePrepForm.vue'
import CoatLineEditor from '@/components/CoatLineEditor.vue'
import { blankCoat, coatTypeFor } from '@/utils/reportModel'

const props = defineProps({
  model: { type: Object, required: true },
  productOptions: { type: Array, default: () => [] },
  blastVendors: { type: Array, default: () => [] },
  paintingVendors: { type: Array, default: () => [] },
  stockLocations: { type: Array, default: () => [] }
})
const emit = defineEmits(['remove'])

const adhesionTypes = [
  { title: 'None', value: 'None' },
  { title: 'Test plate', value: 'TestPlate' },
  { title: 'Production part', value: 'ProductionPart' }
]

function addCoat() {
  if (props.model.coats.length >= 4) return
  props.model.coats.push(blankCoat(props.model.coats.length + 1))
}

function removeCoat(i) {
  props.model.coats.splice(i, 1)
  props.model.coats.forEach((c, idx) => {
    c.sequence = idx + 1
    c.coatType = coatTypeFor(idx + 1)
  })
}
</script>

<template>
  <v-card class="mb-4">
    <v-card-title class="d-flex align-center">
      <span class="text-subtitle-1">Item {{ model.itemNo }}</span>
      <v-spacer />
      <v-btn size="small" variant="text" color="error" prepend-icon="mdi-delete" @click="emit('remove')">Remove item</v-btn>
    </v-card-title>
    <v-card-text>
      <v-row density="compact">
        <v-col cols="6" sm="2"><v-text-field v-model.number="model.itemNo" label="Item no" type="number" variant="outlined" density="comfortable" /></v-col>
        <v-col cols="6" sm="4"><v-text-field v-model="model.serialNumber" label="Serial number" variant="outlined" density="comfortable" /></v-col>
        <v-col cols="6" sm="3"><v-text-field v-model="model.paintingSpec" label="Painting spec" variant="outlined" density="comfortable" /></v-col>
        <v-col cols="6" sm="3"><v-text-field v-model="model.componentLabel" label="Label (BODY/ACTUATOR)" variant="outlined" density="comfortable" /></v-col>
        <v-col cols="12" sm="6"><v-text-field v-model="model.componentDescription" label="Component description" variant="outlined" density="comfortable" /></v-col>
        <v-col cols="6" sm="3"><v-select v-model="model.blastVendorId" :items="blastVendors" label="Blast vendor" clearable variant="outlined" density="comfortable" /></v-col>
        <v-col cols="6" sm="3"><v-select v-model="model.paintingVendorId" :items="paintingVendors" label="Painting vendor" clearable variant="outlined" density="comfortable" /></v-col>
      </v-row>

      <v-switch v-model="model.abrasiveBlasting" label="Abrasive blasting" color="primary" density="comfortable" hide-details class="my-2" />
      <template v-if="model.abrasiveBlasting">
        <div class="text-overline">Surface preparation</div>
        <SurfacePrepForm :model="model.surfacePrep" />
        <v-divider class="my-3" />
      </template>

      <div class="d-flex align-center mb-2">
        <div class="text-overline">Coats ({{ model.coats.length }}/4)</div>
        <v-spacer />
        <v-btn size="small" variant="tonal" prepend-icon="mdi-plus" :disabled="model.coats.length >= 4" @click="addCoat">Add coat</v-btn>
      </div>
      <CoatLineEditor v-for="(coat, i) in model.coats"
                      :key="i"
                      :model="coat"
                      :product-options="productOptions"
                      :stock-locations="stockLocations"
                      @remove="removeCoat(i)" />

      <v-divider class="my-3" />
      <v-row density="compact">
        <v-col cols="6" sm="3"><v-text-field v-model.number="model.requiredTotalDftUm" label="Req'd total DFT µm" type="number" variant="outlined" density="comfortable" /></v-col>
        <v-col cols="6" sm="3"><v-text-field v-model.number="model.measuredTotalDftUm" label="Measured total DFT µm" type="number" variant="outlined" density="comfortable" /></v-col>
        <v-col cols="12" sm="3"><v-select v-model="model.adhesionTestType" :items="adhesionTypes" label="Adhesion test" variant="outlined" density="comfortable" /></v-col>
        <v-col cols="12" sm="3"><v-switch v-model="model.adhesionTestPerformed" label="Adhesion performed" color="primary" density="comfortable" hide-details /></v-col>
        <v-col cols="12" sm="6"><v-text-field v-model="model.mekTestNotes" label="MEK test notes" variant="outlined" density="comfortable" /></v-col>
        <v-col cols="12" sm="6"><v-text-field v-model="model.otherRemarks" label="Other remarks" variant="outlined" density="comfortable" /></v-col>
      </v-row>
    </v-card-text>
  </v-card>
</template>
