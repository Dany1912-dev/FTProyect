import { ref } from 'vue'
import { startTusUpload } from '@/services/tus'

export interface UploadItem {
  id: string
  name: string
  progress: number
  status: 'uploading' | 'done' | 'error'
  error?: string
}

export function useFileUpload(getFolderId: () => string | undefined, onUploaded: () => void) {
  const queue = ref<UploadItem[]>([])

  function uploadFiles(files: FileList | File[]) {
    const folderId = getFolderId()
    if (!folderId) return

    for (const file of Array.from(files)) {
      const item: UploadItem = {
        id: crypto.randomUUID(),
        name: file.name,
        progress: 0,
        status: 'uploading',
      }
      queue.value.push(item)

      startTusUpload({
        file,
        folderId,
        onProgress: (percent) => {
          item.progress = percent
        },
        onSuccess: () => {
          item.status = 'done'
          item.progress = 100
          onUploaded()
          scheduleRemoval(item.id)
        },
        onError: (message) => {
          item.status = 'error'
          item.error = message
          scheduleRemoval(item.id)
        },
      })
    }
  }

  function scheduleRemoval(id: string) {
    setTimeout(() => {
      queue.value = queue.value.filter((i) => i.id !== id)
    }, 3000)
  }

  return { queue, uploadFiles }
}
