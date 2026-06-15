<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'
import { api } from '@/services/api'
import ChangePasswordModal from '@/components/common/ChangePasswordModal.vue'
import SearchBar from '@/components/files/SearchBar.vue'

const auth = useAuthStore()
const router = useRouter()

const showChangePassword = ref(false)

onMounted(() => {
  if (!auth.isOwner) auth.restoreSession()
})

const usagePercent = computed(() => {
  const user = auth.user
  if (!user || !user.storageQuotaBytes) return 0
  return Math.min(100, (user.storageUsedBytes / user.storageQuotaBytes) * 100)
})

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  if (bytes < 1024 * 1024 * 1024) return `${(bytes / 1024 / 1024).toFixed(1)} MB`
  return `${(bytes / 1024 / 1024 / 1024).toFixed(2)} GB`
}

function onSearch(query: string) {
  router.push({ name: 'search', query: { q: query } })
}

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
      <div class="sidebar-header">
        <div class="logo-container">
          <i class="ph ph-cloud logo-icon"></i>
          <span class="sidebar-logo">FTPCloud</span>
        </div>
      </div>

      <nav class="sidebar-nav">
        <SearchBar @search="onSearch" />

        <div class="nav-section">
          <span class="nav-label">Mi espacio</span>
          <RouterLink to="/files" class="nav-item">
            <i class="ph ph-folder"></i>
            <span>Mis archivos</span>
          </RouterLink>
          <RouterLink to="/shared" class="nav-item">
            <i class="ph ph-users"></i>
            <span>Compartidos</span>
          </RouterLink>
          <RouterLink to="/groups" class="nav-item">
            <i class="ph ph-users-three"></i>
            <span>Grupos</span>
          </RouterLink>
          <RouterLink to="/trash" class="nav-item">
            <i class="ph ph-trash"></i>
            <span>Papelera</span>
          </RouterLink>
        </div>

        <template v-if="auth.isAdmin || auth.isOwner">
          <div class="nav-divider" />
          <div class="nav-section">
            <span class="nav-label">Administración</span>
            <RouterLink v-if="auth.isAdmin" to="/users" class="nav-item">
              <i class="ph ph-user-gear"></i>
              <span>Usuarios</span>
            </RouterLink>
            <RouterLink v-if="auth.isOwner" to="/owner" class="nav-item">
              <i class="ph ph-shield-star"></i>
              <span>Panel Owner</span>
            </RouterLink>
          </div>
        </template>
      </nav>

      <div class="sidebar-footer">
        <div v-if="!auth.isOwner && auth.user" class="usage">
          <div class="usage-header">
            <span class="usage-title">Almacenamiento</span>
            <span class="usage-percent">{{ Math.round(usagePercent) }}%</span>
          </div>
          <div class="usage-bar">
            <div class="usage-fill" :style="{ width: `${usagePercent}%`, backgroundColor: usagePercent > 90 ? 'var(--color-danger)' : 'var(--brand-primary)' }" />
          </div>
          <span class="usage-text">
            {{ formatSize(auth.user.storageUsedBytes) }} de {{ formatSize(auth.user.storageQuotaBytes) }}
          </span>
        </div>
        
        <div class="user-profile">
          <div class="avatar">
            {{ auth.user?.username.charAt(0).toUpperCase() }}
          </div>
          <div class="user-info">
            <span class="sidebar-username">{{ auth.user?.username }}</span>
            <span class="sidebar-role">{{ auth.user?.role }}</span>
          </div>
        </div>
        
        <div class="footer-actions">
          <button class="footer-btn" @click="showChangePassword = true" title="Cambiar contraseña">
            <i class="ph ph-key"></i>
          </button>
          <button class="footer-btn danger" @click="handleLogout" title="Cerrar sesión">
            <i class="ph ph-sign-out"></i>
          </button>
        </div>
      </div>
    </aside>

    <main class="main-content">
      <div class="content-wrapper">
        <RouterView />
      </div>
    </main>

    <ChangePasswordModal v-if="showChangePassword" @close="showChangePassword = false" />
  </div>
