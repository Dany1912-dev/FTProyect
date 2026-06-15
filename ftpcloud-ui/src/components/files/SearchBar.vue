<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRoute } from 'vue-router'

const props = defineProps<{ initialQuery?: string }>()
const emit = defineEmits<{ search: [query: string] }>()

const route = useRoute()
const query = ref(props.initialQuery ?? '')

watch(() => route.query.q, (val) => {
  if (val && typeof val === 'string') query.value = val
})

function onSubmit() {
  const q = query.value.trim()
  if (q) emit('search', q)
}
</script>

<template>
  <div class="search-bar">
    <i class="ph ph-magnifying-glass search-icon"></i>
    <input
      v-model="query"
      type="text"
      placeholder="Buscar archivos..."
      class="search-input"
      @keyup.enter="onSubmit"
    />
    <button v-if="query" class="search-clear" @click="query = ''" type="button">
      <i class="ph ph-x"></i>
    </button>
  </div>
</template>

<style scoped>
.search-bar {
  position: relative;
  display: flex;
  align-items: center;
  margin: 0 0.75rem 0.5rem;
}

.search-icon {
  position: absolute;
  left: 0.75rem;
  font-size: 1rem;
  color: var(--color-text-muted);
  pointer-events: none;
}

.search-input {
  width: 100%;
  padding: 0.55rem 2rem 0.55rem 2.25rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-background-mute);
  color: var(--color-text);
  font-size: 0.85rem;
  outline: none;
  transition: border-color var(--transition-fast), background var(--transition-fast);
}

.search-input::placeholder {
  color: var(--color-text-muted);
}

.search-input:focus {
  border-color: var(--brand-primary);
  background: var(--color-background);
}

.search-clear {
  position: absolute;
  right: 0.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  border: none;
  background: var(--color-border);
  color: var(--color-text-muted);
  font-size: 0.7rem;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.search-clear:hover {
  background: var(--color-text-muted);
  color: var(--color-background);
}
</style>
