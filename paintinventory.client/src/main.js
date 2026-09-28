import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import vuetify from './plugins/vuetify'
import { onUnauthorized } from './utils/http'
import { useAuthStore } from './store/auth'
import '@mdi/font/css/materialdesignicons.css'

const pinia = createPinia()

onUnauthorized(() => {
  useAuthStore(pinia).clear()
  if (router.currentRoute.value.name !== 'login') router.push({ name: 'login' })
})

createApp(App)
  .use(pinia)
  .use(router)
  .use(vuetify)
  .mount('#app')
