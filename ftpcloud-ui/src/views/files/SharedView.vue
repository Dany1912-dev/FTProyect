<script setup lang="ts">
import { onMounted } from 'vue'
import { useFilesStore } from '@/stores/files'
import { api } from '@/services/api'
import type { ApiResponse, FileItem, Folder } from '@/types'

const filesStore = useFilesStore()

onMounted(async () => {
  filesStore.setLoading(true)
  try {
    const res = await api.get<ApiResponse<{ folders: Folder[]; files: FileItem[] }>>('/files/shared')
    filesStore.setFolders(res.data.folders)
    filesStore.setFiles(res.data.files)
  } catch (e) {
    console.error(e)
  } finally {
    filesStore.setLoading(false)
  }
})
</script>

<template>
  <div>
    <div class="page-header">
      <h2>Compartidos conmigo</h2>
    </div>

    <div v-if="filesStore.isLoading" class="loading">Cargando...</div>

    <div v-else>
      <div v-if="filesStore.folders.length" class="folders-grid">
        <div v-for="folder in filesStore.folders" :key="folder.id" class="folder-card">
          <span class="folder-icon">📂</span>
          <span class="folder-name">{{ folder.name }}</span>
        </div>
      </div>

      <div v-if="!filesStore.folders.length && !filesStore.files.length" class="empty-state">
        <p>Nadie ha compartido archivos contigo todavia.</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-header {
  margin-bottom: 1.5rem;
}

.page-header h2 {
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0;
  color: var(--color-heading);
}

.loading {
  color: var(--color-text);
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
}

.empty-state {
  text-align: center;
  padding: 3rem;
  color: var(--color-text);
  font-size: 0.95rem;
}
</style>
