<script setup lang="ts">
import { ShieldCheckIcon, KeyRoundIcon, PlusIcon } from '@lucide/vue'
import { computed, onMounted, ref, shallowRef } from 'vue'

import {
  kullaniciAktifligiDegistir,
  kullaniciEkle,
  kullanicilariGetir,
  kullaniciRolleriniGuncelle,
  parolaSifirla,
  rolleriGetir,
  type Kullanici,
  type KullaniciFilter,
  type Rol,
} from '@/api/kullanicilarApi'
import { Yetkiler } from '@/common/auth/yetkiler'
import CalisanSelect from '@/common/components/CalisanSelect.vue'
import AktiflikSwitch from '@/common/components/AktiflikSwitch.vue'
import ConfirmDialog from '@/common/components/ConfirmDialog.vue'
import FormDialog from '@/common/components/FormDialog.vue'
import PageHeader from '@/common/components/PageHeader.vue'
import { useFormSubmit } from '@/common/composables/useFormSubmit'
import { usePagedList } from '@/common/composables/usePagedList'
import { useToast } from '@/common/composables/useToast'
import { rolEtiketi, rolSecenekleri } from '@/common/utils/etiketler'
import { useAuthStore } from '@/stores/authStore'

const auth = useAuthStore()
const toast = useToast()
const ekleme = useFormSubmit()
const rolDuzenleme = useFormSubmit()
const sifirlama = useFormSubmit()

const varsayilanFiltre: KullaniciFilter = { arama: '', aktif: null }
const { filtre, sayfa, sayfaBoyutu, satirlar, toplam, yukleniyor, yukle } = usePagedList(
  kullanicilariGetir,
  varsayilanFiltre,
)
const roller = shallowRef<Rol[]>([])
const korumaliRolAtayabilir = auth.yetkiVar(Yetkiler.RolYonetimi)
const korumaliRoller = computed(() =>
  roller.value.filter((rol) => rol.korumali).map((rol) => rol.ad),
)
const dokunulamaz = (kullanici: Kullanici) =>
  !korumaliRolAtayabilir && kullanici.roller.some((rol) => korumaliRoller.value.includes(rol))
const atanabilirRoller = computed(() =>
  roller.value.filter((rol) => korumaliRolAtayabilir || !rol.korumali).map((rol) => rol.ad),
)

const eklemeAcik = ref(false)
const yeniKullanici = ref({ eposta: '', calisanId: null as number | null, roller: [] as string[] })
const rolAcik = ref(false)
const secilen = ref<Kullanici | null>(null)
const secilenRoller = ref<string[]>([])
const sifirlamaAcik = ref(false)

const basliklar = [
  { title: 'E-posta', key: 'eposta' },
  { title: 'Çalışan', key: 'calisanAdSoyad', value: (k: Kullanici) => k.calisanAdSoyad ?? '—' },
  { title: 'Roller', key: 'roller', sortable: false },
  { title: 'Aktif', key: 'aktif', sortable: false },
  { title: '', key: 'islemler', sortable: false, align: 'end' as const },
]
const aktiflikSecenekleri = [
  { title: 'Aktif', value: true },
  { title: 'Pasif', value: false },
]

function eklemeyiAc() {
  yeniKullanici.value = { eposta: '', calisanId: null, roller: [] }
  eklemeAcik.value = true
}

async function ekle() {
  const sonuc = await ekleme.gonder(() => kullaniciEkle(yeniKullanici.value))
  if (!sonuc) return
  eklemeAcik.value = false
  toast.basariGoster('Kullanıcı açıldı. Geçici parola e-postayla gönderildi.')
  await yukle()
}

function rolleriAc(kullanici: Kullanici) {
  secilen.value = kullanici
  secilenRoller.value = [...kullanici.roller]
  rolAcik.value = true
}

async function rolleriKaydet() {
  const kullanici = secilen.value
  if (!kullanici) return
  const basarili = await rolDuzenleme.gonder(async () => {
    await kullaniciRolleriniGuncelle(kullanici.id, secilenRoller.value)
    return true
  })
  if (!basarili) return
  rolAcik.value = false
  toast.basariGoster('Roller güncellendi.')
  await yukle()
}

function sifirlamayiSor(kullanici: Kullanici) {
  secilen.value = kullanici
  sifirlamaAcik.value = true
}

async function sifirla() {
  const kullanici = secilen.value
  if (!kullanici) return
  const basarili = await sifirlama.gonder(async () => {
    await parolaSifirla(kullanici.id)
    return true
  })
  if (!basarili) return
  sifirlamaAcik.value = false
  toast.basariGoster('Geçici parola e-postayla gönderildi.')
  await yukle()
}

