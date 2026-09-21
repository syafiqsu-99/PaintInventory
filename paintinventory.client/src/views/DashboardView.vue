<script setup>
import { onMounted, computed } from 'vue'
import { storeToRefs } from 'pinia'
import { useInventoryStore } from '@/store/inventory'
import StatCard from '@/components/StatCard.vue'
import UsageChart from '@/components/UsageChart.vue'
import LowStockList from '@/components/LowStockList.vue'

const store = useInventoryStore()
const { dashboard, lowStock } = storeToRefs(store)

const usage = computed(() => dashboard.value?.usage ?? [])

onMounted(() => {
    store.loadDashboard()
    store.loadLowStock()
})
</script>

<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12" sm="4">
        <StatCard title="Products" :value="dashboard?.totalProducts ?? 0" icon="mdi-format-list-bulleted" color="primary" />
      </v-col>
      <v-col cols="12" sm="4">
        <StatCard title="Low stock" :value="dashboard?.lowStockCount ?? 0" icon="mdi-alert" color="error" />
      </v-col>
      <v-col cols="12" sm="4">
        <StatCard title="Total on hand" :value="dashboard?.totalOnHand ?? 0" icon="mdi-package-variant" color="secondary" />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="8">
        <v-card>
          <v-card-title class="text-subtitle-1">Usage — last 14 days</v-card-title>
          <v-card-text>
            <UsageChart :points="usage" />
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" md="4">
        <LowStockList :items="lowStock" />
      </v-col>
    </v-row>
  </v-container>
</template>
