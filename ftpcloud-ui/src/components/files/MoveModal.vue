<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { api } from '@/services/api'
import type { ApiResponse, FolderTreeNode } from '@/types'

const props = defineProps<{
  kind: 'folder' | 'file'
  id: string
  /** Carpeta raíz del árbol actual (path[0]?.id ?? currentFolder.id) */
  rootFolderId: string
  /** Si es una carpeta, no se puede mover dentro de sí misma ni de sus descendientes */
  excludeId?: string
}>()

const emit = defineEmits<{
  close: []
  moved: []
}>()

const nodes = ref<FolderTreeNode[]>([])
const isLoading = ref(true)
const isSubmitting = ref(false)
const error = ref('')

onMounted(async () => {
  try {
    const res = await api.get<ApiResponse<FolderTreeNode[]>>(`/files/folders/${props.rootFolderId}/tree`)
    nodes.value = res.data
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'No se pudo cargar el árbol de carpetas'
  } finally {
    isLoading.value = false
  }
})

// ids que no se pueden elegir como destino: la propia carpeta y sus descendientes
const excludedIds = computed(() => {
  if (props.kind !== 'folder' || !props.excludeId) return new Set<string>()
  const excluded = new Set<string>([props.excludeId])
  let changed = true
  while (changed) {
    changed = false
    for (const node of nodes.value) {
      if (node.parentFolderId && excluded.has(node.parentFolderId) && !excluded.has(node.id)) {
        excluded.add(node.id)
        changed = true
      }
    }
  }
  return excluded
})

function childrenOf(parentId: string | null): FolderTreeNode[] {
  return nodes.value
    .filter((n) => n.parentFolderId === parentId && !excludedIds.value.has(n.id))
    .sort((a, b) => a.name.localeCompare(b.name))
}

// lista plana en orden DFS con su profundidad, para renderizar el árbol indentado
const flatTree = computed(() => {
  const result: { node: FolderTreeNode; depth: number }[] = []
  function walk(parentId: string | null, depth: number) {
    for (const node of childrenOf(parentId)) {
      result.push({ node, depth })
      walk(node.id, depth + 1)
    }
  }
  walk(null, 0)
  return result
})

async function moveTo(targetFolderId: string | null) {
  error.value = ''
  isSubmitting.value = true
  try {
    const endpoint = props.kind === 'folder' ? `/files/folders/${props.id}/move` : `/files/${props.id}/move`
    await api.put(endpoint, { targetFolderId })
    emit('moved')
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Error al mover'
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
          <i class="ph ph-folder-notch-open title-icon"></i>
          <h3 class="modal-title">Mover a...</h3>
        </div>
        <button class="close-btn" @click="$emit('close')">
          <i class="ph ph-x"></i>
        </button>
      </div>

      <div v-if="isLoading" class="loading-state">
        <i class="ph ph-circle-notch spin loading-icon"></i>
        <p>Cargando destinos...</p>
      </div>

      <div v-else class="tree">
        <button
          v-if="kind === 'folder'"
          class="tree-item root"
          :disabled="isSubmitting"
          @click="moveTo(null)"
        >
          <i class="ph ph-house"></i>
          <span>Directorio raíz</span>
        </button>

        <button
          v-for="{ node, depth } in flatTree"
          :key="node.id"
          class="tree-item"
          :style="{ paddingLeft: `${1 + depth * 1.25}rem` }"
          :disabled="isSubmitting"
          @click="moveTo(node.id)"
        >
          <i class="ph-fill ph-folder"></i>
          <span>{{ node.name }}</span>
        </button>

        <div v-if="flatTree.length === 0 && kind !== 'folder'" class="empty-state">
          No hay carpetas de destino disponibles.
        </div>
      </div>

      <div v-if="error" class="error-msg">
        <i class="ph ph-warning-circle"></i>
        <span>{{ error }}</span>
      </div>

      <div class="modal-actions">
        <button type="button" class="btn btn-secondary" @click="$emit('close')">Cancelar</button>
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
  max-width: 440px;
  max-height: 85vh;
  display: flex;
  flex-direction: column;
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

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 0;
  color: var(--color-text-muted);
  gap: 1rem;
}

.loading-icon {
  font-size: 2rem;
  color: var(--brand-primary);
}

.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.tree {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  overflow-y: auto;
  max-height: 50vh;
  padding: 0.25rem;
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  margin-bottom: 0.5rem;
}

/* Custom Scrollbar for tree */
.tree::-webkit-scrollbar {
  width: 6px;
}
.tree::-webkit-scrollbar-track {
  background: transparent;
}
.tree::-webkit-scrollbar-thumb {
  background-color: var(--color-border-hover);
  border-radius: 10px;
}

.tree-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  text-align: left;
  padding: 0.6rem 0.75rem;
  border: 1px solid transparent;
  border-radius: 6px;
  background: transparent;
  color: var(--color-heading);
  font-size: 0.95rem;
  font-weight: 500;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.tree-item i {
  font-size: 1.25rem;
  color: var(--brand-primary);
}

.tree-item:hover:not(:disabled) {
  background: var(--color-background-mute);
  border-color: var(--color-border);
  transform: translateX(2px);
}

.tree-item:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.tree-item.root {
  font-weight: 600;
  border-bottom: 1px solid var(--color-border);
  border-radius: 6px 6px 0 0;
  margin-bottom: 0.25rem;
  padding-bottom: 0.75rem;
  background: var(--color-background);
}
.tree-item.root:hover:not(:disabled) {
  background: var(--color-background-mute);
  transform: none;
}
.tree-item.root i {
  color: var(--color-heading);
}

.empty-state {
  padding: 2rem 1rem;
  text-align: center;
  color: var(--color-text-muted);
  font-size: 0.9rem;
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
  margin-top: 0.5rem;
}

.error-msg i {
  font-size: 1.1rem;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: 1rem;
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
</style>
