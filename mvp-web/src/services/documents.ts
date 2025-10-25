import { Api } from './api'
import type { CreateDocumentInput, DocumentItem } from '../types/document'

export async function listDocuments(skip = 0, take = 10): Promise<DocumentItem[]> {
  return Api.get<DocumentItem[]>(`/api/documents?skip=${skip}&take=${take}`)
}

export async function getDocument(id: number): Promise<DocumentItem> {
  return Api.get<DocumentItem>(`/api/documents/${id}`)
}

export async function createDocument(input: CreateDocumentInput): Promise<DocumentItem> {
  return Api.post<DocumentItem, CreateDocumentInput>('/api/documents', input)
}