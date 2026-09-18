<script setup>
import { ref, watch } from 'vue'
import { useInventoryStore } from '@/store/inventory'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  item: { type: Object, default: null }
})
const emit = defineEmits(['update:modelValue', 'saved'])

const store = useInventoryStore()

const blank = () => ({
  barcode: '', name: '', sku: '', colorCode: '',
  volume: null, unit: '', batch: '', manufacturer: '', reorderLevel: null
})

const form = ref(blank())
const valid = ref(false)
const saving = ref(false)
const error = ref(null)

const rules = {
  required: (v) => (!!v && String(v).trim() !== '') || 'Required'
}

watch(
  () => props.modelValue,
  async (open) => {
    if (!open) return
    error.value = null
    if (props.item) {
      const full = await store.lookup(props.item.barcode)
      form.value = {
        barcode: full.barcode, name: full.name ?? '', sku: full.sku ?? '',
        colorCode: full.colorCode ?? '', volume: full.volume, unit: full.unit ?? '',
        batch: full.batch ?? '', manufacturer: full.manufacturer ?? '',
        reorderLevel: full.reorderLevel
      }
    } else {
      form.value = blank()
    }
  }
)

function close() {
  emit('update:modelValue', false)
}

async function save() {
  if (!valid.value) return
  saving.value = true
  error.value = null

  const payload = {
    Barcode: form.value.barcode,
    Name: form.value.name || null,
    Sku: form.value.sku || null,
    ColorCode: form.value.colorCode || null,
    Volume: form.value.volume,
    Unit: form.value.unit || null,
    Batch: form.value.batch || null,
    Manufacturer: form.value.manufacturer || null,
    ReorderLevel: form.value.reorderLevel
  }

  try {
    if (props.item) {
      await store.updateItem(props.item.id, payload)
    } else {
      await store.createItem(payload)
    }
    emit('saved')
  } catch (err) {
    error.value = err.message
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <v-dialog :model-value="modelValue" max-width="560" @update:model-value="emit('update:modelValue', $event)">
    <v-card>
      <v-card-title>{{ item ? 'Edit item' : 'Add item' }}</v-card-title>
      <v-card-text>
        <v-alert v-if="error" type="error" variant="tonal" density="compact" class="mb-3">
          {{ error }}
        </v-alert>

        <v-form v-model="valid" @submit.prevent="save">
          <v-text-field v-model="form.barcode"
                        label="Barcode"
                        :rules="[rules.required]"
                        :disabled="!!item" />
          <v-row dense>
            <v-col cols="12" sm="6"><v-text-field v-model="form.name" label="Name" /></v-col>
            <v-col cols="12" sm="6"><v-text-field v-model="form.sku" label="SKU" /></v-col>
            <v-col cols="12" sm="6"><v-text-field v-model="form.colorCode" label="Colour code" /></v-col>
            <v-col cols="12" sm="6"><v-text-field v-model="form.manufacturer" label="Manufacturer" /></v-col>
            <v-col cols="6" sm="4"><v-text-field v-model.number="form.volume" label="Volume" type="number" /></v-col>
            <v-col cols="6" sm="4"><v-text-field v-model="form.unit" label="Unit" /></v-col>
            <v-col cols="6" sm="4"><v-text-field v-model.number="form.reorderLevel" label="Reorder level" type="number" /></v-col>
            <v-col cols="6" sm="4"><v-text-field v-model="form.batch" label="Batch" /></v-col>
          </v-row>
        </v-form>
      </v-card-text>
      <v-card-actions>
        <v-spacer />
        <v-btn variant="text" @click="close">Cancel</v-btn>
        <v-btn color="primary" :loading="saving" :disabled="!valid" @click="save">Save</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>
