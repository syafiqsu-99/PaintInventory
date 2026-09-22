<template>
  <v-app-bar color="primary" density="comfortable" flat>
    <v-app-bar-nav-icon @click="emit('toggle-drawer')" />
    <v-app-bar-title class="text-none">Paint Inventory</v-app-bar-title>
    <template #append>
      <v-btn to="/" icon="mdi-barcode-scan" title="Scan" />
    </template>
  </v-app-bar>
</template>

<script setup>
    const emit = defineEmits(['toggle-drawer'])
</script>
