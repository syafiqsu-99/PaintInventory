<template>
  <div>
    <v-card class="mb-4">
      <v-card-title class="text-subtitle-1">Export &amp; templates</v-card-title>
      <v-divider />
      <v-card-text>
        <v-select v-model="entity"
                  :items="entities"
                  label="Table"
                  variant="outlined"
                  density="comfortable"
                  hide-details
                  class="mb-4"
                  style="max-width: 280px"
                  @update:model-value="resetImport" />

        <div class="d-flex flex-wrap ga-2">
          <v-btn color="secondary" variant="tonal" prepend-icon="mdi-download" :href="store.exportUrl(entity)" target="_blank">
            Export CSV
          </v-btn>
          <v-btn variant="text" prepend-icon="mdi-file-download-outline" :href="store.templateUrl(entity)" target="_blank">
            Download template
          </v-btn>
        </div>
      </v-card-text>
    </v-card>

    <v-card>
      <v-card-title class="text-subtitle-1">Import CSV</v-card-title>
      <v-divider />
      <v-card-text>
        <v-file-input v-model="file"
                      accept=".csv,text/csv"
                      label="Choose a CSV file"
                      prepend-icon="mdi-paperclip"
                      variant="outlined"
                      density="comfortable"
                      show-size
                      class="mb-2"
                      @update:model-value="onFile" />

        <div class="d-flex ga-2 mb-4">
          <v-btn color="primary" variant="tonal" :loading="previewing" :disabled="!file" @click="runPreview">
            Preview
          </v-btn>
        </div>

        <template v-if="preview">
          <div class="d-flex flex-wrap ga-2 mb-3">
            <v-chip variant="tonal">{{ preview.total }} rows</v-chip>
            <v-chip color="success" variant="tonal">{{ preview.newCount }} new</v-chip>
            <v-chip color="warning" variant="tonal">{{ preview.duplicateCount }} duplicate</v-chip>
            <v-chip color="error" variant="tonal">{{ preview.invalidCount }} invalid</v-chip>
          </div>

          <v-alert v-if="preview.duplicateCount || preview.invalidCount"
                   type="warning"
                   variant="tonal"
                   density="comfortable"
                   class="mb-3">
            Duplicates and invalid rows are highlighted below. Invalid rows are always skipped. Choose how to
            handle duplicates before importing.
          </v-alert>

          <v-data-table :headers="previewHeaders"
                        :items="preview.rows"
                        density="compact"
                        items-per-page="10"
                        class="mb-4">
            <template #[`item.status`]="{ item }">
              <v-chip :color="statusColor(item.status)" size="x-small" label variant="flat">{{ item.status }}</v-chip>
            </template>
            <template #[`item.key`]="{ item }">
              {{ keyValue(item) }}
            </template>
            <template #[`item.message`]="{ item }">
              <span class="text-medium-emphasis">{{ item.message || '' }}</span>
            </template>
          </v-data-table>

          <v-radio-group v-model="mode" inline hide-details class="mb-3">
            <v-radio label="Skip duplicates" value="SkipDuplicates" />
            <v-radio label="Update existing" value="UpdateDuplicates" />
          </v-radio-group>

          <v-btn color="primary" :loading="committing" :disabled="!preview.newCount && mode === 'SkipDuplicates'" @click="runCommit">
            Import {{ importable }} row{{ importable === 1 ? '' : 's' }}
          </v-btn>
        </template>

        <v-alert v-if="result" type="success" variant="tonal" density="comfortable" class="mt-4">
          Imported into {{ result.entity }}: {{ result.inserted }} added, {{ result.updated }} updated,
          {{ result.skipped }} skipped, {{ result.invalid }} invalid.
        </v-alert>
      </v-card-text>
    </v-card>
  </div>
</template>

<script setup>
  import { ref, computed } from 'vue'
  import { useImportExportStore } from '@/store/importExport'
  import { useProductStore } from '@/store/product'
  import { useVendorStore } from '@/store/vendor'
  import { useUiStore } from '@/store/ui'

  const store = useImportExportStore()
  const productStore = useProductStore()
  const vendorStore = useVendorStore()
  const ui = useUiStore()

  const entities = [
    { title: 'Products', value: 'products' },
    { title: 'Locations', value: 'vendors' }
  ]

  const entity = ref('products')
  const file = ref(null)
  const preview = ref(null)
  const result = ref(null)
  const mode = ref('SkipDuplicates')
  const previewing = ref(false)
  const committing = ref(false)

  const previewHeaders = computed(() => [
    { title: 'Row', key: 'rowNumber', width: 70 },
    { title: 'Status', key: 'status', sortable: false },
    { title: entity.value === 'products' ? 'GTIN' : 'Name', key: 'key', sortable: false },
    { title: 'Note', key: 'message', sortable: false }
  ])

  const importable = computed(() => {
    if (!preview.value) return 0
    return mode.value === 'UpdateDuplicates'
      ? preview.value.newCount + preview.value.duplicateCount
      : preview.value.newCount
  })

  function keyValue(item) {
    return entity.value === 'products' ? item.values.Gtin : item.values.Name
  }

  function statusColor(status) {
    return status === 'New' ? 'success' : status === 'Duplicate' ? 'warning' : 'error'
  }

  function resetImport() {
    file.value = null
    preview.value = null
    result.value = null
  }

  function onFile() {
    preview.value = null
    result.value = null
  }

  async function runPreview() {
    if (!file.value) return
    previewing.value = true
    result.value = null
    try {
      preview.value = await store.preview(entity.value, file.value)
    } catch (e) {
      ui.error(e.message)
    } finally {
      previewing.value = false
    }
  }

  async function runCommit() {
    if (!file.value) return
    committing.value = true
    try {
      result.value = await store.commit(entity.value, file.value, mode.value)
      ui.notify('Import complete.')
      preview.value = null
      file.value = null
      if (entity.value === 'products') productStore.load()
      else vendorStore.load(true)
    } catch (e) {
      ui.error(e.message)
    } finally {
      committing.value = false
    }
  }
</script>
