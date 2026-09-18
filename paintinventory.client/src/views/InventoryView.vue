<script setup>
import { onMounted, ref } from 'vue'
import { storeToRefs } from 'pinia'
import { useInventoryStore } from '@/store/inventory'
import InventoryTable from '@/components/InventoryTable.vue'
import ItemDialog from '@/components/ItemDialog.vue'

const store = useInventoryStore()
const { items, loading } = storeToRefs(store)

const dialog = ref(false)
const editing = ref(null)

function add() {
  editing.value = null
  dialog.value = true
}

function edit(item) {
  editing.value = item
  dialog.value = true
}

async function saved() {
  dialog.value = false
  await store.loadInventory()
}

onMounted(store.loadInventory)
</script>

<template>
  <v-container fluid>
    <InventoryTable :items="items"
                    :loading="loading"
                    @add="add"
                    @edit="edit" />
    <ItemDialog v-model="dialog" :item="editing" @saved="saved" />
  </v-container>
</template>
