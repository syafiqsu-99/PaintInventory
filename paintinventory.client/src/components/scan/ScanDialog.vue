<template>
  <v-dialog :model-value="modelValue"
            :fullscreen="xs"
            max-width="560"
            persistent
            scrollable
            @update:model-value="close">
    <v-card v-if="product">
      <v-card-title class="d-flex align-center py-3">
        <v-icon icon="mdi-barcode-scan" class="me-2" />
        <span class="text-subtitle-1">Record movement</span>
        <v-spacer />
        <v-btn icon="mdi-close" variant="text" @click="close(false)" />
      </v-card-title>

      <v-divider />

      <v-card-text class="pt-4">
        <div class="scan-summary pa-3 mb-4 rounded">
          <div class="text-subtitle-1 font-weight-medium">
            {{ product.productName }}
            <span v-if="product.component !== 'Single'" class="text-medium-emphasis">
              — {{ product.component === 'PartA' ? 'Part A' : 'Part B' }}
            </span>
          </div>
          <div class="text-body-2 text-medium-emphasis mt-1">
            GTIN {{ product.gtin }}
            <span v-if="product.itemCode"> · {{ product.itemCode }}</span>
            <span v-if="product.packVolume"> · {{ product.packVolume }} {{ product.unit }}</span>
          </div>
          <div v-if="!isStaff" class="text-body-2 mt-1">
            <v-icon icon="mdi-map-marker" size="16" /> {{ me?.vendorName }}
          </div>
        </div>

        <v-btn-toggle v-model="form.direction" mandatory divided class="mb-4 w-100 direction-toggle">
          <v-btn v-for="d in directions"
                 :key="d.value"
                 :value="d.value"
                 :disabled="d.disabled"
                 :prepend-icon="d.icon"
                 class="flex-grow-1">
            {{ d.title }}
          </v-btn>
        </v-btn-toggle>

        <v-form v-model="valid">
          <v-select v-if="isStaff"
                    v-model="form.vendorId"
                    :items="stockLocations"
                    label="Location"
                    :rules="[rules.required]"
                    variant="outlined"
                    density="comfortable"
                    class="mb-2" />

          <template v-if="isReceive">
            <v-select v-model="form.transactionId"
                      :items="pendingItems"
                      label="Incoming transfer"
                      :rules="[rules.required]"
                      variant="outlined"
                      density="comfortable"
                      class="mb-2" />
            <v-text-field v-model.number="form.quantity"
                          label="Received qty (cans)"
                          type="number"
                          inputmode="decimal"
                          :hint="selectedPending ? `Sent: ${selectedPending.quantity}` : ''"
                          persistent-hint
                          :rules="[rules.required, rules.positive, rules.notAboveSent]"
                          variant="outlined"
                          density="comfortable"
                          class="mb-2" />
            <ScanTextField v-model="form.batch"
                           label="Batch / lot"
                           scan-key="batch"
                           class="mb-2" />
          </template>

          <template v-else>
            <v-text-field v-model.number="form.quantity"
                          label="Quantity (cans)"
                          type="number"
                          inputmode="decimal"
                          :rules="[rules.required, rules.positive]"
                          variant="outlined"
                          density="comfortable"
                          autofocus
                          class="mb-2" />
            <ScanTextField v-model="form.batch"
                           label="Batch / lot"
                           scan-key="batch"
                           class="mb-2" />

            <template v-if="form.direction === 'in'">
              <ScanDateField v-model="form.manufacturingDate"
                             label="Mfg date"
                             scan-key="manufacturingDate"
                             :max="today"
                             class="mb-2" />
              <ScanDateField v-model="form.bestBefore"
                             label="Best before"
                             scan-key="bestBefore"
                             :min="form.manufacturingDate"
                             :hint="product.tracksExpiry ? 'Tracked for expiry alerts · DD.MM.YYYY' : ''"
                             :persistent-hint="product.tracksExpiry"
                             class="mb-2" />
              <v-text-field v-model="form.source" label="Source" variant="outlined" density="comfortable" class="mb-2" />
            </template>
          </template>

          <v-text-field v-model="form.notes" label="Notes" variant="outlined" density="comfortable" />
        </v-form>
      </v-card-text>

      <v-divider />

      <v-card-actions class="pa-3">
        <v-btn variant="text" size="large" @click="close(false)">Cancel</v-btn>
        <v-spacer />
        <v-btn :color="submitColor"
               :loading="saving"
               :disabled="!valid"
               size="large"
               variant="flat"
               class="px-6"
               @click="submit">
          {{ submitLabel }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
  import { ref, watch, computed } from 'vue'
  import { useDisplay } from 'vuetify'
  import { storeToRefs } from 'pinia'
  import { useVendorStore } from '@/store/vendor'
  import { useStockStore } from '@/store/stock'
  import { useSettingsStore } from '@/store/settings'
  import { useAuthStore } from '@/store/auth'
  import { useUiStore } from '@/store/ui'
  import { useScanCapture } from '@/composables/useScanFields'
  import { formatDisplayDate } from '@/utils/scanParse'
  import ScanTextField from '@/components/common/ScanTextField.vue'
  import ScanDateField from '@/components/common/ScanDateField.vue'

  const props = defineProps({
    modelValue: { type: Boolean, default: false },
    product: { type: Object, default: null },
    pending: { type: Array, default: () => [] },
    initialTransactionId: { type: Number, default: null }
  })
  const emit = defineEmits(['update:modelValue', 'done'])

  const { xs } = useDisplay()
  const vendorStore = useVendorStore()
  const stock = useStockStore()
  const settings = useSettingsStore()
  const auth = useAuthStore()
  const ui = useUiStore()
  const capture = useScanCapture()
  const { vendors } = storeToRefs(vendorStore)
  const { prefs } = storeToRefs(settings)
  const { me, isStaff } = storeToRefs(auth)

  const valid = ref(false)
  const saving = ref(false)
  const form = ref(blank())

  const now = new Date()
  const today = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`

  const directions = computed(() => isStaff.value
    ? [
        { value: 'in', title: 'Stock in', icon: 'mdi-tray-arrow-down' },
        { value: 'out', title: 'Stock out', icon: 'mdi-tray-arrow-up' }
      ]
    : [
        { value: 'receive', title: 'Receive', icon: 'mdi-truck-check-outline', disabled: !props.pending.length },
        { value: 'out', title: 'Use', icon: 'mdi-format-paint' }
      ])

  const isReceive = computed(() => form.value.direction === 'receive')

  const stockLocations = computed(() =>
    vendors.value.filter((v) => v.storesStock).map((v) => ({ title: v.name, value: v.id })))

  const pendingItems = computed(() => props.pending.map((t) => ({
    title: `${t.quantity} from ${t.fromVendorName} · ${formatDisplayDate(t.sentAt?.slice(0, 10))}${t.batch ? ` · ${t.batch}` : ''}`,
    value: t.transactionId
  })))

  const selectedPending = computed(() => props.pending.find((t) => t.transactionId === form.value.transactionId) ?? null)

  const submitLabel = computed(() => ({ in: 'Add to stock', out: isStaff.value ? 'Issue stock' : 'Record usage', receive: 'Confirm receipt' })[form.value.direction])
  const submitColor = computed(() => (form.value.direction === 'out' ? 'primary' : 'success'))

  const rules = {
    required: (v) => (v !== null && v !== undefined && String(v).trim() !== '') || 'Required',
    positive: (v) => Number(v) > 0 || 'Must be greater than 0',
    notAboveSent: (v) => !selectedPending.value || Number(v) <= selectedPending.value.quantity || `Cannot exceed ${selectedPending.value.quantity} sent`
  }

  function defaultDirection() {
    if (isStaff.value) return prefs.value.scanDefaultDirection || 'out'
    return props.pending.length ? 'receive' : 'out'
  }

  function blank() {
    return {
      direction: defaultDirection(),
      vendorId: isStaff.value ? prefs.value.defaultLocationId : auth.vendorId,
      transactionId: null,
      quantity: null,
      batch: null,
      source: null,
      manufacturingDate: null,
      bestBefore: null,
      notes: null
    }
  }

  function applyPending(transactionId) {
    const t = props.pending.find((p) => p.transactionId === transactionId)
    if (!t) return
    form.value.quantity = t.quantity
    form.value.batch = t.batch
  }

  watch(() => props.modelValue, (open) => {
    if (!open) return
    form.value = blank()
    capture.reset()
    if (form.value.direction === 'receive') {
      form.value.transactionId = props.initialTransactionId ?? props.pending[0]?.transactionId ?? null
      applyPending(form.value.transactionId)
    }
  })

  watch(() => form.value.transactionId, (id, previous) => {
    if (id !== previous && isReceive.value) applyPending(id)
  })

  watch(() => form.value.direction, (dir) => {
    if (dir === 'receive' && !form.value.transactionId) {
      form.value.transactionId = props.pending[0]?.transactionId ?? null
      applyPending(form.value.transactionId)
    }
  })

  function close(value) {
    emit('update:modelValue', value === true)
  }

  async function submit() {
    if (!valid.value || !props.product) return
    saving.value = true
    const direction = form.value.direction
    const scanRaw = capture.payload()
    try {
      let result
      if (direction === 'receive') {
        result = await stock.receive({
          transactionId: form.value.transactionId,
          receivedQty: form.value.quantity,
          batch: form.value.batch || null,
          notes: form.value.notes || null,
          scanRaw
        })
      } else {
        const base = {
          productId: props.product.id,
          vendorId: form.value.vendorId,
          quantity: form.value.quantity,
          batch: form.value.batch || null,
          shade: props.product.colour ?? props.product.ralCode ?? null,
          notes: form.value.notes || null,
          scanRaw
        }
        result = direction === 'in'
          ? await stock.stockIn({
              ...base,
              packVolume: props.product.packVolume ?? null,
              source: form.value.source || null,
              manufacturingDate: form.value.manufacturingDate || null,
              bestBefore: form.value.bestBefore || null
            })
          : await stock.stockOut(base)
      }

      const verb = { in: 'Added', out: isStaff.value ? 'Issued' : 'Used', receive: 'Received' }[direction]
      ui.notify(`${verb} ${form.value.quantity} · ${props.product.productName} — on hand ${result.onHandQty}`)
      emit('done', {
        time: new Date(),
        direction,
        productName: props.product.productName,
        quantity: form.value.quantity,
        onHandQty: result.onHandQty,
        vendorName: result.vendorName
      })
      close(false)
    } catch (e) {
      ui.error(e.message)
    } finally {
      saving.value = false
    }
  }
</script>

<style scoped>
  .scan-summary {
    background: rgb(var(--v-theme-background));
    border: 1px solid rgba(var(--v-border-color), 0.12);
  }

  .direction-toggle {
    height: 48px;
  }
</style>
