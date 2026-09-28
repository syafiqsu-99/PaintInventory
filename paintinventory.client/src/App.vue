<template>
  <v-app>
    <StartupSplash v-if="!ready" :failed="failed" />
    <template v-else>
      <template v-if="showChrome">
        <AppBar @toggle-drawer="drawer = !drawer" />
        <NavDrawer v-model="drawer" />
      </template>
      <v-main :class="{ 'app-main': showChrome }">
        <router-view />
      </v-main>
      <BottomNav v-if="showChrome" />
    </template>
    <v-snackbar v-model="snackbar.show" :color="snackbar.color" timeout="3500" location="top right">
      {{ snackbar.text }}
    </v-snackbar>
  </v-app>
</template>

<script setup>
    import { ref, computed, onMounted, onUnmounted } from 'vue'
    import { useRoute } from 'vue-router'
    import { storeToRefs } from 'pinia'
    import AppBar from '@/components/common/AppBar.vue'
    import NavDrawer from '@/components/common/NavDrawer.vue'
    import BottomNav from '@/components/common/BottomNav.vue'
    import StartupSplash from '@/components/common/StartupSplash.vue'
    import { useUiStore } from '@/store/ui'
    import http from '@/utils/http'

    const ui = useUiStore()
    const route = useRoute()
    const { snackbar } = storeToRefs(ui)

    const showChrome = computed(() => !route.meta.public)

    const drawer = ref(false)
    const ready = ref(false)
    const failed = ref(false)
    let stopped = false

    async function connect() {
      while (!ready.value && !stopped) {
        try {
          await http.get('/health')
          ready.value = true
        } catch {
          failed.value = true
          await new Promise((resolve) => setTimeout(resolve, 2000))
        }
      }
    }

    onMounted(connect)
    onUnmounted(() => { stopped = true })
</script>

<style scoped>
  /* Keep content clear of the fixed bottom navigation on mobile. */
  @media (max-width: 959px) {
    .app-main {
      padding-bottom: 56px;
    }
  }

  /* 16px inputs stop mobile browsers zooming in on focus. */
  :deep(.v-field__input) {
    font-size: 16px;
  }
</style>
