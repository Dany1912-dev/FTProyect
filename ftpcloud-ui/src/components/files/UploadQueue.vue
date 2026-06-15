<script setup lang="ts">
import type { UploadItem } from '@/composables/useFileUpload'

defineProps<{
  items: UploadItem[]
}>()
</script>

<template>
  <div v-if="items.length" class="upload-queue">
    <div class="queue-header">
      <i class="ph ph-upload-simple"></i>
      <span>Subiendo {{ items.length }} archivo{{ items.length > 1 ? 's' : '' }}</span>
    </div>
    <div class="queue-list">
      <div v-for="item in items" :key="item.id" class="queue-item">
        <i
          :class="[
            'queue-icon',
            item.status === 'done' ? 'ph ph-check-circle done' : item.status === 'error' ? 'ph ph-x-circle error' : 'ph ph-file',
          ]"
        ></i>
        <div class="queue-info">
          <span class="queue-name" :title="item.name">{{ item.name }}</span>
          <div v-if="item.status === 'uploading'" class="queue-bar-bg">
            <div class="queue-bar-fill" :style="{ width: item.progress + '%' }"></div>
          </div>
          <span v-else-if="item.status === 'error'" class="queue-error">{{ item.error }}</span>
          <span v-else class="queue-done-text">Completado</span>
        </div>
        <span v-if="item.status === 'uploading'" class="queue-percent">{{ item.progress }}%</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.upload-queue {
  position: fixed;
  bottom: 1.5rem;
  right: 1.5rem;
  width: 320px;
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lg);
  z-index: 90;
  overflow: hidden;
  animation: slideUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.queue-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  background: var(--color-background-mute);
  border-bottom: 1px solid var(--color-border);
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--color-heading);
}

.queue-header i {
  color: var(--brand-primary);
}

.queue-list {
  max-height: 240px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
}

.queue-item {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  padding: 0.6rem 1rem;
  border-bottom: 1px solid var(--color-border);
}

.queue-item:last-child {
  border-bottom: none;
}

.queue-icon {
  font-size: 1.1rem;
  color: var(--color-text-muted);
  flex-shrink: 0;
}

.queue-icon.done {
  color: var(--color-success);
}

.queue-icon.error {
  color: var(--color-danger);
}

.queue-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  min-width: 0;
}

.queue-name {
  font-size: 0.8rem;
  font-weight: 500;
  color: var(--color-heading);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.queue-bar-bg {
  width: 100%;
  height: 4px;
  background: var(--color-border);
  border-radius: 2px;
  overflow: hidden;
}

.queue-bar-fill {
  height: 100%;
  background: var(--brand-primary);
  border-radius: 2px;
  transition: width 0.4s linear;
}

.queue-error {
  font-size: 0.7rem;
  color: var(--color-danger);
}

.queue-done-text {
  font-size: 0.7rem;
  color: var(--color-text-muted);
}

.queue-percent {
  font-size: 0.75rem;
  color: var(--color-text-muted);
  flex-shrink: 0;
}
</style>
