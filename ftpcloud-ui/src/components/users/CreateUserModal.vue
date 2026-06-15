<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { api } from '@/services/api'
import type { ApiResponse, User } from '@/types'

const emit = defineEmits<{
  close: []
  created: [user: User]
}>()

const auth = useAuthStore()

const username = ref('')
const email = ref('')
const password = ref('')
const role = ref<'admin' | 'user'>('user')
const error = ref('')
const isSubmitting = ref(false)

async function handleSubmit() {
  error.value = ''
  isSubmitting.value = true

  try {
    const res = await api.post<ApiResponse<User>>('/users', {
      username: username.value,
      email: email.value,
      password: password.value,
      role: role.value,
    })
    emit('created', res.data)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Error al crear el usuario'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="overlay" @click.self="$emit('close')">
    <div class="modal">
      <h3 class="modal-title">Nuevo usuario</h3>

      <form @submit.prevent="handleSubmit" class="modal-form">
        <div class="form-group">
          <label for="new-username">Usuario</label>
          <input id="new-username" v-model="username" type="text" autocomplete="username" required />
        </div>

        <div class="form-group">
          <label for="new-email">Email</label>
          <input id="new-email" v-model="email" type="email" autocomplete="email" required />
        </div>

        <div class="form-group">
          <label for="new-password">Contrasena</label>
          <input
            id="new-password"
            v-model="password"
            type="password"
            autocomplete="new-password"
            minlength="8"
            required
          />
        </div>

        <div v-if="auth.isOwner" class="form-group">
          <label for="new-role">Rol</label>
          <select id="new-role" v-model="role">
            <option value="user">Usuario</option>
            <option value="admin">Administrador</option>
          </select>
        </div>

        <p v-if="error" class="error-msg">{{ error }}</p>

        <div class="modal-actions">
          <button type="button" class="btn-cancel" @click="$emit('close')">Cancelar</button>
          <button type="submit" class="btn-submit" :disabled="isSubmitting">
            {{ isSubmitting ? 'Creando...' : 'Crear' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}

.modal {
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 1.5rem;
  width: 100%;
  max-width: 380px;
}

.modal-title {
  font-size: 1.25rem;
  font-weight: 700;
  margin: 0 0 1.25rem;
  color: var(--color-heading);
}

.modal-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
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

.form-group input,
.form-group select {
  padding: 0.6rem 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-background);
  color: var(--color-text);
  font-size: 0.95rem;
  outline: none;
  transition: border-color 0.15s;
}

.form-group input:focus,
.form-group select:focus {
  border-color: var(--color-heading);
}

.error-msg {
  color: #e05252;
  font-size: 0.875rem;
  margin: 0;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: 0.25rem;
}

.btn-cancel,
.btn-submit {
  padding: 0.6rem 1.1rem;
  border: none;
  border-radius: 6px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.15s;
}

.btn-cancel {
  background: var(--color-background-mute);
  color: var(--color-text);
}

.btn-submit {
  background: var(--color-heading);
  color: var(--color-background);
}

.btn-cancel:hover,
.btn-submit:hover:not(:disabled) {
  opacity: 0.85;
}

.btn-submit:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
