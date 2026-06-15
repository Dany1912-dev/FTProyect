<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useFilesStore } from '@/stores/files'
import { useDialogStore } from '@/stores/dialog'
import { api, BASE_URL } from '@/services/api'
import { startTusUpload } from '@/services/tus'
import type { ApiResponse, FolderContents, Folder, FileItem } from '@/types'
import CreateFolderModal from '@/components/files/CreateFolderModal.vue'
import FolderMembersModal from '@/components/files/FolderMembersModal.vue'
import RenameModal from '@/components/files/RenameModal.vue'
import MoveModal from '@/components/files/MoveModal.vue'

const filesStore = useFilesStore()
const dialog = useDialogStore()

const showCreateGroup = ref(false)
const showMembers = ref(false)
const showCreateFolder = ref(false)
const uploadProgress = ref<number | null>(null)
const fileInput = ref<HTMLInputElement | null>(null)
const myRole = ref<'owner' | 'editor' | 'viewer' | null>(null)
const renameTarget = ref<{ kind: 'folder' | 'file'; id: string; name: string } | null>(null)
const moveTarget = ref<{ kind: 'folder' | 'file'; id: string; rootFolderId: string; excludeId?: string } | null>(null)

onMounted(() => load())

async function load(folderId?: string) {
  filesStore.setLoading(true)
  try {
    const query = folderId ? `?folderId=${folderId}` : ''
    const res = await api.get<ApiResponse<FolderContents>>(`/files/groups${query}`)
    filesStore.setCurrentFolder(res.data.folder ?? null)
    filesStore.setPath(res.data.path)
    filesStore.setFolders(res.data.folders)
    filesStore.setFiles(res.data.files)
    myRole.value = (res.data.myRole as 'owner' | 'editor' | 'viewer' | undefined) ?? null
  } catch (e) {
    await dialog.alert({
      title: 'Error',
      message: e instanceof Error ? e.message : 'No se pudieron cargar los grupos',
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

function onGroupCreated() {
  showCreateGroup.value = false
  load()
}

async function handleDeleteGroup() {
  const folder = filesStore.currentFolder
  if (!folder) return

  const confirmed = await dialog.confirm({
    title: 'Eliminar grupo',
    message: `¿Eliminar el grupo "${folder.name}" y todo su contenido? Se moverá a la papelera.`,
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
      message: e instanceof Error ? e.message : 'No se pudo eliminar el grupo',
    })
  }
}

function onSubfolderCreated() {
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

function rootFolderIdFor(): string {
  return filesStore.path[0]?.id ?? filesStore.currentFolder!.id
}

function handleMoveFolder(folder: Folder) {
  moveTarget.value = { kind: 'folder', id: folder.id, rootFolderId: rootFolderIdFor(), excludeId: folder.id }
}

function handleMoveFile(file: FileItem) {
  moveTarget.value = { kind: 'file', id: file.id, rootFolderId: rootFolderIdFor() }
}

function onMoved() {
  moveTarget.value = null
  load(filesStore.currentFolder?.id)
}

function triggerUpload() {
  fileInput.value?.click()
}

async function onFileSelected(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file || !filesStore.currentFolder) return

  uploadProgress.value = 0
  startTusUpload({
    file,
    folderId: filesStore.currentFolder.id,
    onProgress: (percent) => {
      uploadProgress.value = percent
    },
    onSuccess: async () => {
      uploadProgress.value = null
      input.value = ''
      await load(filesStore.currentFolder!.id)
    },
    onError: async (message) => {
      uploadProgress.value = null
      input.value = ''
      await dialog.alert({ title: 'Error', message })
    },
  })
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

function onLeftGroup() {
  filesStore.setCurrentFolder(null)
  load()
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
      <div class="breadcrumb">
        <span class="breadcrumb-item" :class="{ link: filesStore.currentFolder }" @click="goToRoot">
          Grupos
        </span>
        <template v-for="p in filesStore.path" :key="p.id">
          <span class="separator">/</span>
          <span class="breadcrumb-item link" @click="load(p.id)">{{ p.name }}</span>
        </template>
        <template v-if="filesStore.currentFolder">
          <span class="separator">/</span>
          <span class="breadcrumb-item current">{{ filesStore.currentFolder.name }}</span>
        </template>
      </div>

      <div class="header-actions">
        <template v-if="!filesStore.currentFolder">
          <button class="header-btn" @click="showCreateGroup = true">+ Nuevo grupo</button>
        </template>
        <template v-else>
          <button class="header-btn" @click="showMembers = true">Miembros</button>
          <template v-if="myRole === 'owner' || myRole === 'editor'">
            <button class="header-btn" @click="showCreateFolder = true">+ Nueva subcarpeta</button>
            <button class="header-btn" :disabled="uploadProgress !== null" @click="triggerUpload">
              {{ uploadProgress !== null ? `Subiendo... ${uploadProgress}%` : '+ Subir archivo' }}
            </button>
          </template>
          <button v-if="myRole === 'owner'" class="header-btn danger" @click="handleDeleteGroup">
            Eliminar grupo
          </button>
        </template>
        <input ref="fileInput" type="file" class="hidden-input" @change="onFileSelected" />
      </div>
    </div>

    <p v-if="filesStore.currentFolder" class="folder-subtitle">
      tu rol: {{ myRole === 'owner' ? 'dueño' : myRole === 'editor' ? 'editor' : 'lector' }}
    </p>

    <div v-if="filesStore.isLoading" class="loading">Cargando...</div>

    <div v-else>
      <div v-if="filesStore.folders.length" class="section">
        <h3 class="section-title">Grupos</h3>
        <div class="folders-grid">
          <div
            v-for="folder in filesStore.folders"
            :key="folder.id"
            class="folder-card"
            @click="openFolder(folder)"
          >
            <span class="folder-icon">{{ folder.parentFolderId ? '📁' : '👥' }}</span>
            <span class="folder-name">{{ folder.name }}</span>
            <span class="folder-owner">creado por {{ folder.ownerUsername }}</span>
            <div v-if="filesStore.currentFolder && (myRole === 'owner' || myRole === 'editor')" class="folder-actions">
              <button class="action-btn" @click.stop="handleRenameFolder(folder)">Renombrar</button>
              <button class="action-btn" @click.stop="handleMoveFolder(folder)">Mover</button>
            </div>
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
              <a class="action-btn" :href="`${BASE_URL}/files/${file.id}/download`" download>Descargar</a>
              <template v-if="myRole === 'owner' || myRole === 'editor'">
                <button class="action-btn" @click="handleRenameFile(file)">Renombrar</button>
                <button class="action-btn" @click="handleMoveFile(file)">Mover</button>
                <button class="action-btn danger" @click="handleDeleteFile(file)">Eliminar</button>
              </template>
            </div>
          </div>
        </div>
      </div>

      <div v-if="!filesStore.folders.length && !filesStore.files.length" class="empty-state">
        <p v-if="filesStore.currentFolder">Este grupo no tiene archivos todavía.</p>
        <p v-else>No perteneces a ningún grupo todavía. Crea uno.</p>
      </div>
    </div>

    <CreateFolderModal
      v-if="showCreateGroup"
      endpoint="/files/groups"
      title="Nuevo grupo"
      @close="showCreateGroup = false"
      @created="onGroupCreated"
    />

    <FolderMembersModal
      v-if="showMembers && filesStore.currentFolder"
      :folder="filesStore.currentFolder"
      @close="showMembers = false"
      @left="onLeftGroup"
    />

    <CreateFolderModal
      v-if="showCreateFolder"
      title="Nueva subcarpeta"
      :parent-folder-id="filesStore.currentFolder?.id ?? null"
      @close="showCreateFolder = false"
      @created="onSubfolderCreated"
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
  </div>
</template>

<style scoped>
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.5rem;
  gap: 1rem;
  flex-wrap: wrap;
}

.breadcrumb {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--color-heading);
}

.breadcrumb-item.link {
  cursor: pointer;
}

.breadcrumb-item.link:hover {
  text-decoration: underline;
}

.breadcrumb-item.current {
  color: var(--color-text);
}

.separator {
  color: var(--color-text);
}

.folder-subtitle {
  margin: 0 0 1.5rem;
  font-size: 0.875rem;
  color: var(--color-text);
}

.header-actions {
  display: flex;
  gap: 0.5rem;
}

.hidden-input {
  display: none;
}

.header-btn {
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

.header-btn:hover:not(:disabled) {
  opacity: 0.85;
}

.header-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.header-btn.danger {
  background: #fee2e2;
  color: #991b1b;
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
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.35rem;
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

.folder-owner {
  font-size: 0.75rem;
  color: var(--color-text);
  text-align: center;
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
