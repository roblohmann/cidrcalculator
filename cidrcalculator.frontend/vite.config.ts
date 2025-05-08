import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tsconfigPaths from 'vite-tsconfig-paths'
import mkcert from'vite-plugin-mkcert'

export default defineConfig({
  server: {
    port: 3000,
    https: true
  },
  plugins: [
    react(),
    mkcert(),
    tsconfigPaths()
  ]
})
