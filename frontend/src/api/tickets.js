import { mockGetTickets, mockCreateTicket } from './mockTickets.js'

// Единый переключатель источника данных (true — mock, false — реальный fetch к API)
export const USE_MOCK = true

const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'

function createHttpError(status) {
  let message = `Ошибка запроса (${status})`
  if (status === 400) {
    message = 'Проверьте поля'
  } else if (status >= 500) {
    message = 'Сервер не смог обработать запрос'
  }
  const error = new Error(message)
  error.status = status
  return error
}

export async function getTickets() {
  if (USE_MOCK) {
    const response = await mockGetTickets()
    if (!response.ok) {
      throw createHttpError(response.status)
    }
    return response.data
  }

  let response
  try {
    response = await fetch(`${baseUrl}/api/orders`)
  } catch {
    const networkError = new Error('Нет связи с сервером')
    networkError.isNetworkError = true
    throw networkError
  }

  if (!response.ok) {
    throw createHttpError(response.status)
  }

  return await response.json()
}

export async function createTicket(body) {
  if (USE_MOCK) {
    const response = await mockCreateTicket(body)
    if (!response.ok) {
      throw createHttpError(response.status)
    }
    return response.data
  }

  let response
  try {
    response = await fetch(`${baseUrl}/api/orders`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body)
    })
  } catch {
    const networkError = new Error('Нет связи с сервером')
    networkError.isNetworkError = true
    throw networkError
  }

  if (!response.ok) {
    throw createHttpError(response.status)
  }

  return await response.json()
}
