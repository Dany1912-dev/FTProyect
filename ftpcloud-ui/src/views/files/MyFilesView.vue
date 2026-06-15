<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useFilesStore } from '@/stores/files'
import { useDialogStore } from '@/stores/dialog'
import { api, BASE_URL } from '@/services/api'
import { useFileUpload } from '@/composables/useFileUpload'
import type { ApiResponse, FolderContents, Folder, FileItem } from '@/types'
import CreateFolderModal from '@/components/files/CreateFolderModal.vue'
import FolderMembersModal from '@/components/files/FolderMembersModal.vue'
import RenameModal from '@/components/files/RenameModal.vue'
import MoveModal from '@/components/files/MoveModal.vue'
import FileShareModal from '@/components/files/FileShareModal.vue'
import FilePreviewModal from '@/components/files/FilePreviewModal.vue'
import UploadQueue from '@/components/files/UploadQueue.vue'

const filesStore = useFilesStore()
const dialog = useDialogStore()

const showCreateFolder = ref(false)
const showMembers = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)
const isDragging = ref(false)
const renameTarget = ref<{ kind: 'folder' | 'file'; id: string; name: string } | null>(null)
const moveTarget = ref<{ kind: 'folder' | 'file'; id: string; rootFolderId: string; excludeId?: string } | null>(null)
const showFileShare = ref<{ file: FileItem; folderOwnerId: string } | null>(null)
const previewFile = ref<FileItem | null>(null)

const { queue: uploadQueue, uploadFiles } = useFileUpload(
  () => filesStore.currentFolder?.id,
  () => load(filesStore.currentFolder?.id),
)

onMounted(() => load())

async function load(folderId?: string) {
  filesStore.setLoading(true)
  try {
    const query = folderId ? `?folderId=${folderId}` : ''
    const res = await api.get<ApiResponse<FolderContents>>(`/files/personal${query}`)
    filesStore.setCurrentFolder(res.data.folder ?? null)
    filesStore.setPath(res.data.path)
    filesStore.setFolders(res.data.folders)
    filesStore.setFiles(res.data.files)
  } catch (e) {
    await dialog.alert({
      title: 'Error',
      message: e instanceof Error ? e.message : 'No se pudieron cargar los archivos',
    })
  } finally {
    filesStore.setLoading(false)
  }
}

function openFolder(folder: Folder) {
  load(folder.id)
}

function goToRoot() {
  if (filesStore.currentFolder) load()
}

function onFolderCreated() {
  showCreateFolder.value = false
  load(filesStore.currentFolder?.id)
}

function handleRenameFolder(folder: Folder) {
  renameTarget.value = { kind: 'folder', id: folder.id, name: folder.name }
}

function handleRenameFile(file: FileItem) {
  renameTarget.value = { kind: 'file', id: file.id, name: file.name }
}

function onRenamed() {
  renameTarget.value = null
  load(filesStore.currentFolder?.id)
}

function rootFolderIdFor(folder?: Folder): string {
  if (!filesStore.currentFolder) return folder!.id
  return filesStore.path[0]?.id ?? filesStore.currentFolder.id
}

function handleMoveFolder(folder: Folder) {
  moveTarget.value = { kind: 'folder', id: folder.id, rootFolderId: rootFolderIdFor(folder), excludeId: folder.id }
}

function handleMoveFile(file: FileItem) {
  moveTarget.value = { kind: 'file', id: file.id, rootFolderId: rootFolderIdFor() }
}

function onMoved() {
  moveTarget.value = null
  load(filesStore.currentFolder?.id)
}

async function handleDeleteFolder(folder: Folder) {
  const confirmed = await dialog.confirm({
    title: 'Eliminar carpeta',
    message: `¿Eliminar la carpeta "${folder.name}" y todo su contenido? Se moverá a la papelera.`,
    confirmText: 'Eliminar',
    danger: true,
  })
  if (!confirmed) return

  try {
    await api.delete(`/files/folders/${folder.id}`)
    await load()
  } catch (e) {
    await dialog.alert({
      title: 'Error',
      message: e instanceof Error ? e.message : 'No se pudo eliminar la carpeta',
    })
  }
}

