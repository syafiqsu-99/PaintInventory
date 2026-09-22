<template>
  <v-card max-width="640">
    <v-card-title class="text-subtitle-1">Preferences</v-card-title>
    <v-divider />
    <v-card-text>
      <p class="text-body-2 text-medium-emphasis mb-4">
        Saved on this device. They tune the scan screen and dashboard for how you work.
      </p>

      <v-text-field v-model.number="prefs.expiryWarningDays"
                    label="Expiry warning window (days)"
                    type="number"
                    inputmode="numeric"
                    hint="Stock reaching best-before within this many days shows on the dashboard"
                    persistent-hint
                    variant="outlined"
                    density="comfortable"
                    class="mb-4"
                    style="max-width: 320px" />

      <v-select v-model="prefs.defaultLocationId"
                :items="locationItems"
                label="Default scan location"
                clearable
                hint="Pre-selected when the movement form opens"
                persistent-hint
                variant="outlined"
                density="comfortable"
                class="mb-4"
                style="max-width: 320px" />

      <div class="text-body-2 mb-1">Default movement on scan</div>
      <v-btn-toggle v-model="prefs.scanDefaultDirection" mandatory divided density="comfortable" class="mb-4">
        <v-btn value="in" prepend-icon="mdi-tray-arrow-down">Stock in</v-btn>
        <v-btn value="out" prepend-icon="mdi-tray-arrow-up">Stock out</v-btn>
      </v-btn-toggle>

      <v-switch v-model="prefs.scanBeep" label="Beep on scan" color="primary" density="comfortable" hide-details />
    </v-card-text>
    <v-divider />
    <v-card-actions>
      <v-spacer />
      <v-btn variant="text" @click="settings.reset()">Reset to defaults</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup>
  import { onMounted, computed } from 'vue'
  import { storeToRefs } from 'pinia'
  import { useSettingsStore } from '@/store/settings'
  import { useVendorStore } from '@/store/vendor'

  const settings = useSettingsStore()
  const vendorStore = useVendorStore()
  const { prefs } = storeToRefs(settings)
  const { vendors } = storeToRefs(vendorStore)

  const locationItems = computed(() =>
    vendors.value.filter((v) => v.storesStock).map((v) => ({ title: v.name, value: v.id })))

  onMounted(() => { if (!vendors.value.length) vendorStore.load() })
</script>
