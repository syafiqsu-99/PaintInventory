<script setup>
defineProps({
  failed: { type: Boolean, default: false }
})
</script>

<template>
  <div class="splash d-flex flex-column align-center justify-center">
    <v-icon icon="mdi-format-paint" size="64" color="primary" class="mb-4" />
    <div class="text-h4 font-weight-medium mb-6">Paint Inventory</div>
    <v-progress-circular indeterminate color="primary" size="40" width="4" />
    <div class="text-body-2 text-medium-emphasis mt-4">
      {{ failed ? 'Trouble reaching the server — retrying…' : 'Connecting…' }}
    </div>
  </div>
</template>

<style scoped>
  .splash {
    position: fixed;
    inset: 0;
    background: rgb(var(--v-theme-background));
    z-index: 2000;
  }
</style>
