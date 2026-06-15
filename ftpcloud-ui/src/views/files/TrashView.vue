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
  <div class="page-container">
    <div class="page-header">
      <div class="breadcrumb">
        <span class="breadcrumb-item">
          <i class="ph ph-trash"></i>
          Papelera
        </span>
      </div>

      <div class="header-actions">
        <button
          v-if="folders.length || files.length"
          class="btn btn-danger"
          @click="emptyTrash"
        >
          <i class="ph ph-warning"></i>
          <span class="btn-text">Vaciar papelera</span>
        </button>
      </div>
    </div>

    <div v-if="folders.length || files.length" class="folder-subtitle-card danger-soft">
      <i class="ph ph-info"></i>
      <span>Los elementos en la papelera serán eliminados permanentemente si decides vaciarla o después de 30 días.</span>
    </div>

    <div v-if="isLoading" class="loading-state">
      <i class="ph ph-circle-notch spin loading-icon"></i>
      <p>Cargando papelera...</p>
    </div>

    <div v-else class="content-area">
      <div v-if="folders.length" class="section">
        <div class="section-header">
          <h3 class="section-title">Carpetas</h3>
          <span class="badge">{{ folders.length }}</span>
        </div>
        <div class="folders-grid">
          <div v-for="folder in folders" :key="folder.id" class="folder-card trash-card">
            <div class="folder-icon-wrapper danger-bg">
              <i class="ph-fill ph-folder folder-icon"></i>
            </div>
            <div class="folder-info">
              <span class="folder-name" :title="folder.name">{{ folder.name }}</span>
            </div>
            <div class="folder-actions-menu show-always">
              <button class="icon-action-btn success" @click="restoreFolder(folder)" title="Restaurar">
                <i class="ph ph-arrow-counter-clockwise"></i>
              </button>
              <button class="icon-action-btn danger" @click="deleteFolderForever(folder)" title="Eliminar definitivamente">
                <i class="ph ph-x-circle"></i>
              </button>
            </div>
          </div>
        </div>
      </div>

      <div v-if="files.length" class="section">
        <div class="section-header">
          <h3 class="section-title">Archivos</h3>
          <span class="badge">{{ files.length }}</span>
        </div>
        <div class="files-list">
          <div class="file-list-header">
            <div class="col-name">Nombre</div>
            <div class="col-size">Tamaño</div>
            <div class="col-actions"></div>
          </div>
          <div v-for="file in files" :key="file.id" class="file-row trash-row">
            <div class="col-name">
              <i class="ph ph-file-text file-icon danger-text"></i>
              <span class="file-name" :title="file.name">{{ file.name }}</span>
            </div>
            <div class="col-size">{{ formatSize(file.size) }}</div>
            <div class="col-actions file-actions show-always">
              <button class="icon-action-btn success" @click="restoreFile(file)" title="Restaurar">
                <i class="ph ph-arrow-counter-clockwise"></i>
              </button>
              <button class="icon-action-btn danger" @click="deleteFileForever(file)" title="Eliminar definitivamente">
                <i class="ph ph-x-circle"></i>
              </button>
            </div>
          </div>
        </div>
      </div>

      <div v-if="!folders.length && !files.length" class="empty-state">
        <div class="empty-icon-wrapper success-bg">
          <i class="ph ph-check-circle empty-icon success-text"></i>
        </div>
        <h3 class="empty-title">La papelera está vacía</h3>
        <p class="empty-desc">No hay archivos ni carpetas eliminados recientemente.</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  gap: 2rem;
  animation: fadeIn 0.3s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
  background: var(--glass-bg);
  backdrop-filter: blur(8px);
  padding: 1rem 1.5rem;
  border-radius: var(--radius-lg);
  border: 1px solid var(--color-border);
  box-shadow: var(--shadow-sm);
}

.breadcrumb {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--color-heading);
}

.breadcrumb-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.folder-subtitle-card {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  border-radius: var(--radius-md);
  font-size: 0.9rem;
  font-weight: 500;
  margin-top: -1rem;
}

.folder-subtitle-card.danger-soft {
  background: var(--color-danger-bg);
  color: var(--color-danger);
}

.folder-subtitle-card i {
  font-size: 1.25rem;
}

.header-actions {
  display: flex;
  gap: 0.75rem;
}

.btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.6rem 1rem;
  border-radius: var(--radius-md);
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
  border: 1px solid transparent;
}

.btn i {
  font-size: 1.1rem;
}

.btn-danger {
  background: var(--color-danger);
  color: white;
  box-shadow: var(--shadow-sm);
}