async function aktiflikDegistir(kullanici: Kullanici, aktif: boolean) {
  await kullaniciAktifligiDegistir(kullanici.id, aktif)
  toast.basariGoster(aktif ? 'Kullanıcı aktif yapıldı.' : 'Kullanıcı pasif yapıldı.')
  await yukle()
}

onMounted(async () => {
  roller.value = await rolleriGetir()
})
</script>

<template>
  <v-container fluid>
    <PageHeader baslik="Kullanıcılar">
      <v-btn color="primary" :prepend-icon="PlusIcon" @click="eklemeyiAc">Yeni kullanıcı</v-btn>
    </PageHeader>

    <v-row density="compact" class="mb-2">
      <v-col cols="12" md="6">
        <v-text-field v-model="filtre.arama" label="E-posta ya da ad ara" clearable hide-details />
      </v-col>
      <v-col cols="12" md="3">
        <v-select
          v-model="filtre.aktif"
          :items="aktiflikSecenekleri"
          label="Durum"
          clearable
          hide-details
        />
      </v-col>
    </v-row>

    <v-card>
      <v-data-table-server
        v-model:page="sayfa"
        v-model:items-per-page="sayfaBoyutu"
        :headers="basliklar"
        :items="satirlar"
        :items-length="toplam"
        :loading="yukleniyor"
        :items-per-page-options="[10, 20, 50, 100]"
        no-data-text="Kullanıcı bulunamadı."
      >
        <template #[`item.eposta`]="{ item }">
          {{ item.eposta }}
          <v-chip v-if="item.parolaDegistirmeli" size="x-small" class="ml-1">Geçici parola</v-chip>
        </template>
        <template #[`item.roller`]="{ item }">
          <v-chip v-for="rol in item.roller" :key="rol" size="small" class="mr-1">{{
            rolEtiketi(rol)
          }}</v-chip>
        </template>
        <template #[`item.aktif`]="{ item }">
          <AktiflikSwitch
            :aktif="item.aktif"
            :etiket="`${item.eposta} aktif`"
            :kilitli="dokunulamaz(item)"
            :degistir="(aktif) => aktiflikDegistir(item, aktif)"
          />
        </template>
        <template #[`item.islemler`]="{ item }">
          <v-btn
            :icon="ShieldCheckIcon"
            variant="text"
            size="small"
            :aria-label="`${item.eposta} rollerini düzenle`"
            :disabled="dokunulamaz(item)"
            @click="rolleriAc(item)"
          />
          <v-btn
            :icon="KeyRoundIcon"
            variant="text"
            size="small"
            :aria-label="`${item.eposta} parolasını sıfırla`"
            :disabled="dokunulamaz(item)"
            @click="sifirlamayiSor(item)"
          />
        </template>
      </v-data-table-server>
    </v-card>

    <FormDialog
      v-model="eklemeAcik"
      baslik="Yeni kullanıcı"
      :kaydediliyor="ekleme.gonderiliyor.value"
      @kaydet="ekle"
    >
      <v-text-field
        v-model="yeniKullanici.eposta"
        label="E-posta"
        type="email"
        :error-messages="ekleme.alanHatasi('eposta')"
      />
      <CalisanSelect
        v-model="yeniKullanici.calisanId"
        etiket="Bağlı çalışan (isteğe bağlı)"
        :hatalar="ekleme.alanHatasi('calisanId')"
      />
      <v-select
        v-model="yeniKullanici.roller"
        :items="rolSecenekleri(atanabilirRoller)"
        label="Roller"
        multiple
        chips
        :error-messages="ekleme.alanHatasi('roller')"
      />
    </FormDialog>

    <FormDialog
      v-model="rolAcik"
      :baslik="`Roller: ${secilen?.eposta ?? ''}`"
      :kaydediliyor="rolDuzenleme.gonderiliyor.value"
      @kaydet="rolleriKaydet"
    >
      <v-select
        v-model="secilenRoller"
        :items="rolSecenekleri(atanabilirRoller)"
        label="Roller"
        multiple
        chips
        :error-messages="rolDuzenleme.alanHatasi('roller')"
      />
    </FormDialog>

    <ConfirmDialog
      v-model="sifirlamaAcik"
      baslik="Parolayı sıfırla"
      :metin="`${secilen?.eposta ?? ''} için geçici parola üretilip e-postayla gönderilecek; ilk girişte değiştirmesi istenecek.`"
      onay-metni="Sıfırla"
      onay-rengi="error"
      :calisiyor="sifirlama.gonderiliyor.value"
      @onay="sifirla"
    />
  </v-container>
</template>
