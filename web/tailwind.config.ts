import type { Config } from 'tailwindcss'

export default {
  content: [
    './app/**/*.{ts,tsx}',
    './components/**/*.{ts,tsx}',
    './lib/**/*.{ts,tsx}',
  ],
  theme: {
    extend: {
      colors: {
        brand: {
          50: '#eef3ff',
          100: '#dbe6ff',
          200: '#b8cdff',
          300: '#93b2ff',
          400: '#6d96ff',
          500: '#4a7cff',
          600: '#345fe0',
          700: '#2648b3',
          800: '#1d388a',
          900: '#182e6e'
        }
      },
      borderRadius: {
        xl2: '1.25rem'
      }
    },
  },
  plugins: [],
} satisfies Config
