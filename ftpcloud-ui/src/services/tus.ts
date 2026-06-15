import * as tus from 'tus-js-client'
import { BASE_URL } from './api'

interface StartTusUploadOptions {
  file: File
  folderId: string
  onProgress: (percent: number) => void
  onSuccess: () => void
  onError: (message: string) => void
}

export function startTusUpload(options: StartTusUploadOptions): tus.Upload {
  const upload = new tus.Upload(options.file, {
    endpoint: `${BASE_URL}/files/tus`,
    chunkSize: 8 * 1024 * 1024,
    retryDelays: [0, 1000, 3000, 5000],
    onBeforeRequest: (req) => {
      const xhr = req.getUnderlyingObject() as XMLHttpRequest
      xhr.withCredentials = true
    },
    metadata: {
      filename: options.file.name,
      filetype: options.file.type || 'application/octet-stream',
      folderId: options.folderId,
    },
    onError: (error) => {
      options.onError(error instanceof Error ? error.message : 'No se pudo subir el archivo')
    },
    onProgress: (bytesUploaded, bytesTotal) => {
      options.onProgress(Math.round((bytesUploaded / bytesTotal) * 100))
    },
    onSuccess: () => {
      options.onSuccess()
    },
  })
  upload.start()
  return upload
}
