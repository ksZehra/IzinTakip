<script setup lang="ts">
import { ArrowLeftIcon, PencilIcon, PlusIcon, Trash2Icon } from '@lucide/vue'
import { ref, shallowRef, watch } from 'vue'

import {
  bakiyeGetir,
  calisanAktifligiDegistir,
  calisaniGetir,
  calisanGuncelle,
  hakEkle,
  hakGuncelle,
  haklariGetir,
  hakSil,
  type Bakiye,
  type Calisan,
  type IzinHaklari,
} from '@/api/calisanlarApi'
import { departmanlariGetir, type Departman } from '@/api/departmanlarApi'
import ConfirmDialog from '@/common/components/ConfirmDialog.vue'
import FormDialog from '@/common/components/FormDialog.vue'
import PageHeader from '@/common/components/PageHeader.vue'
import { ApiError } from '@/common/http/ApiError'
import SureInput from '@/common/components/SureInput.vue'
import { useCrudForm } from '@/common/composables/useCrudForm'
import { useFormSubmit } from '@/common/composables/useFormSubmit'
import { useToast } from '@/common/composables/useToast'
import { tarihGoster } from '@/common/utils/tarih'

import CalisanForm from './CalisanForm.vue'
import { bosCalisanFormu, calisanFormaCevir } from './calisanFormu'

const props = defineProps<{ id: number }>()

const toast = useToast()
const kayit = useFormSubmit()
const aktiflik = useFormSubmit()
const hakSilme = useFormSubmit()

type HakSatiri = IzinHaklari['satirlar'][number]
interface HakFormu {
  dakika: number | null
  aciklama: string
  kazanimTarihi: string
}

const calisan = shallowRef<Calisan | null>(null)
const bakiye = shallowRef<Bakiye | null>(null)
const haklar = shallowRef<IzinHaklari | null>(null)
const departmanlar = shallowRef<Departman[]>([])

const duzenlemeAcik = ref(false)
const form = ref(bosCalisanFormu())
const aktiflikAcik = ref(false)
const silinecekHak = ref<HakSatiri | null>(null)
const hakSilmeAcik = ref(false)

function hakIstegi(formu: HakFormu) {
  return {
    dakika: formu.dakika ?? 0,
    aciklama: formu.aciklama,
    kazanimTarihi: formu.kazanimTarihi || null,
  }
}

const {
  acik: hakAcik,
  form: hakFormu,
  yeniKayit: hakYeni,
  yeni: hakEklemeyiAc,
  duzenle: hakDuzenle,
  kaydet: hakKaydet,
  gonderiliyor: hakGonderiliyor,
  alanHatasi: hakAlanHatasi,
} = useCrudForm<HakSatiri, HakFormu>({
  bosForm: () => ({ dakika: 0, aciklama: '', kazanimTarihi: '' }),
  formaCevir: (satir) => ({
    dakika: satir.dakika,
    aciklama: satir.aciklama,
    kazanimTarihi: satir.kazanimTarihi,
  }),
  ekle: (formu) => hakEkle(props.id, hakIstegi(formu)),
  guncelle: (hakId, formu) => hakGuncelle(props.id, hakId, hakIstegi(formu)),
  kaydedildi: () => yukle(),
})

const hakBasliklari = [
  {
    title: 'Tarih',
    key: 'kazanimTarihi',
    value: (h: { kazanimTarihi: string }) => tarihGoster(h.kazanimTarihi),
  },
  { title: 'Süre', key: 'sureMetni' },
  {
    title: 'Kaynak',
    key: 'kaynak',
    value: (h: { kaynak: string }) => (h.kaynak === 'Kural' ? 'Otomatik' : 'Elle'),
  },
  { title: 'Açıklama', key: 'aciklama' },
  { title: '', key: 'islemler', sortable: false, align: 'end' as const },
]

const yuklenemedi = ref(false)

