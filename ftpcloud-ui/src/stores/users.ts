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

  function setLoading(state: boolean) {
    isLoading.value = state
  }

  return {
    users,
    isLoading,
    setUsers,
    addUser,
    removeUser,
    setLoading,
  }
})
