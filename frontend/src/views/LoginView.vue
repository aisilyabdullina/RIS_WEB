<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { login } from '../auth'
import { isMock } from '../api'

const router = useRouter()
const loginValue = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function onSubmit() {
  error.value = ''
  if (!loginValue.value.trim() || !password.value) {
    error.value = 'Введите логин и пароль'
    return
  }
  loading.value = true
  try {
    await login(loginValue.value.trim(), password.value)
    router.push({ name: 'orders' })
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="card narrow">
    <h1>Вход в систему</h1>
    <div class="field">
      <label for="login">Логин</label>
      <input id="login" v-model="loginValue" autocomplete="username" @keyup.enter="onSubmit" />
    </div>
    <div class="field">
      <label for="password">Пароль</label>
      <input id="password" v-model="password" type="password" autocomplete="current-password" @keyup.enter="onSubmit" />
    </div>
    <p v-if="error" class="error">{{ error }}</p>
    <button class="btn" :disabled="loading" @click="onSubmit">{{ loading ? 'Вход…' : 'Войти' }}</button>

    <div v-if="isMock" class="hint">
      <strong>Тестовые учётные записи</strong> (пароль у всех <code>123456</code>):
      <ul>
        <li><code>manager</code> — менеджер по заказам</li>
        <li><code>engineer1</code>, <code>engineer2</code> — инженеры-наладчики</li>
        <li><code>chief</code> — начальник участка</li>
      </ul>
    </div>
  </div>
</template>