async function yukle() {
  yuklenemedi.value = false
  try {
    ;[calisan.value, bakiye.value, haklar.value] = await Promise.all([
      calisaniGetir(props.id),
      bakiyeGetir(props.id),
      haklariGetir(props.id),
    ])
  } catch (hata) {
    if (!(hata instanceof ApiError)) throw hata
    yuklenemedi.value = true
  }
}

async function duzenlemeyiAc() {
  if (!calisan.value) return
  departmanlar.value = await departmanlariGetir()
  form.value = calisanFormaCevir(calisan.value)
  duzenlemeAcik.value = true
}

async function kaydet() {
  const sonuc = await kayit.gonder(() => calisanGuncelle(props.id, form.value))
  if (!sonuc) return
  duzenlemeAcik.value = false
  toast.basariGoster('Kaydedildi.')
  await yukle()
}

function hakSilmeyiSor(satir: HakSatiri) {
  silinecekHak.value = satir
  hakSilmeAcik.value = true
}

async function hakSilOnayla() {
  const satir = silinecekHak.value
  if (!satir) return
  const basarili = await hakSilme.gonder(async () => {
    await hakSil(props.id, satir.id)
    return true
  })
  if (!basarili) return
  hakSilmeAcik.value = false
  toast.basariGoster('Hak satırı silindi.')
  await yukle()
}

async function aktifligiDegistir() {
  const yeniDurum = !calisan.value?.aktif
  const basarili = await aktiflik.gonder(async () => {
    await calisanAktifligiDegistir(props.id, yeniDurum)
    return true
  })
  if (!basarili) return
  aktiflikAcik.value = false
  toast.basariGoster(yeniDurum ? 'Çalışan aktif yapıldı.' : 'Çalışan pasif yapıldı.')
  await yukle()
}

watch(() => props.id, yukle, { immediate: true })
</script>

