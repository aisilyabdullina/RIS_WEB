import { ref } from 'vue'

// Общее реактивное состояние списка заказов на клиенте (имя файла из методички: «ticket» = заказ на контроллер)
// В ЛР6 id и number приходят только из ответа API/mock, а не вычисляются на странице
export const items = ref([])

export function setItems(newItems) {
  items.value = Array.isArray(newItems) ? [...newItems] : []
}

export function addCreatedItem(createdItem) {
  if (createdItem && typeof createdItem === 'object') {
    items.value.push(createdItem)
  }
}
