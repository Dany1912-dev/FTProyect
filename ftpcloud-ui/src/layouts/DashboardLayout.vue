<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'
import { api } from '@/services/api'
import ChangePasswordModal from '@/components/common/ChangePasswordModal.vue'

const auth = useAuthStore()
const router = useRouter()

const showChangePassword = ref(false)

async function handleLogout() {
  try {
    // El backend borra las cookies HttpOnly desde el servidor
    await api.post('/auth/logout')
  } finally {
    // Siempre limpiar estado local y redirigir, haya o no error
    auth.clearUser()
    router.push('/login')
  }
}
</script>

<template>
  <div class="dashboard-layout">
    <aside class="sidebar">
      <div class="sidebar-logo">FTPCloud</div>

      <nav class="sidebar-nav">
        <RouterLink to="/files" class="nav-item">Mis archivos</RouterLink>
        <RouterLink to="/shared" class="nav-item">Compartidos</RouterLink>
        <RouterLink to="/groups" class="nav-item">Grupos</RouterLink>

        <template v-if="auth.isAdmin">
          <div class="nav-divider" />
          <RouterLink to="/users" class="nav-item">Usuarios</RouterLink>
        </template>

        <template v-if="auth.isOwner">
          <RouterLink to="/owner" class="nav-item">Panel owner</RouterLink>
        </template>
      </nav>

      <div class="sidebar-footer">
        <span class="sidebar-username">{{ auth.user?.username }}</span>
        <div class="footer-actions">
          <button class="footer-btn" @click="showChangePassword = true">Contraseña</button>
          <button class="footer-btn" @click="handleLogout">Salir</button>
        </div>
      </div>
    </aside>

    <main class="main-content">
      <RouterView />
    </main>

    <ChangePasswordModal v-if="showChangePassword" @close="showChangePassword = false" />
  </div>
</template>

<style scoped>
.dashboard-layout {
  display: flex;
  min-height: 100vh;
}

.sidebar {
  width: 220px;
  min-height: 100vh;
  background-color: var(--color-background-soft);
  border-right: 1px solid var(--color-border);
  display: flex;
  flex-direction: column;
  padding: 1.5rem 1rem;
  gap: 1rem;
}

.sidebar-logo {
  font-size: 1.25rem;
  font-weight: 700;
  padding: 0.5rem 0;
  color: var(--color-heading);
}

.sidebar-nav {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  flex: 1;
}

.nav-item {
  padding: 0.6rem 0.75rem;
  border-radius: 6px;
  font-size: 0.9rem;
  color: var(--color-text);
  text-decoration: none;
  transition: background 0.15s;
}

.nav-item:hover {
  background-color: var(--color-background-mute);
}

.nav-item.router-link-active {
  background-color: var(--color-background-mute);
  font-weight: 600;
  color: var(--color-heading);
}

.nav-divider {
  height: 1px;
  background-color: var(--color-border);
  margin: 0.5rem 0;
}

.sidebar-footer {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding-top: 0.75rem;
  border-top: 1px solid var(--color-border);
  font-size: 0.85rem;
}

.sidebar-username {
  color: var(--color-text);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-weight: 600;
}

.footer-actions {
  display: flex;
  gap: 0.25rem;
}

.footer-btn {
  flex: 1;
  background: none;
  border: none;
  cursor: pointer;
  color: var(--color-text);
  font-size: 0.8rem;
  padding: 0.4rem 0.5rem;
  border-radius: 4px;
  text-align: center;
}

.footer-btn:hover {
  background-color: var(--color-background-mute);
}

.main-content {
  flex: 1;
  padding: 2rem;
  overflow-y: auto;
}
</style>
