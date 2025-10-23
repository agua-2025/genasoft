export type DocumentStatus = 'Draft' | 'InReview' | 'Approved' | 'Numbered' | 'Signed' | 'Published'

export interface DocumentItem {
  id: number
  typeKey: string
  organId?: number
  number?: number
  year: number
  title: string
  subject?: string
  status: DocumentStatus
  publishedAt?: string
}
