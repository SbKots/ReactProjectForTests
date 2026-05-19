import type {
  SubmitTestRequest,
  TestDetails,
  TestForTaking,
  TestSubmissionResult,
  TestSummary,
  TestWriteRequest
} from "../types";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5088/api";

interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: string[];
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...init?.headers
    }
  });

  if (!response.ok) {
    throw new Error(await readError(response));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

async function readError(response: Response): Promise<string> {
  try {
    const problem = (await response.json()) as ProblemDetails;
    if (problem.errors?.length) {
      return problem.errors.join("\n");
    }

    return problem.detail ?? problem.title ?? `HTTP ${response.status}`;
  } catch {
    return `HTTP ${response.status}`;
  }
}

export const testsApi = {
  list: () => request<TestSummary[]>("/tests"),
  get: (id: string) => request<TestDetails>(`/tests/${id}`),
  getForTaking: (id: string) => request<TestForTaking>(`/tests/${id}/take`),
  create: (payload: TestWriteRequest) =>
    request<TestDetails>("/tests", {
      method: "POST",
      body: JSON.stringify(payload)
    }),
  update: (id: string, payload: TestWriteRequest) =>
    request<TestDetails>(`/tests/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload)
    }),
  remove: (id: string) =>
    request<void>(`/tests/${id}`, {
      method: "DELETE"
    }),
  submit: (id: string, payload: SubmitTestRequest) =>
    request<TestSubmissionResult>(`/tests/${id}/submissions`, {
      method: "POST",
      body: JSON.stringify(payload)
    })
};
