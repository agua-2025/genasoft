import { useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { getDocument } from '../services/documents'
import type { DocumentItem } from '../types/document'

export default function DocumentViewPage() {
  const { id } = useParams()
  const [doc, setDoc] = useState<DocumentItem | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const load = async () => {
      setLoading(true)
      setError(null)
      try {
        const data = await getDocument(Number(id))
        setDoc(data)
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Falha ao carregar documento')
      } finally {
        setLoading(false)
      }
    }
    if (id) load()
  }, [id])

  return (
    <main className="container py-6 space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold">Documento #{id}</h1>
          <p className="text-xs text-neutral-600">Visualização pública do conteúdo do ato.</p>
        </div>
        <Link to="/" className="btn btn-ghost">Voltar</Link>
      </div>

      {loading && <div className="text-sm text-neutral-600">Carregando...</div>}
      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {doc && (
        <div className="card p-4">
          <h2 className="text-lg font-semibold">{doc.title}</h2>
          <div className="mt-3 text-sm text-neutral-700" dangerouslySetInnerHTML={{ __html: doc.textHtml || '' }} />
          {/* TODO: sanitização de HTML */}
        </div>
      )}
    </main>
  )
}