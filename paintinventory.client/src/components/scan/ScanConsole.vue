<template>
  <div>
    <v-card class="mb-4">
      <v-card-text class="text-center py-6">
        <v-icon :icon="busy ? 'mdi-magnify-scan' : 'mdi-barcode-scan'"
                size="72"
                :color="listening ? 'primary' : 'medium-emphasis'"
                :class="{ pulse: listening && !busy }" />
        <div class="text-h6 mt-3">{{ statusText }}</div>
        <div class="text-body-1 text-medium-emphasis mt-1">
          Point the scanner at a paint barcode — the form opens automatically.
        </div>

        <div class="d-flex flex-column flex-sm-row align-stretch align-sm-center ga-2 mt-6">
          <v-text-field ref="scanField"
                        v-model="manual"
                        label="Scan or type GTIN, then Enter"
                        variant="outlined"
                        density="comfortable"
                        hide-details
                        autofocus
                        inputmode="numeric"
                        autocomplete="off"
                        class="flex-grow-1"
                        @keyup.enter="submitManual" />
          <v-btn color="primary"
                 size="large"
                 height="56"
                 :loading="busy"
                 prepend-icon="mdi-magnify"
                 @click="submitManual">
            Look up
          </v-btn>
        </div>
      </v-card-text>
    </v-card>

    <IncomingTransfers v-if="!isStaff"
                       :items="incoming"
                       :loading="incomingLoading"
                       @select="openIncoming"
                       @refresh="loadIncoming" />

    <RecentScans :items="recent" />

    <ScanDialog v-model="dialogOpen"
                :product="resolved"
                :pending="pendingForProduct"
                :initial-transaction-id="initialTransactionId"
                @done="onDone" />
    <ProductRegisterDialog v-if="isStaff" v-model="registerOpen" :gtin="pendingGtin" @registered="onRegistered" />
  </div>
</template>

<script setup>
    import { ref, computed, watch, onMounted, nextTick } from 'vue'
    import { storeToRefs } from 'pinia'
    import { useProductStore } from '@/store/product'
    import { useVendorStore } from '@/store/vendor'
    import { useSettingsStore } from '@/store/settings'
    import { useUiStore } from '@/store/ui'
    import { useAuthStore } from '@/store/auth'
    import { useStockStore } from '@/store/stock'
    import { useScanner } from '@/composables/useScanner'
    import ProductRegisterDialog from '@/components/products/ProductRegisterDialog.vue'
    import ScanDialog from '@/components/scan/ScanDialog.vue'
    import RecentScans from '@/components/scan/RecentScans.vue'
    import IncomingTransfers from '@/components/scan/IncomingTransfers.vue'

    const products = useProductStore()
    const vendorStore = useVendorStore()
    const settings = useSettingsStore()
    const ui = useUiStore()
    const stock = useStockStore()
    const { isStaff } = storeToRefs(useAuthStore())
    const { vendors } = storeToRefs(vendorStore)
    const { prefs } = storeToRefs(settings)

    const scanField = ref(null)
    const manual = ref('')
    const busy = ref(false)
    const resolved = ref(null)
    const dialogOpen = ref(false)
    const registerOpen = ref(false)
    const pendingGtin = ref('')
    const recent = ref([])
    const incoming = ref([])
    const incomingLoading = ref(false)
    const initialTransactionId = ref(null)

    const pendingForProduct = computed(() =>
      resolved.value ? incoming.value.filter((t) => t.productId === resolved.value.id) : [])

    // Hands-free fallback: fires only when focus is NOT in an editable field,
    // so it complements (never double-fires with) the focused input below.
    const { pause, resume } = useScanner(onScan)

    const listening = computed(() => !dialogOpen.value && !registerOpen.value)
    const statusText = computed(() => {
      if (busy.value) return 'Looking up…'
      return listening.value ? 'Ready to scan' : 'Paused — finish the open form'
    })

    watch(listening, (on) => {
      if (on) { resume(); focusField() } else { pause() }
    })

    onMounted(() => {
      if (isStaff.value && !vendors.value.length) vendorStore.load()
      if (!isStaff.value) loadIncoming()
      focusField()
    })

    async function loadIncoming() {
      incomingLoading.value = true
      try {
        incoming.value = await stock.inTransit()
      } catch (e) {
        ui.error(e.message)
      } finally {
        incomingLoading.value = false
      }
    }

    async function openIncoming(transfer) {
      try {
        const product = await products.lookup(transfer.gtin)
        if (!product) return ui.error(`GTIN ${transfer.gtin} is no longer registered.`)
        initialTransactionId.value = transfer.transactionId
        resolved.value = product
        dialogOpen.value = true
      } catch (e) {
        ui.error(e.message)
      }
    }

    function focusField() {
      nextTick(() => scanField.value?.focus())
    }

    function submitManual() {
      const code = manual.value.trim()
      if (code) onScan(code)
    }

    async function onScan(code) {
      const gtin = String(code).trim()
      if (!gtin || busy.value) return
      busy.value = true
      beep()
      try {
        const product = await products.lookup(gtin)
        if (product) {
          initialTransactionId.value = null
          resolved.value = product
          dialogOpen.value = true
        } else if (!isStaff.value) {
          ui.notify(`GTIN ${gtin} is not registered — ask the paint store to register it.`, 'warning')
        } else {
          pendingGtin.value = gtin
          registerOpen.value = true
          ui.notify(`GTIN ${gtin} is new — register it, then record the movement.`, 'warning')
        }
      } catch (e) {
        ui.error(e.message)
      } finally {
        busy.value = false
        manual.value = ''
      }
    }

    function onRegistered(product) {
      if (product) {
        resolved.value = product
        dialogOpen.value = true
      }
    }

    function onDone(entry) {
      recent.value.unshift(entry)
      if (recent.value.length > 25) recent.value.pop()
      if (!isStaff.value && entry.direction === 'receive') loadIncoming()
    }

    function beep() {
      if (!prefs.value.scanBeep) return
      try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)()
        const osc = ctx.createOscillator()
        const gain = ctx.createGain()
        osc.connect(gain)
        gain.connect(ctx.destination)
        osc.frequency.value = 880
        gain.gain.setValueAtTime(0.05, ctx.currentTime)
        osc.start()
        osc.stop(ctx.currentTime + 0.08)
        osc.onended = () => ctx.close()
      } catch { /* audio not available */ }
    }
</script>

<style scoped>
  .pulse {
    animation: pulse 1.8s ease-in-out infinite;
  }

  @keyframes pulse {
    0%, 100% {
      opacity: 1;
    }

    50% {
      opacity: 0.45;
    }
  }

  @media (prefers-reduced-motion: reduce) {
    .pulse {
      animation: none;
    }
  }
</style>
