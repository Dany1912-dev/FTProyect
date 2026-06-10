<script setup lang="ts">
import { onMounted } from 'vue'
import { useFilesStore } from '@/stores/files'
import { api } from '@/services/api'
import type { ApiResponse, Folder } from '@/types'

const filesStore = useFilesStore()

onMounted(async () => {
  filesStore.setLoading(true)
  try {
    const res = await api.get<ApiResponse<Folder[]>>('/files/groups')
    filesStore.setFolders(res.data)
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
      <h2>Grupos</h2>
      <button class="create-btn">+ Nuevo grupo</button>
    </div>

    <div v-if="filesStore.isLoading" class="loading">Cargando grupos...</div>

    <div v-else>
      <div v-if="filesStore.folders.length" class="groups-grid">
        <div v-for="folder in filesStore.folders" :key="folder.id" class="group-card">
          <div class="group-icon">👥</div>
          <div class="group-info">
            <span class="group-name">{{ folder.name }}</span>
            <span class="group-date">Creado {{ new Date(folder.createdAt).toLocaleDateString() }}</span>
          </div>
        </div>
      </div>

      <div v-else class="empty-state">
        <p>No perteneces a ningun grupo todavia. Crea uno o espera una invitacion.</p>
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

.create-btn {
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

.create-btn:hover {
  opacity: 0.85;
}

.loading {
  color: var(--color-text);
}

.groups-grid {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.group-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 1.25rem;
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.15s;
}

.group-card:hover {
  background: var(--color-background-mute);
}

.group-icon {
  font-size: 1.75rem;
  flex-shrink: 0;
}

.group-info {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}

.group-name {
  font-weight: 600;
  font-size: 0.95rem;
  color: var(--color-heading);
}

.group-date {
  font-size: 0.8rem;
  color: var(--color-text);
}

.empty-state {
  text-align: center;
  padding: 3rem;
  color: var(--color-text);
  font-size: 0.95rem;
}
</style>
