import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

import AuthLayout from '@/layouts/AuthLayout.vue'
import DashboardLayout from '@/layouts/DashboardLayout.vue'

import LoginView from '@/views/auth/LoginView.vue'
import DashboardView from '@/views/owner/DashboardView.vue'
import UsersView from '@/views/admin/UsersView.vue'
import MyFilesView from '@/views/files/MyFilesView.vue'
import SharedView from '@/views/files/SharedView.vue'
import GroupsView from '@/views/files/GroupsView.vue'
import TrashView from '@/views/files/TrashView.vue'
import SearchResultsView from '@/views/files/SearchResultsView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    // Rutas publicas (sin autenticacion)
    {
      path: '/login',
      component: AuthLayout,
      children: [
        { path: '', name: 'login', component: LoginView },
      ],
    },

    // Rutas protegidas (requieren autenticacion)
    {
      path: '/',
      component: DashboardLayout,
      meta: { requiresAuth: true },
      children: [
        // Redireccion por defecto
        { path: '', redirect: '/files' },

        // Todos los usuarios autenticados
        { path: 'files', name: 'my-files', component: MyFilesView },
        { path: 'shared', name: 'shared', component: SharedView },
        { path: 'groups', name: 'groups', component: GroupsView },
        { path: 'trash', name: 'trash', component: TrashView },
        { path: 'search', name: 'search', component: SearchResultsView },

        // Solo admin y owner
        {
          path: 'users',
          name: 'users',
          component: UsersView,
          meta: { requiresAdmin: true },
        },

        // Solo owner
        {
          path: 'owner',
          name: 'owner-dashboard',
          component: DashboardView,
          meta: { requiresOwner: true },
        },
      ],
    },

    // Catch-all
    { path: '/:pathMatch(.*)*', redirect: '/login' },
  ],
})

// Guard de navegacion
router.beforeEach((to) => {
  const auth = useAuthStore()

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'login' }
  }

  if (to.meta.requiresOwner && !auth.isOwner) {
    return { name: 'my-files' }
  }

  if (to.meta.requiresAdmin && !auth.isAdmin) {
    return { name: 'my-files' }
  }

  if (to.name === 'login' && auth.isAuthenticated) {
    return auth.isOwner ? { name: 'owner-dashboard' } : { name: 'my-files' }
  }
})

export default router
