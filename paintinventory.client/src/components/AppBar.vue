<script setup>
const links = [
  { to: '/', label: 'Scan', icon: 'mdi-barcode-scan' },
  { to: '/inventory', label: 'Inventory', icon: 'mdi-format-list-bulleted' },
  { to: '/dashboard', label: 'Dashboard', icon: 'mdi-chart-box-outline' }
]
</script>

<template>
  <v-app-bar color="primary" density="comfortable" flat>
    <v-app-bar-title>
      <v-icon icon="mdi-palette" class="mr-2" />
      Paint Inventory
    </v-app-bar-title>

    <template #append>
      <v-btn v-for="link in links"
             :key="link.to"
             :to="link.to"
             :prepend-icon="link.icon"
             variant="text">
        {{ link.label }}
      </v-btn>
    </template>
  </v-app-bar>
</template>
