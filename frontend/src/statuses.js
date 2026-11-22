// Коды статусов заказа (как в API) и их подписи на экране
export const STATUS_LABELS = {
  New: 'Принят',
  InProgress: 'В производстве',
  Closed: 'Отгружен',
  Cancelled: 'Отменён'
}

export function statusLabel(code) {
  return STATUS_LABELS[code] ?? code
}
