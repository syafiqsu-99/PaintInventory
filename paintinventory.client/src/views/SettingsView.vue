<template>
  <v-container fluid class="py-4">
    <h2 class="text-h6 mb-3">Settings</h2>

    <v-tabs v-model="tab" show-arrows color="primary" class="mb-4">
      <v-tab value="products" prepend-icon="mdi-palette-swatch">Products</v-tab>
      <v-tab value="locations" prepend-icon="mdi-domain">Locations</v-tab>
      <v-tab value="data" prepend-icon="mdi-database-import">Import / export</v-tab>
      <v-tab value="prefs" prepend-icon="mdi-cog-outline">Preferences</v-tab>
    </v-tabs>

    <v-window v-model="tab">
      <v-window-item value="products"><ProductsManager /></v-window-item>
      <v-window-item value="locations"><LocationsManager /></v-window-item>
      <v-window-item value="data"><ImportExportPanel /></v-window-item>
      <v-window-item value="prefs"><PreferencesPanel /></v-window-item>
    </v-window>
  </v-container>
</template>

<script setup>
  import { ref } from 'vue'
  import { useRoute } from 'vue-router'
  import ProductsManager from '@/components/settings/ProductsManager.vue'
  import LocationsManager from '@/components/settings/LocationsManager.vue'
  import ImportExportPanel from '@/components/settings/ImportExportPanel.vue'
  import PreferencesPanel from '@/components/settings/PreferencesPanel.vue'

  const route = useRoute()
  const allowed = ['products', 'locations', 'data', 'prefs']
  const tab = ref(allowed.includes(route.query.tab) ? route.query.tab : 'products')
</script>
