<template>
  <v-dialog :model-value="modelValue" :fullscreen="xs" max-width="560" scrollable @update:model-value="close">
    <v-card v-if="product">
      <v-card-title class="d-flex align-center py-3">
        <v-icon icon="mdi-information-outline" class="me-2" color="primary" />
        <span class="text-subtitle-1">Product details</span>
        <v-spacer />
        <v-btn icon="mdi-close" variant="text" aria-label="Close" @click="close" />
      </v-card-title>

      <v-divider />

      <v-card-text class="pt-4">
        <div class="text-h6">{{ product.productName }}</div>
        <div class="text-body-2 text-medium-emphasis mb-3">
          GTIN {{ product.gtin }}<span v-if="product.itemCode"> · {{ product.itemCode }}</span>
        </div>

        <div class="d-flex flex-wrap ga-2 mb-4">
          <v-chip size="small" variant="tonal" color="primary">{{ product.brand }}</v-chip>
          <v-chip size="small" variant="tonal">{{ typeLabel }}</v-chip>
          <v-chip v-if="product.component !== 'Single'" size="small" variant="tonal" color="secondary">
            {{ product.component === 'PartA' ? 'Part A' : 'Part B' }}
          </v-chip>
          <v-chip v-if="product.hazardFlags" size="small" variant="tonal" color="error" prepend-icon="mdi-alert">
            {{ product.hazardFlags }}
          </v-chip>
        </div>

        <template v-for="group in groups" :key="group.title">
          <div v-if="group.rows.length" class="mb-4">
            <div class="text-overline text-medium-emphasis">{{ group.title }}</div>
            <v-list density="compact" class="py-0">
              <v-list-item v-for="row in group.rows" :key="row.label" class="px-0" min-height="36">
                <div class="d-flex justify-space-between ga-4 text-body-1">
                  <span class="text-medium-emphasis">{{ row.label }}</span>
                  <span class="text-end font-weight-medium">{{ row.value }}</span>
                </div>
              </v-list-item>
            </v-list>
          </div>
        </template>

        <div v-if="product.msdsUrl || product.tdsUrl" class="d-flex flex-wrap ga-2">
          <v-btn v-if="product.tdsUrl" :href="product.tdsUrl" target="_blank" rel="noopener"
                 variant="tonal" color="primary" size="large" prepend-icon="mdi-file-document-outline">
            Technical data sheet
          </v-btn>
          <v-btn v-if="product.msdsUrl" :href="product.msdsUrl" target="_blank" rel="noopener"
                 variant="tonal" color="error" size="large" prepend-icon="mdi-file-alert-outline">
            Safety data sheet
          </v-btn>
        </div>
      </v-card-text>

      <v-divider />

      <v-card-actions class="pa-3">
        <v-spacer />
        <v-btn color="primary" variant="flat" size="large" class="px-6" @click="close">Scan next</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
  import { computed } from 'vue'
  import { useDisplay } from 'vuetify'

  const props = defineProps({
    modelValue: { type: Boolean, default: false },
    product: { type: Object, default: null }
  })
  const emit = defineEmits(['update:modelValue'])

  const { xs } = useDisplay()

  const typeLabels = { Coating: 'Coating', Base: 'Base', CuringAgent: 'Curing agent', Thinner: 'Thinner', Cleaner: 'Cleaner' }
  const glossLabels = { Matt: 'Matt', Eggshell: 'Eggshell', SemiGloss: 'Semi-gloss', Gloss: 'Gloss', FullGloss: 'Full gloss' }

  const typeLabel = computed(() => typeLabels[props.product?.productType] ?? props.product?.productType)

  const range = (min, max, unit) => {
    if (min == null && max == null) return null
    if (min != null && max != null) return `${min}–${max} ${unit}`
    return `${min ?? max} ${unit}`
  }

  function rows(pairs) {
    return pairs.filter(([, v]) => v !== null && v !== undefined && v !== '').map(([label, value]) => ({ label, value }))
  }

  const groups = computed(() => {
    const p = props.product
    if (!p) return []
    return [
      {
        title: 'Product',
        rows: rows([
          ['Family', p.productFamily],
          ['Technology', p.technology],
          ['Category', [p.category, p.subCategory].filter(Boolean).join(' · ')],
          ['Pack', p.packVolume != null ? `${p.packVolume} ${p.unit ?? ''}`.trim() : null],
          ['Partner component', p.partnerProductName]
        ])
      },
      {
        title: 'Appearance',
        rows: rows([
          ['Colour', p.colour],
          ['RAL', p.ralCode],
          ['Gloss', glossLabels[p.glossLevel] ?? p.glossLevel]
        ])
      },
      {
        title: 'Application',
        rows: rows([
          ['Mix ratio', p.mixRatio],
          ['Pot life', p.potLifeMinutes != null ? `${p.potLifeMinutes} min` : null],
          ['Dry film thickness', range(p.dftMinUm, p.dftMaxUm, 'µm')],
          ['Wet film thickness', range(p.wftMinUm, p.wftMaxUm, 'µm')],
          ['Coverage', range(p.coverageMinM2L, p.coverageMaxM2L, 'm²/L')],
          ['Volume solids', p.volumeSolidsPct != null ? `${p.volumeSolidsPct} %` : null],
          ['VOC', p.vocGramsPerLitre != null ? `${p.vocGramsPerLitre} g/L` : null],
          ['Temperature resistance', p.temperatureResistance],
          ['Thinner', p.thinnerProductName],
          ['Cleaner', p.cleanerProductName]
        ])
      },
      {
        title: 'Storage & safety',
        rows: rows([
          ['Shelf life', p.shelfLifeMonths != null ? `${p.shelfLifeMonths} months` : null],
          ['UN number', p.unNumber]
        ])
      }
    ]
  })

  function close() {
    emit('update:modelValue', false)
  }
</script>
