import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { FileItem, Folder } from '@/types'

export const useFilesStore = defineStore('files', () => {
  const currentFolder = ref<Folder | null>(null)
  const folders = ref<Folder[]>([])
  const files = ref<FileItem[]>([])
  const isLoading = ref(false)

  function setCurrentFolder(folder: Folder | null) {
    currentFolder.value = folder
  }

  function setFolders(data: Folder[]) {
    folders.value = data
  }

  function setFiles(data: FileItem[]) {
    files.value = data
  }

  function setLoading(state: boolean) {
    isLoading.value = state
  }

  function reset() {
    currentFolder.value = null
    folders.value = []
    files.value = []
  }

  return {
    currentFolder,
    folders,
    files,
    isLoading,
    setCurrentFolder,
    setFolders,
    setFiles,
    setLoading,
    reset,
  }
})
