''use client''
import { useEffect, useState } from 'react'
import { Api } from '@/lib/api'
import EditorClient from '@/app/_components/EditorClient'

export default function DocumentEditorPage({ params }: { params: { id: string } }) {
  const id = Number(params.id)
  const [html, setHtml] = useState<string>('<p>...</p>')
  const [title, setTitle] = useState<string>('(sem título)')
  const [loading, setLoading] = useState(true)
  const [previewHtml, setPreviewHtml] = useState<string | null>(null)

  useEffect(() => {
    Api.listDocuments()
      .then((list) => {
        const doc = list.find((d) => d.id === id)
        if (doc) {
          setTitle(doc.title)
        }
      })
      .finally(() => setLoading(false))
  }, [id])

  if (loading) {
    return <div className="card p-6">Carregando...</div>
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center gap-2">
        <input
          className="bg-neutral-900 border border-neutral-700 rounded-xl px-3 py-2 flex-1 min-w-[12rem]"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
        />
        <button
          className="btn"
          onClick={async () => {
            try {
              await Api.updateDocument(id, { title, textHtml: html })
              alert('Salvo!')
            } catch (e: any) {
              alert(e.message)
            }
          }}
        >
          Salvar
        </button>
        <button className="btn" onClick={() => Api.approve(id).then(() => alert('Aprovado')).catch((e) => alert(e.message))}>
          Enviar p/ aprovação
        </button>
        <button className="btn" onClick={() => Api.allocate(id).then(() => alert('Numerado')).catch((e) => alert(e.message))}>
          Numerar
        </button>
        <button className="btn" onClick={() => Api.sign(id, 'Autoridade').then(() => alert('Assinado')).catch((e) => alert(e.message))}>
          Assinar
        </button>
        <button className="btn btn-primary" onClick={() => Api.publish(id).then(() => alert('Publicado')).catch((e) => alert(e.message))}>
          Publicar
        </button>
        <button
          className="btn"
          onClick={async () => {
            try {
              const res = await Api.getConsolidatedPreview(id)
              setPreviewHtml(res.html)
            } catch (e: any) {
              alert(e.message)
            }
          }}
        >
          Preview Consolidado
        </button>
      </div>

      <div className="grid gap-4 md:grid-cols-[2fr_1fr]">
        <div className="card p-4">
          <EditorClient value={html} onChange={setHtml} />
        </div>
        <div className="card p-4 space-y-4">
          <div>
            <h3 className="font-semibold mb-2">Variáveis</h3>
            <div className="flex flex-wrap gap-2">
              {['{{numero}}', '{{ano}}', '{{autoridade.nome}}'].map((tok) => (
                <button key={tok} className="btn" onClick={() => setHtml((prev) => prev + ' ' + tok)}>
                  {tok}
                </button>
              ))}
            </div>
          </div>
          {previewHtml && (
            <div>
              <h3 className="font-semibold mb-2">Preview Consolidado</h3>
              <div
                className="prose max-w-none bg-white text-black rounded-xl p-4"
                dangerouslySetInnerHTML={{ __html: previewHtml }}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
