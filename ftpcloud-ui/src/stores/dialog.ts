import { defineStore } from 'pinia'
import { ref } from 'vue'

export interface DialogOptions {
  title?: string
  message: string
  confirmText?: string
  cancelText?: string
  danger?: boolean
}

export const useDialogStore = defineStore('dialog', () => {
  const isOpen = ref(false)
  const mode = ref<'confirm' | 'alert'>('alert')
  const options = ref<DialogOptions>({ message: '' })

  let resolver: ((value: boolean) => void) | null = null

  function open(newMode: 'confirm' | 'alert', opts: DialogOptions | string) {
    options.value = typeof opts === 'string' ? { message: opts } : opts
    mode.value = newMode
    isOpen.value = true
  }

  // Reemplazo de window.confirm: resuelve true/false segun el boton elegido
  function confirm(opts: DialogOptions | string): Promise<boolean> {
    open('confirm', opts)
    return new Promise((resolve) => {
      resolver = resolve
    })
  }

  // Reemplazo de window.alert: solo boton de aceptar
  function alert(opts: DialogOptions | string): Promise<void> {
    open('alert', opts)
    return new Promise((resolve) => {
      resolver = () => resolve()
    })
  }

  function resolve(value: boolean) {
    isOpen.value = false
    resolver?.(value)
    resolver = null
  }

  return { isOpen, mode, options, confirm, alert, resolve }
})
