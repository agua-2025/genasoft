import { useMemo } from 'react'
import type { DocumentItem, DocumentStatus } from '../types/document'
import { useNavigate } from 'react-router-dom'

interface TableProps {
  data: DocumentItem[]
  loading: boolean
  error: string | null
  search: string
  typeKey: string
  status: DocumentStatus | ''
  onSearch: (v: string) => void
  onTypeKey: (v: string) => void
  onStatus: (v: DocumentStatus | '') => void
  onClear: () => void
}

export function DocumentTable(props: TableProps) {
  const {
    data, loading, error,
    search, typeKey, status,
    onSearch, onTypeKey, onStatus, onClear,
  } = props

  const navigate = useNavigate()

  const filtered = useMemo(() => {
    return data.filter(d => {
      const matchesSearch = search ? (
        d.title.toLowerCase().includes(search.toLowerCase()) ||
        String(d.id).includes(search)
      ) : true
      const matchesType = typeKey ? d.typeKey === typeKey : true
      const matchesStatus = status ? d.status === status : true
      return matchesSearch && matchesType && matchesStatus
    })
  }, [data, search, typeKey, status])

  return (
    <div className="card p-4">
      <div className="mb-3 flex flex-wrap items-center gap-2">
        <input
          value={search}
          onChange={(e) => onSearch(e.target.value)}
          placeholder="Buscar por título ou ID"
          className="rounded-lg border border-neutral-300 px-3 py-2 text-sm"
        />
        <input
          value={typeKey}
          onChange={(e) => onTypeKey(e.target.value)}
          placeholder="typeKey"
          className="rounded-lg border border-neutral-300 px-3 py-2 text-sm"
        />
        <select
          value={status}
          onChange={(e) => onStatus(e.target.value as DocumentStatus | '')}
          className="rounded-lg border border-neutral-300 px-3 py-2 text-sm"
        >
          <option value="">Status</option>
          {['Draft','InReview','Approved','Numbered','Signed','Published'].map(s => (
            <option key={s} value={s}>{s}</option>
          ))}
        </select>
        <button className="btn btn-ghost" onClick={onClear}>Limpar filtros</button>
      </div>

      {loading && <div className="text-sm text-neutral-600">Carregando...</div>}
      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
          {error}
        </div>
      )}
      {!loading && filtered.length === 0 && !error && (
        <div className="text-sm text-neutral-600">Nenhum documento encontrado.</div>
      )}

      {!loading && filtered.length > 0 && (
        <table className="table">
          <thead>
            <tr>
              <th>id</th>
              <th>typeKey</th>
              <th>title</th>
              <th>createdBy</th>
              <th>createdAtUtc</th>
              <th>status</th>
              <th>ações</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((d) => (
              <tr key={d.id}>
                <td>{d.id}</td>
                <td><span className="badge border-neutral-300 bg-neutral-100">{d.typeKey}</span></td>
                <td>{d.title}</td>
                <td>{d.createdBy ?? '—'}</td>
                <td>{d.createdAtUtc ? new Date(d.createdAtUtc).toLocaleString('pt-BR') : '—'}</td>
                <td>{d.status}</td>
                <td>
                  <div className="flex flex-wrap gap-2">
                    <button className="btn btn-ghost" onClick={() => navigate(`/documents/${d.id}`)}>🔍 visualizar</button>
                    <button className="btn btn-ghost" disabled>✏️ editar</button>
                    <button className="btn btn-ghost" disabled>🗑️ excluir</button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}