<template>
  <section class="hero text-center">
    <v-img :src="logo" width="88" height="88" class="mx-auto mb-4 hero__logo" alt="" />
    <h1 class="hero__title">Paint Inventory</h1>
    <p class="hero__subtitle">Paint stock, barcode scanning and coating reports</p>
    <v-chip :color="isStaff ? 'success' : 'white'"
            :prepend-icon="isStaff ? 'mdi-lock-open-variant' : 'mdi-barcode-scan'"
            variant="flat"
            size="large"
            class="mt-4">
      {{ isStaff ? 'Staff access' : 'Scan-only mode' }}
    </v-chip>
  </section>
</template>

<script setup>
  import { storeToRefs } from 'pinia'
  import logo from '@/assets/logo.svg'
  import { useAuthStore } from '@/store/auth'

  const { isStaff } = storeToRefs(useAuthStore())
</script>

<style scoped>
  .hero__logo {
    filter: drop-shadow(0 6px 16px rgba(0, 0, 0, 0.25));
  }

  .hero__title {
    font-size: clamp(1.75rem, 5vw, 2.75rem);
    font-weight: 600;
    line-height: 1.2;
  }

  .hero__subtitle {
    margin-top: 8px;
    font-size: 1.05rem;
    opacity: 0.85;
  }
</style>
