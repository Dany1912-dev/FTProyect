<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useDialogStore } from '@/stores/dialog'
import { api } from '@/services/api'
import type { ApiResponse, Folder, FolderMemberInfo } from '@/types'

const props = defineProps<{ folder: Folder }>()

const emit = defineEmits<{
  close: []
  left: []
}>()

const auth = useAuthStore()
const dialog = useDialogStore()

const members = ref<FolderMemberInfo[]>([])
const isLoading = ref(true)
const error = ref('')

const newUsername = ref('')
const newRole = ref<'editor' | 'viewer'>('editor')
const isAdding = ref(false)

const currentUserId = computed(() => auth.user!.id)
const isOwner = computed(() => props.folder.ownerId === currentUserId.value)

onMounted(load)

async function load() {
  isLoading.value = true
  try {
    const res = await api.get<ApiResponse<FolderMemberInfo[]>>(`/files/folders/${props.folder.id}/members`)
    members.value = res.data
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'No se pudieron cargar los miembros'
  } finally {
    isLoading.value = false
  }
}

async function handleAdd() {
  error.value = ''
  isAdding.value = true
  try {
    const res = await api.post<ApiResponse<FolderMemberInfo>>(`/files/folders/${props.folder.id}/members`, {
      username: newUsername.value,
      role: newRole.value,
    })
    members.value.push(res.data)
    newUsername.value = ''
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'No se pudo agregar el usuario'
  } finally {
    isAdding.value = false
  }
}

async function handleRemove(member: FolderMemberInfo) {
  const isSelf = member.userId === currentUserId.value
  const confirmed = await dialog.confirm({
    title: isSelf ? 'Salir' : 'Quitar acceso',
    message: isSelf
      ? `¿Salir de "${props.folder.name}"? Dejarás de tener acceso a esta carpeta.`
      : `¿Quitar a "${member.username}" de "${props.folder.name}"?`,
    confirmText: isSelf ? 'Salir' : 'Quitar',
    danger: true,
  })
  if (!confirmed) return

  try {
    await api.delete(`/files/folders/${props.folder.id}/members/${member.userId}`)
    if (isSelf) {
      emit('left')
      emit('close')
      return
    }
    members.value = members.value.filter((m) => m.userId !== member.userId)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'No se pudo completar la acción'
  }
}
</script>

<template>
  <div class="overlay" @click.self="$emit('close')">
    <div class="modal">
      <div class="modal-header">
        <div class="modal-title-wrapper">
          <i class="ph ph-users title-icon"></i>
          <h3 class="modal-title">Miembros de "{{ folder.name }}"</h3>
        </div>
        <button class="close-btn" @click="$emit('close')">
          <i class="ph ph-x"></i>
        </button>
      </div>

      <div v-if="isLoading" class="loading-state">
        <i class="ph ph-circle-notch spin loading-icon"></i>
        <p>Cargando miembros...</p>
      </div>

      <template v-else>
        <div class="members-list">
          <div class="member-row owner-row">
            <div class="member-avatar owner">
              {{ folder.ownerUsername.charAt(0).toUpperCase() }}
            </div>
            <div class="member-info">
              <span class="member-name">{{ folder.ownerUsername }}</span>
              <span class="member-role">Dueño</span>
            </div>
          </div>

          <div v-for="member in members" :key="member.userId" class="member-row">
            <div class="member-avatar">
              {{ member.username.charAt(0).toUpperCase() }}
            </div>
            <div class="member-info">
              <span class="member-name">{{ member.username }}</span>
              <span class="member-role">{{ member.role === 'editor' ? 'Editor' : 'Lector' }}</span>
            </div>
            <button
              v-if="isOwner || member.userId === currentUserId"
              class="icon-action-btn danger"
              :title="member.userId === currentUserId ? 'Salir' : 'Quitar acceso'"
              @click="handleRemove(member)"
            >
              <i :class="member.userId === currentUserId ? 'ph ph-sign-out' : 'ph ph-user-minus'"></i>
            </button>
          </div>

          <div v-if="!members.length" class="empty-state">
            <i class="ph ph-lock-key empty-icon"></i>
            <p>Solo tú tienes acceso a esta carpeta.</p>
          </div>
        </div>

        <form v-if="isOwner" class="add-form" @submit.prevent="handleAdd">
          <div class="form-group">
            <label for="member-username">Invitar usuario</label>
            <div class="add-row">
              <div class="input-wrapper">
                <i class="ph ph-user"></i>
                <input
                  id="member-username"
                  v-model="newUsername"
                  type="text"
                  placeholder="Nombre de usuario"
                  autocomplete="off"
                  required
                />
              </div>
              <div class="select-wrapper">
                <select v-model="newRole">
                  <option value="editor">Editor</option>
                  <option value="viewer">Lector</option>
                </select>
                <i class="ph ph-caret-down"></i>
              </div>
              <button type="submit" class="btn btn-primary btn-add" :disabled="isAdding || !newUsername.trim()">
                <i :class="isAdding ? 'ph ph-spinner-gap spin' : 'ph ph-paper-plane-right'"></i>
              </button>
            </div>
          </div>
        </form>
      </template>

      <div v-if="error" class="error-msg">
        <i class="ph ph-warning-circle"></i>
        <span>{{ error }}</span>
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
  max-width: 460px;
  max-height: 85vh;
  display: flex;
  flex-direction: column;
  box-shadow: var(--shadow-lg);
  animation: slideUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px) scale(0.95); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

