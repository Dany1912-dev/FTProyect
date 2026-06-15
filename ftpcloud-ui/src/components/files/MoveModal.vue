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
      <h3 class="modal-title">Mover a...</h3>

      <div v-if="isLoading" class="loading">Cargando...</div>

      <div v-else class="tree">
        <button
          v-if="kind === 'folder'"
          class="tree-item root"
          :disabled="isSubmitting"
          @click="moveTo(null)"
        >
          📁 Raíz (mover fuera de cualquier carpeta)
        </button>

        <button
          v-for="{ node, depth } in flatTree"
          :key="node.id"
          class="tree-item"
          :style="{ paddingLeft: `${0.75 + depth}rem` }"
          :disabled="isSubmitting"
          @click="moveTo(node.id)"
        >
          📁 {{ node.name }}
        </button>
      </div>

      <p v-if="error" class="error-msg">{{ error }}</p>

      <div class="modal-actions">
        <button type="button" class="btn-cancel" @click="$emit('close')">Cancelar</button>
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
  z-index: 100;
}

.modal {
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 1.5rem;
  width: 100%;
  max-width: 420px;
  max-height: 80vh;
  display: flex;
  flex-direction: column;
}

.modal-title {
  font-size: 1.25rem;
  font-weight: 700;
  margin: 0 0 1.25rem;
  color: var(--color-heading);
}

.loading {
  color: var(--color-text);
  font-size: 0.9rem;
}

.tree {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  overflow-y: auto;
  max-height: 50vh;
}

.tree-item {
  text-align: left;
  padding: 0.5rem 0.75rem;
  border: 1px solid transparent;
  border-radius: 6px;
  background: none;
  color: var(--color-text);
  font-size: 0.9rem;
  cursor: pointer;
  transition: background 0.15s;
}

.tree-item:hover:not(:disabled) {
  background: var(--color-background-mute);
}

.tree-item:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.tree-item.root {
  font-weight: 600;
  border-bottom: 1px solid var(--color-border);
  border-radius: 0;
  margin-bottom: 0.25rem;
  padding-bottom: 0.6rem;
}

.error-msg {
  color: #e05252;
  font-size: 0.875rem;
  margin: 0.75rem 0 0;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: 1rem;
}

.btn-cancel {
  padding: 0.6rem 1.1rem;
  border: none;
  border-radius: 6px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  background: var(--color-background-mute);
  color: var(--color-text);
  transition: opacity 0.15s;
}

.btn-cancel:hover {
  opacity: 0.85;
}
</style>
