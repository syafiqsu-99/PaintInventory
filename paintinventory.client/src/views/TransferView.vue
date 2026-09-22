<template>
  <v-container>
    <v-row justify="center">
      <v-col cols="12" md="8" lg="6">
        <StockTransferForm />
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
  import StockTransferForm from '@/components/stock/StockTransferForm.vue'
</script>
