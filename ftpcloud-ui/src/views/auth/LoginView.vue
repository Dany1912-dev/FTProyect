<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { api } from '@/services/api'
import type { ApiResponse, User } from '@/types'

const router = useRouter()
const auth = useAuthStore()

const username = ref('')
const password = ref('')
const error = ref('')
const isLoading = ref(false)

async function handleLogin() {
  error.value = ''
  isLoading.value = true

  try {
    // El backend setea las cookies HttpOnly, el body solo devuelve datos del usuario
    const res = await api.post<ApiResponse<User>>('/auth/login', {
      username: username.value,
      password: password.value,
    })

    auth.setUser(res.data)

    if (res.data.role === 'owner') {
      router.push('/owner')
    } else {
      router.push('/files')
    }
  } catch (e: unknown) {
    error.value = e instanceof Error ? e.message : 'Credenciales incorrectas'
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <div class="login-card">
    <h1 class="login-title">FTPCloud</h1>
    <p class="login-subtitle">Inicia sesion para continuar</p>

    <form @submit.prevent="handleLogin" class="login-form">
      <div class="form-group">
        <label for="username">Usuario</label>
        <input
          id="username"
          v-model="username"
          type="text"
          autocomplete="username"
          placeholder="Tu usuario"
          required
        />
      </div>

      <div class="form-group">
        <label for="password">Contrasena</label>
        <input
          id="password"
          v-model="password"
          type="password"
          autocomplete="current-password"
          placeholder="Tu contrasena"
          required
        />
      </div>

      <p v-if="error" class="error-msg">{{ error }}</p>

      <button type="submit" class="login-btn" :disabled="isLoading">
        {{ isLoading ? 'Entrando...' : 'Entrar' }}
      </button>
    </form>
  </div>
</template>

<style scoped>
.login-card {
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 2.5rem 2rem;
  width: 100%;
  max-width: 380px;
}

.login-title {
  font-size: 1.75rem;
  font-weight: 700;
  margin: 0 0 0.25rem;
  color: var(--color-heading);
}

.login-subtitle {
  color: var(--color-text);
  margin: 0 0 2rem;
  font-size: 0.95rem;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.form-group label {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--color-heading);
}

.form-group input {
  padding: 0.6rem 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-background);
  color: var(--color-text);
  font-size: 0.95rem;
  outline: none;
  transition: border-color 0.15s;
}

.form-group input:focus {
  border-color: var(--color-heading);
}

.error-msg {
  color: #e05252;
  font-size: 0.875rem;
  margin: 0;
}

.login-btn {
  padding: 0.7rem;
  background-color: var(--color-heading);
  color: var(--color-background);
  border: none;
  border-radius: 6px;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.15s;
}

.login-btn:hover:not(:disabled) {
  opacity: 0.85;
}

.login-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
