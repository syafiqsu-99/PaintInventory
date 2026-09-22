<template>
  <v-container fluid class="py-4">
    <div class="d-flex align-center mb-4">
      <h2 class="text-h6">Dashboard</h2>
      <v-spacer />
      <v-btn variant="text" size="small" prepend-icon="mdi-refresh" :loading="summaryLoading" @click="reload">
        Refresh
      </v-btn>
    </div>

    <v-row dense class="mb-2">
      <v-col cols="6" sm="4" md="2">
        <StatCard title="Products" :value="s.totalProducts" icon="mdi-format-list-bulleted" color="primary" />
      </v-col>
      <v-col cols="6" sm="4" md="2">
        <StatCard title="Locations" :value="s.totalLocations" icon="mdi-domain" color="secondary" />
      </v-col>
      <v-col cols="6" sm="4" md="2">
        <StatCard title="Total on hand" :value="formatNumber(s.totalOnHand)" icon="mdi-package-variant" color="secondary" />
      </v-col>
      <v-col cols="6" sm="4" md="2">
        <StatCard title="Low stock" :value="s.lowStockCount" icon="mdi-alert" color="warning" />
      </v-col>
      <v-col cols="6" sm="4" md="2">
        <StatCard title="Out of stock" :value="s.outOfStockCount" icon="mdi-package-variant-closed-remove" color="error" />
      </v-col>
      <v-col cols="6" sm="4" md="2">
        <StatCard :title="`Expiring ≤ ${s.expiryWindowDays}d`" :value="s.expiringSoonCount" icon="mdi-clock-alert-outline" color="warning" />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" lg="8">
        <v-card class="mb-4">
          <v-card-title class="text-subtitle-1">Usage — last 14 days</v-card-title>
          <v-divider />
          <v-card-text>
            <UsageChart :points="s.usage" />
          </v-card-text>
        </v-card>

        <v-card>
          <v-card-title class="text-subtitle-1">Stock by location</v-card-title>
          <v-divider />
          <v-card-text>
            <StockByLocationChart :items="s.stockByLocation" />
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" lg="4">
        <ExpiringSoonList :items="s.expiringSoon" class="mb-4" />
        <TopUsedList :items="s.topUsed" class="mb-4" />
        <LowStockList :items="s.lowStock" />
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
  import { onMounted, computed } from 'vue'
  import { storeToRefs } from 'pinia'
  import { useInventoryStore } from '@/store/inventory'
  import { useSettingsStore } from '@/store/settings'
  import { formatNumber } from '@/utils/format'
  import StatCard from '@/components/common/StatCard.vue'
  import UsageChart from '@/components/common/UsageChart.vue'
  import LowStockList from '@/components/stock/LowStockList.vue'
  import ExpiringSoonList from '@/components/dashboard/ExpiringSoonList.vue'
  import StockByLocationChart from '@/components/dashboard/StockByLocationChart.vue'
  import TopUsedList from '@/components/dashboard/TopUsedList.vue'

  const store = useInventoryStore()
  const settings = useSettingsStore()
  const { summary, summaryLoading } = storeToRefs(store)
  const { prefs } = storeToRefs(settings)

  const empty = {
    totalProducts: 0, totalLocations: 0, lowStockCount: 0, outOfStockCount: 0,
    expiringSoonCount: 0, totalOnHand: 0, expiryWindowDays: prefs.value.expiryWarningDays,
    usage: [], stockByLocation: [], topUsed: [], expiringSoon: [], lowStock: []
  }
  const s = computed(() => summary.value ?? empty)

  function reload() {
    store.loadSummary(prefs.value.expiryWarningDays)
  }

  onMounted(reload)
</script>
