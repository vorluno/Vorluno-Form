/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{ts,tsx}"],
  theme: {
    extend: {
      boxShadow: {
        soft: "0 10px 20px -10px rgba(0,0,0,0.25), 0 6px 12px -8px rgba(0,0,0,0.15)"
      }
    }
  },
  plugins: [],
}
