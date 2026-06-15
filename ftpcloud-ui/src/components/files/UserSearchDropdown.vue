<script setup lang="ts">
import type { UserSummary } from '@/types'

defineProps<{
  modelValue: string
  results: UserSummary[]
  isSearching: boolean
  showDropdown: boolean
  disabled?: boolean
  placeholder?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [string]
  input: []
  focus: []
  blur: []
  select: [UserSummary]
}>()
</script>

<template>
  <div class="search-wrapper">
    <div class="search-input-group">
      <i class="ph ph-magnifying-glass search-icon"></i>
      <input
        :value="modelValue"
        type="text"
        :placeholder="placeholder ?? 'Buscar usuario...'"
        class="search-input"
        autocomplete="off"
        @input="emit('update:modelValue', ($event.target as HTMLInputElement).value); emit('input')"
        @focus="emit('focus')"
        @blur="emit('blur')"
      />
      <i v-if="isSearching" class="ph ph-spinner-gap spin search-spinner"></i>
    </div>
    <div v-if="showDropdown" class="user-dropdown">
      <div v-if="results.length" class="user-dropdown-list">
        <button
          v-for="user in results"
          :key="user.id"
          type="button"
          class="user-option"
          :disabled="disabled"
          @click="emit('select', user)"
        >
          <div class="user-avatar">{{ user.username.charAt(0).toUpperCase() }}</div>
          <span class="user-name">{{ user.username }}</span>
          <i class="ph ph-plus-circle add-icon"></i>
        </button>
      </div>
      <div v-else-if="!isSearching" class="empty-results">
        No se encontraron usuarios.
      </div>
    </div>
  </div>
</template>

<style scoped>
.search-wrapper {
  position: relative;
}

.search-input-group {
  display: flex;
  align-items: center;
  position: relative;
}

.search-icon {
  position: absolute;
  left: 0.75rem;
  color: var(--color-text-muted);
  font-size: 1rem;
  pointer-events: none;
}

.search-spinner {
  position: absolute;
  right: 0.75rem;
  color: var(--color-text-muted);
  font-size: 1rem;
}

.search-input {
  width: 100%;
  padding: 0.55rem 0.75rem 0.55rem 2.25rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-background);
  color: var(--color-text);
  font-size: 0.9rem;
  outline: none;
  transition: border-color var(--transition-fast);
}

.search-input:focus {
  border-color: var(--brand-primary);
}

.user-dropdown {
  position: absolute;
  top: calc(100% + 0.35rem);
  left: 0;
  right: 0;
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-lg);
  z-index: 10;
  max-height: 180px;
  overflow-y: auto;
}

.user-dropdown-list {
  display: flex;
  flex-direction: column;
}

.user-option {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  width: 100%;
  padding: 0.5rem 0.75rem;
  border: none;
  background: transparent;
  cursor: pointer;
  text-align: left;
  transition: background var(--transition-fast);
}

.user-option:hover:not(:disabled) {
  background: var(--color-background-mute);
}

.user-option:disabled {
  opacity: 0.6;
  cursor: not-allowed;
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
  font-size: 0.75rem;
  font-weight: 600;
  flex-shrink: 0;
}

.user-name {
  flex: 1;
  font-size: 0.9rem;
  font-weight: 500;
  color: var(--color-heading);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.add-icon {
  color: var(--brand-primary);
  font-size: 1.1rem;
}

.empty-results {
  text-align: center;
  padding: 0.75rem;
  font-size: 0.85rem;
  color: var(--color-text-muted);
}
</style>
