import { useMemo } from 'react';
import type { Cell } from '../types';

interface BoardProps {
  cells: Cell[];
  cellSize?: number;
  boardWidth?: number;
  boardHeight?: number;
  onCellClick?: (x: number, y: number) => void;
}

export function Board({
  cells,
  cellSize = 12,
  boardWidth = 100,
  boardHeight = 100,
  onCellClick
}: BoardProps) {
  // Center the board at (0, 0)
  const minX = -Math.floor(boardWidth / 2);
  const maxX = Math.floor(boardWidth / 2);
  const minY = -Math.floor(boardHeight / 2);
  const maxY = Math.floor(boardHeight / 2);

  const width = boardWidth;
  const height = boardHeight;
  const axisLabelSize = 20; // Space for axis labels

  const handleClick = (e: React.MouseEvent<SVGElement>) => {
    if (!onCellClick) return;

    const svg = e.currentTarget.ownerSVGElement || e.currentTarget;
    const rect = svg.getBoundingClientRect();
    const x = Math.floor((e.clientX - rect.left - axisLabelSize) / cellSize) + minX;
    const y = Math.floor((e.clientY - rect.top) / cellSize) + minY;

    // Ensure click is within bounds
    if (x >= minX && x <= maxX && y >= minY && y <= maxY) {
      onCellClick(x, y);
    }
  };

  // Calculate which axis labels to show (show every 5th or 10th depending on size)
  const labelInterval = useMemo(() => {
    if (width > 50) return 10;
    return 5;
  }, [width]);
  const svgWidth = width * cellSize;
  const svgHeight = height * cellSize;

  return (
    <div className="overflow-auto border border-gray-700 rounded-lg bg-gray-900 max-h-[600px]">
      <svg
        width={svgWidth + axisLabelSize}
        height={svgHeight + axisLabelSize}
        className="cursor-pointer"
      >
        {/* Background */}
        <rect
          x={axisLabelSize}
          y={0}
          width={svgWidth}
          height={svgHeight}
          fill="#1f2937"
        />

        {/* Grid lines */}
        {Array.from({ length: width + 1 }, (_, i) => (
          <line
            key={`v${i}`}
            x1={axisLabelSize + i * cellSize}
            y1={0}
            x2={axisLabelSize + i * cellSize}
            y2={svgHeight}
            stroke="#374151"
            strokeWidth={0.5}
          />
        ))}
        {Array.from({ length: height + 1 }, (_, i) => (
          <line
            key={`h${i}`}
            x1={axisLabelSize}
            y1={i * cellSize}
            x2={axisLabelSize + svgWidth}
            y2={i * cellSize}
            stroke="#374151"
            strokeWidth={0.5}
          />
        ))}

        {/* Y-axis labels */}
        {Array.from({ length: Math.ceil(height / labelInterval) + 1 }, (_, i) => {
          const y = minY + i * labelInterval;
          if (y > maxY) return null;
          return (
            <text
              key={`ylabel${y}`}
              x={axisLabelSize - 4}
              y={(y - minY) * cellSize + cellSize / 2 + 4}
              fontSize={10}
              fill="#9ca3af"
              textAnchor="end"
            >
              {y}
            </text>
          );
        })}

        {/* X-axis labels */}
        {Array.from({ length: Math.ceil(width / labelInterval) + 1 }, (_, i) => {
          const x = minX + i * labelInterval;
          if (x > maxX) return null;
          return (
            <text
              key={`xlabel${x}`}
              x={axisLabelSize + (x - minX) * cellSize + cellSize / 2}
              y={svgHeight + 14}
              fontSize={10}
              fill="#9ca3af"
              textAnchor="middle"
            >
              {x}
            </text>
          );
        })}

        {/* Click handler overlay */}
        <rect
          x={axisLabelSize}
          y={0}
          width={svgWidth}
          height={svgHeight}
          fill="transparent"
          onClick={handleClick}
        />

        {/* Living cells */}
        {cells.map(cell => (
          <rect
            key={`${cell.x},${cell.y}`}
            x={axisLabelSize + (cell.x - minX) * cellSize + 1}
            y={(cell.y - minY) * cellSize + 1}
            width={cellSize - 2}
            height={cellSize - 2}
            fill="#22c55e"
            rx={2}
          />
        ))}
      </svg>
    </div>
  );
}
