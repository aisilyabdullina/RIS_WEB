import { createRouter, createWebHistory } from 'vue-router'
import OrderList from './pages/OrderList.vue'
import OrderNew from './pages/OrderNew.vue'
import Login from './pages/Login.vue'

const routes = [
  { path: '/', component: OrderList },
  { path: '/new', component: OrderNew },
  { path: '/login', component: Login }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
