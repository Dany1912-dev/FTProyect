<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useUsersStore } from '@/stores/users'
import { useDialogStore } from '@/stores/dialog'
import { api } from '@/services/api'
import type { ApiResponse, User } from '@/types'

import EditQuotaModal from '@/components/users/EditQuotaModal.vue'
import ChangeUserRoleModal from '@/components/users/ChangeUserRoleModal.vue'
import ChangeUserPasswordModal from '@/components/users/ChangeUserPasswordModal.vue'
import ConfirmDialog from '@/components/common/ConfirmDialog.vue'

const usersStore = useUsersStore()
const dialog = useDialogStore()

const activeMenuId = ref<string | null>(null)
const activeModal = ref<'quota' | 'role' | 'password' | null>(null)
const selectedUser = ref<User | null>(null)

// Handle click outside to close menus
const handleWindowClick = (e: MouseEvent) => {
  const target = e.target as HTMLElement
  if (!target.closest('.user-actions')) {
    activeMenuId.value = null
  }
}

onMounted(async () => {
  window.addEventListener('click', handleWindowClick)
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

onUnmounted(() => {
  window.removeEventListener('click', handleWindowClick)
})

function toggleMenu(userId: string) {
  activeMenuId.value = activeMenuId.value === userId ? null : userId
}

function openModal(modalName: 'quota' | 'role' | 'password', user: User) {
  selectedUser.value = user
  activeModal.value = modalName
  activeMenuId.value = null
}

function handleUserUpdated(updatedUser: User) {
  const index = usersStore.users.findIndex((u) => u.id === updatedUser.id)
  if (index !== -1) {
    usersStore.users[index] = updatedUser
  }
  activeModal.value = null
}

async function handleDelete(user: User) {
  activeMenuId.value = null
  const confirmed = await dialog.confirm({
    title: 'Eliminar usuario',
    message: `¿Estás seguro de que deseas eliminar al usuario "${user.username}"? Esta acción eliminará su cuenta y todos sus archivos de forma permanente.`,
    confirmText: 'Eliminar',
    danger: true,
  })

  if (confirmed) {
    try {
      await api.delete(`/users/${user.id}`)
      usersStore.setUsers(usersStore.users.filter((u) => u.id !== user.id))
    } catch (e) {
      console.error(e)
      dialog.alert({
        title: 'Error',
        message: 'No se pudo eliminar el usuario',
        danger: true,
      })
    }
  }
}
</script>

<template>
  <div class="page-container">
    <div class="page-header">
      <div class="header-content">
        <div class="header-title">
          <i class="ph ph-shield-star"></i>
          <h2>Panel del Owner</h2>
        </div>
        <p class="page-subtitle">Gestión completa de todos los usuarios del sistema</p>
      </div>
    </div>

    <div v-if="usersStore.isLoading" class="loading-state">
      <i class="ph ph-circle-notch spin loading-icon"></i>
      <p>Cargando información del sistema...</p>
    </div>

    <div v-else class="content-area">
      <div class="users-grid">
        <div v-for="user in usersStore.users" :key="user.id" class="user-card">
          <div class="user-avatar" :class="user.role">{{ user.username.charAt(0).toUpperCase() }}</div>
          
          <div class="user-info">
            <span class="user-name">{{ user.username }}</span>
            <span class="user-email">{{ user.email }}</span>
          </div>
          
          <span class="user-badge" :class="user.role">{{ user.role }}</span>
          
          <div class="user-actions">
            <button class="action-btn" @click.stop="toggleMenu(user.id)" title="Opciones">
              <i class="ph ph-dots-three-vertical"></i>
            </button>
            
            <div v-if="activeMenuId === user.id" class="dropdown-menu">
              <button class="dropdown-item" @click="openModal('quota', user)">
                <i class="ph ph-hard-drives"></i>
                Editar cuota
              </button>
              <button class="dropdown-item" @click="openModal('role', user)">
                <i class="ph ph-shield-check"></i>
                Cambiar rol
              </button>
              <button class="dropdown-item" @click="openModal('password', user)">
                <i class="ph ph-password"></i>
                Cambiar contraseña
              </button>
              <div class="dropdown-divider"></div>
              <button class="dropdown-item danger" @click="handleDelete(user)">
                <i class="ph ph-trash"></i>
                Eliminar usuario
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modals -->
    <EditQuotaModal
      v-if="activeModal === 'quota' && selectedUser"
      :user="selectedUser"
      @close="activeModal = null"
      @updated="handleUserUpdated"
    />
    
    <ChangeUserRoleModal
      v-if="activeModal === 'role' && selectedUser"
      :user="selectedUser"
      @close="activeModal = null"
      @updated="handleUserUpdated"
    />
    
    <ChangeUserPasswordModal
      v-if="activeModal === 'password' && selectedUser"
      :user="selectedUser"
      @close="activeModal = null"
      @updated="handleUserUpdated"
    />

    <ConfirmDialog />
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
  padding: 1.5rem;
  border-radius: var(--radius-lg);
  border: 1px solid var(--color-border);
  box-shadow: var(--shadow-sm);
}

.header-content {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.header-title {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  color: var(--color-heading);
}

.header-title i {
  font-size: 2rem;
  color: var(--brand-primary);
}

.header-title h2 {
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0;
  letter-spacing: -0.02em;
}

.page-subtitle {
  color: var(--color-text-muted);
  margin: 0;
  font-size: 0.95rem;
  padding-left: 2.75rem;
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

.users-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
  gap: 1.25rem;
}

.user-card {
  position: relative;
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1.25rem;
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-sm);
  transition: all var(--transition-normal);
}

.user-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
  border-color: var(--color-border-hover);
}

