/** @type {import('tailwindcss').Config} */
import daisyui from "daisyui"

module.exports = {
    content: [
        './../**/*.{razor,html}',
        './../**/(Layout|Pages)/*.{razor,html}',
    ],
  theme: {
    extend: {},
  },
    plugins: [
        daisyui
    ],
}

