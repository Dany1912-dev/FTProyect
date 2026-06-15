import { fileURLToPath, URL } from 'node:url'
import fs from 'node:fs'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  server: {
    https: {
      cert: fs.readFileSync(fileURLToPath(new URL('./.cert/localhost.pem', import.meta.url))),
      key: fs.readFileSync(fileURLToPath(new URL('./.cert/localhost.key', import.meta.url))),
    },
    port: 5173,
  },
})