.user-avatar {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 1.25rem;
  flex-shrink: 0;
  color: white;
  box-shadow: var(--shadow-sm);
}

.user-avatar.owner {
  background: linear-gradient(135deg, var(--color-danger), #991b1b);
}

.user-avatar.admin {
  background: linear-gradient(135deg, var(--brand-primary), var(--brand-primary-hover));
}

.user-avatar.user {
  background: linear-gradient(135deg, #6b7280, #4b5563);
}

.user-info {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-width: 0;
}

.user-name {
  font-weight: 600;
  font-size: 1rem;
  color: var(--color-heading);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.user-email {
  font-size: 0.85rem;
  color: var(--color-text-muted);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.user-badge {
  font-size: 0.75rem;
  font-weight: 700;
  padding: 0.25rem 0.6rem;
  border-radius: var(--radius-full);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  flex-shrink: 0;
}

.user-badge.owner {
  background: var(--color-danger-bg);
  color: var(--color-danger);
}

.user-badge.admin {
  background: var(--brand-primary-light);
  color: var(--brand-primary-hover);
}

.user-badge.user {
  background: var(--color-background-mute);
  color: var(--color-text-muted);
}

.user-actions {
  position: relative;
  display: flex;
  align-items: center;
}

.action-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: var(--radius-sm);
  border: 1px solid transparent;
  background: transparent;
  color: var(--color-text-muted);
  font-size: 1.5rem;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.action-btn:hover {
  background: var(--color-background-mute);
  color: var(--color-heading);
  border-color: var(--color-border);
}

.dropdown-menu {
  position: absolute;
  top: 100%;
  right: 0;
  margin-top: 0.5rem;
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-lg);
  min-width: 200px;
  z-index: 50;
  display: flex;
  flex-direction: column;
  padding: 0.5rem 0;
  animation: scaleIn 0.2s cubic-bezier(0.16, 1, 0.3, 1);
  transform-origin: top right;
}

@keyframes scaleIn {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}

.dropdown-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  width: 100%;
  padding: 0.6rem 1rem;
  border: none;
  background: transparent;
  color: var(--color-text);
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  text-align: left;
  transition: all var(--transition-fast);
}

.dropdown-item i {
  font-size: 1.1rem;
  color: var(--color-text-muted);
}

.dropdown-item:hover {
  background: var(--color-background-mute);
  color: var(--color-heading);
}

.dropdown-item:hover i {
  color: var(--color-heading);
}

.dropdown-item.danger {
  color: var(--color-danger);
}

.dropdown-item.danger i {
  color: var(--color-danger);
}

.dropdown-item.danger:hover {
  background: var(--color-danger-bg);
}

.dropdown-divider {
  height: 1px;
  background: var(--color-border);
  margin: 0.25rem 0;
}
</style>
