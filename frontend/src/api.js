import { mockApi } from './mock'

const USE_MOCK = import.meta.env.VITE_USE_MOCK !== 'false'
const BASE = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

async function http(method, path, body) {
  const token = localStorage.getItem('token')
  const res = await fetch(BASE + path, {
    method,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    },
    body: body ? JSON.stringify(body) : undefined
  })
  const data = await res.json().catch(() => null)
  if (!res.ok) throw new Error(data?.error || `Ошибка сервера (${res.status})`)
  return data
}

const realApi = {
  login: (login, password) => http('POST', '/auth/login', { login, password }),
  getModels: () => http('GET', '/models'),
  getEngineers: () => http('GET', '/users?role=Engineer'),
  getOrders: () => http('GET', '/orders'),
  createOrder: (payload) => http('POST', '/orders', payload),
  assign: (id, assigneeUserId) => http('PATCH', `/orders/${id}/assignee`, { assigneeUserId }),
  setStatus: (id, status) => http('PATCH', `/orders/${id}/status`, { status })
}

export const api = USE_MOCK ? mockApi : realApi
export const isMock = USE_MOCK
