import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  // Third arg '' also loads vars without a VITE_ prefix (API_PROXY_TARGET below is dev-server
  // only and must never end up in the client bundle, unlike VITE_* vars).
  const env = loadEnv(mode, process.cwd(), '')

  return {
    plugins: [react()],
    server: {
      proxy: {
        // DomainTracker.API has no CORS policy configured, and its dev HTTPS certificate isn't
        // trusted - proxying server-side avoids both: the browser only ever talks to the Vite
        // origin, so no CORS preflight happens, and `secure: false` lets this proxy accept the
        // API's self-signed cert without the browser needing to trust it.
        '/api': {
          target: env.API_PROXY_TARGET || 'https://localhost:7253',
          changeOrigin: true,
          secure: false,
        },
      },
    },
  }
})
