// Учебный mock-модуль (имитация ответов сервера ASP.NET Core)
// Переключатель для проверки ошибки 500 (Шаг B и Шаг G методички)
export const force500 = false

// Переключатель для проверки пустого списка [] со статусом 200 (Шаг D методички)
export const forceEmpty = false

const DELAY_MS = 500

function wait(ms = DELAY_MS) {
  return new Promise((resolve) => setTimeout(resolve, ms))
}

let nextId = 5

const mockItems = [
  {
    id: 1,
    number: 'ЗК-2026-0001',
    title: 'Контроллеры для линии фасовки',
    modelId: 1,
    description: '10 шт., интерфейс Modbus RTU, исполнение для цеха с повышенной температурой',
    status: 'New'
  },
  {
    id: 2,
    number: 'ЗК-2026-0002',
    title: 'Модули аналогового ввода для насосной станции',
    modelId: 4,
    description: '4 шт., входы 4–20 мА, монтаж на DIN-рейку, поставка вместе с паспортами',
    status: 'InProgress'
  },
  {
    id: 3,
    number: 'ЗК-2026-0003',
    title: 'Контроллер локальной автоматики КСУ',
    modelId: 5,
    description: '2 шт., интерфейс CAN, комплект кабелей подключения',
    status: 'Closed'
  },
  {
    id: 4,
    number: 'ЗК-2026-0004',
    title: 'Партия ПЛК-120 для котельной',
    modelId: 2,
    description: '5 шт., интерфейс Modbus TCP; заказ отменён заказчиком до начала сборки',
    status: 'Cancelled'
  }
]

export async function mockGetTickets() {
  await wait()

  if (force500) {
    return {
      ok: false,
      status: 500,
      data: { message: 'Сервер не смог обработать запрос' }
    }
  }

  if (forceEmpty) {
    return {
      ok: true,
      status: 200,
      data: []
    }
  }

  return {
    ok: true,
    status: 200,
    data: [...mockItems]
  }
}

export async function mockCreateTicket(body) {
  await wait()

  if (force500) {
    return {
      ok: false,
      status: 500,
      data: { message: 'Сервер не смог обработать запрос' }
    }
  }

  const cleanTitle = typeof body?.title === 'string' ? body.title.trim() : ''
  const modelId = Number(body?.modelId ?? body?.siteId)
  const cleanDescription = typeof body?.description === 'string' ? body.description.trim() : ''

  if (
    cleanTitle.length < 5 ||
    cleanTitle.length > 80 ||
    !Number.isInteger(modelId) ||
    modelId <= 0 ||
    cleanDescription.length < 10 ||
    cleanDescription.length > 500
  ) {
    return {
      ok: false,
      status: 400,
      data: { message: 'Проверьте поля' }
    }
  }

  const id = nextId++
  const number = `ЗК-2026-${String(id).padStart(4, '0')}`

  const created = {
    id,
    number,
    title: cleanTitle,
    modelId,
    description: cleanDescription,
    status: 'New'
  }

  mockItems.push(created)

  return {
    ok: true,
    status: 201,
    data: created
  }
}
