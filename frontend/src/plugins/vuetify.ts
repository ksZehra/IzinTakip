import 'vuetify/styles/core'
import '@/styles/vuetify-utilities.scss'
import '@/styles/main.css'

import {
  ArrowDownIcon,
  ArrowUpIcon,
  CalendarIcon,
  CheckIcon,
  ChevronDownIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  ChevronsLeftIcon,
  ChevronsRightIcon,
  ChevronsUpDownIcon,
  ChevronUpIcon,
  CircleAlertIcon,
  CircleCheckIcon,
  CircleDotIcon,
  CircleIcon,
  CircleXIcon,
  InfoIcon,
  LoaderCircleIcon,
  MenuIcon,
  MinusIcon,
  PaperclipIcon,
  PencilIcon,
  PlusIcon,
  SquareCheckIcon,
  SquareIcon,
  SquareMinusIcon,
  TriangleAlertIcon,
  UploadIcon,
  XIcon,
} from '@lucide/vue'
import { createVuetify } from 'vuetify'
import { aliases, mdi } from 'vuetify/iconsets/mdi-svg'
import { tr } from 'vuetify/locale'

import { kayitliTema } from '@/common/utils/tema'

const tabloVarsayilanlari = { mobile: null, mobileBreakpoint: 'sm', hover: true }
const formAlaniVarsayilanlari = { variant: 'outlined', density: 'comfortable', hideDetails: 'auto' }

const ikonlar = {
  ...aliases,
  collapse: ChevronUpIcon,
  complete: CheckIcon,
  cancel: CircleXIcon,
  close: XIcon,
  delete: CircleXIcon,
  clear: CircleXIcon,
  success: CircleCheckIcon,
  info: InfoIcon,
  warning: TriangleAlertIcon,
  error: CircleAlertIcon,
  prev: ChevronLeftIcon,
  next: ChevronRightIcon,
  checkboxOn: SquareCheckIcon,
  checkboxOff: SquareIcon,
  checkboxIndeterminate: SquareMinusIcon,
  delimiter: CircleIcon,
  sortAsc: ArrowUpIcon,
  sortDesc: ArrowDownIcon,
  expand: ChevronDownIcon,
  menu: MenuIcon,
  subgroup: ChevronDownIcon,
  dropdown: ChevronDownIcon,
  radioOn: CircleDotIcon,
  radioOff: CircleIcon,
  edit: PencilIcon,
  loading: LoaderCircleIcon,
  first: ChevronsLeftIcon,
  last: ChevronsRightIcon,
  unfold: ChevronsUpDownIcon,
  file: PaperclipIcon,
  plus: PlusIcon,
  minus: MinusIcon,
  calendar: CalendarIcon,
  upload: UploadIcon,
} as typeof aliases

export function createAppVuetify() {
  return createVuetify({
    theme: {
      defaultTheme: kayitliTema(),
      themes: {
        light: {
          dark: false,
          colors: {
            background: '#F5F8FB',
            surface: '#FFFFFF',
            primary: '#3F6283',
            secondary: '#475569',
            success: '#166534',
            warning: '#92400E',
            error: '#B91C1C',
            info: '#3F6283',
            violet: '#6D28D9',
            pink: '#BE185D',
            amber: '#92400E',
            deep: '#4F7090',
          },
          variables: {
            'medium-emphasis-opacity': 0.8,
            'border-color': '#14263A',
            'border-opacity': 0.1,
          },
        },
        dark: {
          dark: true,
          colors: {
            background: '#0F1B28',
            surface: '#162436',
            primary: '#8FB0D0',
            'on-primary': '#0F1B28',
            secondary: '#9CA3AF',
            success: '#86EFAC',
            warning: '#FCD34D',
            error: '#FCA5A5',
            info: '#8FB0D0',
            violet: '#C4B5FD',
            pink: '#F9A8D4',
            amber: '#FCD34D',
            deep: '#25384E',
          },
          variables: {
            'medium-emphasis-opacity': 0.8,
            'border-color': '#FFFFFF',
            'border-opacity': 0.12,
          },
        },
      },
    },
    locale: { locale: 'tr', messages: { tr } },
    date: { locale: { tr: 'tr-TR' } },
    icons: { defaultSet: 'mdi', aliases: ikonlar, sets: { mdi } },
    defaults: {
      VCard: { rounded: 'lg', variant: 'flat', border: true },
      VAppBar: { flat: true, border: 'b' },
      VNavigationDrawer: { border: 'e' },
      VListItem: { rounded: 'lg' },
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
