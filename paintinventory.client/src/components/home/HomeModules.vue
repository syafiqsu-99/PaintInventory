<template>
  <section class="modules" aria-label="Modules">
    <v-card v-for="m in modules"
            :key="m.to"
            class="module"
            :class="{ 'module--locked': m.staff && !isStaff }"
            rounded="lg"
            :aria-label="m.staff && !isStaff ? `${m.title} (staff only)` : m.title"
            @click="open(m)">
      <div class="d-flex align-center ga-3 pa-4">
        <v-avatar :color="m.color" size="48" variant="flat">
          <v-icon :icon="m.icon" size="26" color="white" />
        </v-avatar>
        <div class="flex-grow-1 min-w-0">
          <div class="text-subtitle-1 font-weight-medium">{{ m.title }}</div>
          <div class="text-body-2 text-medium-emphasis">{{ m.description }}</div>
        </div>
        <v-icon v-if="m.staff && !isStaff" icon="mdi-lock" color="medium-emphasis" />
        <v-icon v-else icon="mdi-chevron-right" color="medium-emphasis" />
      </div>
    </v-card>
  </section>
</template>

<script setup>
  import { storeToRefs } from 'pinia'
  import { useRouter } from 'vue-router'
  import { useAuthStore } from '@/store/auth'

  const auth = useAuthStore()
  const router = useRouter()
  const { isStaff } = storeToRefs(auth)

  const modules = [
    { to: '/scan', title: 'Scan', description: 'Scan a tin to see product details', icon: 'mdi-barcode-scan', color: 'primary' },
    { to: '/stock-level', title: 'Stock level', description: 'On-hand by location, low stock', icon: 'mdi-warehouse', color: 'teal', staff: true },
    { to: '/transfer', title: 'Transfer', description: 'Move stock between locations', icon: 'mdi-swap-horizontal', color: 'indigo', staff: true },
    { to: '/reports', title: 'Coating reports', description: 'Paint reports and PDF output', icon: 'mdi-file-document-outline', color: 'deep-orange', staff: true },
    { to: '/dashboard', title: 'Dashboard', description: 'Usage, expiry and low-stock KPIs', icon: 'mdi-view-dashboard', color: 'purple', staff: true },
    { to: '/products', title: 'Products', description: 'Jotun / International product master', icon: 'mdi-palette-swatch', color: 'pink', staff: true },
    { to: '/vendors', title: 'Locations', description: 'Stores, blasters and painters', icon: 'mdi-domain', color: 'blue-grey', staff: true },
    { to: '/settings', title: 'Settings', description: 'Import/export, preferences, password', icon: 'mdi-cog-outline', color: 'grey-darken-2', staff: true }
  ]

  function open(m) {
    if (m.staff && !isStaff.value) return auth.requestUnlock(m.to)
    router.push(m.to)
  }
</script>

<style scoped>
  .modules {
    width: 100%;
    max-width: 1100px;
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    gap: 16px;
  }

  .module {
    transition: transform 0.15s ease, box-shadow 0.15s ease;
  }

    .module:hover {
      transform: translateY(-2px);
    }

  .module--locked {
    opacity: 0.85;
  }

  .min-w-0 {
    min-width: 0;
  }

  @media (max-width: 1279px) {
    .modules {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
  }

  @media (max-width: 599px) {
    .modules {
      grid-template-columns: 1fr;
      gap: 12px;
    }
  }

  @media (prefers-reduced-motion: reduce) {
    .module {
      transition: none;
    }

      .module:hover {
        transform: none;
      }
  }
</style>
