<template>
  <v-navigation-drawer :model-value="modelValue" temporary @update:model-value="emit('update:modelValue', $event)">
    <v-list-item class="py-4" prepend-icon="mdi-format-paint" title="Paint Inventory" subtitle="Stock & coating" />
    <v-divider />

    <v-list nav density="comfortable">
      <template v-for="(group, gi) in groups" :key="gi">
        <v-list-subheader>{{ group.title }}</v-list-subheader>
        <v-list-item v-for="l in group.links"
                     :key="l.to"
                     :to="l.to"
                     :prepend-icon="l.icon"
                     :title="l.title"
                     @click="emit('update:modelValue', false)" />
        <v-divider v-if="gi < groups.length - 1" class="my-2" />
      </template>
    </v-list>
  </v-navigation-drawer>
</template>

<script setup>
  defineProps({
    modelValue: { type: Boolean, default: false }
  })
  const emit = defineEmits(['update:modelValue'])

  const groups = [
    {
      title: 'Operations',
      links: [
        { to: '/', title: 'Scan', icon: 'mdi-barcode-scan' },
        { to: '/stock-level', title: 'Stock level', icon: 'mdi-warehouse' },
        { to: '/transfer', title: 'Transfer', icon: 'mdi-swap-horizontal' }
      ]
    },
    {
      title: 'Insights',
      links: [
        { to: '/dashboard', title: 'Dashboard', icon: 'mdi-view-dashboard' },
        { to: '/reports', title: 'Reports', icon: 'mdi-file-document-outline' }
      ]
    },
    {
      title: 'Manage',
      links: [
        { to: '/settings', title: 'Settings', icon: 'mdi-cog-outline' }
      ]
    }
  ]
</script>
