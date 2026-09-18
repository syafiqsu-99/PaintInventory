<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  item: { type: Object, required: true }
})
const emit = defineEmits(['confirm', 'cancel'])

const action = ref('Use')
const quantity = ref(1)
const unit = ref(props.item.unit || '')
const location = ref('')
const notes = ref('')
const operator = ref('')
const valid = ref(false)

const actions = ['Use', 'Receive', 'Adjust']

const quantityLabel = computed(() =>
  action.value === 'Adjust' ? 'Counted quantity (new on-hand)' : 'Quantity'
)

const rules = {
  required: (v) => (v !== null && v !== '' && v !== undefined) || 'Required',
  nonNegative: (v) => (Number(v) >= 0) || 'Must be 0 or greater'
}

function submit() {
  if (!valid.value) return
  emit('confirm', {
    Action: action.value,
    Quantity: Number(quantity.value),
    Unit: unit.value || null,
    Location: location.value || null,
    Notes: notes.value || null,
    Operator: operator.value || null,
    DeviceId: navigator.userAgent
  })
}
</script>

<template>
  <v-form v-model="valid" @submit.prevent="submit">
    <v-select v-model="action" :items="actions" label="Action" />

    <v-text-field v-model.number="quantity"
                  :label="quantityLabel"
                  type="number"
                  min="0"
                  :rules="[rules.required, rules.nonNegative]" />

    <v-row dense>
      <v-col cols="6">
        <v-text-field v-model="unit" label="Unit" />
      </v-col>
      <v-col cols="6">
        <v-text-field v-model="location" label="Location" />
      </v-col>
    </v-row>

    <v-text-field v-model="operator" label="Operator (optional)" />
    <v-text-field v-model="notes" label="Notes (optional)" />

    <div class="d-flex ga-2 mt-2">
      <v-btn type="submit" color="primary" :disabled="!valid" prepend-icon="mdi-check">
        Confirm
      </v-btn>
      <v-btn variant="text" @click="emit('cancel')">Cancel</v-btn>
    </div>
  </v-form>
</template>
