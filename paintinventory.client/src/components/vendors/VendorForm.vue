<template>
  <v-dialog :model-value="modelValue" :fullscreen="xs" max-width="520" scrollable @update:model-value="emit('update:modelValue', $event)">
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

        <template v-if="vendor">
          <v-divider class="my-4" />
          <div class="text-subtitle-2 mb-1">Site access code</div>
          <p class="text-body-2 text-medium-emphasis mb-3">
            {{ form.isOwnCompany ? 'Own-company sites sign in with full staff access.' : 'Vendor sites can only receive transfers and record usage at this location.' }}
            <span v-if="!form.isOwnCompany && !form.storesStock"> Tick “Stores stock” so transfers can be received here.</span>
          </p>

          <v-alert v-if="issuedCode" type="success" variant="tonal" density="comfortable" class="mb-3">
            <div class="text-body-2">New code — shown once, share it with the site:</div>
            <div class="d-flex align-center">
              <span class="text-h5 font-weight-bold me-2 code">{{ issuedCode }}</span>
              <v-btn icon="mdi-content-copy" variant="text" title="Copy" @click="copyCode" />
            </div>
          </v-alert>

          <div class="d-flex flex-wrap ga-2">
            <v-btn color="primary" variant="tonal" prepend-icon="mdi-key-variant" :loading="codeBusy" @click="issueCode">
              {{ vendor.hasAccessCode ? 'Reset code' : 'Create code' }}
            </v-btn>
            <v-btn v-if="vendor.hasAccessCode && !revoked" color="error" variant="text" :loading="codeBusy" @click="revokeCode">
              Revoke
            </v-btn>
          </div>
        </template>
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
    import { useDisplay } from 'vuetify'
    import { useVendorStore } from '@/store/vendor'
    import { useUiStore } from '@/store/ui'

    const props = defineProps({
        modelValue: { type: Boolean, default: false },
        vendor: { type: Object, default: null }
    })
    const emit = defineEmits(['update:modelValue', 'saved'])

    const vendorStore = useVendorStore()
    const ui = useUiStore()

    const { xs } = useDisplay()

    const form = ref(blank())
    const saving = ref(false)
    const valid = ref(false)
    const issuedCode = ref('')
    const codeBusy = ref(false)
    const revoked = ref(false)

    function blank() {
        return { name: '', isOwnCompany: false, storesStock: true, doesBlasting: false, doesPainting: false }
    }

    watch(() => props.modelValue, (open) => {
        if (!open) return
        form.value = props.vendor ? { ...props.vendor } : blank()
        issuedCode.value = ''
        revoked.value = false
    })

    async function issueCode() {
        codeBusy.value = true
        try {
          const result = await vendorStore.setAccessCode(props.vendor.id)
          issuedCode.value = result.accessCode
          revoked.value = false
          emit('saved')
        } catch (e) {
          ui.error(e.message)
        } finally {
          codeBusy.value = false
        }
    }

    async function revokeCode() {
        codeBusy.value = true
        try {
          await vendorStore.revokeAccessCode(props.vendor.id)
          issuedCode.value = ''
          revoked.value = true
          ui.notify('Access code revoked.')
          emit('saved')
        } catch (e) {
          ui.error(e.message)
        } finally {
          codeBusy.value = false
        }
    }

    async function copyCode() {
        try {
          await navigator.clipboard.writeText(issuedCode.value)
          ui.notify('Code copied.')
        } catch {
          ui.error('Copy not available — note the code manually.')
        }
    }

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

<style scoped>
  .code {
    letter-spacing: 0.15em;
    font-variant-numeric: tabular-nums;
  }
</style>
