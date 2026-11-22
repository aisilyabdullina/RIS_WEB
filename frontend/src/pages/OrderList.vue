<script setup>
import { ref, onMounted } from 'vue'
import OrderTable from '../components/OrderTable.vue'
import { items, setItems } from '../ordersStore.js'
import { getOrders } from '../api/orders.js'

// Четыре взаимоисключающих состояния списка: 'loading' | 'empty' | 'data' | 'error'
const screen = ref('loading')
const errorMessage = ref('')

async function loadOrders() {
  screen.value = 'loading'
  errorMessage.value = ''

  try {
    const data = await getOrders()
    setItems(data)
    screen.value = data.length === 0 ? 'empty' : 'data'
  } catch (err) {
    errorMessage.value = err?.message || 'Нет связи с сервером'
    screen.value = 'error'
  }
}

onMounted(() => {
  loadOrders()
})
</script>

<template>
  <section aria-labelledby="list-heading">
    <h2 id="list-heading">Список заказов на контроллеры</h2>

    <div v-if="screen === 'loading'" class="state-card state-loading" role="status">
      Загрузка…
    </div>

    <div v-else-if="screen === 'empty'" class="state-card state-empty">
      Записей пока нет
    </div>

    <div v-else-if="screen === 'error'" class="state-card state-error" role="alert">
      <p class="error-title">{{ errorMessage }}</p>
      <button type="button" class="retry-btn" @click="loadOrders">
        Повторить попытку
      </button>
    </div>

    <OrderTable v-else :items="items" />
  </section>
</template>

<style scoped>
h2 {
  margin-top: 0;
  font-size: 1.3rem;
  color: #111827;
}


.state-card {
  padding: 24px;
  border-radius: 8px;
  border: 1px solid #d1d5db;
  background-color: #f9fafb;
  font-size: 1rem;
  font-weight: 500;
  color: #374151;
}

.state-loading {
  background-color: #eff6ff;
  border-color: #bfdbfe;
  color: #1e40af;
}

.state-empty {
  background-color: #f9fafb;
  color: #4b5563;
}

.state-error {
  background-color: #fef2f2;
  border-color: #fecaca;
  color: #991b1b;
}

.error-title {
  margin: 0 0 12px;
  font-weight: 600;
}

.retry-btn {
  padding: 8px 14px;
  font-size: 0.9rem;
  font-weight: 600;
  color: #991b1b;
  background-color: #ffffff;
  border: 1px solid #f87171;
  border-radius: 6px;
  cursor: pointer;
}

.retry-btn:hover {
  background-color: #fee2e2;
}
</style>
