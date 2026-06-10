import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { User, ApiResponse } from '@/types'
import { api } from '@/services/api'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)

  // El token vive en una cookie HttpOnly, el frontend nunca lo toca
  const isAuthenticated = computed(() => user.value !== null)
  const isOwner = computed(() => user.value?.role === 'owner')
  const isAdmin = computed(() => user.value?.role === 'admin' || isOwner.value)

  // Llamado al hacer login exitoso: recibimos solo los datos del usuario
  function setUser(userData: User) {
    user.value = userData
  }

  // Limpia el estado local (el backend borra las cookies en /auth/logout)
  function clearUser() {
    user.value = null
  }

  // Al cargar la app: verificar si hay sesion activa via cookie
  async function restoreSession(): Promise<void> {
    try {
      const res = await api.get<ApiResponse<User>>('/auth/me')
      user.value = res.data
    } catch {
      user.value = null
    }
  }

  return {
    user,
    isAuthenticated,
    isOwner,
    isAdmin,
    setUser,
    clearUser,
    restoreSession,
  }
})
