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
    <div class="login-header">
      <div class="logo-icon">
        <i class="ph ph-cloud"></i>
      </div>
      <h1 class="login-title">FTPCloud</h1>
      <p class="login-subtitle">Inicia sesión para continuar</p>
    </div>

    <form @submit.prevent="handleLogin" class="login-form">
      <div class="form-group">
        <label for="username">Usuario</label>
        <div class="input-wrapper">
          <i class="ph ph-user input-icon"></i>
          <input
            id="username"
            v-model="username"
            type="text"
            autocomplete="username"
            placeholder="Tu usuario"
            required
          />
        </div>
      </div>

      <div class="form-group">
        <label for="password">Contraseña</label>
        <div class="input-wrapper">
          <i class="ph ph-lock-key input-icon"></i>
          <input
            id="password"
            v-model="password"
            type="password"
            autocomplete="current-password"
            placeholder="Tu contraseña"
            required
          />
        </div>
      </div>

      <div v-if="error" class="error-msg">
        <i class="ph ph-warning-circle"></i>
        <span>{{ error }}</span>
      </div>

      <button type="submit" class="login-btn" :disabled="isLoading">
        <i v-if="isLoading" class="ph ph-spinner-gap spin"></i>
        <span>{{ isLoading ? 'Entrando...' : 'Entrar' }}</span>
        <i v-if="!isLoading" class="ph ph-arrow-right"></i>
      </button>
    </form>
  </div>
</template>

<style scoped>
.login-card {
  background: var(--glass-bg);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border: 1px solid var(--glass-border);
  border-radius: var(--radius-lg);
  padding: 3rem 2.5rem;
  width: 100%;
  max-width: 420px;
  box-shadow: var(--shadow-float);
  animation: slideUp 0.5s ease-out;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.login-header {
  text-align: center;
  margin-bottom: 2.5rem;
}

.logo-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 56px;
  height: 56px;
  background: linear-gradient(135deg, var(--brand-primary), var(--brand-primary-hover));
  color: white;
  border-radius: var(--radius-md);
  font-size: 2rem;
  margin-bottom: 1rem;
  box-shadow: var(--shadow-md);
}

.login-title {
  font-size: 1.75rem;
  font-weight: 700;
  margin: 0 0 0.5rem;
  color: var(--color-heading);
  letter-spacing: -0.02em;
}

.login-subtitle {
  color: var(--color-text-muted);
  margin: 0;
  font-size: 0.95rem;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-group label {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--color-heading);
}

.input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.input-icon {
  position: absolute;
  left: 1rem;
  font-size: 1.25rem;
  color: var(--color-text-muted);
  transition: color var(--transition-fast);
}

.form-group input {
  width: 100%;
  padding: 0.75rem 1rem 0.75rem 3rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-background-soft);
  color: var(--color-heading);
  font-size: 0.95rem;
  font-family: inherit;
  outline: none;
  transition: all var(--transition-fast);
}

.form-group input::placeholder {
  color: var(--color-text-muted);
}

.form-group input:focus {
  border-color: var(--brand-primary);
  box-shadow: 0 0 0 3px var(--brand-primary-light);
}

.form-group input:focus + .input-icon,
.form-group input:focus ~ .input-icon /* Depending on DOM order, but here icon is before input. So we use focus-within on wrapper */ {
  color: var(--brand-primary);
}

.input-wrapper:focus-within .input-icon {
  color: var(--brand-primary);
}

.error-msg {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--color-danger);
  background: var(--color-danger-bg);
  padding: 0.75rem 1rem;
  border-radius: var(--radius-md);
  font-size: 0.875rem;
  font-weight: 500;
  margin: 0;
}

.error-msg i {
  font-size: 1.1rem;
}

.login-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 0.875rem;
  background-color: var(--color-heading);
  color: var(--color-background);
  border: none;
  border-radius: var(--radius-md);
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
  margin-top: 0.5rem;
}

.login-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: var(--shadow-md);
  background-color: var(--brand-primary);
  color: white;
}

.login-btn:active:not(:disabled) {
  transform: translateY(0);
}

.login-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}
</style>
