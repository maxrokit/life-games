import type { Cell } from '../types';

interface Pattern {
  name: string;
  cells: Cell[];
}

const PATTERNS: Pattern[] = [
  {
    name: 'Glider',
    cells: [
      { x: 1, y: 0 },
      { x: 2, y: 1 },
      { x: 0, y: 2 }, { x: 1, y: 2 }, { x: 2, y: 2 },
    ],
  },
  {
    name: 'Blinker',
    cells: [
      { x: 0, y: 0 }, { x: 1, y: 0 }, { x: 2, y: 0 },
    ],
  },
  {
    name: 'Block',
    cells: [
      { x: 0, y: 0 }, { x: 1, y: 0 },
      { x: 0, y: 1 }, { x: 1, y: 1 },
    ],
  },
  {
    name: 'Beehive',
    cells: [
      { x: 1, y: 0 }, { x: 2, y: 0 },
      { x: 0, y: 1 }, { x: 3, y: 1 },
      { x: 1, y: 2 }, { x: 2, y: 2 },
    ],
  },
  {
    name: 'Toad',
    cells: [
      { x: 1, y: 0 }, { x: 2, y: 0 }, { x: 3, y: 0 },
      { x: 0, y: 1 }, { x: 1, y: 1 }, { x: 2, y: 1 },
    ],
  },
  {
    name: 'Pulsar',
    cells: [
      // Top left quadrant pattern (mirrored)
      { x: 2, y: 0 }, { x: 3, y: 0 }, { x: 4, y: 0 },
      { x: 8, y: 0 }, { x: 9, y: 0 }, { x: 10, y: 0 },
      { x: 0, y: 2 }, { x: 5, y: 2 }, { x: 7, y: 2 }, { x: 12, y: 2 },
      { x: 0, y: 3 }, { x: 5, y: 3 }, { x: 7, y: 3 }, { x: 12, y: 3 },
      { x: 0, y: 4 }, { x: 5, y: 4 }, { x: 7, y: 4 }, { x: 12, y: 4 },
      { x: 2, y: 5 }, { x: 3, y: 5 }, { x: 4, y: 5 },
      { x: 8, y: 5 }, { x: 9, y: 5 }, { x: 10, y: 5 },
      { x: 2, y: 7 }, { x: 3, y: 7 }, { x: 4, y: 7 },
      { x: 8, y: 7 }, { x: 9, y: 7 }, { x: 10, y: 7 },
      { x: 0, y: 8 }, { x: 5, y: 8 }, { x: 7, y: 8 }, { x: 12, y: 8 },
      { x: 0, y: 9 }, { x: 5, y: 9 }, { x: 7, y: 9 }, { x: 12, y: 9 },
      { x: 0, y: 10 }, { x: 5, y: 10 }, { x: 7, y: 10 }, { x: 12, y: 10 },
      { x: 2, y: 12 }, { x: 3, y: 12 }, { x: 4, y: 12 },
      { x: 8, y: 12 }, { x: 9, y: 12 }, { x: 10, y: 12 },
    ],
  },
  {
    name: 'Gosper Gun',
    cells: [
      { x: 0, y: 4 }, { x: 0, y: 5 }, { x: 1, y: 4 }, { x: 1, y: 5 },
      { x: 10, y: 4 }, { x: 10, y: 5 }, { x: 10, y: 6 },
      { x: 11, y: 3 }, { x: 11, y: 7 },
      { x: 12, y: 2 }, { x: 12, y: 8 },
      { x: 13, y: 2 }, { x: 13, y: 8 },
      { x: 14, y: 5 },
      { x: 15, y: 3 }, { x: 15, y: 7 },
      { x: 16, y: 4 }, { x: 16, y: 5 }, { x: 16, y: 6 },
      { x: 17, y: 5 },
      { x: 20, y: 2 }, { x: 20, y: 3 }, { x: 20, y: 4 },
      { x: 21, y: 2 }, { x: 21, y: 3 }, { x: 21, y: 4 },
      { x: 22, y: 1 }, { x: 22, y: 5 },
      { x: 24, y: 0 }, { x: 24, y: 1 }, { x: 24, y: 5 }, { x: 24, y: 6 },
      { x: 34, y: 2 }, { x: 34, y: 3 }, { x: 35, y: 2 }, { x: 35, y: 3 },
    ],
  },
];

interface PatternsProps {
  onSelect: (cells: Cell[]) => void;
}

export function Patterns({ onSelect }: PatternsProps) {
  const handleSelect = (patternCells: Cell[]) => {
    // Patterns are already designed to be near (0, 0), just pass them through
    onSelect(patternCells);
  };

  return (
    <div className="flex flex-wrap gap-2">
      {PATTERNS.map((pattern) => (
        <button
          key={pattern.name}
          onClick={() => handleSelect(pattern.cells)}
          className="px-3 py-1 text-sm bg-gray-700 hover:bg-gray-600 rounded transition-colors"
        >
          {pattern.name}
        </button>
      ))}
    </div>
  );
}