</template>

<style scoped>
.dashboard-layout {
  display: flex;
  min-height: 100vh;
  background-color: var(--color-background-mute);
}

.sidebar {
  width: 260px;
  min-height: 100vh;
  background-color: var(--color-background-soft);
  border-right: 1px solid var(--color-border);
  display: flex;
  flex-direction: column;
  box-shadow: var(--shadow-sm);
  z-index: 10;
}

.sidebar-header {
  padding: 1.5rem;
  border-bottom: 1px solid transparent;
}

.logo-container {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.logo-icon {
  font-size: 1.75rem;
  color: var(--brand-primary);
}

.sidebar-logo {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--color-heading);
  letter-spacing: -0.02em;
}

.sidebar-nav {
  display: flex;
  flex-direction: column;
  padding: 1rem 0.75rem;
  flex: 1;
  overflow-y: auto;
}

.nav-section {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.nav-label {
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted);
  padding: 0.75rem 0.75rem 0.25rem;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.6rem 0.75rem;
  border-radius: var(--radius-md);
  font-size: 0.95rem;
  font-weight: 500;
  color: var(--color-text);
  text-decoration: none;
  transition: all var(--transition-fast);
}

.nav-item i {
  font-size: 1.25rem;
  color: var(--color-text-muted);
  transition: color var(--transition-fast);
}

.nav-item:hover {
  background-color: var(--color-background-mute);
  color: var(--color-heading);
}

.nav-item:hover i {
  color: var(--brand-primary);
}

.nav-item.router-link-active {
  background-color: var(--brand-primary-light);
  color: var(--brand-primary-hover);
  font-weight: 600;
}

.nav-item.router-link-active i {
  color: var(--brand-primary);
}

.nav-divider {
  height: 1px;
  background-color: var(--color-border);
  margin: 1rem 0.75rem;
}

.sidebar-footer {
  padding: 1.25rem;
  border-top: 1px solid var(--color-border);
  background-color: var(--color-background-soft);
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.usage {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
  background: var(--color-background-mute);
  padding: 0.75rem;
  border-radius: var(--radius-sm);
}

.usage-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.usage-title {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--color-heading);
}

.usage-percent {
  font-size: 0.75rem;
  color: var(--color-text-muted);
}

.usage-bar {
  height: 6px;
  border-radius: 3px;
  background: var(--color-border);
  overflow: hidden;
}

.usage-fill {
  height: 100%;
  transition: width 0.3s ease, background-color 0.3s ease;
}

.usage-text {
  font-size: 0.7rem;
  color: var(--color-text-muted);
  text-align: right;
}

.user-profile {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.avatar {
  width: 36px;
  height: 36px;
  border-radius: var(--radius-full);
  background: linear-gradient(135deg, var(--brand-primary), var(--brand-primary-hover));
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 1rem;
  flex-shrink: 0;
  box-shadow: var(--shadow-sm);
}

.user-info {
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.sidebar-username {
  color: var(--color-heading);
  font-weight: 600;
  font-size: 0.9rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.sidebar-role {
  color: var(--color-text-muted);
  font-size: 0.75rem;
  text-transform: capitalize;
}

.footer-actions {
  display: flex;
  gap: 0.5rem;
}

.footer-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-background-mute);
  border: 1px solid transparent;
  cursor: pointer;
  color: var(--color-text);
  font-size: 1.1rem;
  padding: 0.5rem;
  border-radius: var(--radius-md);
  transition: all var(--transition-fast);
}

.footer-btn:hover {
  background-color: var(--color-border);
  color: var(--color-heading);
}

.footer-btn.danger:hover {
  background-color: var(--color-danger-bg);
  color: var(--color-danger);
  border-color: var(--color-danger-bg);
}

.main-content {
  flex: 1;
  overflow-y: auto;
  position: relative;
}

.content-wrapper {
  padding: 2rem 3rem;
  max-width: 1400px;
  margin: 0 auto;
}
</style>
