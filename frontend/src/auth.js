import { reactive } from 'vue'
import { api } from './api'

export const auth = reactive({
  user: JSON.parse(localStorage.getItem('user') || 'null')
})

export async function login(loginValue, password) {
  const r = await api.login(loginValue, password)
  localStorage.setItem('token', r.token)
  localStorage.setItem('user', JSON.stringify(r))
  auth.user = r
}

export function logout() {
  localStorage.removeItem('token')
  localStorage.removeItem('user')
  auth.user = null
}
