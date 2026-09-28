<template>
  <v-card>
    <v-card-text class="pt-6">
      <div class="text-center mb-6">
        <v-icon icon="mdi-format-paint" size="48" color="primary" />
        <div class="text-h6 mt-2">Paint Inventory</div>
        <div class="text-body-1 text-medium-emphasis">Sign in with your site's access code</div>
      </div>

      <v-form v-model="valid" @submit.prevent="submit">
        <v-select v-model="form.vendorId"
                  :items="sites"
                  :loading="loadingSites"
                  item-title="name"
                  item-value="id"
                  label="Site"
                  :rules="[rules.required]"
                  variant="outlined"
                  class="mb-2" />
        <v-text-field v-model="form.operatorName"
                      label="Your name"
                      :rules="[rules.required, rules.name]"
                      autocomplete="name"
                      variant="outlined"
                      class="mb-2" />
        <v-text-field v-model="form.accessCode"
                      label="Access code"
                      :type="showCode ? 'text' : 'password'"
                      :append-inner-icon="showCode ? 'mdi-eye-off' : 'mdi-eye'"
                      :rules="[rules.required]"
                      inputmode="numeric"
                      autocomplete="current-password"
                      variant="outlined"
                      class="mb-2"
                      @click:append-inner="showCode = !showCode" />

        <v-alert v-if="error" type="error" variant="tonal" density="comfortable" class="mb-4">{{ error }}</v-alert>

        <v-btn type="submit"
               color="primary"
               size="x-large"
               block
               :loading="busy"
               :disabled="!valid">
          Sign in
        </v-btn>
      </v-form>
    </v-card-text>
  </v-card>
</template>

<script setup>
  import { onMounted, ref } from 'vue'
  import { useRoute, useRouter } from 'vue-router'
  import { useAuthStore } from '@/store/auth'

  const auth = useAuthStore()
  const router = useRouter()
  const route = useRoute()

  const KEY = 'paint-inventory.login'

  const sites = ref([])
  const loadingSites = ref(false)
  const valid = ref(false)
  const busy = ref(false)
  const showCode = ref(false)
  const error = ref('')
  const form = ref({ vendorId: null, operatorName: '', accessCode: '' })

  const rules = {
    required: (v) => (v !== null && v !== undefined && String(v).trim() !== '') || 'Required',
    name: (v) => String(v ?? '').trim().length >= 2 || 'At least 2 characters'
  }

  onMounted(async () => {
    try {
      const saved = JSON.parse(localStorage.getItem(KEY) ?? 'null')
      if (saved) Object.assign(form.value, { vendorId: saved.vendorId, operatorName: saved.operatorName })
    } catch { /* storage unavailable */ }

    loadingSites.value = true
    try {
      sites.value = await auth.sites()
    } catch (e) {
      error.value = e.message
    } finally {
      loadingSites.value = false
    }
  })

  async function submit() {
    if (!valid.value) return
    busy.value = true
    error.value = ''
    try {
      await auth.login({
        vendorId: form.value.vendorId,
        operatorName: form.value.operatorName.trim(),
        accessCode: form.value.accessCode.trim()
      })
      try {
        localStorage.setItem(KEY, JSON.stringify({ vendorId: form.value.vendorId, operatorName: form.value.operatorName.trim() }))
      } catch { /* storage unavailable */ }
      const redirect = typeof route.query.redirect === 'string' && route.query.redirect.startsWith('/') ? route.query.redirect : '/'
      router.replace(redirect)
    } catch (e) {
      error.value = e.status === 429 ? 'Too many attempts — wait a minute and try again.' : e.message
      form.value.accessCode = ''
    } finally {
      busy.value = false
    }
  }
</script>
