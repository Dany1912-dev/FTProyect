import { ref, computed } from 'vue'
import { api } from '@/services/api'
import type { ApiResponse, UserSummary } from '@/types'

export function useUserSearch(excludeIds: () => string[]) {
  const searchQuery = ref('')
  const searchResults = ref<UserSummary[]>([])
  const isSearching = ref(false)
  const showDropdown = ref(false)
  let searchTimeout: ReturnType<typeof setTimeout> | undefined

  const filteredResults = computed(() =>
    searchResults.value.filter((u) => !excludeIds().includes(u.id)),
  )

  function onSearchInput() {
    showDropdown.value = true
    if (searchTimeout) clearTimeout(searchTimeout)
    searchTimeout = setTimeout(search, 250)
  }

  function onSearchBlur() {
    setTimeout(() => { showDropdown.value = false }, 150)
  }

  async function search() {
    isSearching.value = true
    try {
      const res = await api.get<ApiResponse<UserSummary[]>>(
        `/files/shareable-users?q=${encodeURIComponent(searchQuery.value)}`,
      )
      searchResults.value = res.data
    } catch {
      searchResults.value = []
    } finally {
      isSearching.value = false
    }
  }

  function reset() {
    searchQuery.value = ''
    searchResults.value = []
    showDropdown.value = false
  }

  return { searchQuery, filteredResults, isSearching, showDropdown, onSearchInput, onSearchBlur, reset }
}
