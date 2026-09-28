import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import vuetify from './plugins/vuetify'
import { onUnauthorized } from './utils/http'
import { useAuthStore } from './store/auth'
import '@mdi/font/css/materialdesignicons.css'

const pinia = createPinia()

onUnauthorized(() => useAuthStore(pinia).onDenied())

createApp(App)
  .use(pinia)
  .use(router)
  .use(vuetify)
  .mount('#app')
