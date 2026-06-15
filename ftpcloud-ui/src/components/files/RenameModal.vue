<script setup lang="ts">
import { ref } from 'vue'
import { api } from '@/services/api'
import type { ApiResponse, Folder, FileItem } from '@/types'

const props = defineProps<{
  kind: 'folder' | 'file'
  id: string
  currentName: string
}>()

const emit = defineEmits<{
  close: []
  renamed: [item: Folder | FileItem]
}>()

const name = ref(props.currentName)
const error = ref('')
const isSubmitting = ref(false)

async function handleSubmit() {
  error.value = ''
  isSubmitting.value = true
  try {
    const endpoint = props.kind === 'folder' ? `/files/folders/${props.id}` : `/files/${props.id}`
    const res = await api.put<ApiResponse<Folder | FileItem>>(endpoint, { name: name.value })
    emit('renamed', res.data)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Error al renombrar'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="overlay" @click.self="$emit('close')">
    <div class="modal">
      <h3 class="modal-title">Renombrar</h3>

      <form @submit.prevent="handleSubmit" class="modal-form">
        <div class="form-group">
          <label for="rename-name">Nombre</label>
          <input id="rename-name" v-model="name" type="text" autofocus required />
        </div>

        <p v-if="error" class="error-msg">{{ error }}</p>

        <div class="modal-actions">
          <button type="button" class="btn-cancel" @click="$emit('close')">Cancelar</button>
          <button type="submit" class="btn-submit" :disabled="isSubmitting">
            {{ isSubmitting ? 'Guardando...' : 'Guardar' }}
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
