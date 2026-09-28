<template>
  <v-bottom-navigation grow color="primary" class="d-md-none" :elevation="8">
    <v-btn to="/" value="scan">
      <v-icon icon="mdi-barcode-scan" />
      Scan
    </v-btn>
    <v-btn to="/stock-level" value="stock">
      <v-icon icon="mdi-warehouse" />
      Stock
    </v-btn>
    <template v-if="isStaff">
      <v-btn to="/dashboard" value="dashboard">
        <v-icon icon="mdi-view-dashboard" />
        Dashboard
      </v-btn>
      <v-btn to="/settings" value="settings">
        <v-icon icon="mdi-cog-outline" />
        Settings
      </v-btn>
    </template>
  </v-bottom-navigation>
</template>

<script setup>
  import { storeToRefs } from 'pinia'
  import { useAuthStore } from '@/store/auth'

  const { isStaff } = storeToRefs(useAuthStore())
</script>
