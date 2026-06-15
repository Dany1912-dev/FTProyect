<script setup lang="ts">
import { useDialogStore } from '@/stores/dialog'

const dialog = useDialogStore()
</script>

<template>
  <div v-if="dialog.isOpen" class="overlay" @click.self="dialog.resolve(false)">
    <div class="modal">
      <h3 v-if="dialog.options.title" class="modal-title">{{ dialog.options.title }}</h3>
      <p class="modal-message">{{ dialog.options.message }}</p>

      <div class="modal-actions">
        <button
          v-if="dialog.mode === 'confirm'"
          type="button"
          class="btn-cancel"
          @click="dialog.resolve(false)"
        >
          {{ dialog.options.cancelText || 'Cancelar' }}
        </button>
        <button
          type="button"
          class="btn-submit"
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
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 200;
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
  font-size: 1.1rem;
  font-weight: 700;
  margin: 0 0 0.75rem;
  color: var(--color-heading);
}

.modal-message {
  color: var(--color-text);
  font-size: 0.9rem;
  margin: 0;
  line-height: 1.5;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: 1.5rem;
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

.btn-submit.danger {
  background: #dc2626;
  color: #fff;
}

.btn-cancel:hover,
.btn-submit:hover {
  opacity: 0.85;
}
</style>
