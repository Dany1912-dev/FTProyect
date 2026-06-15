<script setup lang="ts">
import { useDialogStore } from '@/stores/dialog'

const dialog = useDialogStore()
</script>

<template>
  <div v-if="dialog.isOpen" class="overlay" @click.self="dialog.resolve(false)">
    <div class="modal">
      <div class="modal-content">
        <div class="modal-icon" :class="{ danger: dialog.options.danger }">
          <i :class="dialog.options.danger ? 'ph ph-warning' : 'ph ph-info'"></i>
        </div>
        <div class="modal-body">
          <h3 v-if="dialog.options.title" class="modal-title">{{ dialog.options.title }}</h3>
          <p class="modal-message">{{ dialog.options.message }}</p>
        </div>
      </div>

      <div class="modal-actions">
        <button
          v-if="dialog.mode === 'confirm'"
          type="button"
          class="btn btn-secondary"
          @click="dialog.resolve(false)"
        >
          {{ dialog.options.cancelText || 'Cancelar' }}
        </button>
        <button
          type="button"
          class="btn btn-primary"
          :class="{ danger: dialog.options.danger }"
          @click="dialog.resolve(true)"
        >
          {{ dialog.options.confirmText || (dialog.mode === 'alert' ? 'Aceptar' : 'Confirmar') }}
        </button>
      </div>
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
  z-index: 200;
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
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px) scale(0.95); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

.modal-content {
  display: flex;
  gap: 1rem;
  align-items: flex-start;
}

.modal-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: var(--brand-primary-light);
  color: var(--brand-primary);
  font-size: 1.75rem;
  flex-shrink: 0;
}

.modal-icon.danger {
  background: var(--color-danger-bg);
  color: var(--color-danger);
}

.modal-body {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding-top: 0.25rem;
}

.modal-title {
  font-size: 1.15rem;
  font-weight: 700;
  margin: 0;
  color: var(--color-heading);
}

.modal-message {
  color: var(--color-text-muted);
  font-size: 0.95rem;
  margin: 0;
  line-height: 1.5;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
}

.btn {
  display: flex;
  align-items: center;
  justify-content: center;
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

.btn-primary:hover {
  background: var(--brand-primary-hover);
  transform: translateY(-1px);
  box-shadow: var(--shadow-md);
}

.btn-primary.danger {
  background: var(--color-danger);
}

.btn-primary.danger:hover {
  background: #b91c1c; /* A slightly darker red for hover */
}
</style>
