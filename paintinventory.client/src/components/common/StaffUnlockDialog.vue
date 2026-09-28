<template>
  <v-dialog :model-value="unlockOpen" :fullscreen="xs" max-width="420" persistent @update:model-value="close">
    <v-card>
      <v-card-title class="d-flex align-center py-3">
        <v-icon icon="mdi-shield-lock-outline" class="me-2" color="primary" />
        <span class="text-subtitle-1">Staff access</span>
        <v-spacer />
        <v-btn icon="mdi-close" variant="text" aria-label="Close" @click="close" />
      </v-card-title>
      <v-divider />
      <v-card-text class="pt-4">
        <p class="text-body-1 text-medium-emphasis mb-4">
          Enter the staff password to open stock, reports and settings. Scanning stays available to everyone.
        </p>
        <v-alert v-if="!passwordConfigured" type="warning" variant="tonal" density="comfortable" class="mb-4">
          No staff password is set yet. Ask IT to set <code>Auth__StaffPassword</code> on the server.
        </v-alert>
        <v-form @submit.prevent="submit">
          <v-text-field v-model="password"
                        label="Password"
                        :type="show ? 'text' : 'password'"
                        :append-inner-icon="show ? 'mdi-eye-off' : 'mdi-eye'"
                        :error-messages="error ? [error] : []"
                        autocomplete="current-password"
                        variant="outlined"
                        autofocus
                        @click:append-inner="show = !show"
                        @update:model-value="error = ''" />
          <v-btn type="submit" color="primary" size="x-large" block :loading="busy" :disabled="!password" class="mt-2">
            Unlock
          </v-btn>
        </v-form>
      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<script setup>
  import { ref, watch } from 'vue'
  import { useDisplay } from 'vuetify'
  import { storeToRefs } from 'pinia'
  import { useRouter } from 'vue-router'
  import { useAuthStore } from '@/store/auth'
  import { useUiStore } from '@/store/ui'

  const { xs } = useDisplay()
  const auth = useAuthStore()
  const ui = useUiStore()
  const router = useRouter()
  const { unlockOpen, passwordConfigured } = storeToRefs(auth)

  const password = ref('')
  const show = ref(false)
  const busy = ref(false)
  const error = ref('')

  watch(unlockOpen, (open) => {
    if (open) { password.value = ''; error.value = ''; show.value = false }
  })

  function close() {
    auth.cancelUnlock()
  }

  async function submit() {
    if (!password.value) return
    busy.value = true
    try {
      const next = await auth.unlock(password.value)
      ui.notify('Staff access unlocked.')
      if (next) router.push(next)
    } catch (e) {
      error.value = e.status === 429 ? 'Too many attempts — wait a minute and try again.' : (e.message || 'Incorrect password.')
      password.value = ''
    } finally {
      busy.value = false
    }
  }
</script>
