import { defineConfig } from 'vite';
import { resolve } from 'path';

export default defineConfig({
  build: {
    outDir: 'dist',
    assetsDir: 'assets',
    emptyOutDir: true,
    cssCodeSplit: false, // Bundle all CSS into one file
    rollupOptions: {
      input: resolve(__dirname, 'js/main.js'),
      output: {
        format: 'umd',
        name: 'Letmein',
        entryFileNames: 'js/letmein.js',
        chunkFileNames: 'js/[name].js',
        assetFileNames: (assetInfo) => {
          // CSS from JS imports
          if (assetInfo.name && assetInfo.name.endsWith('.css')) {
            return 'css/letmein.css';
          }
          // Other assets (images, fonts, etc.)
          return 'assets/[name].[ext]';
        },
        globals: {
          jquery: 'jQuery'
        }
      }
    },
    minify: 'terser',
    terserOptions: {
      compress: {
        drop_console: false,
        pure_funcs: [],
        keep_fnames: true
      },
      mangle: false, // Disable all mangling for debugging
      keep_classnames: true,
      keep_fnames: true
    },
    sourcemap: false
  },
  server: {
    port: 5173,
    strictPort: true
  }
});
