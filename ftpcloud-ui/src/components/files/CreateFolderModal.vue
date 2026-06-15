<script setup lang="ts">
import { ref } from 'vue'
import { api } from '@/services/api'
import type { ApiResponse, Folder } from '@/types'

const props = withDefaults(
  defineProps<{ endpoint?: string; title?: string; parentFolderId?: string | null }>(),
  {
    endpoint: '/files/folders',
    title: 'Nueva carpeta',
    parentFolderId: null,
  },
)

const emit = defineEmits<{
  close: []
  created: [folder: Folder]
}>()

const name = ref('')
const error = ref('')
const isSubmitting = ref(false)

async function handleSubmit() {
  error.value = ''
  isSubmitting.value = true
  try {
    const res = await api.post<ApiResponse<Folder>>(props.endpoint, {
      name: name.value,
      parentFolderId: props.parentFolderId,
    })
    emit('created', res.data)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Error al crear la carpeta'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="overlay" @click.self="$emit('close')">
    <div class="modal">
      <div class="modal-header">
        <div class="modal-title-wrapper">
          <i class="ph ph-folder-plus title-icon"></i>
          <h3 class="modal-title">{{ title }}</h3>
        </div>
        <button class="close-btn" @click="$emit('close')">
          <i class="ph ph-x"></i>
        </button>
      </div>

      <form @submit.prevent="handleSubmit" class="modal-form">
        <div class="form-group">
          <label for="folder-name">Nombre de la carpeta</label>
          <input 
            id="folder-name" 
            v-model="name" 
            type="text" 
            placeholder="Ej. Documentos importantes"
            autofocus 
            required 
          />
        </div>

        <div v-if="error" class="error-msg">
          <i class="ph ph-warning-circle"></i>
          <span>{{ error }}</span>
        </div>

        <div class="modal-actions">
          <button type="button" class="btn btn-secondary" @click="$emit('close')">Cancelar</button>
          <button type="submit" class="btn btn-primary" :disabled="isSubmitting || !name.trim()">
            <i :class="isSubmitting ? 'ph ph-spinner-gap spin' : 'ph ph-check'"></i>
            <span>{{ isSubmitting ? 'Creando...' : 'Crear carpeta' }}</span>
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
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
  animation: fadeIn 0.2s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

.modal {
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: 1.5rem;
  width: 100%;
  max-width: 400px;
  box-shadow: var(--shadow-lg);
  animation: slideUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px) scale(0.95); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.5rem;
}

.modal-title-wrapper {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.title-icon {
  font-size: 1.5rem;
  color: var(--brand-primary);
}

.modal-title {
  font-size: 1.25rem;
  font-weight: 700;
  margin: 0;
  color: var(--color-heading);
}

.close-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: var(--radius-sm);
  border: none;
  background: transparent;
  color: var(--color-text-muted);
  font-size: 1.25rem;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.close-btn:hover {
  background: var(--color-background-mute);
  color: var(--color-heading);
}

.modal-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
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

.form-group input {
  padding: 0.75rem 1rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-background-soft);
  color: var(--color-text);
  font-size: 0.95rem;
  outline: none;
  transition: all var(--transition-fast);
  box-shadow: inset 0 1px 2px rgba(0, 0, 0, 0.05);
}

.form-group input:focus {
  border-color: var(--brand-primary);
  background: var(--color-background);
  box-shadow: 0 0 0 3px var(--brand-primary-light);
}

.form-group input::placeholder {
  color: var(--color-text-muted);
  opacity: 0.7;
}

.error-msg {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--color-danger);
  background: var(--color-danger-bg);
  padding: 0.75rem;
  border-radius: var(--radius-md);
  font-size: 0.875rem;
  margin: 0;
}

.error-msg i {
  font-size: 1.1rem;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  margin-top: 0.5rem;
}

.btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.6rem 1.25rem;
  border-radius: var(--radius-md);
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
  border: 1px solid transparent;
}

.btn-secondary {
  background: var(--color-background-mute);
  color: var(--color-text);
  border-color: var(--color-border);
}

.btn-secondary:hover {
  background: var(--color-border);
  color: var(--color-heading);
}

.btn-primary {
  background: var(--brand-primary);
  color: white;
  box-shadow: var(--shadow-sm);
}

.btn-primary:hover:not(:disabled) {
  background: var(--brand-primary-hover);
  transform: translateY(-1px);
  box-shadow: var(--shadow-md);
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}
</style>
