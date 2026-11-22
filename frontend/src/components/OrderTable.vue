<script setup>
import { modelName } from '../modelCatalog.js'
import { statusLabel } from '../statuses.js'

defineProps({
  items: {
    type: Array,
    required: true
  }
})
</script>

<template>
  <table class="orders-table">
    <thead>
      <tr>
        <th scope="col">ID</th>
        <th scope="col">Номер</th>
        <th scope="col">Наименование</th>
        <th scope="col">Модель</th>
        <th scope="col">Описание</th>
        <th scope="col">Статус</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="item in items" :key="item.id">
        <td>{{ item.id }}</td>
        <td class="mono">{{ item.number }}</td>
        <td>{{ item.title }}</td>
        <td>{{ modelName(item.modelId) }}</td>
        <td>{{ item.description }}</td>
        <td>
          <span class="status-badge" :data-status="item.status">
            {{ statusLabel(item.status) }}
          </span>
        </td>
      </tr>
    </tbody>
  </table>
</template>

<style scoped>
.orders-table {
  width: 100%;
  border-collapse: collapse;
  background-color: #ffffff;
  border: 1px solid #d1d5db;
}

.orders-table th,
.orders-table td {
  padding: 12px 14px;
  text-align: left;
  border-bottom: 1px solid #e5e7eb;
}

.orders-table th {
  background-color: #f9fafb;
  font-weight: 600;
  color: #374151;
}

.mono {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  font-weight: 600;
}

.status-badge {
  display: inline-block;
  padding: 4px 10px;
  font-size: 0.82rem;
  font-weight: 600;
  border-radius: 4px;
  background-color: #e5e7eb;
  color: #1f2937;
}

.status-badge[data-status='New'] {
  background-color: #dbeafe;
  color: #1e40af;
}

.status-badge[data-status='InProgress'] {
  background-color: #fef3c7;
  color: #92400e;
}

.status-badge[data-status='Closed'] {
  background-color: #d1fae5;
  color: #065f46;
}

.status-badge[data-status='Cancelled'] {
  background-color: #fee2e2;
  color: #991b1b;
}
</style>