function triggerUpload() {
  fileInput.value?.click()
}

function onFileSelected(event: Event) {
  const input = event.target as HTMLInputElement
  if (!input.files?.length) return
  uploadFiles(input.files)
  input.value = ''
}

function onDragOver() {
  if (filesStore.currentFolder) isDragging.value = true
}

function onDragLeave() {
  isDragging.value = false
}

function onDrop(event: DragEvent) {
  isDragging.value = false
  if (!filesStore.currentFolder || !event.dataTransfer?.files.length) return
  uploadFiles(event.dataTransfer.files)
}

async function handleDeleteFile(file: FileItem) {
  const confirmed = await dialog.confirm({
    title: 'Eliminar archivo',
    message: `¿Eliminar "${file.name}"? Se moverá a la papelera.`,
    confirmText: 'Eliminar',
    danger: true,
  })
  if (!confirmed) return

  try {
    await api.delete(`/files/${file.id}`)
    await load(filesStore.currentFolder?.id)
  } catch (e) {
    await dialog.alert({
      title: 'Error',
      message: e instanceof Error ? e.message : 'No se pudo eliminar el archivo',
    })
  }
}

function handleShareFile(file: FileItem) {
  const ownerId = filesStore.path[0]?.ownerId ?? filesStore.currentFolder!.ownerId
  showFileShare.value = { file, folderOwnerId: ownerId }
}

