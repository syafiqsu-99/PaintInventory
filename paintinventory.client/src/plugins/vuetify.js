import 'vuetify/styles'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'

export default createVuetify({
  components,
  directives,
  icons: { defaultSet: 'mdi' },
  theme: {
    defaultTheme: 'app',
    themes: {
      app: {
        dark: false,
        colors: {
          primary: '#1565C0',
          secondary: '#455A64',
          surface: '#FFFFFF',
          background: '#F5F7FA',
          error: '#C62828',
          success: '#2E7D32',
          warning: '#EF6C00',
          'on-surface': '#1A1A1A'
        }
      }
    }
  }
})
