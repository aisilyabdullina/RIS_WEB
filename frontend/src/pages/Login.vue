<script setup>
import { ref } from 'vue'

const login = ref('')
const password = ref('')
</script>

<template>
  <section aria-labelledby="login-heading">
    <h2 id="login-heading">Вход в систему</h2>
    <p class="section-hint">
      Авторизация сотрудников производственного участка (менеджер по заказам, начальник участка, инженер-наладчик).
    </p>

    <form class="card-form" @submit.prevent>
      <div class="form-group">
        <label for="login">Логин сотрудника</label>
        <input
          id="login"
          v-model="login"
          name="login"
          type="text"
          placeholder="manager"
          required
        />
      </div>

      <div class="form-group">
        <label for="password">Пароль</label>
        <input
          id="password"
          v-model="password"
          name="password"
          type="password"
          placeholder="Введите пароль"
          required
        />
      </div>

      <button type="submit" class="primary-btn">Войти</button>
    </form>
  </section>
</template>

<style scoped>
h2 {
  margin-top: 0;
  font-size: 1.3rem;
  color: #111827;
}

.section-hint {
  margin-bottom: 16px;
  font-size: 0.9rem;
  color: #4b5563;
}

.card-form {
  max-width: 560px;
  padding: 20px;
  background-color: #f9fafb;
  border: 1px solid #d1d5db;
  border-radius: 8px;
}

.form-group {
  display: flex;
  flex-direction: column;
  margin-bottom: 16px;
}

.form-group label {
  margin-bottom: 6px;
  font-weight: 600;
  font-size: 0.95rem;
  color: #111827;
}

.form-group input {
  padding: 10px 12px;
  font-size: 0.95rem;
  font-family: inherit;
  color: #111827;
  background-color: #ffffff;
  border: 1px solid #9ca3af;
  border-radius: 6px;
}

.form-group input:focus {
  outline: 2px solid #2563eb;
  border-color: #2563eb;
}

.primary-btn {
  padding: 10px 20px;
  font-size: 0.95rem;
  font-weight: 600;
  color: #ffffff;
  background-color: #2563eb;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

.primary-btn:hover {
  background-color: #1d4ed8;
}
</style>