function handlePreviewFile(file: FileItem) {
  previewFile.value = file
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
        <span class="breadcrumb-item" :class="{ link: filesStore.currentFolder }" @click="goToRoot">
          <i class="ph ph-house"></i>
          Mis archivos
        </span>
        <template v-for="p in filesStore.path" :key="p.id">
          <i class="ph ph-caret-right separator"></i>
          <span class="breadcrumb-item link" @click="load(p.id)">{{ p.name }}</span>
        </template>
        <template v-if="filesStore.currentFolder">
          <i class="ph ph-caret-right separator"></i>
          <span class="breadcrumb-item current">{{ filesStore.currentFolder.name }}</span>
        </template>
      </div>

      <div class="header-actions">
        <button class="btn btn-secondary" @click="showCreateFolder = true">
          <i class="ph ph-folder-plus"></i>
          <span class="btn-text">{{ filesStore.currentFolder ? 'Nueva subcarpeta' : 'Nueva carpeta' }}</span>
        </button>
        <template v-if="filesStore.currentFolder">
          <button class="btn btn-secondary" @click="showMembers = true">
            <i class="ph ph-share-network"></i>
            <span class="btn-text">Compartir</span>
          </button>
          <button class="btn btn-primary" @click="triggerUpload">
            <i class="ph ph-upload-simple"></i>
            <span class="btn-text">Subir archivos</span>
          </button>
        </template>
        <input ref="fileInput" type="file" multiple class="hidden-input" @change="onFileSelected" />
      </div>
    </div>

    <div v-if="filesStore.isLoading" class="loading-state">
      <i class="ph ph-circle-notch spin loading-icon"></i>
      <p>Cargando tus archivos...</p>
    </div>

    <div
      v-else
      class="content-area"
      :class="{ 'drag-active': isDragging }"
      @dragover.prevent="onDragOver"
      @dragleave.prevent="onDragLeave"
      @drop.prevent="onDrop"
    >
      <div v-if="filesStore.folders.length" class="section">
        <div class="section-header">
          <h3 class="section-title">Carpetas</h3>
          <span class="badge">{{ filesStore.folders.length }}</span>
        </div>
        <div class="folders-grid">
          <div
            v-for="folder in filesStore.folders"
            :key="folder.id"
            class="folder-card"
            @click="openFolder(folder)"
          >
            <div class="folder-icon-wrapper">
              <i class="ph-fill ph-folder folder-icon"></i>
            </div>
            <div class="folder-info">
              <span class="folder-name" :title="folder.name">{{ folder.name }}</span>
            </div>
            <div class="folder-actions-menu" @click.stop>
              <button class="icon-action-btn" @click="handleRenameFolder(folder)" title="Renombrar">
                <i class="ph ph-pencil-simple"></i>
              </button>
              <button class="icon-action-btn" @click="handleMoveFolder(folder)" title="Mover">
                <i class="ph ph-folder-notch-open"></i>
              </button>
              <button class="icon-action-btn danger" @click="handleDeleteFolder(folder)" title="Eliminar">
                <i class="ph ph-trash"></i>
              </button>
            </div>
          </div>
        </div>
      </div>

      <div v-if="filesStore.files.length" class="section">
        <div class="section-header">
          <h3 class="section-title">Archivos</h3>
          <span class="badge">{{ filesStore.files.length }}</span>
        </div>
        <div class="files-list">
          <div class="file-list-header">
            <div class="col-name">Nombre</div>
            <div class="col-size">Tamaño</div>
            <div class="col-actions"></div>
          </div>
          <div v-for="file in filesStore.files" :key="file.id" class="file-row">
            <div class="col-name clickable" @click="handlePreviewFile(file)">
              <i class="ph ph-file-text file-icon"></i>
              <span class="file-name" :title="file.name">{{ file.name }}</span>
            </div>
            <div class="col-size">{{ formatSize(file.size) }}</div>
            <div class="col-actions file-actions">
              <button class="icon-action-btn" @click="handlePreviewFile(file)" title="Vista previa">
                <i class="ph ph-eye"></i>
              </button>
              <a class="icon-action-btn" :href="`${BASE_URL}/files/${file.id}/download`" download title="Descargar">
                <i class="ph ph-download-simple"></i>
              </a>
              <button class="icon-action-btn" @click="handleRenameFile(file)" title="Renombrar">
                <i class="ph ph-pencil-simple"></i>
              </button>
              <button class="icon-action-btn" @click="handleMoveFile(file)" title="Mover">
                <i class="ph ph-folder-notch-open"></i>
              </button>
              <button class="icon-action-btn" @click="handleShareFile(file)" title="Compartir">
                <i class="ph ph-share-network"></i>
              </button>
              <button class="icon-action-btn danger" @click="handleDeleteFile(file)" title="Eliminar">
                <i class="ph ph-trash"></i>
              </button>
            </div>
          </div>
        </div>
      </div>

      <div v-if="!filesStore.folders.length && !filesStore.files.length" class="empty-state">
        <div class="empty-icon-wrapper">
          <i class="ph ph-folder-open empty-icon"></i>
        </div>
        <h3 class="empty-title">Esta carpeta está vacía</h3>
        <p class="empty-desc" v-if="filesStore.currentFolder">Arrastra archivos aquí o usa el botón de subir archivo.</p>
        <p class="empty-desc" v-else>No tienes carpetas todavía. Crea la primera para empezar a organizar.</p>
      </div>
    </div>

    <CreateFolderModal
      v-if="showCreateFolder"
      :title="filesStore.currentFolder ? 'Nueva subcarpeta' : 'Nueva carpeta'"
      :parent-folder-id="filesStore.currentFolder?.id ?? null"
      @close="showCreateFolder = false"
      @created="onFolderCreated"
    />

    <FolderMembersModal
      v-if="showMembers && filesStore.currentFolder"
      :folder="filesStore.currentFolder"
      @close="showMembers = false"
    />

    <FileShareModal
      v-if="showFileShare"
      :file="showFileShare.file"
      :folder-owner-id="showFileShare.folderOwnerId"
      @close="showFileShare = null"
    />

    <RenameModal
      v-if="renameTarget"
      :kind="renameTarget.kind"
      :id="renameTarget.id"
      :current-name="renameTarget.name"
      @close="renameTarget = null"
      @renamed="onRenamed"
    />

    <MoveModal
      v-if="moveTarget"
      :kind="moveTarget.kind"
      :id="moveTarget.id"
      :root-folder-id="moveTarget.rootFolderId"
      :exclude-id="moveTarget.excludeId"
      @close="moveTarget = null"
      @moved="onMoved"
    />

    <FilePreviewModal
      v-if="previewFile"
      :file="previewFile"
      @close="previewFile = null"
    />

    <UploadQueue :items="uploadQueue" />
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
  transition: color var(--transition-fast);
}

