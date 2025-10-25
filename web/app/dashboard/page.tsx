'use client'

import { useEffect, useMemo, useState } from 'react'
import { Api } from '@/lib/api'
import type { DocumentItem } from '@/lib/types'

export default function DashboardPage() {
  const [documents, setDocuments] = useState<DocumentItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [lastUpdatedAt, setLastUpdatedAt] = useState<Date | null>(null)

  useEffect(() => {
    let active = true
    const load = async () => {
      setLoading(true)
      setError(null)
      try {
        const items = await Api.listDocuments()
        if (!active) return
        setDocuments(items)
        setLastUpdatedAt(new Date())
      } catch (err) {
        if (!active) return
        setError(err instanceof Error ? err.message : 'Falha ao carregar documentos.')
      } finally {
        if (active) setLoading(false)
      }
    }
    load()
    return () => {
      active = false
    }
  }, [])

  const published = useMemo(() => {
    return documents
      .filter((doc) => doc.status === 'Published')
      .sort((a, b) => {
        const numberDiff = (b.number ?? 0) - (a.number ?? 0)
        if (numberDiff !== 0) return numberDiff
        return b.id - a.id
      })
      .slice(0, 6)
  }, [documents])

  return (
    <div className='space-y-6'>
      <header className='flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between'>
        <div>
          <h1 className='text-2xl font-semibold text-neutral-900'>Painel administrativo</h1>
          <p className='text-sm text-neutral-500'>Visão geral dos atos oficiais.</p>
        </div>
        {lastUpdatedAt && (
          <span className='text-xs uppercase tracking-wide text-neutral-400'>
            Atualizado às{' '}
            {lastUpdatedAt.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
          </span>
        )}
      </header>

      {error && (
        <div className='card border-red-500/40 bg-red-950/30 p-5 text-sm text-red-200'>
          <strong className='block text-red-100'>Não foi possível carregar os dados.</strong>
          <span className='opacity-80'>{error}</span>
        </div>
      )}

      <section className='card p-5'>
        <header className='mb-4'>
          <h2 className='text-lg font-semibold'>Publicados recentemente</h2>
          <p className='text-xs text-neutral-400'>Últimos atos já disponíveis para consulta pública.</p>
        </header>

        {loading ? (
          <div className='space-y-3'>
            {Array.from({ length: 5 }).map((_, idx) => (
              <div key={idx} className='h-12 animate-pulse rounded-xl bg-neutral-800/60' />
            ))}
          </div>
        ) : published.length === 0 ? (
          <div className='rounded-xl border border-dashed border-neutral-800 p-8 text-center text-sm text-neutral-400'>
            Nenhum documento publicado ainda.
          </div>
        ) : (
          <ul className='space-y-3'>
            {published.map((doc) => (
              <li
                key={doc.id}
                className='flex flex-wrap items-center gap-3 rounded-xl border border-neutral-800/80 bg-neutral-900/70 p-4 transition hover:border-neutral-700'
              >
                <div className='min-w-[220px] flex-1'>
                  <p className='text-sm font-medium text-neutral-100'>{doc.title}</p>
                  <p className='text-xs text-neutral-400'>
                    {doc.typeKey} · Nº {doc.number ?? '—'}/{doc.year}
                  </p>
                </div>
                <span className='rounded-full border border-emerald-500/40 bg-emerald-500/10 px-3 py-1 text-xs font-medium text-emerald-200'>
                  Publicado
                </span>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  )
}
