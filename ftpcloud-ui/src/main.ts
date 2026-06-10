import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { useAuthStore } from '@/stores/auth'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)

const authStore = useAuthStore()

// Escuchar el evento de logout forzado (cuando el refresh token también expiró)
window.addEventListener('auth:logout', () => {
  authStore.clearUser()
  router.push('/login')
})

// Intentar restaurar la sesion antes de montar la app
// Esto evita que el router guard redirija al login innecesariamente al recargar
authStore.restoreSession().finally(() => {
  app.mount('#app')
})
