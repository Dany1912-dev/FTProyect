<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useDialogStore } from '@/stores/dialog'
import { api } from '@/services/api'
import type { ApiResponse, TrashContents, Folder, FileItem } from '@/types'

const dialog = useDialogStore()

const folders = ref<Folder[]>([])
const files = ref<FileItem[]>([])
const isLoading = ref(false)

onMounted(load)

async function load() {
  isLoading.value = true
  try {
    const res = await api.get<ApiResponse<TrashContents>>('/files/trash')
    folders.value = res.data.folders
    files.value = res.data.files
  } catch (e) {
    await dialog.alert({
      title: 'Error',
      message: e instanceof Error ? e.message : 'No se pudo cargar la papelera',
    })
  } finally {
    isLoading.value = false
  }
}

async function restoreFolder(folder: Folder) {
  try {
    await api.post(`/files/trash/folders/${folder.id}/restore`)
    await load()
  } catch (e) {
    await dialog.alert({
      title: 'Error',
      message: e instanceof Error ? e.message : 'No se pudo restaurar la carpeta',
    })
  }
}

async function restoreFile(file: FileItem) {
  try {
    await api.post(`/files/trash/files/${file.id}/restore`)
    await load()
  } catch (e) {
    await dialog.alert({
      title: 'Error',
      message: e instanceof Error ? e.message : 'No se pudo restaurar el archivo',
    })
  }
}

async function deleteFolderForever(folder: Folder) {
  const confirmed = await dialog.confirm({
    title: 'Eliminar permanentemente',
    message: `¿Eliminar definitivamente la carpeta "${folder.name}" y todo su contenido? Esta acción no se puede deshacer.`,
    confirmText: 'Eliminar',
    danger: true,
  })
  if (!confirmed) return

  try {
    await api.delete(`/files/trash/folders/${folder.id}`)
    await load()
  } catch (e) {
    await dialog.alert({
      title: 'Error',
      message: e instanceof Error ? e.message : 'No se pudo eliminar la carpeta',
    })
  }
}

async function deleteFileForever(file: FileItem) {
  const confirmed = await dialog.confirm({
    title: 'Eliminar permanentemente',
    message: `¿Eliminar definitivamente "${file.name}"? Esta acción no se puede deshacer.`,
    confirmText: 'Eliminar',
    danger: true,
  })
  if (!confirmed) return

  try {
    await api.delete(`/files/trash/files/${file.id}`)
    await load()
  } catch (e) {
    await dialog.alert({
      title: 'Error',
      message: e instanceof Error ? e.message : 'No se pudo eliminar el archivo',
    })
  }
}

async function emptyTrash() {
  const confirmed = await dialog.confirm({
    title: 'Vaciar papelera',
    message: '¿Eliminar definitivamente todo el contenido de la papelera? Esta acción no se puede deshacer.',
    confirmText: 'Vaciar',
    danger: true,
  })
  if (!confirmed) return

  try {
    await api.delete('/files/trash')
    await load()
  } catch (e) {
    await dialog.alert({
      title: 'Error',
      message: e instanceof Error ? e.message : 'No se pudo vaciar la papelera',
    })
  }
}

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
      <h2 class="page-title">Papelera</h2>
      <button
        v-if="folders.length || files.length"
        class="header-btn danger"
        @click="emptyTrash"
      >
        Vaciar papelera
      </button>
    </div>

    <div v-if="isLoading" class="loading">Cargando...</div>

    <div v-else>
      <div v-if="folders.length" class="section">
        <h3 class="section-title">Carpetas</h3>
        <div class="folders-grid">
          <div v-for="folder in folders" :key="folder.id" class="folder-card">
            <span class="folder-icon">📁</span>
            <span class="folder-name">{{ folder.name }}</span>
            <div class="folder-actions">
              <button class="action-btn" @click="restoreFolder(folder)">Restaurar</button>
              <button class="action-btn danger" @click="deleteFolderForever(folder)">Eliminar</button>
            </div>
          </div>
        </div>
      </div>

      <div v-if="files.length" class="section">
        <h3 class="section-title">Archivos</h3>
        <div class="files-list">
          <div v-for="file in files" :key="file.id" class="file-row">
            <span class="file-name">{{ file.name }}</span>
            <span class="file-size">{{ formatSize(file.size) }}</span>
            <div class="file-actions">
              <button class="action-btn" @click="restoreFile(file)">Restaurar</button>
              <button class="action-btn danger" @click="deleteFileForever(file)">Eliminar</button>
            </div>
          </div>
        </div>
      </div>

      <div v-if="!folders.length && !files.length" class="empty-state">
        <p>La papelera está vacía.</p>
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

.page-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--color-heading);
  margin: 0;
}

.header-btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 6px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.15s;
}

.header-btn.danger {
  background: #fee2e2;
  color: #991b1b;
}

.header-btn:hover {
  opacity: 0.85;
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
  gap: 0.35rem;
  padding: 1rem;
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: 8px;
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

.folder-actions {
  display: flex;
  gap: 0.35rem;
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
  display: inline-block;
  padding: 0.3rem 0.6rem;
  border-radius: 4px;
  border: 1px solid var(--color-border);
  background: var(--color-background);
  font-size: 0.8rem;
  cursor: pointer;
  color: var(--color-text);
  text-decoration: none;
  line-height: 1.2;
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
