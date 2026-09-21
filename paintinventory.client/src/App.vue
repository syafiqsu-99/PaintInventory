<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { storeToRefs } from 'pinia'
import AppBar from '@/components/AppBar.vue'
import StartupSplash from '@/components/StartupSplash.vue'
import { useUiStore } from '@/store/ui'
import http from '@/utils/http'

const ui = useUiStore()
const { snackbar } = storeToRefs(ui)

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

<template>
  <v-app>
    <StartupSplash v-if="!ready" :failed="failed" />
    <template v-else>
      <AppBar />
      <v-main>
        <router-view />
      </v-main>
    </template>
    <v-snackbar v-model="snackbar.show" :color="snackbar.color" timeout="3500" location="top right">
      {{ snackbar.text }}
    </v-snackbar>
  </v-app>
</template>
