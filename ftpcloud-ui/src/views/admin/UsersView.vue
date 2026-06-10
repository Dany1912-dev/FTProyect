<script setup lang="ts">
import { onMounted } from 'vue'
import { useUsersStore } from '@/stores/users'
import { useAuthStore } from '@/stores/auth'
import { api } from '@/services/api'
import type { ApiResponse, User } from '@/types'

const usersStore = useUsersStore()
const auth = useAuthStore()

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
</script>

<template>
  <div>
    <div class="page-header">
      <h2>Usuarios</h2>
      <button class="create-btn">+ Nuevo usuario</button>
    </div>

    <div v-if="usersStore.isLoading" class="loading">Cargando...</div>

    <table v-else class="users-table">
      <thead>
        <tr>
          <th>Usuario</th>
          <th>Email</th>
          <th>Rol</th>
          <th>Creado</th>
          <th v-if="auth.isOwner">Acciones</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="user in usersStore.users" :key="user.id">
          <td class="td-username">{{ user.username }}</td>
          <td>{{ user.email }}</td>
          <td>
            <span class="role-badge" :class="user.role">{{ user.role }}</span>
          </td>
          <td class="td-date">{{ new Date(user.createdAt).toLocaleDateString() }}</td>
          <td v-if="auth.isOwner" class="td-actions">
            <button class="action-btn danger">Eliminar</button>
          </td>
        </tr>
      </tbody>
    </table>
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

.users-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;
}

.users-table th {
  text-align: left;
  padding: 0.6rem 0.75rem;
  border-bottom: 2px solid var(--color-border);
  color: var(--color-heading);
  font-weight: 600;
}

.users-table td {
  padding: 0.75rem;
  border-bottom: 1px solid var(--color-border);
  color: var(--color-text);
}

.td-username {
  font-weight: 600;
  color: var(--color-heading) !important;
}

.td-date {
  font-size: 0.8rem;
}

.role-badge {
  font-size: 0.7rem;
  font-weight: 600;
  padding: 0.2rem 0.5rem;
  border-radius: 99px;
  text-transform: uppercase;
}

.role-badge.owner {
  background: #fef3c7;
  color: #92400e;
}

.role-badge.admin {
  background: #dbeafe;
  color: #1e40af;
}

.role-badge.user {
  background: var(--color-background-mute);
  color: var(--color-text);
}

.td-actions {
  display: flex;
  gap: 0.5rem;
}

.action-btn {
  padding: 0.3rem 0.6rem;
  border-radius: 4px;
  border: none;
  font-size: 0.8rem;
  cursor: pointer;
}

.action-btn.danger {
  background: #fee2e2;
  color: #991b1b;
}

.action-btn.danger:hover {
  background: #fecaca;
}
</style>
