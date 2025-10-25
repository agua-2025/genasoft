'use client'
import { useEffect, useState } from 'react'
import Link from 'next/link'
import type { DocumentItem } from '@/lib/types'
import { Api } from '@/lib/api'

export default function DocumentsPage() {
  const [items, setItems] = useState<DocumentItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    Api.listDocuments().then(setItems).catch(e => setError(String(e))).finally(()=>setLoading(false))
  }, [])

  return (
    <div className="space-y-6">
      {/* Toolbar mínima */}
      <div className="flex items-center gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-neutral-900">Documentos</h1>
          <p className="text-sm text-neutral-500">Gerencie e consulte os atos oficiais.</p>
        </div>
        <button
          className="btn btn-primary ml-auto"
          onClick={async () => {
            try {
              const res = await Api.createDocument({
                typeKey: 'portaria',
                organId: 1,
                title: 'Nova minuta',
                textHtml: '<section id="art-1"><p>Art. 1º ...</p></section>'
              })
              location.href = '/documents/' + res.id
            } catch (e:any) { alert(e.message) }
          }}
        >
          Criar documento
        </button>
      </div>

      {/* Lista de documentos */}
      <div className="card">
        <div className="p-4 border-b border-neutral-200">
          <h3 className="font-semibold text-neutral-900">Documentos</h3>
        </div>
        {loading && <div className="p-6 text-neutral-600">Carregando...</div>}
        {error && <div className="p-6 text-red-600">{error}</div>}
        <ul className="divide-y divide-neutral-200">
          {items.map(d => (
            <li key={d.id} className="p-4 flex items-center gap-3">
              <span className="badge">{d.typeKey}</span>
              <div className="flex-1">
                <div className="font-medium text-neutral-900">{d.title}</div>
                <div className="text-xs text-neutral-500">#{d.number ?? '—'} · {d.year} · {d.status}</div>
              </div>
              <Link href={'/documents/' + d.id} className="btn">Abrir</Link>
            </li>
          ))}
          {!loading && items.length === 0 && (
            <li className="p-6 text-neutral-600">Nenhum documento encontrado.</li>
          )}
        </ul>
      </div>
    </div>
  )
}
