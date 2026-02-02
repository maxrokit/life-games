import { useState, useEffect, useCallback, useRef } from 'react';
import { Board } from './components/Board';
import { Controls } from './components/Controls';
import { Patterns } from './components/Patterns';
import { createBoard, fetchFromLink, getFinalState } from './api/boardsApi';
import type { Cell, Link, BoardResponse } from './types';

function App() {
  const [cells, setCells] = useState<Cell[]>([]);
  const [boardId, setBoardId] = useState<string | null>(null);
  const [links, setLinks] = useState<Record<string, Link>>({});
  const [generation, setGeneration] = useState(0);
  const [isRunning, setIsRunning] = useState(false);
  const [speed, setSpeed] = useState(200);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [useApi, setUseApi] = useState(true);
  const [finalStateMessage, setFinalStateMessage] = useState<string | null>(null);
  const [calculatingFinalState, setCalculatingFinalState] = useState(false);
  const [boardWidth, setBoardWidth] = useState(100);
  const [boardHeight, setBoardHeight] = useState(100);

  const initialCellsRef = useRef<Cell[]>([]);
  const intervalRef = useRef<number | null>(null);

  // Local Game of Life computation
  const computeNextGeneration = useCallback((currentCells: Cell[]): Cell[] => {
    const cellSet = new Set(currentCells.map(c => `${c.x},${c.y}`));
    const cellsToCheck = new Set<string>();

    currentCells.forEach(cell => {
      for (let dx = -1; dx <= 1; dx++) {
        for (let dy = -1; dy <= 1; dy++) {
          cellsToCheck.add(`${cell.x + dx},${cell.y + dy}`);
        }
      }
    });

    const nextCells: Cell[] = [];

    cellsToCheck.forEach(key => {
      const [x, y] = key.split(',').map(Number);
      let neighbors = 0;

      for (let dx = -1; dx <= 1; dx++) {
        for (let dy = -1; dy <= 1; dy++) {
          if (dx === 0 && dy === 0) continue;
          if (cellSet.has(`${x + dx},${y + dy}`)) neighbors++;
        }
      }

      const isAlive = cellSet.has(key);
      if ((isAlive && (neighbors === 2 || neighbors === 3)) || (!isAlive && neighbors === 3)) {
        nextCells.push({ x, y });
      }
    });

    return nextCells;
  }, []);

  const step = useCallback(async () => {
    if (cells.length === 0) return;

    if (useApi && links.next) {
      try {
        setLoading(true);
        const response = await fetchFromLink<BoardResponse>(links.next.href);
        setCells(response.cells);
        setGeneration(response.generationNumber);
        setLinks(response.links);
      } catch {
        setError('Failed to get next generation');
        setIsRunning(false);
      } finally {
        setLoading(false);
      }
    } else {
      setCells(prev => computeNextGeneration(prev));
      setGeneration(prev => prev + 1);
    }
  }, [cells.length, useApi, links.next, computeNextGeneration]);

  const run = useCallback(() => {
    setIsRunning(true);
  }, []);

  const pause = useCallback(() => {
    setIsRunning(false);
  }, []);

  const reset = useCallback(async () => {
    setIsRunning(false);
    if (useApi && links.self) {
      try {
        setLoading(true);
        // Generation 0 is 'self' link from the initial board response or the board itself
        const response = await fetchFromLink<BoardResponse>(links.self.href);
        setCells(response.cells);
        setGeneration(0);
        setLinks(response.links);
      } catch {
        setError('Failed to reset board');
      } finally {
        setLoading(false);
      }
    } else {
      setCells(initialCellsRef.current);
      setGeneration(0);
    }
  }, [useApi, links.self]);

  const clear = useCallback(() => {
    setIsRunning(false);
    setCells([]);
    setBoardId(null);
    setLinks({});
    setGeneration(0);
    initialCellsRef.current = [];
    setError(null);
    setFinalStateMessage(null);
  }, []);

  const showFinalState = useCallback(async () => {
    if (!useApi || !boardId || cells.length === 0) return;

    setIsRunning(false);
    setFinalStateMessage(null);
    setCalculatingFinalState(true);

    try {
      setLoading(true);
      const response = await getFinalState(boardId);

      setCells(response.cells);
      setGeneration(response.finalGenerationNumber);

      // Generate informative message
      let message = '';

      if (response.reachedMaxIterations) {
        message = `⚠️ Reached maximum iterations (${response.maxIterations}) without finding a stable state. Showing generation ${response.finalGenerationNumber}.`;
      } else if (response.isCyclic) {
        message = `✓ Found oscillating pattern with period ${response.cycleLength} starting at generation ${response.cycleStartGeneration}. Showing generation ${response.finalGenerationNumber}.`;
      } else {
        message = `✓ Found stable state (still life) at generation ${response.finalGenerationNumber}.`;
      }

      setFinalStateMessage(message);
    } catch {
      setError('Failed to compute final state');
    } finally {
      setLoading(false);
      setCalculatingFinalState(false);
    }
  }, [useApi, boardId, cells.length]);

  const handleCellClick = useCallback((x: number, y: number) => {
    if (isRunning) return;

    setCells(prev => {
      const exists = prev.some(c => c.x === x && c.y === y);
      if (exists) {
        return prev.filter(c => !(c.x === x && c.y === y));
      } else {
        return [...prev, { x, y }];
      }
    });
    setGeneration(0);
    setBoardId(null);
    setLinks({});
    setFinalStateMessage(null);
  }, [isRunning]);

  const handlePatternSelect = useCallback(async (patternCells: Cell[]) => {
    setIsRunning(false);
    setCells(patternCells);
    initialCellsRef.current = patternCells;
    setGeneration(0);
    setError(null);

    if (useApi) {
      try {
        setLoading(true);
        const response = await createBoard({ name: null, cells: patternCells });
        setBoardId(response.id);
        setLinks(response.links);
      } catch {
        setError('Failed to create board on server');
      } finally {
        setLoading(false);
      }
    } else {
      setBoardId(null);
      setLinks({});
    }
  }, [useApi]);

  // Auto-run effect
  useEffect(() => {
    if (isRunning && cells.length > 0) {
      intervalRef.current = window.setInterval(step, speed);
    }

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
    };
  }, [isRunning, speed, step, cells.length]);

  return (
    <div className="min-h-screen p-6">
      <div className="max-w-6xl mx-auto space-y-6">
        <header className="text-center">
          <h1 className="text-3xl font-bold text-white mb-2">Conway's Game of Life</h1>
          <p className="text-gray-400">Click cells to toggle, select a pattern, or draw your own</p>
        </header>

        {error && (
          <div className="p-4 bg-red-900/50 border border-red-700 rounded-lg text-red-200">
            {error}
            <button onClick={() => setError(null)} className="ml-4 underline">Dismiss</button>
          </div>
        )}

        {calculatingFinalState && (
          <div className="p-4 bg-blue-900/50 border border-blue-700 rounded-lg text-blue-200">
            <div className="flex items-center gap-3">
              <svg className="animate-spin h-5 w-5 text-blue-200" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              <span>Computing final state... This may take a moment for complex patterns.</span>
            </div>
          </div>
        )}

        {finalStateMessage && !calculatingFinalState && (
          <div className="p-4 bg-purple-900/50 border border-purple-700 rounded-lg text-purple-200">
            {finalStateMessage}
            <button onClick={() => setFinalStateMessage(null)} className="ml-4 underline">Dismiss</button>
          </div>
        )}

        <div className="flex flex-wrap items-center gap-6 p-4 bg-gray-800 rounded-lg">
          <label className="flex items-center gap-2 cursor-pointer">
            <input
              type="checkbox"
              checked={useApi}
              onChange={(e) => setUseApi(e.target.checked)}
              className="w-4 h-4"
            />
            <span className="text-sm text-gray-300">Use API</span>
          </label>

          <div className="flex items-center gap-4">
            <label className="flex items-center gap-2">
              <span className="text-sm text-gray-400">Width:</span>
              <input
                type="number"
                min={20}
                max={200}
                value={boardWidth}
                onChange={(e) => setBoardWidth(Number(e.target.value))}
                className="w-16 px-2 py-1 bg-gray-700 text-white rounded text-sm"
              />
            </label>
            <label className="flex items-center gap-2">
              <span className="text-sm text-gray-400">Height:</span>
              <input
                type="number"
                min={20}
                max={200}
                value={boardHeight}
                onChange={(e) => setBoardHeight(Number(e.target.value))}
                className="w-16 px-2 py-1 bg-gray-700 text-white rounded text-sm"
              />
            </label>
          </div>

          {loading && <span className="text-sm text-yellow-400">Loading...</span>}
        </div>

        <div className="space-y-2">
          <h2 className="text-sm font-medium text-gray-400">Patterns</h2>
          <Patterns onSelect={handlePatternSelect} />
        </div>

        <Controls
          isRunning={isRunning}
          generation={generation}
          cellCount={cells.length}
          onStep={step}
          onRun={run}
          onPause={pause}
          onReset={reset}
          onClear={clear}
          onShowFinalState={showFinalState}
          speed={speed}
          onSpeedChange={setSpeed}
          disabled={loading}
          useApi={useApi}
        />

        <div className="flex justify-center">
          <Board
            cells={cells}
            cellSize={12}
            boardWidth={boardWidth}
            boardHeight={boardHeight}
            onCellClick={handleCellClick}
          />
        </div>

        <footer className="text-center text-sm text-gray-500">
          <p>Cells: {cells.length} | Generation: {generation}</p>
          {boardId && <p className="text-xs mt-1">Board ID: {boardId}</p>}
        </footer>
      </div>
    </div>
  );
}

export default App;
