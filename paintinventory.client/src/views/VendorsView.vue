<template>
  <v-container fluid>
    <div class="d-flex align-center mb-3">
      <h2 class="text-h6">Locations &amp; vendors</h2>
      <v-spacer />
      <v-btn color="primary" prepend-icon="mdi-plus" @click="add">Add</v-btn>
    </div>
    <v-card>
      <v-data-table-virtual :headers="headers" :items="vendors" :loading="loading" density="comfortable">
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
          <v-btn size="small" variant="text" icon="mdi-pencil" @click="edit(item)" />
        </template>
      </v-data-table-virtual>
    </v-card>

    <VendorForm v-model="showForm" :vendor="editing" @saved="reload" />
  </v-container>
</template>

<script setup>
    import { onMounted, ref } from 'vue'
    import { storeToRefs } from 'pinia'
    import { useVendorStore } from '@/store/vendor'
    import VendorForm from '@/components/vendors/VendorForm.vue'

    const vendorStore = useVendorStore()
    const { vendors, loading } = storeToRefs(vendorStore)

    const showForm = ref(false)
    const editing = ref(null)

    const headers = [
        { title: 'Name', key: 'name' },
        { title: 'Own', key: 'isOwnCompany' },
        { title: 'Stock', key: 'storesStock' },
        { title: 'Blasting', key: 'doesBlasting' },
        { title: 'Painting', key: 'doesPainting' },
        { title: '', key: 'actions', sortable: false, align: 'end' }
    ]

    function add() {
        editing.value = null
        showForm.value = true
    }

    function edit(v) {
        editing.value = v
        showForm.value = true
    }

    function reload() {
        vendorStore.load(true)
    }

    onMounted(reload)
</script>
