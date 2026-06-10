<script setup lang="ts">
import { onMounted } from 'vue'
import { useUsersStore } from '@/stores/users'
import { api } from '@/services/api'
import type { ApiResponse, User } from '@/types'

const usersStore = useUsersStore()

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
      <h2>Panel de administracion</h2>
      <p class="page-subtitle">Vista general de todos los usuarios del sistema</p>
    </div>

    <div v-if="usersStore.isLoading" class="loading">Cargando usuarios...</div>

    <div v-else class="users-grid">
      <div v-for="user in usersStore.users" :key="user.id" class="user-card">
        <div class="user-avatar">{{ user.username.charAt(0).toUpperCase() }}</div>
        <div class="user-info">
          <span class="user-name">{{ user.username }}</span>
          <span class="user-email">{{ user.email }}</span>
        </div>
        <span class="user-badge" :class="user.role">{{ user.role }}</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-header {
  margin-bottom: 2rem;
}

.page-header h2 {
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0 0 0.25rem;
  color: var(--color-heading);
}

.page-subtitle {
  color: var(--color-text);
  margin: 0;
  font-size: 0.9rem;
}

.loading {
  color: var(--color-text);
  font-size: 0.95rem;
}

.users-grid {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.user-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: 8px;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: var(--color-background-mute);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 1rem;
  color: var(--color-heading);
  flex-shrink: 0;
}

.user-info {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-width: 0;
}

.user-name {
  font-weight: 600;
  font-size: 0.95rem;
  color: var(--color-heading);
}

.user-email {
  font-size: 0.8rem;
  color: var(--color-text);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.user-badge {
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.2rem 0.6rem;
  border-radius: 99px;
  text-transform: uppercase;
  flex-shrink: 0;
}

.user-badge.owner {
  background: #fef3c7;
  color: #92400e;
}

.user-badge.admin {
  background: #dbeafe;
  color: #1e40af;
}

.user-badge.user {
  background: var(--color-background-mute);
  color: var(--color-text);
}
</style>