.breadcrumb-item.link {
  cursor: pointer;
}

.breadcrumb-item.link:hover {
  color: var(--brand-primary);
}

.breadcrumb-item.current {
  color: var(--color-text);
  font-weight: 600;
}

.separator {
  color: var(--color-text-muted);
  font-size: 1rem;
}

.header-actions {
  display: flex;
  gap: 0.75rem;
}

.hidden-input {
  display: none;
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

.btn-primary {
  background: var(--brand-primary);
  color: white;
  box-shadow: var(--shadow-sm);
}

.btn-primary:hover:not(:disabled) {
  background: var(--brand-primary-hover);
  transform: translateY(-1px);
  box-shadow: var(--shadow-md);
}

.btn-secondary {
  background: var(--color-background-soft);
  color: var(--color-heading);
  border-color: var(--color-border);
}

.btn-secondary:hover:not(:disabled) {
  background: var(--color-background-mute);
  border-color: var(--color-border-hover);
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
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
  cursor: pointer;
  transition: all var(--transition-normal);
  box-shadow: var(--shadow-sm);
  overflow: hidden;
}

.folder-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
  border-color: var(--color-border-hover);
}

.folder-icon-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border-radius: var(--radius-sm);
  background: var(--brand-primary-light);
  color: var(--brand-primary);
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
}

.folder-actions-menu {
  position: absolute;
  top: 0.75rem;
  right: 0.75rem;
  display: flex;
  gap: 0.25rem;
  opacity: 0;
  transform: translateX(10px);
  transition: all var(--transition-fast);
  background: var(--color-background-soft);
  padding: 0.25rem;
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-sm);
  border: 1px solid var(--color-border);
}

.folder-card:hover .folder-actions-menu {
  opacity: 1;
  transform: translateX(0);
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

.file-row:last-child {
  border-bottom: none;
}

.file-row:hover {
  background: var(--color-background-mute);
}

.col-name {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  min-width: 0;
}

.col-name.clickable {
  cursor: pointer;
}

.col-name.clickable:hover .file-name {
  color: var(--brand-primary);
}

.content-area {
  border-radius: var(--radius-md);
  transition: outline var(--transition-fast), background var(--transition-fast);
  outline: 2px dashed transparent;
  outline-offset: 4px;
}

.content-area.drag-active {
  outline-color: var(--brand-primary);
  background: var(--brand-primary-light);
}

.file-icon {
  font-size: 1.5rem;
  color: var(--brand-primary);
}

.file-name {
  font-size: 0.9rem;
  font-weight: 500;
  color: var(--color-heading);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.col-size {
  width: 120px;
  font-size: 0.85rem;
  color: var(--color-text-muted);
}

.col-actions {
  width: 140px;
  display: flex;
  justify-content: flex-end;
  gap: 0.25rem;
  opacity: 0.4;
  transition: opacity var(--transition-fast);
}

.file-row:hover .col-actions {
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
  background: var(--brand-primary-light);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 1.5rem;
}

.empty-icon {
  font-size: 3rem;
  color: var(--brand-primary);
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
