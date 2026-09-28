<template>
  <v-card max-width="640">
    <v-card-title class="text-subtitle-1">Staff password</v-card-title>
    <v-divider />
    <v-card-text>
      <p class="text-body-2 text-medium-emphasis mb-4">
        One shared password unlocks stock, reports and settings. Anyone can still scan products without it.
        Once set here, the <code>Auth__StaffPassword</code> environment variable is no longer used.
      </p>
      <v-form ref="formRef" v-model="valid" @submit.prevent="save">
        <v-text-field v-model="current"
                      label="Current password"
                      type="password"
                      autocomplete="current-password"
                      :rules="[rules.required]"
                      variant="outlined"
                      class="mb-2" />
        <v-text-field v-model="next"
                      label="New password"
                      type="password"
                      autocomplete="new-password"
                      :rules="[rules.required, rules.min]"
                      variant="outlined"
                      class="mb-2" />
        <v-text-field v-model="confirm"
                      label="Confirm new password"
                      type="password"
                      autocomplete="new-password"
                      :rules="[rules.required, rules.match]"
                      variant="outlined"
                      class="mb-2" />
        <v-btn type="submit" color="primary" size="large" :loading="saving" :disabled="!valid">Change password</v-btn>
      </v-form>
    </v-card-text>
  </v-card>
</template>

<script setup>
  import { ref } from 'vue'
  import { useAuthStore } from '@/store/auth'
  import { useUiStore } from '@/store/ui'

  const auth = useAuthStore()
  const ui = useUiStore()

  const formRef = ref(null)
  const valid = ref(false)
  const saving = ref(false)
  const current = ref('')
  const next = ref('')
  const confirm = ref('')

  const rules = {
    required: (v) => !!v || 'Required',
    min: (v) => (v?.length ?? 0) >= 6 || 'At least 6 characters',
    match: (v) => v === next.value || 'Passwords do not match'
  }

  async function save() {
    if (!valid.value) return
    saving.value = true
    try {
      await auth.changePassword(current.value, next.value)
      ui.notify('Staff password changed.')
      formRef.value?.reset()
    } catch (e) {
      ui.error(e.message)
    } finally {
      saving.value = false
    }
  }
</script>
