import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { User } from '@/types'

export const useUsersStore = defineStore('users', () => {
  const users = ref<User[]>([])
  const isLoading = ref(false)

  function setUsers(data: User[]) {
    users.value = data
  }

  function addUser(user: User) {
    users.value.push(user)
  }

  function removeUser(id: string) {
    users.value = users.value.filter((u) => u.id !== id)
  }

  function updateUser(id: string, updates: Partial<User>) {
    const idx = users.value.findIndex((u) => u.id === id)
    if (idx !== -1) {
      users.value[idx] = { ...users.value[idx], ...updates } as User
    }
  }

  function setLoading(state: boolean) {
    isLoading.value = state
  }

  return {
    users,
    isLoading,
    setUsers,
    addUser,
    removeUser,
    updateUser,
    setLoading,
  }
})
