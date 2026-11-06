<script setup>
import { ref, computed, onMounted } from 'vue'
import { api } from '../api'
import { auth } from '../auth'
import { STATUS_LABELS } from '../statuses'

const orders = ref([])
const models = ref([])
const engineers = ref([])
const loading = ref(true)
const error = ref('')
const filterStatus = ref('')
const filterAssignee = ref('')
const pick = ref({}) // выбранный инженер для каждого заказа: { [orderId]: userId }

const me = computed(() => auth.user)
const modelName = (id) => models.value.find((m) => m.id === id)?.name ?? `#${id}`
const engineerName = (id) => engineers.value.find((u) => u.id === id)?.fullName ?? null
const fmtDate = (iso) => new Date(iso).toLocaleString('ru-RU', { dateStyle: 'short', timeStyle: 'short' })

async function load() {
  loading.value = true
  error.value = ''
  try {
    const [o, m, e] = await Promise.all([api.getOrders(), api.getModels(), api.getEngineers()])
    orders.value = o
    models.value = m
    engineers.value = e
  } catch (err) {
    error.value = err.message
  } finally {
    loading.value = false
  }
}
onMounted(load)

// Что видит каждая роль
const visible = computed(() => {
  let list = [...orders.value]
  if (me.value.role === 'Manager') {
    list = list.filter((o) => o.createdByUserId === me.value.userId)
  } else if (me.value.role === 'Engineer') {
    // только свои активные заказы
    list = list.filter(
      (o) => o.assigneeUserId === me.value.userId && (o.status === 'New' || o.status === 'InProgress')
    )
  } else {
    if (filterStatus.value) list = list.filter((o) => o.status === filterStatus.value)
    if (filterAssignee.value === 'none') list = list.filter((o) => o.assigneeUserId == null)
    else if (filterAssignee.value) list = list.filter((o) => o.assigneeUserId === Number(filterAssignee.value))
  }
  // новые сверху
  return list.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
})

async function run(action) {
  error.value = ''
  try {
    const updated = await action()
    const i = orders.value.findIndex((o) => o.id === updated.id)
    if (i !== -1) orders.value[i] = { ...orders.value[i], ...updated }
  } catch (err) {
    error.value = err.message
  }
}

const assign = (o) => {
  if (!pick.value[o.id]) {
    error.value = 'Выберите инженера из списка'
    return
  }
  return run(() => api.assign(o.id, Number(pick.value[o.id])))
}
const setStatus = (o, status) => run(() => api.setStatus(o.id, status))
</script>

<template>
  <div class="row between">
    <h1>{{ me.role === 'Engineer' ? 'Мои заказы в работе' : me.role === 'Manager' ? 'Мои заказы' : 'Все заказы' }}</h1>
    <router-link v-if="me.role === 'Manager'" class="btn" :to="{ name: 'order-new' }">+ Создать заказ</router-link>
  </div>

  <div v-if="me.role === 'Chief'" class="row filters">
    <select v-model="filterStatus">
      <option value="">Все статусы</option>
      <option v-for="(label, code) in STATUS_LABELS" :key="code" :value="code">{{ label }}</option>
    </select>
    <select v-model="filterAssignee">
      <option value="">Все исполнители</option>
      <option value="none">Не назначен</option>
      <option v-for="u in engineers" :key="u.id" :value="u.id">{{ u.fullName }}</option>
    </select>
  </div>

  <p v-if="error" class="error">{{ error }}</p>
  <p v-if="loading" class="muted">Загрузка…</p>
  <p v-else-if="!visible.length" class="muted">Заказов нет</p>

  <div v-else class="table-wrap">
    <table>
      <thead>
        <tr>
          <th>Номер</th>
          <th>Наименование</th>
          <th>Модель</th>
          <th>Статус</th>
          <th>Исполнитель</th>
          <th>Создан</th>
          <th>Действия</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="o in visible" :key="o.id">
          <td class="nowrap">{{ o.number }}</td>
          <td>
            <strong>{{ o.title }}</strong>
            <div class="muted small">{{ o.description }}</div>
          </td>
          <td>{{ modelName(o.modelId) }}</td>
          <td><span class="badge" :class="'st-' + o.status">{{ STATUS_LABELS[o.status] }}</span></td>
          <td>{{ engineerName(o.assigneeUserId) ?? '—' }}</td>
          <td class="nowrap">{{ fmtDate(o.createdAt) }}</td>
          <td class="actions">
            <!-- Начальник участка: назначение и отмена -->
            <template v-if="me.role === 'Chief'">
              <template v-if="o.status === 'New'">
                <select v-model="pick[o.id]">
                  <option :value="undefined">— инженер —</option>
                  <option v-for="u in engineers" :key="u.id" :value="u.id">{{ u.fullName }}</option>
                </select>
                <button class="btn btn-small" @click="assign(o)">Назначить</button>
              </template>
              <button
                v-if="o.status === 'New' || o.status === 'InProgress'"
                class="btn btn-small btn-danger"
                @click="setStatus(o, 'Cancelled')"
              >Отменить</button>
            </template>

            <!-- Инженер: смена статуса своих заказов -->
            <template v-else-if="me.role === 'Engineer'">
              <button v-if="o.status === 'New'" class="btn btn-small" @click="setStatus(o, 'InProgress')">В производство</button>
              <template v-if="o.status === 'InProgress'">
                <button class="btn btn-small" @click="setStatus(o, 'Closed')">Отгружен</button>
                <button class="btn btn-small btn-danger" @click="setStatus(o, 'Cancelled')">Отменить</button>
              </template>
            </template>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
