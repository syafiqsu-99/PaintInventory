<template>
  <v-app-bar color="primary" density="comfortable" flat>
    <v-app-bar-nav-icon @click="emit('toggle-drawer')" />
    <v-app-bar-title class="text-none">
      <span :class="{ 'd-none d-sm-inline': me }">Paint Inventory</span>
    </v-app-bar-title>
    <template #append>
      <v-chip v-if="me"
              variant="tonal"
              color="white"
              size="small"
              prepend-icon="mdi-map-marker"
              class="session-chip me-1">
        <span class="text-truncate">{{ me.vendorName }} · {{ me.operator }}</span>
      </v-chip>
      <v-btn to="/" icon="mdi-barcode-scan" title="Scan" class="d-none d-sm-flex" />
      <v-btn icon="mdi-logout" title="Sign out" @click="signOut" />
    </template>
  </v-app-bar>
</template>

<script setup>
    import { storeToRefs } from 'pinia'
    import { useRouter } from 'vue-router'
    import { useAuthStore } from '@/store/auth'

    const emit = defineEmits(['toggle-drawer'])

    const auth = useAuthStore()
    const router = useRouter()
    const { me } = storeToRefs(auth)

    async function signOut() {
        await auth.logout()
        router.replace({ name: 'login' })
    }
</script>

<style scoped>
  .session-chip {
    max-width: 46vw;
  }
</style>
