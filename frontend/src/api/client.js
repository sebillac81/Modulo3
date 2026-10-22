const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5079'

export class ApiError extends Error {
  constructor(message, status, problem) {
    super(message)
    this.status = status
    this.problem = problem
  }
}

async function request(path, options = {}) {
  const res = await fetch(`${API_URL}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  })

  if (!res.ok) {
    const problem = await res.json().catch(() => null)
    const message = problem?.errors
      ? Object.values(problem.errors).flat().join(' ')
      : (problem?.title ?? `Error ${res.status}`)
    throw new ApiError(message, res.status, problem)
  }

  if (res.status === 204) return null
  return res.json()
}

export const apiGet = (path) => request(path)
export const apiPost = (path, body) =>
  request(path, { method: 'POST', body: JSON.stringify(body) })
