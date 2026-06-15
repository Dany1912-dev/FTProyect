<script setup lang="ts">
import { ref, computed } from 'vue'
import { api } from '@/services/api'
import type { ApiResponse, User } from '@/types'

const props = defineProps<{
  user: User
}>()

const emit = defineEmits<{
  close: []
  updated: [user: User]
}>()

const gbInput = ref(props.user.storageQuotaBytes / (1024 * 1024 * 1024))
const error = ref('')
const isSubmitting = ref(false)

const currentUsageGB = computed(() =>
  (props.user.storageUsedBytes / (1024 * 1024 * 1024)).toFixed(2),
)

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  if (bytes < 1024 * 1024 * 1024) return `${(bytes / 1024 / 1024).toFixed(1)} MB`
  return `${(bytes / 1024 / 1024 / 1024).toFixed(2)} GB`
}

async function handleSubmit() {
  error.value = ''

  const gb = Number(gbInput.value)
  if (!gb || gb <= 0) {
    error.value = 'La cuota debe ser mayor a 0 GB'
    return
  }

  const quotaBytes = Math.round(gb * 1024 * 1024 * 1024)
  if (quotaBytes < props.user.storageUsedBytes) {
    error.value = `La cuota (${gb} GB) no puede ser menor al uso actual (${currentUsageGB.value} GB)`
    return
  }

  isSubmitting.value = true
  try {
    const res = await api.put<ApiResponse<User>>(`/users/${props.user.id}/quota`, {
      quotaBytes,
    })
    emit('updated', res.data)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Error al actualizar la cuota'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="overlay" @click.self="$emit('close')">
    <div class="modal">
      <h3 class="modal-title">Editar cuota — {{ user.username }}</h3>

      <form @submit.prevent="handleSubmit" class="modal-form">
        <p class="usage-info">
          Uso actual: <strong>{{ formatSize(user.storageUsedBytes) }}</strong>
          ({{ currentUsageGB }} GB)
        </p>

        <div class="form-group">
          <label for="quota-gb">Cuota (GB)</label>
          <input
            id="quota-gb"
            v-model.number="gbInput"
            type="number"
            min="0.1"
            step="0.1"
            autofocus
            required
          />
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

.usage-info {
  margin: 0;
  font-size: 0.875rem;
  color: var(--color-text);
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
