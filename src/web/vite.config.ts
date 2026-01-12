// @ts-nocheck
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// ⚠️ Ajusta el puerto del backend si tu API corre en otro (mira launchSettings.json)
const BACKEND = 'https://localhost:7150';

// ⚠️ Cambia por tu IP LAN si fuese distinta
const LAN_IP = '192.168.0.2';
const PORT = 5174;

export default defineConfig({
  plugins: [react()],
  server: {
    host: true,          // equivale a --host (escucha en 0.0.0.0)
    port: PORT,
    strictPort: true,
    // Hot Module Reload en la IP LAN para móviles
    hmr: { host: LAN_IP, port: PORT },

    // Proxy para que /api funcione desde el teléfono sin CORS
    proxy: {
      '/api': {
        target: BACKEND,
        changeOrigin: true,
        secure: false,   // permite cert HTTPS de desarrollo
      },
    },
  },
  build: {
    outDir: 'dist',
    sourcemap: true,
  },
});
