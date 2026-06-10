<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useFilesStore } from '@/stores/files'
import { api } from '@/services/api'
import type { ApiResponse, FileItem, Folder } from '@/types'

const filesStore = useFilesStore()
const uploadProgress = ref(0)
const isUploading = ref(false)

onMounted(async () => {
  filesStore.setLoading(true)
  try {
    const res = await api.get<ApiResponse<{ folders: Folder[]; files: FileItem[] }>>('/files/personal')
    filesStore.setFolders(res.data.folders)
    filesStore.setFiles(res.data.files)
  } catch (e) {
    console.error(e)
  } finally {
    filesStore.setLoading(false)
  }
})

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  if (bytes < 1024 * 1024 * 1024) return `${(bytes / 1024 / 1024).toFixed(1)} MB`
  return `${(bytes / 1024 / 1024 / 1024).toFixed(2)} GB`
}
</script>

<template>
  <div>
    <div class="page-header">
      <h2>Mis archivos</h2>
      <button class="upload-btn">+ Subir archivo</button>
    </div>

    <div v-if="isUploading" class="upload-progress">
      <span>Subiendo... {{ uploadProgress }}%</span>
      <div class="progress-bar">
        <div class="progress-fill" :style="{ width: `${uploadProgress}%` }" />
      </div>
    </div>

    <div v-if="filesStore.isLoading" class="loading">Cargando archivos...</div>

    <div v-else>
      <div v-if="filesStore.folders.length" class="section">
        <h3 class="section-title">Carpetas</h3>
        <div class="folders-grid">
          <div v-for="folder in filesStore.folders" :key="folder.id" class="folder-card">
            <span class="folder-icon">📁</span>
            <span class="folder-name">{{ folder.name }}</span>
          </div>
        </div>
      </div>

      <div v-if="filesStore.files.length" class="section">
        <h3 class="section-title">Archivos</h3>
        <div class="files-list">
          <div v-for="file in filesStore.files" :key="file.id" class="file-row">
            <span class="file-name">{{ file.name }}</span>
            <span class="file-size">{{ formatSize(file.size) }}</span>
            <div class="file-actions">
              <button class="action-btn">Descargar</button>
              <button class="action-btn danger">Eliminar</button>
            </div>
          </div>
        </div>
      </div>

      <div v-if="!filesStore.folders.length && !filesStore.files.length" class="empty-state">
        <p>No tienes archivos todavia. Sube tu primer archivo.</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.5rem;
}

.page-header h2 {
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0;
  color: var(--color-heading);
}

.upload-btn {
  padding: 0.5rem 1rem;
  background: var(--color-heading);
  color: var(--color-background);
  border: none;
  border-radius: 6px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.15s;
}

.upload-btn:hover {
  opacity: 0.85;
}

.upload-progress {
  margin-bottom: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
  font-size: 0.875rem;
  color: var(--color-text);
}

.progress-bar {
  height: 6px;
  background: var(--color-background-mute);
  border-radius: 99px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: var(--color-heading);
  border-radius: 99px;
  transition: width 0.2s;
}

.loading {
  color: var(--color-text);
}

.section {
  margin-bottom: 2rem;
}

.section-title {
  font-size: 0.875rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text);
  margin: 0 0 0.75rem;
}

.folders-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: 0.75rem;
}

.folder-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 1rem;
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.15s;
}

.folder-card:hover {
  background: var(--color-background-mute);
}

.folder-icon {
  font-size: 2rem;
}

.folder-name {
  font-size: 0.85rem;
  text-align: center;
  color: var(--color-heading);
  font-weight: 500;
  word-break: break-word;
}

.files-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.file-row {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.75rem 1rem;
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: 8px;
}

.file-name {
  flex: 1;
  font-size: 0.9rem;
  color: var(--color-heading);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.file-size {
  font-size: 0.8rem;
  color: var(--color-text);
  flex-shrink: 0;
}

.file-actions {
  display: flex;
  gap: 0.4rem;
  flex-shrink: 0;
}

.action-btn {
  padding: 0.3rem 0.6rem;
  border-radius: 4px;
  border: 1px solid var(--color-border);
  background: var(--color-background);
  font-size: 0.8rem;
  cursor: pointer;
  color: var(--color-text);
}

.action-btn.danger {
  background: #fee2e2;
  color: #991b1b;
  border-color: #fecaca;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  color: var(--color-text);
  font-size: 0.95rem;
}
</style>
