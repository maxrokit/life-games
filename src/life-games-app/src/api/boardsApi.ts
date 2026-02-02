import type { BoardResponse, CreateBoardRequest, FinalStateResponse } from '../types';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL?.replace(/\/+$/, '') ?? '';
const API_BASE = `${apiBaseUrl}/api/boards`;

interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

class ApiError extends Error {
  public readonly status: number;
  public readonly problemDetails?: ProblemDetails;

  constructor(
    message: string,
    status: number,
    problemDetails?: ProblemDetails
  ) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.problemDetails = problemDetails;
  }
}

async function handleResponse<T>(response: Response, defaultMessage: string): Promise<T> {
  if (!response.ok) {
    let errorMessage = defaultMessage;
    let problemDetails: ProblemDetails | undefined;

    // Try to parse Problem Details (RFC 7807) response
    const contentType = response.headers.get('content-type');
    if (contentType?.includes('application/problem+json') || contentType?.includes('application/json')) {
      try {
        problemDetails = await response.json();
        if (problemDetails?.title) {
          errorMessage = problemDetails.title;
        }
        if (problemDetails?.detail) {
          errorMessage = problemDetails.detail;
        }
        // Include validation errors if present
        if (problemDetails?.errors) {
          const validationErrors = Object.entries(problemDetails.errors)
            .map(([field, errors]) => `${field}: ${Array.isArray(errors) ? errors.join(', ') : errors}`)
            .join('; ');
          if (validationErrors) {
            errorMessage = `${errorMessage} (${validationErrors})`;
          }
        }
      } catch {
        // If JSON parsing fails, use default message
      }
    }

    throw new ApiError(errorMessage, response.status, problemDetails);
  }

  return response.json();
}

export async function createBoard(request: CreateBoardRequest): Promise<BoardResponse> {
  const response = await fetch(API_BASE, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  return handleResponse<BoardResponse>(response, 'Failed to create board');
}

export async function getBoard(id: string): Promise<BoardResponse> {
  const response = await fetch(`${API_BASE}/${id}`);
  return handleResponse<BoardResponse>(response, 'Failed to get board');
}

export async function getNextGeneration(id: string): Promise<BoardResponse> {
  const response = await fetch(`${API_BASE}/${id}/next`);
  return handleResponse<BoardResponse>(response, 'Failed to get next generation');
}

export async function getGeneration(id: string, generation: number): Promise<BoardResponse> {
  const response = await fetch(`${API_BASE}/${id}/generations/${generation}`);
  return handleResponse<BoardResponse>(response, 'Failed to get generation');
}

export async function getFinalState(id: string): Promise<FinalStateResponse> {
  const response = await fetch(`${API_BASE}/${id}/final`);
  return handleResponse<FinalStateResponse>(response, 'Failed to get final state');
}

export async function fetchFromLink<T>(link: string | undefined): Promise<T> {
  if (!link) throw new Error('Link is not available');
  
  // Handle relative vs absolute links
  const url = link.startsWith('http') ? link : `${apiBaseUrl}${link}`;
  
  const response = await fetch(url);
  return handleResponse<T>(response, 'Failed to fetch from link');
}

export { ApiError };
