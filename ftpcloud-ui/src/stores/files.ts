import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { FileItem, Folder } from '@/types'

export const useFilesStore = defineStore('files', () => {
  const currentFolder = ref<Folder | null>(null)
  const path = ref<Folder[]>([])
  const folders = ref<Folder[]>([])
  const files = ref<FileItem[]>([])
  const isLoading = ref(false)

  function setCurrentFolder(folder: Folder | null) {
    currentFolder.value = folder
  }

  function setPath(data: Folder[]) {
    path.value = data
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
    path.value = []
    folders.value = []
    files.value = []
  }

  return {
    currentFolder,
    path,
    folders,
    files,
    isLoading,
    setCurrentFolder,
    setPath,
    setFolders,
    setFiles,
    setLoading,
    reset,
  }
})
