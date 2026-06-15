import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { useAuthStore } from '@/stores/auth'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)

const authStore = useAuthStore()

// Escuchar el evento de logout forzado (cuando el refresh token también expiró)
window.addEventListener('auth:logout', () => {
  authStore.clearUser()
  router.push('/login')
})

// Restaurar la sesion ANTES de instalar el router: app.use(router) dispara
// la navegacion inicial (y su guard) de forma sincrona. Si el router se
// instala antes de que esto resuelva, el guard corre con user=null y manda
// a /login aunque la cookie sea valida.
authStore.restoreSession().finally(() => {
  app.use(router)
  app.mount('#app')
})
