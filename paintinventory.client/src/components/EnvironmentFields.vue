<script setup>
import { watch, computed } from 'vue'
import { dewPoint, belowDewMargin } from '@/utils/dewpoint'

const props = defineProps({
  model: { type: Object, required: true }
})

watch(
  () => [props.model.airTempC, props.model.humidityPct],
  () => {
    const dp = dewPoint(props.model.airTempC, props.model.humidityPct)
    if (dp !== null) props.model.dewPointC = dp
  }
)

const warn = computed(() => belowDewMargin(props.model.substrateTempC, props.model.dewPointC))
</script>

<template>
  <div>
    <v-row density="compact">
      <v-col cols="6" sm="3">
        <v-text-field v-model.number="model.humidityPct" label="Humidity %" type="number" variant="outlined" density="comfortable" />
      </v-col>
      <v-col cols="6" sm="3">
        <v-text-field v-model.number="model.airTempC" label="Air °C" type="number" variant="outlined" density="comfortable" />
      </v-col>
      <v-col cols="6" sm="3">
        <v-text-field v-model.number="model.substrateTempC" label="Substrate °C" type="number" variant="outlined" density="comfortable" />
      </v-col>
      <v-col cols="6" sm="3">
        <v-text-field v-model.number="model.dewPointC" label="Dew point °C" type="number" variant="outlined" density="comfortable" hint="Auto from humidity + air" persistent-hint />
      </v-col>
    </v-row>
    <v-alert v-if="warn" type="warning" variant="tonal" density="compact" class="mt-1">
      Substrate is within 3 °C of the dew point — painting is not recommended.
    </v-alert>
  </div>
</template>
