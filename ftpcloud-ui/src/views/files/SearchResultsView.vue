<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { api, BASE_URL } from '@/services/api'
import { useDialogStore } from '@/stores/dialog'
import type { ApiResponse, SearchResults, SearchResultItem } from '@/types'
import FilePreviewModal from '@/components/files/FilePreviewModal.vue'

const route = useRoute()
const router = useRouter()
const dialog = useDialogStore()

const items = ref<SearchResultItem[]>([])
const query = ref('')
const totalCount = ref(0)
const isLoading = ref(false)
const previewFile = ref<SearchResultItem | null>(null)

onMounted(async () => {
  const q = route.query.q
  if (!q || typeof q !== 'string' || !q.trim()) return
  query.value = q.trim()
  await search()
})

async function search() {
  isLoading.value = true
  try {
    const res = await api.get<ApiResponse<SearchResults>>(`/files/search?q=${encodeURIComponent(query.value)}`)
    items.value = res.data.items
    totalCount.value = res.data.totalCount
  } catch (e) {
    await dialog.alert({
      title: 'Error',
      message: e instanceof Error ? e.message : 'Error al buscar',
    })
  } finally {
    isLoading.value = false
  }
}

function navigateToFolder(item: SearchResultItem) {
  const view = item.source === 'own' ? 'my-files'
    : item.source === 'group' ? 'groups'
    : 'shared'
  router.push({ name: view, query: { folderId: item.id } })
}

function handleItemClick(item: SearchResultItem) {
  if (item.kind === 'folder') {
    navigateToFolder(item)
  } else {
    previewFile.value = item
  }
}

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  if (bytes < 1024 * 1024 * 1024) return `${(bytes / 1024 / 1024).toFixed(1)} MB`
  return `${(bytes / 1024 / 1024 / 1024).toFixed(2)} GB`
}

function sourceLabel(source: string): string {
  switch (source) {
    case 'own': return 'Propio'
    case 'shared': return 'Compartido'
    case 'group': return 'Grupo'
    case 'file_share': return 'Compartido directamente'
    default: return source
  }
}
</script>

<template>
  <div>
    <div class="page-header">
      <h2 class="page-title">Resultados de búsqueda</h2>
      <span v-if="query" class="search-query">«{{ query }}» — {{ totalCount }} resultados</span>
    </div>

    <div v-if="isLoading" class="loading">Buscando...</div>

    <div v-else-if="!query" class="empty-state">
      <i class="ph ph-magnifying-glass empty-icon"></i>
      <p>Usa la barra de búsqueda para encontrar archivos y carpetas.</p>
    </div>

    <div v-else-if="!items.length" class="empty-state">
      <i class="ph ph-smiley-sad empty-icon"></i>
      <p>No se encontraron resultados para «{{ query }}».</p>
    </div>

    <div v-else class="results-list">
      <div
        v-for="item in items"
        :key="`${item.kind}-${item.id}`"
        class="result-row clickable"
        @click="handleItemClick(item)"
      >
        <div class="result-icon">
          <i v-if="item.kind === 'folder'" class="ph ph-folder"></i>
          <i v-else class="ph ph-file"></i>
        </div>
        <div class="result-info">
          <span class="result-name">{{ item.name }}</span>
          <span v-if="item.parentName" class="result-parent">{{ item.parentName }}</span>
        </div>
        <span v-if="item.kind === 'file' && item.size" class="result-size">{{ formatSize(item.size) }}</span>
        <span class="result-source" :class="item.source">{{ sourceLabel(item.source) }}</span>
        <a
          v-if="item.kind === 'file'"
          class="result-download"
          :href="`${BASE_URL}/files/${item.id}/download`"
          @click.stop
          title="Descargar"
        >
          <i class="ph ph-download-simple"></i>
        </a>
      </div>
    </div>

    <FilePreviewModal
      v-if="previewFile"
      :file="{ id: previewFile.id, name: previewFile.name, size: previewFile.size ?? 0, mimeType: previewFile.mimeType ?? '' }"
      @close="previewFile = null"
    />
  </div>
</template>

<style scoped>
.page-header {
  display: flex;
  align-items: baseline;
  gap: 1rem;
  margin-bottom: 1.5rem;
  flex-wrap: wrap;
}

.page-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--color-heading);
  margin: 0;
}

.search-query {
  font-size: 0.9rem;
  color: var(--color-text-muted);
}

.loading {
  color: var(--color-text);
  padding: 2rem 0;
  text-align: center;
}

.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  color: var(--color-text-muted);
  font-size: 0.95rem;
}

.empty-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
  display: block;
  color: var(--color-border);
}

.results-list {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

.result-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  transition: background var(--transition-fast);
}

.result-row.clickable {
  cursor: pointer;
}

.result-row.clickable:hover {
  background: var(--color-background-mute);
}

.result-icon {
  font-size: 1.5rem;
  color: var(--brand-primary);
  flex-shrink: 0;
}

.result-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.result-name {
  font-weight: 600;
  color: var(--color-heading);
  font-size: 0.9rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.result-parent {
  font-size: 0.75rem;
  color: var(--color-text-muted);
}

.result-size {
  font-size: 0.8rem;
  color: var(--color-text);
  flex-shrink: 0;
}

.result-source {
  font-size: 0.7rem;
  font-weight: 600;
  padding: 0.2rem 0.5rem;
  border-radius: 99px;
  flex-shrink: 0;
  white-space: nowrap;
}

.result-source.own {
  background: var(--color-background-mute);
  color: var(--color-text);
}

.result-source.shared {
  background: #dbeafe;
  color: #1e40af;
}

.result-source.group {
  background: #fef3c7;
  color: #92400e;
}

.result-source.file_share {
  background: #e0e7ff;
  color: #3730a3;
}

.result-download {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: var(--radius-sm);
  color: var(--brand-primary);
  text-decoration: none;
  font-size: 1.1rem;
  transition: background var(--transition-fast);
  flex-shrink: 0;
}

.result-download:hover {
  background: var(--brand-primary-light);
}
</style>
