interface ControlsProps {
  isRunning: boolean;
  generation: number;
  cellCount: number;
  onStep: () => void;
  onRun: () => void;
  onPause: () => void;
  onReset: () => void;
  onClear: () => void;
  onShowFinalState: () => void;
  speed: number;
  onSpeedChange: (speed: number) => void;
  disabled?: boolean;
  useApi?: boolean;
}

export function Controls({
  isRunning,
  generation,
  cellCount,
  onStep,
  onRun,
  onPause,
  onReset,
  onClear,
  onShowFinalState,
  speed,
  onSpeedChange,
  disabled = false,
  useApi = false,
}: ControlsProps) {
  return (
    <div className="flex flex-wrap items-center gap-4 p-4 bg-gray-800 rounded-lg">
      <div className="flex items-center gap-2">
        <button
          onClick={onStep}
          disabled={disabled || isRunning}
          className="px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-600 disabled:cursor-not-allowed rounded font-medium transition-colors"
        >
          Step
        </button>

        {isRunning ? (
          <button
            onClick={onPause}
            disabled={disabled}
            className="px-4 py-2 bg-yellow-600 hover:bg-yellow-700 disabled:bg-gray-600 disabled:cursor-not-allowed rounded font-medium transition-colors"
          >
            Pause
          </button>
        ) : (
          <button
            onClick={onRun}
            disabled={disabled}
            className="px-4 py-2 bg-green-600 hover:bg-green-700 disabled:bg-gray-600 disabled:cursor-not-allowed rounded font-medium transition-colors"
          >
            Run
          </button>
        )}

        <button
          onClick={onReset}
          disabled={disabled || generation === 0}
          className="px-4 py-2 bg-orange-600 hover:bg-orange-700 disabled:bg-gray-600 disabled:cursor-not-allowed rounded font-medium transition-colors"
        >
          Reset
        </button>

        <button
          onClick={onClear}
          disabled={disabled}
          className="px-4 py-2 bg-red-600 hover:bg-red-700 disabled:bg-gray-600 disabled:cursor-not-allowed rounded font-medium transition-colors"
        >
          Clear
        </button>

        <button
          onClick={onShowFinalState}
          disabled={disabled || !useApi || cellCount === 0 || isRunning}
          className="px-4 py-2 bg-purple-600 hover:bg-purple-700 disabled:bg-gray-600 disabled:cursor-not-allowed rounded font-medium transition-colors"
          title={!useApi ? "API mode required" : cellCount === 0 ? "Add some cells first" : "Show final stable state"}
        >
          Show Final State
        </button>
      </div>

      <div className="flex items-center gap-2">
        <label className="text-sm text-gray-400">Speed:</label>
        <input
          type="range"
          min={1}
          max={1000}
          step={10}
          value={1001 - speed}
          onChange={(e) => onSpeedChange(1001 - Number(e.target.value))}
          className="w-24"
          title={`${speed}ms = ${Math.round(1000/speed)} requests/second`}
        />
        <span className="text-sm text-gray-400 w-20">{speed}ms</span>
      </div>

      <div className="flex items-center gap-4 text-sm text-gray-400">
        <div>
          Generation: <span className="font-mono text-white">{generation}</span>
        </div>
        <div>
          Cells: <span className="font-mono text-white">{cellCount}</span>
        </div>
      </div>
    </div>
  );
}
