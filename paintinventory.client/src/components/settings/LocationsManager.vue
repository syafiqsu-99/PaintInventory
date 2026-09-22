<template>
  <div>
    <div class="d-flex align-center flex-wrap ga-2 mb-3">
      <v-text-field v-model="search"
                    placeholder="Search locations"
                    prepend-inner-icon="mdi-magnify"
                    variant="outlined"
                    density="comfortable"
                    hide-details
                    clearable
                    class="flex-grow-1"
                    style="min-width: 200px" />
      <v-btn color="primary" prepend-icon="mdi-plus" @click="add">Add location</v-btn>
    </div>

    <v-card>
      <v-data-table :headers="headers"
                    :items="vendors"
                    :search="search"
                    :loading="loading"
                    density="comfortable"
                    items-per-page="25">
        <template #[`item.isOwnCompany`]="{ item }">
          <v-icon v-if="item.isOwnCompany" icon="mdi-check" color="success" size="small" />
        </template>
        <template #[`item.storesStock`]="{ item }">
          <v-icon v-if="item.storesStock" icon="mdi-check" color="success" size="small" />
        </template>
        <template #[`item.doesBlasting`]="{ item }">
          <v-icon v-if="item.doesBlasting" icon="mdi-check" color="success" size="small" />
        </template>
        <template #[`item.doesPainting`]="{ item }">
          <v-icon v-if="item.doesPainting" icon="mdi-check" color="success" size="small" />
        </template>
        <template #[`item.actions`]="{ item }">
          <v-btn size="small" variant="text" icon="mdi-pencil" title="Edit" @click="edit(item)" />
        </template>
        <template #no-data>
          <div class="text-medium-emphasis py-8">No locations yet. Add one or import a CSV.</div>
        </template>
      </v-data-table>
    </v-card>

    <VendorForm v-model="showForm" :vendor="editing" @saved="reload" />
  </div>
</template>

<script setup>
  import { onMounted, ref } from 'vue'
  import { storeToRefs } from 'pinia'
  import { useVendorStore } from '@/store/vendor'
  import VendorForm from '@/components/vendors/VendorForm.vue'

  const vendorStore = useVendorStore()
  const { vendors, loading } = storeToRefs(vendorStore)

  const search = ref('')
  const showForm = ref(false)
  const editing = ref(null)

  const headers = [
    { title: 'Name', key: 'name' },
    { title: 'Own', key: 'isOwnCompany', align: 'center' },
    { title: 'Stock', key: 'storesStock', align: 'center' },
    { title: 'Blasting', key: 'doesBlasting', align: 'center' },
    { title: 'Painting', key: 'doesPainting', align: 'center' },
    { title: '', key: 'actions', sortable: false, align: 'end' }
  ]

  function add() { editing.value = null; showForm.value = true }
  function edit(v) { editing.value = v; showForm.value = true }
  function reload() { vendorStore.load(true) }

  onMounted(reload)
  defineExpose({ reload })
</script>
