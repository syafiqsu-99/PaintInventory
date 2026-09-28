<template>
  <v-container class="py-6 px-4">
    <v-row justify="center">
      <v-col cols="12" sm="8" md="5" lg="4">
        <LoginForm />
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
  import LoginForm from '@/components/auth/LoginForm.vue'
</script>
