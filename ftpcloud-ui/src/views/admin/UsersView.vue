<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useUsersStore } from '@/stores/users'
import { useAuthStore } from '@/stores/auth'
import { useDialogStore } from '@/stores/dialog'
import { api } from '@/services/api'
import CreateUserModal from '@/components/users/CreateUserModal.vue'
import EditQuotaModal from '@/components/users/EditQuotaModal.vue'
import type { ApiResponse, User } from '@/types'

const usersStore = useUsersStore()
const auth = useAuthStore()
const dialog = useDialogStore()

const showCreateModal = ref(false)
const editingQuotaUser = ref<User | null>(null)

onMounted(async () => {
  usersStore.setLoading(true)
  try {
    const res = await api.get<ApiResponse<User[]>>('/users')
    usersStore.setUsers(res.data)
  } catch (e) {
    console.error(e)
  } finally {
    usersStore.setLoading(false)
  }
})

function onCreated(user: User) {
  usersStore.addUser(user)
  showCreateModal.value = false
}

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  if (bytes < 1024 * 1024 * 1024) return `${(bytes / 1024 / 1024).toFixed(1)} MB`
  return `${(bytes / 1024 / 1024 / 1024).toFixed(2)} GB`
}

function onQuotaUpdated(user: User) {
  usersStore.updateUser(user.id, {
    storageQuotaBytes: user.storageQuotaBytes,
    storageUsedBytes: user.storageUsedBytes,
  })
  editingQuotaUser.value = null
}

async function handleDelete(user: User) {
  const confirmed = await dialog.confirm({
    title: 'Eliminar usuario',
    message: `¿Eliminar al usuario "${user.username}"? Esta accion no se puede deshacer.`,
    confirmText: 'Eliminar',
    danger: true,
  })
  if (!confirmed) return

  try {
    await api.delete(`/users/${user.id}`)
    usersStore.removeUser(user.id)
  } catch (e) {
    await dialog.alert({
      title: 'Error',
      message: e instanceof Error ? e.message : 'Error al eliminar el usuario',
    })
  }
}
</script>

<template>
  <div class="page-container">
    <div class="page-header">
      <div class="header-title">
        <i class="ph ph-users"></i>
        <h2>Gestión de Usuarios</h2>
      </div>
      <button class="btn btn-primary" @click="showCreateModal = true">
        <i class="ph ph-user-plus"></i>
        <span class="btn-text">Nuevo usuario</span>
      </button>
    </div>

    <div v-if="usersStore.isLoading" class="loading-state">
      <i class="ph ph-circle-notch spin loading-icon"></i>
      <p>Cargando usuarios...</p>
    </div>

    <div v-else class="content-area">
      <div class="table-container">
        <table class="premium-table">
          <thead>
            <tr>
              <th>Usuario</th>
              <th>Email</th>
              <th>Rol</th>
              <th>Uso de almacenamiento</th>
              <th>Fecha de creación</th>
              <th v-if="auth.isOwner" class="text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in usersStore.users" :key="user.id">
              <td>
                <div class="user-cell">
                  <div class="avatar-small">
                    {{ user.username.charAt(0).toUpperCase() }}
                  </div>
                  <span class="td-username">{{ user.username }}</span>
                </div>
              </td>
              <td class="td-email">{{ user.email }}</td>
              <td>
                <span class="role-badge" :class="user.role">{{ user.role }}</span>
              </td>
              <td class="td-usage">
                <template v-if="user.role !== 'owner'">
                  <div class="usage-progress">
                    <div class="usage-bar">
                      <div class="usage-fill" :style="{ width: Math.min(100, (user.storageUsedBytes / user.storageQuotaBytes) * 100) + '%' }"></div>
                    </div>
                    <span class="usage-text">{{ formatSize(user.storageUsedBytes) }} / {{ formatSize(user.storageQuotaBytes) }}</span>
                  </div>
                </template>
                <template v-else>
                  <span class="unlimited-badge">Ilimitado</span>
                </template>
              </td>
              <td class="td-date">{{ new Date(user.createdAt).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' }) }}</td>
              <td v-if="auth.isOwner" class="td-actions">
                <button
                  class="icon-action-btn"
                  :disabled="user.role === 'owner'"
                  @click="editingQuotaUser = user"
                  title="Editar cuota"
                >
                  <i class="ph ph-hard-drives"></i>
                </button>
                <button
                  class="icon-action-btn danger"
                  :disabled="user.role === 'owner'"
                  @click="handleDelete(user)"
                  title="Eliminar usuario"
                >
                  <i class="ph ph-trash"></i>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <CreateUserModal v-if="showCreateModal" @close="showCreateModal = false" @created="onCreated" />

    <EditQuotaModal
      v-if="editingQuotaUser"
      :user="editingQuotaUser"
      @close="editingQuotaUser = null"
      @updated="onQuotaUpdated"
    />
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

.header-title {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  color: var(--color-heading);
}

.header-title i {
  font-size: 1.75rem;
  color: var(--brand-primary);
}

.header-title h2 {
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0;
  letter-spacing: -0.02em;
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

.btn-primary:hover {
  background: var(--brand-primary-hover);
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

.table-container {
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  overflow-x: auto;
}

.premium-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.premium-table th {
  padding: 1rem 1.25rem;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted);
  background: var(--color-background-mute);
  border-bottom: 1px solid var(--color-border);
}

.premium-table td {
  padding: 1rem 1.25rem;
  border-bottom: 1px solid var(--color-border);
  color: var(--color-text);
  vertical-align: middle;
}

.premium-table tr:last-child td {
  border-bottom: none;
}

.premium-table tbody tr {
  transition: background var(--transition-fast);
}

.premium-table tbody tr:hover {
  background: var(--color-background-mute);
}

.user-cell {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.avatar-small {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--brand-primary), var(--brand-primary-hover));
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 0.85rem;
  box-shadow: var(--shadow-sm);
}

.td-username {
  font-weight: 600;
  color: var(--color-heading);
}

.td-email {
  color: var(--color-text-muted);
  font-size: 0.9rem;
}

.role-badge {
  font-size: 0.75rem;
  font-weight: 700;
  padding: 0.25rem 0.6rem;
  border-radius: var(--radius-full);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  display: inline-block;
}

.role-badge.owner {
  background: var(--color-danger-bg);
  color: var(--color-danger);
}

.role-badge.admin {
  background: var(--brand-primary-light);
  color: var(--brand-primary-hover);
}

.role-badge.user {
  background: var(--color-background-mute);
  color: var(--color-text-muted);
}

.usage-progress {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  width: 200px;
}

.usage-bar {
  flex: 1;
  height: 6px;
  background: var(--color-border);
  border-radius: 3px;
  overflow: hidden;
}

.usage-fill {
  height: 100%;
  background: var(--brand-primary);
  border-radius: 3px;
}

.usage-text {
  font-size: 0.8rem;
  color: var(--color-text-muted);
  white-space: nowrap;
}

.unlimited-badge {
  font-size: 0.8rem;
  font-weight: 500;
  color: var(--color-text-muted);
  background: var(--color-background-mute);
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
}

.td-date {
  font-size: 0.9rem;
  color: var(--color-text-muted);
}

.td-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.25rem;
}

.text-right {
  text-align: right;
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

.icon-action-btn:hover:not(:disabled) {
  background: var(--color-border);
  color: var(--color-heading);
}

.icon-action-btn.danger:hover:not(:disabled) {
  background: var(--color-danger-bg);
  color: var(--color-danger);
}

.icon-action-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}
</style>
