<template>
  <v-btn v-if="isStaff"
         variant="tonal"
         prepend-icon="mdi-lock-open-variant"
         class="me-2"
         aria-label="Lock staff access"
         title="Lock — back to scan-only"
         @click="onLock">
    <span class="d-none d-sm-inline">Staff</span>
  </v-btn>
  <v-btn v-else
         variant="text"
         icon="mdi-lock"
         class="me-1"
         aria-label="Unlock staff access"
         title="Staff unlock"
         @click="auth.requestUnlock()" />
</template>

<script setup>
  import { storeToRefs } from 'pinia'
  import { useRoute, useRouter } from 'vue-router'
  import { useAuthStore } from '@/store/auth'
  import { useUiStore } from '@/store/ui'

  const auth = useAuthStore()
  const ui = useUiStore()
  const route = useRoute()
  const router = useRouter()
  const { isStaff } = storeToRefs(auth)

  async function onLock() {
    await auth.lock()
    ui.notify('Locked — scan-only mode.')
    if (route.meta.staff) router.push({ name: 'home' })
  }
</script>
