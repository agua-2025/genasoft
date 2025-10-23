import type { DocumentItem } from './types'
import { getBearer } from './auth'

const BASE = process.env.NEXT_PUBLIC_API_BASE ?? 'http://localhost:5099'

async function api<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(init?.headers || {}),
      ...(getBearer() ? { Authorization: getBearer() } : {}),
    },
    cache: 'no-store',
  })
  if (!res.ok) {
    const txt = await res.text()
    throw new Error(`${res.status} ${res.statusText} - ${txt}`)
  }
  return res.json() as Promise<T>
}

export const Api = {
  listDocuments(q?: { typeKey?: string; year?: number; status?: string }) {
    const qs = new URLSearchParams()
    if (q?.typeKey) qs.set('typeKey', q.typeKey)
    if (q?.year) qs.set('year', String(q.year))
    if (q?.status) qs.set('status', q.status)
    const suffix = qs.toString() ? `?${qs.toString()}` : ''
    return api<DocumentItem[]>(`/api/documents${suffix}`)
  },
  getConsolidatedPreview(id: number) {
    return api<{ html: string }>(`/api/documents/${id}/consolidated-preview`)
  },
  createDocument(payload: any) {
    return api<{ id: number }>(`/api/documents`, { method: 'POST', body: JSON.stringify(payload) })
  },
  updateDocument(id: number, payload: any) {
    return api(`/api/documents/${id}`, { method: 'PUT', body: JSON.stringify(payload) })
  },
  approve(id: number) {
    return api(`/api/documents/${id}/approve`, { method: 'POST', body: '{}' })
  },
  allocate(id: number) {
    return api(`/api/documents/${id}/allocate-number`, { method: 'POST', body: '{}' })
  },
  sign(id: number, signerName?: string) {
    return api(`/api/documents/${id}/sign`, { method: 'POST', body: JSON.stringify({ signerName }) })
  },
  publish(id: number) {
    return api(`/api/documents/${id}/publish`, { method: 'POST', body: '{}' })
  },
}
