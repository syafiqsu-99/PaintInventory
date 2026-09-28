<template>
  <v-dialog :model-value="modelValue" max-width="900" scrollable @update:model-value="emit('update:modelValue', $event)">
    <v-card>
      <v-card-title>{{ product ? 'Edit product' : 'New product' }}</v-card-title>
      <v-card-text style="max-height: 70vh">
        <v-form v-model="valid">
          <div class="text-overline text-medium-emphasis mt-1">Identity &amp; brand</div>
          <v-row dense>
            <v-col cols="12" sm="6"><v-text-field v-model="form.gtin" label="GTIN" :rules="[rules.required]" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="12" sm="6"><v-text-field v-model="form.itemCode" label="Item code" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="12" sm="6"><v-text-field v-model="form.productName" label="Product name" :rules="[rules.required]" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="12" sm="6"><v-text-field v-model="form.description" label="Description" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="12" sm="4"><v-select v-model="form.brand" :items="brandOptions" label="Brand" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="12" sm="4"><v-text-field v-model="form.productFamily" label="Product family" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="6" sm="2"><v-text-field v-model.number="form.packVolume" label="Pack volume" type="number" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="6" sm="2"><v-text-field v-model="form.unit" label="Unit" variant="outlined" density="comfortable" /></v-col>
          </v-row>

          <div class="text-overline text-medium-emphasis mt-2">Classification</div>
          <v-row dense>
            <v-col cols="12" sm="4"><v-select v-model="form.productType" :items="productTypeOptions" label="Product type" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="12" sm="4"><v-select v-model="form.component" :items="componentOptions" label="Component" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="12" sm="4"><v-text-field v-model="form.technology" label="Technology (epoxy, PU…)" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="12" sm="4"><v-text-field v-model="form.category" label="Category (primer/topcoat…)" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="12" sm="4"><v-text-field v-model="form.subCategory" label="Sub-category" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="12" sm="4"><v-select v-model="form.glossLevel" :items="glossOptions" label="Gloss level" variant="outlined" density="comfortable" clearable /></v-col>
            <v-col cols="12" sm="6"><v-text-field v-model="form.colour" label="Colour" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="12" sm="6"><v-text-field v-model="form.ralCode" label="RAL code" variant="outlined" density="comfortable" /></v-col>
          </v-row>

          <div class="text-overline text-medium-emphasis mt-2">Formulation &amp; application</div>
          <v-row dense>
            <v-col cols="6" sm="3"><v-text-field v-model="form.mixRatio" label="Mix ratio A:B" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="6" sm="3"><v-text-field v-model.number="form.potLifeMinutes" label="Pot life (min)" type="number" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="6" sm="3"><v-text-field v-model.number="form.volumeSolidsPct" label="Volume solids %" type="number" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="6" sm="3"><v-text-field v-model.number="form.vocGramsPerLitre" label="VOC (g/l)" type="number" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="12" sm="6"><v-select v-model="form.thinnerProductId" :items="productOptions" label="Recommended thinner" variant="outlined" density="comfortable" clearable /></v-col>
            <v-col cols="12" sm="6"><v-select v-model="form.cleanerProductId" :items="productOptions" label="Cleaner" variant="outlined" density="comfortable" clearable /></v-col>
            <v-col cols="6" sm="3"><v-text-field v-model.number="form.dftMinUm" label="DFT min (µm)" type="number" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="6" sm="3"><v-text-field v-model.number="form.dftMaxUm" label="DFT max (µm)" type="number" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="6" sm="3"><v-text-field v-model.number="form.wftMinUm" label="WFT min (µm)" type="number" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="6" sm="3"><v-text-field v-model.number="form.wftMaxUm" label="WFT max (µm)" type="number" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="6" sm="3"><v-text-field v-model.number="form.coverageMinM2L" label="Coverage min (m²/l)" type="number" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="6" sm="3"><v-text-field v-model.number="form.coverageMaxM2L" label="Coverage max (m²/l)" type="number" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="12" sm="6"><v-text-field v-model="form.temperatureResistance" label="Temperature resistance" variant="outlined" density="comfortable" /></v-col>
          </v-row>

          <div class="text-overline text-medium-emphasis mt-2">Handling &amp; documents</div>
          <v-row dense>
            <v-col cols="6" sm="3"><v-text-field v-model.number="form.shelfLifeMonths" label="Shelf life (months)" type="number" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="6" sm="3"><v-text-field v-model="form.unNumber" label="UN number" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="12" sm="6"><v-text-field v-model="form.hazardFlags" label="Hazard flags" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="12" sm="6"><v-text-field v-model="form.tdsUrl" label="TDS URL" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="12" sm="6"><v-text-field v-model="form.msdsUrl" label="MSDS/SDS URL" variant="outlined" density="comfortable" /></v-col>
            <v-col cols="12"><v-checkbox v-model="form.tracksExpiry" label="Track best-before / shelf life" density="comfortable" hide-details /></v-col>
          </v-row>
        </v-form>
      </v-card-text>
      <v-card-actions>
        <v-spacer />
        <v-btn variant="text" @click="emit('update:modelValue', false)">Cancel</v-btn>
        <v-btn color="primary" :loading="saving" :disabled="!valid" @click="save">Save</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
    import { ref, computed, watch, onMounted } from 'vue'
    import { storeToRefs } from 'pinia'
    import { useProductStore } from '@/store/product'
    import { useUiStore } from '@/store/ui'

    const props = defineProps({
      modelValue: { type: Boolean, default: false },
      product: { type: Object, default: null }
    })
    const emit = defineEmits(['update:modelValue', 'saved'])

    const products = useProductStore()
    const ui = useUiStore()
    const { products: productList } = storeToRefs(products)

    const brandOptions = ['Jotun', 'International']
    const productTypeOptions = [
      { title: 'Coating', value: 'Coating' },
      { title: 'Base', value: 'Base' },
      { title: 'Curing agent', value: 'CuringAgent' },
      { title: 'Thinner', value: 'Thinner' },
      { title: 'Cleaner', value: 'Cleaner' }
    ]
    const componentOptions = [
      { title: 'Single component', value: 'Single' },
      { title: 'Part A (base)', value: 'PartA' },
      { title: 'Part B (hardener)', value: 'PartB' }
    ]
    const glossOptions = ['Matt', 'Eggshell', 'SemiGloss', 'Gloss', 'FullGloss']

    const productOptions = computed(() =>
      productList.value
        .filter((p) => !props.product || p.id !== props.product.id)
        .map((p) => ({ title: p.productName, value: p.id })))

    const form = ref(blank())
    const saving = ref(false)
    const valid = ref(false)

    function blank() {
      return {
        gtin: '', itemCode: null, productName: '', description: null,
        brand: 'Jotun', productFamily: null, packVolume: null, unit: 'L',
        productType: 'Coating', component: 'Single',
        technology: null, category: null, subCategory: null, colour: null, ralCode: null, glossLevel: null,
        mixRatio: null, potLifeMinutes: null, thinnerProductId: null, cleanerProductId: null,
        volumeSolidsPct: null, vocGramsPerLitre: null,
        dftMinUm: null, dftMaxUm: null, wftMinUm: null, wftMaxUm: null,
        coverageMinM2L: null, coverageMaxM2L: null, temperatureResistance: null,
        shelfLifeMonths: null, partnerProductId: null,
        unNumber: null, hazardFlags: null, msdsUrl: null, tdsUrl: null,
        tracksExpiry: false
      }
    }

    watch(() => props.modelValue, (open) => {
      if (open) form.value = props.product ? { ...blank(), ...props.product } : blank()
    })

    onMounted(() => { if (!productList.value.length) products.load() })

    const rules = { required: (v) => (!!v && String(v).trim() !== '') || 'Required' }

    async function save() {
      if (!valid.value) return
      saving.value = true
      try {
        const payload = { ...form.value }
        Object.keys(payload).forEach((k) => { if (payload[k] === '') payload[k] = null })
        if (props.product) await products.update(props.product.id, payload)
        else await products.create(payload)
        ui.notify('Product saved.')
        emit('saved')
        emit('update:modelValue', false)
      } catch (e) {
        ui.error(e.message)
      } finally {
        saving.value = false
      }
    }
</script>