.btn-danger:hover:not(:disabled) {
  background: #b91c1c;
  transform: translateY(-1px);
  box-shadow: var(--shadow-md);
}

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem;
  color: var(--color-text-muted);
  gap: 1rem;
}

.loading-icon {
  font-size: 2.5rem;
  color: var(--brand-primary);
}

.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.section {
  margin-bottom: 2.5rem;
}

.section-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1.25rem;
}

.section-title {
  font-size: 1rem;
  font-weight: 600;
  color: var(--color-heading);
  margin: 0;
}

.badge {
  background: var(--color-background-mute);
  color: var(--color-text);
  padding: 0.2rem 0.6rem;
  border-radius: var(--radius-full);
  font-size: 0.75rem;
  font-weight: 600;
}

.folders-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 1rem;
}

.folder-card {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding: 1.25rem;
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-sm);
  overflow: hidden;
  transition: all var(--transition-normal);
}

.folder-card.trash-card {
  opacity: 0.85;
}

.folder-card.trash-card:hover {
  opacity: 1;
  border-color: var(--color-border-hover);
  box-shadow: var(--shadow-md);
}

.folder-icon-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border-radius: var(--radius-sm);
}

.folder-icon-wrapper.danger-bg {
  background: var(--color-danger-bg);
  color: var(--color-danger);
}

.folder-icon {
  font-size: 1.75rem;
}

.folder-info {
  display: flex;
  flex-direction: column;
  width: 100%;
}

.folder-name {
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--color-heading);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  width: 100%;
  text-decoration: line-through;
  opacity: 0.8;
}

.folder-actions-menu.show-always {
  position: absolute;
  top: 0.75rem;
  right: 0.75rem;
  display: flex;
  gap: 0.25rem;
  opacity: 1;
  transform: none;
  background: var(--color-background-soft);
  padding: 0.25rem;
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-sm);
  border: 1px solid var(--color-border);
}

.files-list {
  display: flex;
  flex-direction: column;
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  overflow: hidden;
  box-shadow: var(--shadow-sm);
}

.file-list-header {
  display: flex;
  align-items: center;
  padding: 0.75rem 1.25rem;
  background: var(--color-background-mute);
  border-bottom: 1px solid var(--color-border);
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted);
}

.file-row {
  display: flex;
  align-items: center;
  padding: 0.75rem 1.25rem;
  border-bottom: 1px solid var(--color-border);
  transition: background var(--transition-fast);
}

.file-row.trash-row {
  opacity: 0.85;
}

.file-row.trash-row:hover {
  opacity: 1;
  background: var(--color-background-mute);
}

.file-row:last-child {
  border-bottom: none;
}

.col-name {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  min-width: 0;
}

.file-icon {
  font-size: 1.5rem;
}

.file-icon.danger-text {
  color: var(--color-danger);
}

.file-name {
  font-size: 0.9rem;
  font-weight: 500;
  color: var(--color-heading);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  text-decoration: line-through;
  opacity: 0.8;
}

.col-size {
  width: 120px;
  font-size: 0.85rem;
  color: var(--color-text-muted);
}

.col-actions {
  width: 100px;
  display: flex;
  justify-content: flex-end;
  gap: 0.25rem;
}

.col-actions.show-always {
  opacity: 1;
}

.icon-action-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: var(--radius-sm);
  border: 1px solid transparent;
  background: transparent;
  font-size: 1.1rem;
  color: var(--color-text);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.icon-action-btn:hover {
  background: var(--color-border);
  color: var(--color-heading);
}

.icon-action-btn.danger:hover {
  background: var(--color-danger-bg);
  color: var(--color-danger);
}

.icon-action-btn.success:hover {
  background: var(--color-success-bg);
  color: var(--color-success);
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 5rem 2rem;
  text-align: center;
  background: var(--color-background-soft);
  border: 1px dashed var(--color-border-hover);
  border-radius: var(--radius-md);
}

.empty-icon-wrapper {
  width: 80px;
  height: 80px;
  border-radius: var(--radius-full);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 1.5rem;
}

.empty-icon-wrapper.success-bg {
  background: var(--color-success-bg);
}

.empty-icon.success-text {
  color: var(--color-success);
}

.empty-icon {
  font-size: 3rem;
}

.empty-title {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--color-heading);
  margin: 0 0 0.5rem;
}

.empty-desc {
  color: var(--color-text-muted);
  font-size: 0.95rem;
  max-width: 400px;
  margin: 0;
}

@media (max-width: 768px) {
  .btn-text {
    display: none;
  }
  .col-size {
    display: none;
  }
}
</style>
