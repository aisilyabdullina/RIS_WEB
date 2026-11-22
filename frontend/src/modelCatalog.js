// Справочник моделей контроллеров для отображения на клиенте
export const controllerModels = [
  { id: 1, name: 'ПЛК-110' },
  { id: 2, name: 'ПЛК-120' },
  { id: 3, name: 'МВ-8DI8DO' },
  { id: 4, name: 'МВ-8AI' },
  { id: 5, name: 'КСУ-300' }
]

export function modelName(id) {
  return controllerModels.find((m) => m.id === id)?.name ?? `Модель №${id}`
}
