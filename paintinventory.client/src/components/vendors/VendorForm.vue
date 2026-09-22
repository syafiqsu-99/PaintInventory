<template>
  <v-dialog :model-value="modelValue" max-width="520" @update:model-value="emit('update:modelValue', $event)">
    <v-card>
      <v-card-title>{{ vendor ? 'Edit location / vendor' : 'Add location / vendor' }}</v-card-title>
      <v-card-text>
        <v-form v-model="valid">
          <v-text-field v-model="form.name" label="Name" :rules="[rules.required]" variant="outlined" density="comfortable" />
          <v-checkbox v-model="form.isOwnCompany" label="Own company" density="comfortable" hide-details />
          <v-checkbox v-model="form.storesStock" label="Stores stock (a location)" density="comfortable" hide-details />
          <v-checkbox v-model="form.doesBlasting" label="Does blasting" density="comfortable" hide-details />
          <v-checkbox v-model="form.doesPainting" label="Does painting" density="comfortable" hide-details />
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
    import { ref, watch } from 'vue'
    import { useVendorStore } from '@/store/vendor'
    import { useUiStore } from '@/store/ui'

    const props = defineProps({
        modelValue: { type: Boolean, default: false },
        vendor: { type: Object, default: null }
    })
    const emit = defineEmits(['update:modelValue', 'saved'])

    const vendorStore = useVendorStore()
    const ui = useUiStore()

    const form = ref(blank())
    const saving = ref(false)
    const valid = ref(false)

    function blank() {
        return { name: '', isOwnCompany: false, storesStock: true, doesBlasting: false, doesPainting: false }
    }

    watch(() => props.modelValue, (open) => {
        if (open) form.value = props.vendor ? { ...props.vendor } : blank()
    })

    const rules = { required: (v) => (!!v && v.trim() !== '') || 'Required' }

    async function save() {
        if (!valid.value) return
        saving.value = true
        try {
          if (props.vendor) await vendorStore.update(props.vendor.id, form.value)
          else await vendorStore.create(form.value)
          ui.notify('Location saved.')
          emit('saved')
          emit('update:modelValue', false)
        } catch (e) {
          ui.error(e.message)
        } finally {
          saving.value = false
        }
    }
</script>
