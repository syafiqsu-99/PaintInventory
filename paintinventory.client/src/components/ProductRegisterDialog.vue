<script setup>
import { ref, watch } from 'vue'
import { useProductStore } from '@/store/product'
import { useUiStore } from '@/store/ui'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  gtin: { type: String, default: '' }
})
const emit = defineEmits(['update:modelValue', 'registered'])

const products = useProductStore()
const ui = useUiStore()

const componentOptions = [
  { title: 'Single component', value: 'Single' },
  { title: 'Part A (base)', value: 'PartA' },
  { title: 'Part B (hardener)', value: 'PartB' }
]

const form = ref(blank())
const saving = ref(false)
const valid = ref(false)

function blank() {
  return {
    gtin: '', itemCode: null, productName: '', description: null,
    component: 'Single', packVolume: null, unit: 'L', defaultShade: null,
    ralCode: null, manufacturer: null, mixRatio: null, partnerProductId: null,
    unNumber: null, hazardFlags: null, tracksExpiry: false
  }
}

watch(() => props.modelValue, (open) => {
  if (open) {
    form.value = blank()
    form.value.gtin = props.gtin
  }
})

const rules = { required: (v) => (!!v && String(v).trim() !== '') || 'Required' }

async function save() {
  if (!valid.value) return
  saving.value = true
  try {
    const payload = { ...form.value }
    Object.keys(payload).forEach((k) => { if (payload[k] === '') payload[k] = null })
    const created = await products.create(payload)
    ui.notify(`Registered ${created.productName}.`)
    emit('registered', created)
    emit('update:modelValue', false)
  } catch (e) {
    ui.error(e.message)
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <v-dialog :model-value="modelValue" max-width="640" @update:model-value="emit('update:modelValue', $event)">
    <v-card>
      <v-card-title>Register new product</v-card-title>
      <v-card-text>
        <v-form v-model="valid">
          <v-row density="compact">
            <v-col cols="12" sm="6">
              <v-text-field v-model="form.gtin" label="GTIN" variant="outlined" density="comfortable" readonly />
            </v-col>
            <v-col cols="12" sm="6">
              <v-text-field v-model="form.itemCode" label="Item code" variant="outlined" density="comfortable" />
            </v-col>
            <v-col cols="12" sm="6">
              <v-text-field v-model="form.productName" label="Product name" :rules="[rules.required]" variant="outlined" density="comfortable" />
            </v-col>
            <v-col cols="12" sm="6">
              <v-select v-model="form.component" :items="componentOptions" label="Component" variant="outlined" density="comfortable" />
            </v-col>
            <v-col cols="6" sm="3">
              <v-text-field v-model.number="form.packVolume" label="Pack volume" type="number" variant="outlined" density="comfortable" />
            </v-col>
            <v-col cols="6" sm="3">
              <v-text-field v-model="form.unit" label="Unit" variant="outlined" density="comfortable" />
            </v-col>
            <v-col cols="12" sm="6">
              <v-text-field v-model="form.defaultShade" label="Shade / colour" variant="outlined" density="comfortable" />
            </v-col>
            <v-col cols="12" sm="6">
              <v-text-field v-model="form.ralCode" label="RAL code" variant="outlined" density="comfortable" />
            </v-col>
            <v-col cols="12" sm="6">
              <v-text-field v-model="form.manufacturer" label="Manufacturer" variant="outlined" density="comfortable" />
            </v-col>
            <v-col cols="6" sm="3">
              <v-text-field v-model="form.mixRatio" label="Mix ratio A:B" variant="outlined" density="comfortable" />
            </v-col>
            <v-col cols="6" sm="3">
              <v-text-field v-model="form.unNumber" label="UN number" variant="outlined" density="comfortable" />
            </v-col>
            <v-col cols="12">
              <v-checkbox v-model="form.tracksExpiry" label="Track best-before / shelf life" density="comfortable" hide-details />
            </v-col>
          </v-row>
        </v-form>
      </v-card-text>
      <v-card-actions>
        <v-spacer />
        <v-btn variant="text" @click="emit('update:modelValue', false)">Cancel</v-btn>
        <v-btn color="primary" :loading="saving" :disabled="!valid" @click="save">Register</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>
