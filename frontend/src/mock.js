// Тестовый "backend" в памяти браузера. Повторяет правила docs/api-contract.md.
// После обновления страницы заказы возвращаются к начальным (это нормально для ЛР3).

const delay = (ms = 150) => new Promise((r) => setTimeout(r, ms))

const users = [
  { id: 1, login: 'manager', password: '123456', fullName: 'Иванов Алексей Петрович', role: 'Manager' },
  { id: 2, login: 'engineer1', password: '123456', fullName: 'Петров Сергей Николаевич', role: 'Engineer' },
  { id: 3, login: 'engineer2', password: '123456', fullName: 'Сидорова Мария Андреевна', role: 'Engineer' },
  { id: 4, login: 'chief', password: '123456', fullName: 'Кузнецов Дмитрий Олегович', role: 'Chief' }
]

const models = [
  { id: 1, name: 'ПЛК-110', article: 'ИКС.421457.110', controllerType: 'ПЛК', ioCount: 16, interface: 'Modbus RTU', stockQty: 12 },
  { id: 2, name: 'ПЛК-120', article: 'ИКС.421457.120', controllerType: 'ПЛК', ioCount: 32, interface: 'Modbus TCP', stockQty: 5 },
  { id: 3, name: 'МВ-8DI8DO', article: 'ИКС.426439.008', controllerType: 'Модуль ввода-вывода', ioCount: 16, interface: 'Modbus RTU', stockQty: 20 },
  { id: 4, name: 'МВ-8AI', article: 'ИКС.426439.018', controllerType: 'Модуль ввода-вывода', ioCount: 8, interface: 'Modbus RTU', stockQty: 9 },
  { id: 5, name: 'КСУ-300', article: 'ИКС.421457.300', controllerType: 'Контроллер локальной автоматики', ioCount: 24, interface: 'CAN', stockQty: 3 }
]

let orders = [
  {
    id: 1, number: 'ORD-2026-001', title: 'Контроллеры для линии фасовки',
    description: '10 шт., интерфейс Modbus RTU, исполнение для цеха с повышенной температурой',
    status: 'New', modelId: 1, createdByUserId: 1, assigneeUserId: null,
    createdAt: '2026-10-12T09:15:00Z', updatedAt: '2026-10-12T09:15:00Z'
  },
  {
    id: 2, number: 'ORD-2026-002', title: 'Модули ввода-вывода для насосной',
    description: '4 шт., аналоговые входы 4–20 мА, монтаж на DIN-рейку',
    status: 'InProgress', modelId: 4, createdByUserId: 1, assigneeUserId: 2,
    createdAt: '2026-10-13T11:40:00Z', updatedAt: '2026-10-14T08:05:00Z'
  },
  {
    id: 3, number: 'ORD-2026-003', title: 'Контроллер локальной автоматики КСУ',
    description: '2 шт., интерфейс CAN, комплект с кабелями подключения',
    status: 'Closed', modelId: 5, createdByUserId: 1, assigneeUserId: 3,
    createdAt: '2026-10-05T14:20:00Z', updatedAt: '2026-10-10T16:30:00Z'
  }
]

function currentUser() {
  const u = JSON.parse(localStorage.getItem('user') || 'null')
  if (!u) throw new Error('Требуется вход в систему')
  return u
}

const clone = (o) => JSON.parse(JSON.stringify(o))
const findOrder = (id) => {
  const o = orders.find((x) => x.id === Number(id))
  if (!o) throw new Error(`Заказ с id=${id} не найден`)
  return o
}

export const mockApi = {
  async login(login, password) {
    await delay()
    const u = users.find((x) => x.login === login && x.password === password)
    if (!u) throw new Error('Неверный логин или пароль')
    return { token: `mock-token-${u.id}`, userId: u.id, role: u.role, fullName: u.fullName }
  },

  async getModels() {
    await delay()
    return clone(models)
  },

  async getEngineers() {
    await delay()
    return users.filter((u) => u.role === 'Engineer').map((u) => ({ id: u.id, fullName: u.fullName }))
  },

  async getOrders() {
    await delay()
    currentUser()
    return clone(orders)
  },

  async createOrder({ title, modelId, description }) {
    await delay()
    const me = currentUser()
    if (me.role !== 'Manager') throw new Error('Создавать заказы может только менеджер')
    if (!title || title.length < 5 || title.length > 80) throw new Error('Наименование: от 5 до 80 символов')
    if (!(Number(modelId) > 0)) throw new Error('Поле modelId должно быть больше 0')
    if (!description || description.length < 10 || description.length > 500) throw new Error('Описание: от 10 до 500 символов')
    const id = orders.length ? Math.max(...orders.map((o) => o.id)) + 1 : 1
    const now = new Date().toISOString()
    const order = {
      id, number: `ORD-2026-${String(id).padStart(3, '0')}`, title, description,
      status: 'New', modelId: Number(modelId), createdByUserId: me.userId, assigneeUserId: null,
      createdAt: now, updatedAt: now
    }
    orders.push(order)
    return clone(order)
  },

  async assign(id, assigneeUserId) {
    await delay()
    const me = currentUser()
    if (me.role !== 'Chief') throw new Error('Назначать исполнителя может только начальник участка')
    const o = findOrder(id)
    if (o.status !== 'New') throw new Error('Назначить исполнителя можно только на заказ в статусе «Принят»')
    o.assigneeUserId = Number(assigneeUserId)
    o.updatedAt = new Date().toISOString()
    return clone(o)
  },

  async setStatus(id, status) {
    await delay()
    const me = currentUser()
    const o = findOrder(id)
    const allowed = {
      New: ['InProgress', 'Cancelled'],
      InProgress: ['Closed', 'Cancelled'],
      Closed: [],
      Cancelled: []
    }
    if (!allowed[o.status].includes(status)) {
      throw new Error(`Нельзя перевести заказ из статуса '${o.status}' в '${status}'`)
    }
    const isOwnEngineer = me.role === 'Engineer' && o.assigneeUserId === me.userId
    const isChiefCancel = me.role === 'Chief' && status === 'Cancelled'
    if (!isOwnEngineer && !isChiefCancel) throw new Error('Недостаточно прав для смены статуса этого заказа')
    o.status = status
    o.updatedAt = new Date().toISOString()
    return clone(o)
  }
}