<template>
  <v-container style="max-width: 1100px">
    <PageHeader
      :baslik="calisan ? `${calisan.ad} ${calisan.soyad}` : 'Çalışan'"
      :aciklama="
        calisan ? `${calisan.departmanAdi} · ${calisan.aktif ? 'Aktif' : 'Pasif'}` : undefined
      "
    >
      <v-btn variant="text" :prepend-icon="ArrowLeftIcon" :to="{ name: 'calisanlar' }"
        >Çalışanlar</v-btn
      >
      <v-btn v-if="calisan" variant="tonal" :prepend-icon="PencilIcon" @click="duzenlemeyiAc">
        Düzenle
      </v-btn>
      <v-btn v-if="calisan" variant="text" @click="aktiflikAcik = true">
        {{ calisan.aktif ? 'Pasif yap' : 'Aktif yap' }}
      </v-btn>
    </PageHeader>

    <v-alert v-if="yuklenemedi" type="warning" variant="tonal">
      Kayıt açılamadı. Silinmiş ya da görme yetkiniz olmayabilir. Çalışan listesinden tekrar seçin.
    </v-alert>
    <v-skeleton-loader v-else-if="!calisan" type="article" />

    <v-row v-else>
      <v-col cols="12" md="4">
        <v-card title="Bilgiler" class="mb-4">
          <v-card-text>
            <dl class="d-flex flex-column ga-2">
              <div>
                <dt class="text-medium-emphasis">İşe giriş</dt>
                <dd>{{ tarihGoster(calisan.iseGirisTarihi) }}</dd>
              </div>
              <div>
                <dt class="text-medium-emphasis">Doğum tarihi</dt>
                <dd>{{ tarihGoster(calisan.dogumTarihi) }}</dd>
              </div>
              <div>
                <dt class="text-medium-emphasis">Hak başlangıç kıdem yılı</dt>
                <dd>{{ calisan.hakBaslangicKidemYili }}</dd>
              </div>
              <div>
                <dt class="text-medium-emphasis">Kullanıcı hesabı</dt>
                <dd>{{ calisan.kullaniciEposta ?? 'Yok' }}</dd>
              </div>
            </dl>
          </v-card-text>
        </v-card>
        <v-card v-if="bakiye" title="Bakiye">
          <v-card-text>
            <p class="text-headline-small text-primary">{{ bakiye.kullanilabilirMetni }}</p>
            <p class="text-body-medium text-medium-emphasis">
              Hak {{ bakiye.hakMetni }} · Kullanılan {{ bakiye.kullanilanMetni }} · Bekleyen
              {{ bakiye.bekleyenMetni }}
            </p>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" md="8">
        <v-card>
          <v-card-item>
            <v-card-title>Hak satırları</v-card-title>
            <v-card-subtitle v-if="haklar">Toplam {{ haklar.toplamSureMetni }}</v-card-subtitle>
            <template #append>
              <v-btn :prepend-icon="PlusIcon" variant="tonal" @click="hakEklemeyiAc"
                >Hak satırı ekle</v-btn
              >
            </template>
          </v-card-item>
          <v-data-table
            :headers="hakBasliklari"
            :items="haklar?.satirlar ?? []"
            items-per-page="-1"
            hide-default-footer
            no-data-text="Hak satırı yok."
          >
            <template #[`item.islemler`]="{ item }">
              <template v-if="item.kaynak === 'Elle'">
                <v-btn
                  :icon="PencilIcon"
                  variant="text"
                  size="small"
                  :aria-label="`${item.aciklama} düzenle`"
                  @click="hakDuzenle(item)"
                />
                <v-btn
                  :icon="Trash2Icon"
                  variant="text"
                  size="small"
                  color="error"
                  :aria-label="`${item.aciklama} sil`"
                  @click="hakSilmeyiSor(item)"
                />
              </template>
            </template>
          </v-data-table>
        </v-card>
      </v-col>
    </v-row>

    <CalisanForm
      v-model="duzenlemeAcik"
      v-model:form="form"
      baslik="Çalışanı Düzenle"
      :departmanlar="departmanlar"
      :kaydediliyor="kayit.gonderiliyor.value"
      :alan-hatasi="kayit.alanHatasi"
      @kaydet="kaydet"
    />

    <FormDialog
      v-model="hakAcik"
      :baslik="hakYeni ? 'Hak Satırı Ekle' : 'Hak Satırını Düzenle'"
      :kaydediliyor="hakGonderiliyor"
      @kaydet="hakKaydet"
    >
      <SureInput
        v-model="hakFormu.dakika"
        etiket="Süre"
        eksi-olabilir
        :hatalar="hakAlanHatasi('dakika')"
      />
      <v-text-field
        v-model="hakFormu.aciklama"
        label="Açıklama"
        required
        hint="Örn. devreden bakiye, düzeltme"
        :error-messages="hakAlanHatasi('aciklama')"
      />
      <v-text-field
        v-model="hakFormu.kazanimTarihi"
        label="Tarih (Boşsa Bugün)"
        type="date"
        :error-messages="hakAlanHatasi('kazanimTarihi')"
      />
    </FormDialog>

    <ConfirmDialog
      v-model="hakSilmeAcik"
      baslik="Hak Satırını Sil"
      :metin="`${silinecekHak?.sureMetni ?? ''} süreli “${silinecekHak?.aciklama ?? ''}” satırı silinecek. Eski değeri işlem kayıtlarında kalır.`"
      onay-metni="Sil"
      onay-rengi="error"
      :calisiyor="hakSilme.gonderiliyor.value"
      @onay="hakSilOnayla"
    />

    <ConfirmDialog
      v-if="calisan"
      v-model="aktiflikAcik"
      :baslik="calisan.aktif ? 'Çalışanı Pasif Yap' : 'Çalışanı Aktif Yap'"
      :metin="
        calisan.aktif
          ? 'Pasif çalışan talep açamaz; bağlı kullanıcı hesabı da kapanır.'
          : 'Kullanıcı hesabı kendiliğinden açılmaz; kullanıcı yönetiminden açılır.'
      "
      :onay-metni="calisan.aktif ? 'Pasif yap' : 'Aktif yap'"
      :onay-rengi="calisan.aktif ? 'error' : 'primary'"
      :calisiyor="aktiflik.gonderiliyor.value"
      @onay="aktifligiDegistir"
    />
  </v-container>
</template>
