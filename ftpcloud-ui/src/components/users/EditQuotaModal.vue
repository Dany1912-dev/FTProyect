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

function decreaseQuota() {
  const current = Number(gbInput.value) || 0;
  if (current > 0.1) {
    gbInput.value = parseFloat((current - 0.1).toFixed(1));
  }
}

function increaseQuota() {
  const current = Number(gbInput.value) || 0;
  gbInput.value = parseFloat((current + 0.1).toFixed(1));
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
      <div class="modal-header">
        <div class="modal-title-wrapper">
          <i class="ph ph-hard-drives title-icon"></i>
          <h3 class="modal-title">Editar cuota</h3>
        </div>
        <button class="close-btn" @click="$emit('close')">
          <i class="ph ph-x"></i>
        </button>
      </div>

      <form @submit.prevent="handleSubmit" class="modal-form">
        <div class="user-subtitle">
          <div class="user-avatar">{{ user.username.charAt(0).toUpperCase() }}</div>
          <span>{{ user.username }}</span>
        </div>

        <div class="usage-card">
          <div class="usage-header">
            <span>Uso actual</span>
            <strong>{{ formatSize(user.storageUsedBytes) }} ({{ currentUsageGB }} GB)</strong>
          </div>
          <div class="usage-bar-bg">
            <div class="usage-fill" :style="{ width: Math.min(100, (user.storageUsedBytes / (gbInput * 1024 * 1024 * 1024)) * 100) + '%' }"></div>
          </div>
        </div>

        <div class="form-group">
          <label for="quota-gb">Límite de cuota (GB)</label>
          <div class="number-input-wrapper">
            <button type="button" class="stepper-btn" @click="decreaseQuota" :disabled="gbInput <= 0.1">
              <i class="ph ph-minus"></i>
            </button>
            <div class="input-container">
              <input
                id="quota-gb"
                v-model.number="gbInput"
                type="number"
                min="0.1"
                step="0.1"
                autofocus
                required
              />
              <span class="unit">GB</span>
            </div>
            <button type="button" class="stepper-btn" @click="increaseQuota">
              <i class="ph ph-plus"></i>
            </button>
          </div>
        </div>

        <div v-if="error" class="error-msg">
          <i class="ph ph-warning-circle"></i>
          <span>{{ error }}</span>
        </div>

        <div class="modal-actions">
          <button type="button" class="btn btn-secondary" @click="$emit('close')">Cancelar</button>
          <button type="submit" class="btn btn-primary" :disabled="isSubmitting">
            <i :class="isSubmitting ? 'ph ph-spinner-gap spin' : 'ph ph-check'"></i>
            <span>{{ isSubmitting ? 'Guardando...' : 'Guardar cambios' }}</span>
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
  margin-bottom: 1.25rem;
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

.user-subtitle {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  font-weight: 600;
  color: var(--color-heading);
}

.user-avatar {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: linear-gradient(135deg, #6b7280, #4b5563);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.8rem;
}

.usage-card {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  background: var(--color-background-mute);
  padding: 1rem;
  border-radius: var(--radius-md);
}

.usage-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.875rem;
  color: var(--color-text);
}

.usage-header strong {
  color: var(--color-heading);
}

.usage-bar-bg {
  width: 100%;
  height: 8px;
  background: var(--color-border);
  border-radius: 4px;
  overflow: hidden;
}

.usage-fill {
  height: 100%;
  background: var(--brand-primary);
  border-radius: 4px;
  transition: width 0.3s ease;
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

.number-input-wrapper {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.stepper-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  border: 1px solid var(--color-border);
  background: var(--color-background-soft);
  color: var(--color-text);
  font-size: 1.1rem;
  cursor: pointer;
  transition: all var(--transition-fast);
  flex-shrink: 0;
}

.stepper-btn:hover:not(:disabled) {
  background: var(--color-background-mute);
  border-color: var(--brand-primary);
  color: var(--brand-primary);
}

.stepper-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.input-container {
  position: relative;
  flex: 1;
  display: flex;
  align-items: center;
}

.input-container input {
  width: 100%;
  padding: 0.75rem 3rem 0.75rem 1rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-background-soft);
  color: var(--color-text);
  font-size: 1rem;
  font-weight: 500;
  outline: none;
  text-align: center;
  transition: all var(--transition-fast);
  box-shadow: inset 0 1px 2px rgba(0, 0, 0, 0.05);
}

/* Hide default spin buttons */
.input-container input[type="number"]::-webkit-inner-spin-button,
.input-container input[type="number"]::-webkit-outer-spin-button {
  -webkit-appearance: none;
  margin: 0;
}

.input-container input[type="number"] {
  -moz-appearance: textfield; /* Firefox */
}

.input-container input:focus {
  border-color: var(--brand-primary);
  background: var(--color-background);
  box-shadow: 0 0 0 3px var(--brand-primary-light);
}

.unit {
  position: absolute;
  right: 1rem;
  color: var(--color-text-muted);
  font-weight: 600;
  font-size: 0.9rem;
  pointer-events: none;
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
