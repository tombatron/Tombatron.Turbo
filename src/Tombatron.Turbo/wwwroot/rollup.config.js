import resolve from '@rollup/plugin-node-resolve';
import terser from '@rollup/plugin-terser';

export default [
  // ESM build (modern browsers)
  {
    input: 'src/turbo-stream-source-signalr.js',
    output: {
      file: 'dist/turbo-signalr.esm.js',
      format: 'esm',
      sourcemap: true
    },
    plugins: [
      resolve()
    ]
  },
  // UMD build (legacy support)
  {
    input: 'src/turbo-stream-source-signalr.js',
    output: {
      file: 'dist/turbo-signalr.js',
      format: 'umd',
      name: 'TurboSignalR',
      sourcemap: true
    },
    plugins: [
      resolve()
    ]
  },
  // Minified UMD build
  {
    input: 'src/turbo-stream-source-signalr.js',
    output: {
      file: 'dist/turbo-signalr.min.js',
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
