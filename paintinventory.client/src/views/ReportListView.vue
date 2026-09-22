<template>
  <v-container fluid>
    <div class="d-flex align-center mb-3">
      <h2 class="text-h6">Paint reports</h2>
      <v-spacer />
      <v-btn color="primary" prepend-icon="mdi-plus" to="/reports/new">New report</v-btn>
    </div>
    <v-card>
      <v-data-table-virtual :headers="headers" :items="reports" :loading="loading" density="comfortable">
        <template #[`item.createdAt`]="{ item }">
          {{ fmtDate(item.createdAt) }}
        </template>
        <template #[`item.actions`]="{ item }">
          <v-btn size="small"
                 variant="text"
                 icon="mdi-file-pdf-box"
                 color="error"
                 :href="`/api/reports/${item.id}/pdf`"
                 target="_blank"
                 title="Download PDF" />
          <v-btn size="small" variant="text" icon="mdi-pencil" :to="`/reports/${item.id}`" />
          <v-btn size="small" variant="text" icon="mdi-delete" color="error" @click="remove(item)" />
        </template>
      </v-data-table-virtual>
    </v-card>
  </v-container>
</template>

<script setup>
  import { onMounted } from 'vue'
  import { storeToRefs } from 'pinia'
  import { useReportStore } from '@/store/report'
  import { useUiStore } from '@/store/ui'

  const reportStore = useReportStore()
  const ui = useUiStore()
  const { reports, loading } = storeToRefs(reportStore)

  const headers = [
      { title: 'IPO', key: 'ipo' },
      { title: 'Customer', key: 'customer' },
      { title: 'Project', key: 'project' },
      { title: 'Items', key: 'itemCount', align: 'end' },
      { title: 'Prepared by', key: 'preparedBy' },
      { title: 'Created', key: 'createdAt' },
      { title: '', key: 'actions', sortable: false, align: 'end' }
  ]

  function reload() {
      reportStore.load()
  }

  async function remove(item) {
      if (!confirm(`Delete report ${item.ipo}?`)) return
      try {
        await reportStore.remove(item.id)
        ui.notify('Report deleted.')
        reload()
      } catch (e) {
        ui.error(e.message)
      }
  }

  function fmtDate(s) {
      return s ? new Date(s).toLocaleDateString() : ''
  }

  onMounted(reload)
</script>
