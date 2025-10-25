import { useEffect, useMemo, useState } from 'react'
import Header from '../components/Header'
import { StatCard } from '../components/StatCard'
import { DocumentTable } from '../components/DocumentTable'
import { Modal } from '../components/Modal'
import { createDocument, listDocuments } from '../services/documents'
import type { DocumentItem, DocumentStatus } from '../types/document'

export default function DashboardPage() {
  const [docs, setDocs] = useState<DocumentItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [search, setSearch] = useState('')
  const [typeKey, setTypeKey] = useState('')
  const [status, setStatus] = useState<DocumentStatus | ''>('')

  const [open, setOpen] = useState(false)
  const [form, setForm] = useState({ typeKey: '', organId: 1, title: '', textHtml: '' })
  const [toast, setToast] = useState<string | null>(null)

  useEffect(() => {
    const load = async () => {
      setLoading(true)
      setError(null)
      try {
        const data = await listDocuments(0, 10)
        setDocs(data)
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Falha ao carregar documentos')
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [])

  const stats = useMemo(() => {
    return {
      total: docs.length,
      pendentes: docs.filter(d => d.status !== 'Published').length,
      publicados: docs.filter(d => d.status === 'Published').length,
      usuarios: 4,
    }
  }, [docs])

  const onClear = () => { setSearch(''); setTypeKey(''); setStatus('') }

  const onCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await createDocument({
        typeKey: form.typeKey,
        organId: form.organId,
        title: form.title,
        textHtml: form.textHtml,
      })
      setToast('Documento criado com sucesso')
      setOpen(false)
      setForm({ typeKey: '', organId: 1, title: '', textHtml: '' })
      const data = await listDocuments(0, 10)
      setDocs(data)
    } catch (err) {
      setToast(err instanceof Error ? err.message : 'Erro ao criar documento')
    } finally {
      setTimeout(() => setToast(null), 3000)
    }
  }

  return (
    <div className="min-h-screen">
      <Header />
      <main className="container py-6 space-y-6">
        {/* Top row: stats + agenda */}
        <div className="grid gap-6 lg:grid-cols-3">
          <div className="space-y-4 lg:col-span-2">
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
              <StatCard title="Total de documentos" value={stats.total} />
              <StatCard title="Pendentes" value={stats.pendentes} />
              <StatCard title="Publicados" value={stats.publicados} />
              <StatCard title="Usuários" value={stats.usuarios} />
            </div>

            {/* Explorar processos */}
            <section className="space-y-3">
              <h3 className="text-sm font-semibold text-neutral-700">Explorar processos</h3>
              <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                {[
                  'Legislativo em andamento',
                  'Administrativo em andamento',
                  'Legislação em andamento',
                  'Consulta Geral',
                  'Criados pelo meu grupo',
                  'Arquivados',
                  'Comunicações',
                  'Protocolos recentes',
                ].map((label) => (
                  <div key={label} className="card flex items-center gap-3 p-3">
                    <div className="h-6 w-6 rounded-md bg-brand-600/20 text-brand-700 grid place-items-center">•</div>
                    <span className="text-sm text-neutral-800">{label}</span>
                  </div>
                ))}
              </div>
            </section>
          </div>

          {/* Agenda oficial (simplificada) */}
          <aside className="card p-4">
            <div className="flex items-center justify-between">
              <div>
                <h3 className="text-sm font-semibold text-neutral-700">Agenda oficial</h3>
                <p className="text-xs text-neutral-600">Confira os próximos eventos da Câmara</p>
              </div>
            </div>
            <div className="mt-4 grid grid-cols-7 gap-1 text-center text-xs">
              {['Dom','Seg','Ter','Qua','Qui','Sex','Sáb'].map(d => (
                <div key={d} className="p-1 font-medium text-neutral-700">{d}</div>
              ))}
              {Array.from({ length: 35 }).map((_, i) => (
                <div key={i} className={`p-2 rounded ${i === 20 ? 'bg-brand-600 text-white' : 'bg-neutral-100 text-neutral-700'}`}>{i + 1}</div>
              ))}
            </div>
            <p className="mt-3 text-xs text-neutral-600">Não há nenhum evento para a data selecionada.</p>
          </aside>
        </div>

        {/* Documentos */}
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold">Documentos</h2>
          <button className="btn btn-primary" onClick={() => setOpen(true)}>+ Criar documento</button>
        </div>

        <DocumentTable
          data={docs}
          loading={loading}
          error={error}
          search={search}
          typeKey={typeKey}
          status={status}
          onSearch={setSearch}
          onTypeKey={setTypeKey}
          onStatus={setStatus}
          onClear={onClear}
        />
      </main>

      {toast && (
        <div className="fixed bottom-4 left-1/2 -translate-x-1/2 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-2 text-sm text-emerald-700 shadow">
          {toast}
        </div>
      )}

      <Modal open={open} title="Novo Documento" onClose={() => setOpen(false)}>
        <form onSubmit={onCreate} className="space-y-3">
          <div className="grid gap-3 sm:grid-cols-2">
            <div>
              <label className="block text-xs font-medium text-neutral-600">typeKey</label>
              <input
                value={form.typeKey}
                onChange={e => setForm(s => ({ ...s, typeKey: e.target.value }))}
                required
                className="w-full rounded-lg border border-neutral-300 px-3 py-2 text-sm"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-neutral-600">organId</label>
              <input
                type="number"
                value={form.organId}
                onChange={e => setForm(s => ({ ...s, organId: Number(e.target.value) }))}
                required
                className="w-full rounded-lg border border-neutral-300 px-3 py-2 text-sm"
              />
            </div>
          </div>
          <div>
            <label className="block text-xs font-medium text-neutral-600">title</label>
            <input
              value={form.title}
              onChange={e => setForm(s => ({ ...s, title: e.target.value }))}
              required
              className="w-full rounded-lg border border-neutral-300 px-3 py-2 text-sm"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-neutral-600">textHtml</label>
            <textarea
              value={form.textHtml}
              onChange={e => setForm(s => ({ ...s, textHtml: e.target.value }))}
              required
              rows={6}
              className="w-full rounded-lg border border-neutral-300 px-3 py-2 text-sm"
            />
          </div>
          <div className="flex justify-end gap-2">
            <button type="button" className="btn btn-ghost" onClick={() => setOpen(false)}>Cancelar</button>
            <button type="submit" className="btn btn-primary">Salvar</button>
          </div>
        </form>
      </Modal>
    </div>
  )
}