.modal-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  margin-bottom: 1.25rem;
}

.modal-title-wrapper {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex: 1;
  min-width: 0;
}

.title-icon {
  font-size: 1.5rem;
  color: var(--brand-primary);
  flex-shrink: 0;
}

.modal-title {
  font-size: 1.25rem;
  font-weight: 700;
  margin: 0;
  color: var(--color-heading);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
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
  flex-shrink: 0;
}

.close-btn:hover {
  background: var(--color-background-mute);
  color: var(--color-heading);
}

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 0;
  color: var(--color-text-muted);
  gap: 1rem;
}

.loading-icon {
  font-size: 2rem;
  color: var(--brand-primary);
}

.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.members-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  overflow-y: auto;
  max-height: 40vh;
  margin-bottom: 1.25rem;
  padding-right: 0.25rem;
}

/* Custom Scrollbar */
.members-list::-webkit-scrollbar {
  width: 6px;
}
.members-list::-webkit-scrollbar-track {
  background: transparent;
}
.members-list::-webkit-scrollbar-thumb {
  background-color: var(--color-border-hover);
  border-radius: 10px;
}

.member-row {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.75rem;
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  transition: all var(--transition-fast);
}

.member-row:hover {
  background: var(--color-background-mute);
  border-color: var(--color-border-hover);
}

.member-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #6b7280, #4b5563);
  color: white;
  font-weight: 700;
  font-size: 1rem;
  flex-shrink: 0;
}

.member-avatar.owner {
  background: linear-gradient(135deg, var(--brand-primary), var(--brand-primary-hover));
}

.member-info {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-width: 0;
}

.member-name {
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--color-heading);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.member-role {
  font-size: 0.8rem;
  color: var(--color-text-muted);
}

.owner-row .member-role {
  color: var(--brand-primary);
  font-weight: 600;
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

.icon-action-btn.danger:hover {
  background: var(--color-danger-bg);
  color: var(--color-danger);
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 2rem 1rem;
  text-align: center;
  color: var(--color-text-muted);
  background: var(--color-background-soft);
  border: 1px dashed var(--color-border);
  border-radius: var(--radius-md);
  gap: 0.5rem;
}

.empty-icon {
  font-size: 2rem;
  color: var(--color-border-hover);
}

.empty-state p {
  margin: 0;
  font-size: 0.9rem;
}

.add-form {
  margin-top: auto;
  padding-top: 1.25rem;
  border-top: 1px solid var(--color-border);
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-group label {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--color-heading);
}

.add-row {
  display: flex;
  gap: 0.5rem;
}

.input-wrapper {
  position: relative;
  flex: 1;
}

.input-wrapper i {
  position: absolute;
  left: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
  color: var(--color-text-muted);
}

.input-wrapper input {
  width: 100%;
  padding: 0.7rem 0.75rem 0.7rem 2.25rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-background-soft);
  color: var(--color-text);
  font-size: 0.95rem;
  outline: none;
  transition: all var(--transition-fast);
}

.input-wrapper input:focus {
  border-color: var(--brand-primary);
  background: var(--color-background);
  box-shadow: 0 0 0 3px var(--brand-primary-light);
}

.select-wrapper {
  position: relative;
  width: 110px;
}

.select-wrapper select {
  width: 100%;
  padding: 0.7rem 2rem 0.7rem 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-background-soft);
  color: var(--color-text);
  font-size: 0.95rem;
  outline: none;
  transition: all var(--transition-fast);
  appearance: none;
  cursor: pointer;
}

.select-wrapper select:focus {
  border-color: var(--brand-primary);
  background: var(--color-background);
  box-shadow: 0 0 0 3px var(--brand-primary-light);
}

.select-wrapper i {
  position: absolute;
  right: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
  color: var(--color-text-muted);
  pointer-events: none;
}

.btn-add {
  padding: 0 1rem;
}

.error-msg {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--color-danger);
  background: var(--color-danger-bg);
  padding: 0.75rem;
  border-radius: var(--radius-md);
  font-size: 0.875rem;
  margin-top: 1rem;
}

.error-msg i {
  font-size: 1.1rem;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 1.25rem;
}

.btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 0.6rem 1.25rem;
  border-radius: var(--radius-md);
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
  border: 1px solid transparent;
}

.btn-secondary {
  background: var(--color-background-mute);
  color: var(--color-text);
  border-color: var(--color-border);
}

.btn-secondary:hover {
  background: var(--color-border);
  color: var(--color-heading);
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

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}
</style>
