<template>
  <v-dialog :model-value="modelValue" max-width="440" @update:model-value="emit('update:modelValue', $event)">
    <v-card>
      <v-card-title class="text-subtitle-1">Reorder level</v-card-title>
      <v-card-subtitle v-if="product">{{ product.productName }}</v-card-subtitle>
      <v-card-text>
        <v-form v-model="valid">
          <v-select v-model="form.vendorId" :items="locations" label="Location" :rules="[rules.required]" variant="outlined" density="comfortable" />
          <v-text-field v-model.number="form.reorderLevel" label="Reorder at (cans)" type="number" :rules="[rules.required]" variant="outlined" density="comfortable" />
        </v-form>
      </v-card-text>
      <v-card-actions>
        <v-spacer />
        <v-btn variant="text" @click="emit('update:modelValue', false)">Cancel</v-btn>
        <v-btn color="primary" :loading="saving" :disabled="!valid" @click="save">Save</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
  import { ref, watch, onMounted, computed } from 'vue'
  import { storeToRefs } from 'pinia'
  import { useVendorStore } from '@/store/vendor'
  import { useInventoryStore } from '@/store/inventory'
  import { useUiStore } from '@/store/ui'

  const props = defineProps({
        modelValue: { type: Boolean, default: false },
        product: { type: Object, default: null }
  })
  const emit = defineEmits(['update:modelValue', 'saved'])

  const vendorStore = useVendorStore()
  const inventory = useInventoryStore()
  const ui = useUiStore()
  const { vendors } = storeToRefs(vendorStore)

  const form = ref({ vendorId: null, reorderLevel: null })
  const saving = ref(false)
  const valid = ref(false)

  const locations = computed(() => vendors.value.filter((v) => v.storesStock).map((v) => ({ title: v.name, value: v.id })))

  watch(() => props.modelValue, (open) => {
        if (open) form.value = { vendorId: null, reorderLevel: null }
  })

  onMounted(() => { if (!vendors.value.length) vendorStore.load() })

  const rules = { required: (v) => (v !== null && v !== undefined && String(v).trim() !== '') || 'Required' }

  async function save() {
        if (!valid.value || !props.product) return
        saving.value = true
        try {
          await inventory.setReorder({
            productId: props.product.id,
            vendorId: form.value.vendorId,
            reorderLevel: form.value.reorderLevel === '' ? null : form.value.reorderLevel
          })
          ui.notify('Reorder level saved.')
          emit('saved')
          emit('update:modelValue', false)
        } catch (e) {
          ui.error(e.message)
        } finally {
          saving.value = false
        }
  }
</script>
