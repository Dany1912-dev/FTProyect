<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { BASE_URL } from '@/services/api'
import type { FileItem } from '@/types'

const props = defineProps<{
  file: Pick<FileItem, 'id' | 'name' | 'size' | 'mimeType'>
}>()

defineEmits<{ close: [] }>()

const previewUrl = `${BASE_URL}/files/${props.file.id}/preview`
const downloadUrl = `${BASE_URL}/files/${props.file.id}/download`

const textContent = ref('')
const textError = ref('')
const isLoadingText = ref(false)

const TEXT_PREFIXES = ['text/']
const TEXT_TYPES = ['application/json', 'application/xml', 'application/javascript', 'application/x-yaml']

const kind = computed<'image' | 'pdf' | 'video' | 'audio' | 'text' | 'unsupported'>(() => {
  const mime = props.file.mimeType ?? ''
  if (mime.startsWith('image/')) return 'image'
  if (mime === 'application/pdf') return 'pdf'
  if (mime.startsWith('video/')) return 'video'
  if (mime.startsWith('audio/')) return 'audio'
  if (TEXT_PREFIXES.some((p) => mime.startsWith(p)) || TEXT_TYPES.includes(mime)) return 'text'
  return 'unsupported'
})

onMounted(() => {
  if (kind.value === 'text') loadText()
})

const MAX_TEXT_BYTES = 256 * 1024

async function loadText() {
  if (props.file.size > MAX_TEXT_BYTES) {
    textError.value = 'El archivo es demasiado grande para previsualizar como texto.'
    return
  }

  isLoadingText.value = true
  try {
    const res = await fetch(previewUrl, { credentials: 'include' })
    if (!res.ok) throw new Error(`Error ${res.status}`)
    textContent.value = await res.text()
  } catch (e) {
    textError.value = e instanceof Error ? e.message : 'No se pudo cargar el archivo'
  } finally {
    isLoadingText.value = false
  }
}

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  if (bytes < 1024 * 1024 * 1024) return `${(bytes / 1024 / 1024).toFixed(1)} MB`
  return `${(bytes / 1024 / 1024 / 1024).toFixed(2)} GB`
}
</script>

<template>
  <div class="overlay" @click.self="$emit('close')">
    <div class="modal" :class="`kind-${kind}`">
      <div class="modal-header">
        <div class="modal-title-wrapper">
          <i class="ph ph-file title-icon"></i>
          <h3 class="modal-title" :title="file.name">{{ file.name }}</h3>
        </div>
        <div class="header-actions">
          <a class="header-btn" :href="downloadUrl" download title="Descargar">
            <i class="ph ph-download-simple"></i>
          </a>
          <button class="header-btn" @click="$emit('close')" title="Cerrar">
            <i class="ph ph-x"></i>
          </button>
        </div>
      </div>

      <div class="preview-body">
        <img v-if="kind === 'image'" :src="previewUrl" :alt="file.name" class="preview-image" />

        <iframe v-else-if="kind === 'pdf'" :src="previewUrl" class="preview-pdf" title="Vista previa PDF"></iframe>

        <video v-else-if="kind === 'video'" :src="previewUrl" controls class="preview-media"></video>

        <div v-else-if="kind === 'audio'" class="preview-audio-wrapper">
          <i class="ph ph-music-notes preview-audio-icon"></i>
          <audio :src="previewUrl" controls class="preview-audio"></audio>
        </div>

        <div v-else-if="kind === 'text'" class="preview-text-wrapper">
          <div v-if="isLoadingText" class="preview-loading">
            <i class="ph ph-circle-notch spin"></i>
            <span>Cargando...</span>
          </div>
          <p v-else-if="textError" class="preview-error">{{ textError }}</p>
          <pre v-else class="preview-text">{{ textContent }}</pre>
        </div>

        <div v-else class="preview-unsupported">
          <i class="ph ph-file-x unsupported-icon"></i>
          <p>No hay vista previa disponible para este archivo.</p>
          <span class="file-meta">{{ formatSize(file.size) }} · {{ file.mimeType }}</span>
          <a :href="downloadUrl" download class="btn btn-primary">
            <i class="ph ph-download-simple"></i>
            <span>Descargar</span>
          </a>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.55);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
  animation: fadeIn 0.2s ease-out;
  padding: 1.5rem;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

.modal {
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lg);
  width: 100%;
  max-width: 900px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  animation: slideUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  overflow: hidden;
}

.modal.kind-unsupported,
.modal.kind-audio {
  max-width: 480px;
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px) scale(0.97); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 1rem 1.25rem;
  border-bottom: 1px solid var(--color-border);
  flex-shrink: 0;
}

.modal-title-wrapper {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  min-width: 0;
}

.title-icon {
  font-size: 1.25rem;
  color: var(--brand-primary);
  flex-shrink: 0;
}

.modal-title {
  font-size: 1rem;
  font-weight: 700;
  margin: 0;
  color: var(--color-heading);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.header-actions {
  display: flex;
  gap: 0.25rem;
  flex-shrink: 0;
}

.header-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: var(--radius-sm);
  border: none;
  background: transparent;
  color: var(--color-text-muted);
  font-size: 1.25rem;
  cursor: pointer;
  text-decoration: none;
  transition: all var(--transition-fast);
}

.header-btn:hover {
  background: var(--color-background-mute);
  color: var(--color-heading);
}

.preview-body {
  flex: 1;
  overflow: auto;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 200px;
  background: var(--color-background-mute);
}

.preview-image {
  max-width: 100%;
  max-height: 80vh;
  object-fit: contain;
}

.preview-pdf {
  width: 100%;
  height: 80vh;
  border: none;
  background: white;
}

.preview-media {
  max-width: 100%;
  max-height: 80vh;
}

.preview-audio-wrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1.5rem;
  padding: 3rem 2rem;
  width: 100%;
}

.preview-audio-icon {
  font-size: 4rem;
  color: var(--brand-primary);
}

.preview-audio {
  width: 100%;
}

.preview-text-wrapper {
  width: 100%;
  height: 70vh;
  overflow: auto;
  background: var(--color-background);
}

.preview-text {
  margin: 0;
  padding: 1.25rem;
  font-family: 'Courier New', monospace;
  font-size: 0.85rem;
  color: var(--color-text);
  white-space: pre-wrap;
  word-break: break-word;
}

.preview-loading,
.preview-error {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  justify-content: center;
  padding: 3rem;
  color: var(--color-text-muted);
}

.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.preview-unsupported {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.75rem;
  padding: 3rem 2rem;
  text-align: center;
  color: var(--color-text-muted);
}

.unsupported-icon {
  font-size: 3rem;
  color: var(--color-text-muted);
}

.file-meta {
  font-size: 0.8rem;
}

.btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.6rem 1.1rem;
  border-radius: var(--radius-md);
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  border: none;
  text-decoration: none;
  margin-top: 0.5rem;
  transition: all var(--transition-fast);
}

.btn-primary {
  background: var(--brand-primary);
  color: white;
}

.btn-primary:hover {
  background: var(--brand-primary-hover);
}
</style>
