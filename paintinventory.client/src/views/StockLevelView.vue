<template>
  <v-container fluid>
    <v-row class="mb-2" align="center">
      <v-col cols="12" sm="4">
        <v-select v-model="vendorId"
                  :items="locationItems"
                  label="Location"
                  variant="outlined"
                  density="comfortable"
                  hide-details
                  @update:model-value="reload" />
      </v-col>
      <v-spacer />
      <v-col cols="auto">
        <v-btn color="secondary" variant="tonal" :href="exportHref" target="_blank" prepend-icon="mdi-download">
          Export
        </v-btn>
      </v-col>
    </v-row>
    <StockLevelTable :items="levels" :loading="loading" />
  </v-container>
</template>

<script setup>
    import { onMounted, ref, computed } from 'vue'
    import { storeToRefs } from 'pinia'
    import { useInventoryStore } from '@/store/inventory'
    import { useVendorStore } from '@/store/vendor'
    import StockLevelTable from '@/components/stock/StockLevelTable.vue'

    const inventory = useInventoryStore()
    const vendorStore = useVendorStore()
    const { levels, loading } = storeToRefs(inventory)
    const { vendors } = storeToRefs(vendorStore)

    const vendorId = ref(null)

    const locationItems = computed(() => [
        { title: 'All locations', value: null },
        ...vendors.value.filter((v) => v.storesStock).map((v) => ({ title: v.name, value: v.id }))
    ])

    const exportHref = computed(() =>
        vendorId.value ? `/api/inventory/export?vendorId=${vendorId.value}` : '/api/inventory/export')

    function reload() {
        inventory.loadLevels(vendorId.value)
    }

    onMounted(async () => {
        if (!vendors.value.length) await vendorStore.load()
        reload()
    })
</script>
