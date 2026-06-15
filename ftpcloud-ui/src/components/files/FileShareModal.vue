<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useDialogStore } from '@/stores/dialog'
import { api } from '@/services/api'
import { useUserSearch } from '@/composables/useUserSearch'
import UserSearchDropdown from './UserSearchDropdown.vue'
import type { ApiResponse, FileItem, FileShareInfo, UserSummary } from '@/types'

const props = defineProps<{
  file: FileItem
  folderOwnerId: string
}>()

const emit = defineEmits<{ close: [] }>()

const auth = useAuthStore()
const dialog = useDialogStore()

const shares = ref<FileShareInfo[]>([])
const isLoading = ref(true)
const error = ref('')

const isAdding = ref(false)

const currentUserId = computed(() => auth.user!.id)
const isOwner = computed(() => props.folderOwnerId === currentUserId.value)

const {
  searchQuery, filteredResults, isSearching, showDropdown, onSearchInput, onSearchBlur, reset: resetSearch,
} = useUserSearch(() => shares.value.map((s) => s.userId))

onMounted(load)

async function load() {
  isLoading.value = true
  try {
    const res = await api.get<ApiResponse<FileShareInfo[]>>(`/files/${props.file.id}/shares`)
    shares.value = res.data
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'No se pudieron cargar los compartidos'
  } finally {
    isLoading.value = false
  }
}

async function handleAdd(user: UserSummary) {
  error.value = ''
  isAdding.value = true
  try {
    const res = await api.post<ApiResponse<FileShareInfo>>(`/files/${props.file.id}/shares`, {
      username: user.username,
      role: 'viewer',
    })
    shares.value.push(res.data)
    resetSearch()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'No se pudo compartir el archivo'
  } finally {
    isAdding.value = false
  }
}

async function handleRemove(share: FileShareInfo) {
  const confirmed = await dialog.confirm({
    title: 'Quitar acceso',
    message: share.userId === currentUserId.value
      ? '¿Dejar de tener acceso a este archivo?'
      : `¿Quitar el acceso a "${share.username}"?`,
    confirmText: share.userId === currentUserId.value ? 'Salir' : 'Quitar',
    danger: true,
  })
  if (!confirmed) return

  try {
    await api.delete(`/files/${props.file.id}/shares/${share.userId}`)
    shares.value = shares.value.filter((s) => s.userId !== share.userId)
  } catch (e) {
    await dialog.alert({
      title: 'Error',
      message: e instanceof Error ? e.message : 'No se pudo quitar el acceso',
    })
  }
}
</script>

<template>
  <div class="overlay" @click.self="$emit('close')">
    <div class="modal">
      <div class="modal-header">
        <div class="modal-title-wrapper">
          <i class="ph ph-share-network title-icon"></i>
          <h3 class="modal-title">Compartir archivo</h3>
        </div>
        <button class="close-btn" @click="$emit('close')">
          <i class="ph ph-x"></i>
        </button>
      </div>

      <div class="file-info">
        <i class="ph ph-file file-icon"></i>
        <span class="file-name">{{ file.name }}</span>
      </div>

      <div v-if="isLoading" class="loading">Cargando...</div>

      <div v-else class="members-section">
        <!-- Owner -->
        <div class="member-row owner">
          <div class="member-avatar">{{ props.folderOwnerId === currentUserId ? auth.user?.username.charAt(0).toUpperCase() : '?' }}</div>
          <span class="member-name">Dueño</span>
          <span class="role-badge owner">Owner</span>
        </div>

        <!-- Shares -->
        <div v-for="share in shares" :key="share.userId" class="member-row">
          <div class="member-avatar">{{ share.username.charAt(0).toUpperCase() }}</div>
          <span class="member-name">{{ share.username }}</span>
          <span class="role-badge viewer">Lector</span>
          <button
            v-if="isOwner || share.userId === currentUserId"
            class="action-btn danger"
            @click="handleRemove(share)"
          >
            <i :class="share.userId === currentUserId ? 'ph ph-sign-out' : 'ph ph-x'"></i>
          </button>
        </div>

        <div v-if="!shares.length && isOwner" class="empty-shares">
          Este archivo no está compartido con nadie todavía.
        </div>
      </div>

      <!-- Add form (owner only) -->
      <div v-if="isOwner" class="add-form">
        <UserSearchDropdown
          v-model="searchQuery"
          :results="filteredResults"
          :is-searching="isSearching"
          :show-dropdown="showDropdown"
          :disabled="isAdding"
          placeholder="Buscar usuario..."
          @input="onSearchInput"
          @focus="onSearchInput"
          @blur="onSearchBlur"
          @select="handleAdd"
        />
        <p v-if="error" class="error-msg">
          <i class="ph ph-warning-circle"></i>
          <span>{{ error }}</span>
        </p>
      </div>

      <div class="modal-actions">
        <button type="button" class="btn btn-secondary" @click="$emit('close')">Cerrar</button>
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
  max-width: 420px;
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
  margin-bottom: 1rem;
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

.file-info {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  margin-bottom: 1.25rem;
}

.file-icon {
  font-size: 1.25rem;
  color: var(--brand-primary);
}

.file-name {
  font-weight: 600;
  color: var(--color-heading);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.loading {
  text-align: center;
  color: var(--color-text-muted);
  padding: 1.5rem 0;
}

.members-section {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.member-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.5rem 0.75rem;
  background: var(--color-background-mute);
  border-radius: var(--radius-sm);
}

.member-avatar {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: linear-gradient(135deg, #6b7280, #4b5563);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.75rem;
  font-weight: 600;
  flex-shrink: 0;
}

.member-name {
  flex: 1;
  font-size: 0.9rem;
  font-weight: 500;
  color: var(--color-heading);
}

.role-badge {
  font-size: 0.65rem;
  font-weight: 600;
  padding: 0.15rem 0.5rem;
  border-radius: 99px;
  text-transform: uppercase;
}

.role-badge.owner {
  background: #fef3c7;
  color: #92400e;
}

.role-badge.viewer {
  background: var(--color-background);
  color: var(--color-text);
  border: 1px solid var(--color-border);
}

.action-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border-radius: var(--radius-sm);
  border: none;
  background: transparent;
  color: var(--color-text-muted);
  font-size: 0.9rem;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.action-btn.danger:hover {
  background: var(--color-danger-bg);
  color: var(--color-danger);
}

.empty-shares {
  text-align: center;
  padding: 1rem 0;
  font-size: 0.85rem;
  color: var(--color-text-muted);
}

.add-form {
  border-top: 1px solid var(--color-border);
  padding-top: 1rem;
}

.error-msg {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  color: var(--color-danger);
  font-size: 0.8rem;
  margin: 0.5rem 0 0;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: 1rem;
}

.btn {
  padding: 0.6rem 1.1rem;
  border: none;
  border-radius: var(--radius-sm);
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: opacity var(--transition-fast);
}

.btn-secondary {
  background: var(--color-background-mute);
  color: var(--color-text);
}

.btn-secondary:hover {
  opacity: 0.85;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.spin {
  animation: spin 1s linear infinite;
}
</style>
