const API_BASE = import.meta.env.VITE_API_BASE ?? '/api'

const DEV_HEADERS: Record<string, string> = {
  'X-Dev-Auth': 'dev',
  'X-Dev-Roles': 'Admin,Editor',
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...DEV_HEADERS,
      ...(init?.headers ?? {}),
    },
  })
  if (!res.ok) {
    const text = await res.text().catch(() => '')
    throw new Error(`API error ${res.status}: ${text || res.statusText}`)
  }
  return res.json() as Promise<T>
}

export const Api = {
  get: <T>(path: string) => request<T>(path, { method: 'GET' }),
  post: <T, B = unknown>(path: string, body: B) =>
    request<T>(path, { method: 'POST', body: JSON.stringify(body) }),
}