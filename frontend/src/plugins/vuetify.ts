import '@fontsource-variable/roboto/wght.css'
import 'vuetify/styles/core'
import '@/styles/vuetify-utilities.scss'
import '@/styles/main.css'

import { createVuetify } from 'vuetify'
import { aliases, mdi } from 'vuetify/iconsets/mdi-svg'
import { tr } from 'vuetify/locale'

const tabloVarsayilanlari = { mobile: null, mobileBreakpoint: 'sm' }
const formAlaniVarsayilanlari = { variant: 'outlined', density: 'comfortable', hideDetails: 'auto' }

export function createAppVuetify() {
  return createVuetify({
    theme: {
      defaultTheme: 'light',
      themes: {
        light: {
          colors: {
            primary: '#1565C0',
            secondary: '#455A64',
            success: '#2E7D32',
            warning: '#9A5B00',
            error: '#C62828',
            info: '#01579B',
          },
          variables: {
            'medium-emphasis-opacity': 0.8,
          },
        },
      },
    },
    locale: { locale: 'tr', messages: { tr } },
    date: { locale: { tr: 'tr-TR' } },
    icons: { defaultSet: 'mdi', aliases, sets: { mdi } },
    defaults: {
      VDataTable: tabloVarsayilanlari,
      VDataTableServer: tabloVarsayilanlari,
      VTextField: formAlaniVarsayilanlari,
      VTextarea: formAlaniVarsayilanlari,
      VSelect: formAlaniVarsayilanlari,
      VAutocomplete: formAlaniVarsayilanlari,
      VNumberInput: formAlaniVarsayilanlari,
      VDateInput: formAlaniVarsayilanlari,
      VFileInput: formAlaniVarsayilanlari,
    },
  })
}
