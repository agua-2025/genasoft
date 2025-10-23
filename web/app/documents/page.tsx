'use client'
import { useEffect, useState } from 'react'
import Link from 'next/link'

type DocumentItem = {
  id: number; typeKey: string; number?: number; year: number; title: string; status: string;
}

async function api<T>(path: string, init?: RequestInit): Promise<T> {
  const base = process.env.NEXT_PUBLIC_API_BASE ?? 'http://localhost:5110'
  const auth = process.env.NEXT_PUBLIC_MOCK_BEARER ? { 'Authorization': 'Bearer ' + process.env.NEXT_PUBLIC_MOCK_BEARER } : {}
  const res = await fetch(base + path, { ...init, headers: { 'Content-Type':'application/json', ...auth } })
  if (!res.ok) throw new Error(await res.text())
  return res.json() as Promise<T>
}

export default function DocumentsPage() {
  const [items, setItems] = useState<DocumentItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    api<DocumentItem[]>('/api/documents').then(setItems).catch(e => setError(String(e))).finally(()=>setLoading(false))
  }, [])

  return (
    <div className='space-y-4'>
      <div className='flex items-center gap-2'>
        <h1 className='text-2xl font-semibold'>Documentos</h1>
        <button className='btn btn-primary ml-auto' onClick={async () => {
          try {
            const res = await api<{id:number}>('/api/documents', {
              method:'POST',
              body: JSON.stringify({
                typeKey:'portaria',
                organId:1,
                title:'Nova minuta',
                textHtml:'<section id=\"art-1\"><p>Art. 1º ...</p></section>'
              })
            })
            location.href = '/documents/' + res.id
          } catch (e:any) { alert(e.message) }
        }}>+ Nova minuta</button>
      </div>

      {loading && <div className='card p-6'>Carregando...</div>}
      {error && <div className='card p-6 text-red-300'>{error}</div>}

      <ul className='grid gap-3'>
        {items.map(d => (
          <li key={d.id} className='card p-4 flex items-center gap-3'>
            <span className='badge'>{d.typeKey}</span>
            <div className='flex-1'>
              <div className='font-medium'>{d.title}</div>
              <div className='text-xs opacity-70'>#{d.number ?? '—'} · {d.year} · {d.status}</div>
            </div>
            <Link href={'/documents/' + d.id} className='btn'>Editar</Link>
          </li>
        ))}
      </ul>
    </div>
  )
}
