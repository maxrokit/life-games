# Life Games App - Frontend

React + TypeScript + Vite + Tailwind CSS frontend for Conway's Game of Life.

## Features

- **Local Mode**: Run the Game of Life simulation entirely in the browser
- **API Mode**: Connect to the backend API for server-side computation and persistence
- **Interactive Grid**: Click cells to toggle them on/off
- **Preset Patterns**: Quick-load famous patterns:
  - Glider (spaceship)
  - Blinker (period-2 oscillator)
  - Block (still life)
  - Beehive (still life)
  - Toad (period-2 oscillator)
  - Pulsar (period-3 oscillator)
  - Gosper Glider Gun
- **Controls**:
  - Step: Advance one generation
  - Run/Pause: Auto-advance at configurable speed
  - Reset: Return to initial state
  - Clear: Empty the board
  - Speed slider: Adjust auto-run speed (50ms - 1000ms)

## Setup

```bash
npm install
```

## Development

```bash
npm run dev
```

Opens at http://localhost:5173

## Build

```bash
npm run build
npm run preview  # Preview production build
```

## API Integration

1. Start the backend API (see `../life-games-api/README.md`)
2. Toggle "Use API" checkbox in the app
3. The app will proxy API requests to `http://localhost:5000`

For separate frontend/backend hosting, set `VITE_API_BASE_URL` to the API origin:

```bash
# Example
VITE_API_BASE_URL=https://api.example.com
```

## Tech Stack

- **React 18** - UI framework
- **TypeScript** - Type safety
- **Vite** - Fast build tool and dev server
- **Tailwind CSS v4** - Styling
- **@tailwindcss/vite** - Tailwind integration
