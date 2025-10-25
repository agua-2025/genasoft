export type DocumentStatus = 'Draft' | 'InReview' | 'Approved' | 'Numbered' | 'Signed' | 'Published'

export interface DocumentItem {
  id: number
  typeKey: string
  title: string
  status: DocumentStatus
  year?: number | null
  number?: number | null
  createdBy?: string | null
  createdAtUtc?: string | null
  organId?: number | null
  textHtml?: string | null
}

export interface CreateDocumentInput {
  typeKey: string
  organId: number
  title: string
  textHtml: string
}