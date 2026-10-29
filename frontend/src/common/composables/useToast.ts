import { ref } from 'vue'
import type { SnackbarQueueMessage } from 'vuetify'

const mesajlar = ref<SnackbarQueueMessage[]>([])

function ekle(text: string, color: string, timeout: number) {
  mesajlar.value.push({ text, color, timeout })
}

export function useToast() {
  return {
    mesajlar,
    basariGoster: (metin: string) => ekle(metin, 'success', 4000),
    bilgiGoster: (metin: string) => ekle(metin, 'info', 6000),
    hataGoster: (metin: string) => ekle(metin, 'error', 8000),
  }
}
