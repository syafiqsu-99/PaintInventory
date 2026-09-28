<template>
  <v-app>
    <v-overlay :model-value="booting || bootError" class="align-center justify-center" persistent
               scrim="#ffffff" opacity="1" style="z-index:3000;">
      <div class="text-center px-4" style="max-width:420px;">
        <v-img :src="logoNoBg" width="120" height="120" class="mx-auto mb-6" alt="Paint Inventory" />

        <template v-if="!bootError">
          <v-progress-circular color="primary" indeterminate size="42" width="4" />
          <div class="mt-4 text-medium-emphasis">
            {{ attempt <= 1 ? 'Loading…' : `Connecting to server… (attempt ${attempt})` }}
          </div>
        </template>

        <template v-else>
          <v-icon color="error" size="42">mdi-lan-disconnect</v-icon>
          <div class="mt-4 mb-1 font-weight-medium">Can't reach the server</div>
          <div class="text-medium-emphasis text-body-2 mb-4">
            The API isn't responding. Make sure the backend (PaintInventory.Server) is running,
            then retry.
          </div>
          <v-btn color="primary" size="large" prepend-icon="mdi-refresh" @click="boot">Retry</v-btn>
        </template>
      </div>
    </v-overlay>

    <v-app-bar v-if="!route.meta.bare" color="primary" flat>
      <button type="button" class="brand-home d-flex align-center ms-3"
              aria-label="Go to the home page" @click="onNav('/')">
        <v-img :src="logo" width="40" height="40" class="me-3" alt="" />
        <span class="text-h6 d-none d-sm-inline">Paint Inventory</span>
      </button>

      <v-spacer />

      <template v-for="item in navItems" :key="item.to">
        <v-btn v-if="mdAndUp"
               :variant="isActive(item.to) ? 'tonal' : 'text'"
               :prepend-icon="item.icon"
               class="me-1"
               :aria-label="item.label"
               @click="onNav(item.to)">
          {{ item.label }}
        </v-btn>
        <v-btn v-else
               :variant="isActive(item.to) ? 'tonal' : 'text'"
               :icon="item.icon"
               :aria-label="item.label"
               :title="item.label"
               @click="onNav(item.to)" />
      </template>

      <StaffNavButton v-if="!booting && !bootError" />
      <v-progress-linear :active="navigating" indeterminate color="white" absolute location="bottom" height="3" />
    </v-app-bar>

    <v-main>
      <div :class="route.meta.fullBleed ? 'full-bleed' : 'page'">
        <router-view v-if="!booting && !bootError" v-slot="{ Component, route: viewRoute }">
          <transition name="page" mode="out-in">
            <component :is="Component" :key="viewRoute.matched[0]?.path" />
          </transition>
        </router-view>
      </div>
    </v-main>

    <v-footer v-if="!route.meta.bare" color="primary" app class="text-caption justify-space-between px-4 d-none d-sm-flex">
      <span>Paint Inventory</span>
      <span>&copy; {{ year }} Emerson — Paint Inventory</span>
    </v-footer>

    <StaffUnlockDialog v-if="!booting && !bootError" />

    <v-snackbar v-model="snackbar.show" :color="snackbar.color" timeout="3500" location="top">
      {{ snackbar.text }}
    </v-snackbar>
  </v-app>
</template>

<script setup>
  import { computed, ref } from 'vue'
  import { useRoute, useRouter } from 'vue-router'
  import { useDisplay } from 'vuetify'
  import { storeToRefs } from 'pinia'
  import logo from '@/assets/logo.svg'
  import logoNoBg from '@/assets/logo-no-bg.svg'
  import StaffNavButton from '@/components/common/StaffNavButton.vue'
  import StaffUnlockDialog from '@/components/common/StaffUnlockDialog.vue'
  import { useAuthStore } from '@/store/auth'
  import { useUiStore } from '@/store/ui'
  import http from '@/utils/http'

  const route = useRoute()
  const router = useRouter()
  const { mdAndUp } = useDisplay()
  const auth = useAuthStore()
  const { isStaff } = storeToRefs(auth)
  const { snackbar } = storeToRefs(useUiStore())

  const allNavItems = [
    { to: '/scan', label: 'Scan', icon: 'mdi-barcode-scan' },
    { to: '/stock-level', label: 'Stock', icon: 'mdi-warehouse', staff: true },
    { to: '/reports', label: 'Reports', icon: 'mdi-file-document-outline', staff: true },
    { to: '/settings', label: 'Settings', icon: 'mdi-cog-outline', staff: true }
  ]
  const navItems = computed(() => allNavItems.filter((item) => !item.staff || isStaff.value))

  const year = computed(() => new Date().getFullYear())

  const booting = ref(true)
  const bootError = ref(false)
  const attempt = ref(0)

  const sleep = (ms) => new Promise((r) => setTimeout(r, ms))

  async function boot() {
    booting.value = true
    bootError.value = false
    const maxAttempts = 8
    const delays = [500, 1000, 2000, 3000, 4000, 5000, 5000]

    for (let i = 0; i < maxAttempts; i++) {
      attempt.value = i + 1
      try {
        await http.get('/health')
        await auth.load(true)
        booting.value = false
        return
      } catch {
        if (i < maxAttempts - 1) await sleep(delays[i] ?? 5000)
      }
    }
    booting.value = false
    bootError.value = true
  }

  function isActive(path) {
    return route.path === path || route.path.startsWith(`${path}/`)
  }

  function onNav(path) {
    if (path === route.path) return
    if (allNavItems.find((i) => i.to === path)?.staff && !isStaff.value) return auth.requestUnlock(path)
    router.push(path)
  }

  const navigating = ref(false)
  let navTimer = null
  router.beforeEach(() => {
    clearTimeout(navTimer)
    navTimer = setTimeout(() => { navigating.value = true }, 120)
  })
  router.afterEach(() => {
    clearTimeout(navTimer)
    navigating.value = false
  })
  router.onError(() => {
    clearTimeout(navTimer)
    navigating.value = false
  })

  boot()
</script>

<style scoped>
  .brand-home {
    background: none;
    border: 0;
    padding: 0;
    color: inherit;
    cursor: pointer;
  }

    .brand-home:focus-visible {
      outline: 2px solid currentColor;
      outline-offset: 4px;
      border-radius: 4px;
    }

  .full-bleed {
    min-height: 100%;
  }

  .page-enter-active,
  .page-leave-active {
    transition: opacity 0.15s ease, transform 0.15s ease;
  }

  .page-enter-from {
    opacity: 0;
    transform: translateY(6px);
  }

  .page-leave-to {
    opacity: 0;
  }

  @media (prefers-reduced-motion: reduce) {
    .page-enter-active,
    .page-leave-active {
      transition: none;
    }
  }

  /* 16px inputs stop mobile browsers zooming in on focus. */
  :deep(.v-field__input) {
    font-size: 16px;
  }
</style>
