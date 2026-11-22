import { ref } from 'vue'

// Общее реактивное состояние списка заказов на клиенте.
// Поля id и number приходят из ответа API, на странице они не вычисляются.
export const items = ref([])

export function setItems(newItems) {
  items.value = Array.isArray(newItems) ? [...newItems] : []
}

export function addCreatedItem(createdItem) {
  if (createdItem && typeof createdItem === 'object') {
    items.value.push(createdItem)
  }
}
