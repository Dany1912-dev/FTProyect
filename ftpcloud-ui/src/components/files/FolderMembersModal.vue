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
      <h3 class="modal-title">Miembros de "{{ folder.name }}"</h3>

      <div v-if="isLoading" class="loading">Cargando...</div>

      <template v-else>
        <div class="members-list">
          <div class="member-row">
            <span class="member-name">{{ folder.ownerUsername }}</span>
            <span class="member-role owner-badge">Dueño</span>
          </div>

          <div v-for="member in members" :key="member.userId" class="member-row">
            <span class="member-name">{{ member.username }}</span>
            <span class="member-role">{{ member.role === 'editor' ? 'Editor' : 'Lector' }}</span>
            <button
              v-if="isOwner || member.userId === currentUserId"
              class="member-remove"
              :class="{ 'leave-btn': member.userId === currentUserId }"
              :title="member.userId === currentUserId ? 'Salir' : 'Quitar acceso'"
              @click="handleRemove(member)"
            >
              {{ member.userId === currentUserId ? 'Salir' : '×' }}
            </button>
          </div>

          <p v-if="!members.length" class="empty-msg">Nadie más tiene acceso todavía.</p>
        </div>

        <form v-if="isOwner" class="modal-form add-form" @submit.prevent="handleAdd">
          <div class="form-group">
            <label for="member-username">Compartir con</label>
            <div class="add-row">
              <input
                id="member-username"
                v-model="newUsername"
                type="text"
                placeholder="usuario"
                autocomplete="off"
                required
              />
              <select v-model="newRole">
                <option value="editor">Editor</option>
                <option value="viewer">Lector</option>
              </select>
              <button type="submit" class="btn-submit" :disabled="isAdding">
                {{ isAdding ? '...' : 'Agregar' }}
              </button>
            </div>
          </div>
        </form>
      </template>

      <p v-if="error" class="error-msg">{{ error }}</p>

      <div class="modal-actions">
        <button type="button" class="btn-cancel" @click="$emit('close')">Cerrar</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}

.modal {
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 1.5rem;
  width: 100%;
  max-width: 420px;
}

.modal-title {
  font-size: 1.25rem;
  font-weight: 700;
  margin: 0 0 1.25rem;
  color: var(--color-heading);
  word-break: break-word;
}

.loading {
  color: var(--color-text);
  font-size: 0.9rem;
}

.members-list {
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
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: 6px;
}

.member-name {
  flex: 1;
  font-size: 0.9rem;
  color: var(--color-heading);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.member-role {
  font-size: 0.8rem;
  color: var(--color-text);
  flex-shrink: 0;
}

.owner-badge {
  font-weight: 600;
}

.member-remove {
  flex-shrink: 0;
  width: 1.6rem;
  height: 1.6rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  border-radius: 50%;
  background: var(--color-background);
  color: var(--color-text);
  font-size: 1rem;
  line-height: 1;
  cursor: pointer;
  transition:
    background 0.15s,
    color 0.15s;
}

.member-remove:hover {
  background: #fee2e2;
  color: #991b1b;
}

.member-remove.leave-btn {
  width: auto;
  height: auto;
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
  font-size: 0.8rem;
}

.empty-msg {
  text-align: center;
  padding: 0.75rem;
  color: var(--color-text);
  font-size: 0.875rem;
  margin: 0;
}

.modal-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.form-group label {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--color-heading);
}

.add-row {
  display: flex;
  gap: 0.5rem;
}

.add-row input {
  flex: 1;
  min-width: 0;
}

.form-group input,
.form-group select,
.add-row select {
  padding: 0.6rem 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-background);
  color: var(--color-text);
  font-size: 0.95rem;
  outline: none;
  transition: border-color 0.15s;
}

.form-group input:focus,
.add-row select:focus {
  border-color: var(--color-heading);
}

.error-msg {
  color: #e05252;
  font-size: 0.875rem;
  margin: 1rem 0 0;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: 1rem;
}

.btn-cancel,
.btn-submit {
  padding: 0.6rem 1.1rem;
  border: none;
  border-radius: 6px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.15s;
  white-space: nowrap;
}

.btn-cancel {
  background: var(--color-background-mute);
  color: var(--color-text);
}

.btn-submit {
  background: var(--color-heading);
  color: var(--color-background);
}

.btn-cancel:hover,
.btn-submit:hover:not(:disabled) {
  opacity: 0.85;
}

.btn-submit:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
