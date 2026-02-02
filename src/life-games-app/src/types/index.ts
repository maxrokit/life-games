export interface Cell {
  x: number;
  y: number;
}

export interface Link {
  href: string;
  rel: string;
  method?: string;
}

export interface BoardResponse {
  id: string;
  name: string | null;
  createdAt: string;
  generationNumber: number;
  cells: Cell[];
  links: Record<string, Link>;
}

export interface FinalStateResponse {
  boardId: string;
  finalGenerationNumber: number;
  cells: Cell[];
  isCyclic: boolean;
  cycleLength: number | null;
  cycleStartGeneration: number | null;
  reachedMaxIterations: boolean;
  maxIterations: number;
  links: Record<string, Link>;
}

export interface CreateBoardRequest {
  name: string | null;
  cells: Cell[];
}
