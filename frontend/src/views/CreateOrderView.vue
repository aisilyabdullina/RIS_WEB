<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { api } from '../api'

const router = useRouter()
const models = ref([])
const title = ref('')
const modelId = ref(0)
const description = ref('')
const error = ref('')
const submitted = ref(false)
const loading = ref(false)

onMounted(async () => {
  try {
    models.value = await api.getModels()
  } catch (e) {
    error.value = e.message
  }
})

const errors = computed(() => {
  const e = {}
  const t = title.value.trim()
  const d = description.value.trim()
  if (t.length < 5 || t.length > 80) e.title = 'Наименование: от 5 до 80 символов'
  if (!(Number(modelId.value) > 0)) e.modelId = 'Выберите модель контроллера'
  if (d.length < 10 || d.length > 500) e.description = 'Описание: от 10 до 500 символов'
  return e
})

async function onSubmit() {
  submitted.value = true
  error.value = ''
  if (Object.keys(errors.value).length) return
  loading.value = true
  try {
    await api.createOrder({
      title: title.value.trim(),
      modelId: Number(modelId.value),
      description: description.value.trim()
    })
    router.push({ name: 'orders' })
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="card narrow">
    <h1>Новый заказ на контроллер</h1>

    <div class="field">
      <label for="title">Наименование заказа</label>
      <input id="title" v-model="title" placeholder="Например: Контроллеры для линии фасовки" />
      <small v-if="submitted && errors.title" class="error">{{ errors.title }}</small>
    </div>

    <div class="field">
      <label for="model">Модель контроллера</label>
      <select id="model" v-model="modelId">
        <option :value="0">— выберите модель —</option>
        <option v-for="m in models" :key="m.id" :value="m.id">
          {{ m.name }} ({{ m.controllerType }}, {{ m.interface }})
        </option>
      </select>
      <small v-if="submitted && errors.modelId" class="error">{{ errors.modelId }}</small>
    </div>

    <div class="field">
      <label for="description">Описание (комплектация, количество, требования заказчика)</label>
      <textarea id="description" v-model="description" rows="5"></textarea>
      <small class="muted">{{ description.trim().length }} / 500</small>
      <small v-if="submitted && errors.description" class="error">{{ errors.description }}</small>
    </div>

    <p v-if="error" class="error">{{ error }}</p>
    <div class="row">
      <button class="btn" :disabled="loading" @click="onSubmit">{{ loading ? 'Создание…' : 'Создать' }}</button>
      <router-link class="btn btn-light" :to="{ name: 'orders' }">Отмена</router-link>
    </div>
  </div>
</template>
