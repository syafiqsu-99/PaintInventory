<script setup>
const links = [
    { to: '/', title: 'Stock In', icon: 'mdi-tray-arrow-down' },
    { to: '/stock-out', title: 'Stock Out', icon: 'mdi-tray-arrow-up' },
    { to: '/stock-level', title: 'Stock Level', icon: 'mdi-warehouse' },
    { to: '/reports', title: 'Reports', icon: 'mdi-file-document-outline' },
    { to: '/dashboard', title: 'Dashboard', icon: 'mdi-view-dashboard' },
    { to: '/vendors', title: 'Locations', icon: 'mdi-domain' }
]
</script>

<template>
  <v-app-bar color="primary" density="comfortable" flat>
    <v-app-bar-title>Paint Inventory</v-app-bar-title>
    <template #append>
      <v-btn v-for="l in links"
             :key="l.to"
             :to="l.to"
             :prepend-icon="l.icon"
             variant="text"
             class="text-none">
        {{ l.title }}
      </v-btn>
    </template>
  </v-app-bar>
</template>
