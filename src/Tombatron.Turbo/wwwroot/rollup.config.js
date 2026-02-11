import resolve from '@rollup/plugin-node-resolve';
import terser from '@rollup/plugin-terser';

export default [
  // ===========================================
  // NPM BUILDS (SignalR as peer dependency)
  // ===========================================

  // ESM build for npm (modern bundlers)
  {
    input: 'src/turbo-stream-source-signalr.js',
    output: {
      file: 'dist/turbo-signalr.esm.js',
      format: 'esm',
      sourcemap: true
    },
    external: ['@microsoft/signalr'],
    plugins: []
  },

  // UMD build for npm (legacy/CDN with external SignalR)
  {
    input: 'src/turbo-stream-source-signalr.js',
    output: {
      file: 'dist/turbo-signalr.js',
      format: 'umd',
      name: 'TurboSignalR',
      sourcemap: true,
      globals: {
        '@microsoft/signalr': 'signalR'
      }
    },
    external: ['@microsoft/signalr'],
    plugins: []
  },

  // ===========================================
  // NUGET BUILDS (SignalR bundled, self-contained)
  // ===========================================

  // ESM build with SignalR bundled (for modern browsers via NuGet)
  {
    input: 'src/turbo-stream-source-signalr.js',
    output: {
      file: 'dist/turbo-signalr.bundled.esm.js',
      format: 'esm',
      sourcemap: true
    },
    plugins: [
      resolve()
    ]
  },

  // UMD build with SignalR bundled (for NuGet static assets)
  {
    input: 'src/turbo-stream-source-signalr.js',
    output: {
      file: 'dist/turbo-signalr.bundled.js',
      format: 'umd',
      name: 'TurboSignalR',
      sourcemap: true
    },
    plugins: [
      resolve()
    ]
  },

  // Minified UMD with SignalR bundled (primary for NuGet)
  {
    input: 'src/turbo-stream-source-signalr.js',
    output: {
      file: 'dist/turbo-signalr.bundled.min.js',
      format: 'umd',
      name: 'TurboSignalR',
      sourcemap: true
    },
    plugins: [
      resolve(),
      terser()
    ]
  }
];
