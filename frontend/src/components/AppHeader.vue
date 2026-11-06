<script setup>
</script>

<template>
  <header class="app-header">
    <div class="brand">
      <h1>КонтрОрдер · АО «НИЦ «Инкомсистем»</h1>
      <p class="subtitle">Система учёта заказов контроллеров</p>
    </div>

    <nav class="screen-nav" aria-label="Основная навигация">
      <router-link to="/" class="nav-link">Список</router-link>
      <router-link to="/new" class="nav-link">Создать</router-link>
      <router-link to="/login" class="nav-link">Вход</router-link>
    </nav>
  </header>
</template>

<style scoped>
.app-header {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  padding-bottom: 20px;
  margin-bottom: 24px;
  border-bottom: 2px solid #e5e7eb;
}

.brand h1 {
  margin: 0;
  font-size: 1.4rem;
  color: #111827;
}

.subtitle {
  margin: 4px 0 0;
  font-size: 0.9rem;
  color: #4b5563;
}

.screen-nav {
  display: flex;
  gap: 8px;
}

.nav-link {
  padding: 8px 16px;
  font-size: 0.95rem;
  font-weight: 500;
  color: #374151;
  background-color: #f3f4f6;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  text-decoration: none;
  transition: background-color 0.15s ease;
}

.nav-link:hover {
  background-color: #e5e7eb;
}

.nav-link.router-link-exact-active {
  color: #ffffff;
  background-color: #2563eb;
  border-color: #2563eb;
}
</style>
