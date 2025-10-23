'use client'
import { useEffect, useRef } from 'react'
import ClassicEditor from '@ckeditor/ckeditor5-build-classic'

export default function EditorClient({ value, onChange }:{ value:string; onChange:(html:string)=>void }) {
  const ref = useRef<HTMLDivElement|null>(null)
  const editorRef = useRef<any>(null)

  useEffect(() => {
    if (!ref.current) return
    ClassicEditor.create(ref.current, {
      toolbar: ['heading','|','bold','italic','underline','|','bulletedList','numberedList','|','insertTable','link','findAndReplace','|','undo','redo'],
    })
    .then((editor:any) => {
      editorRef.current = editor
      editor.setData(value)
      editor.model.document.on('change:data', () => onChange(editor.getData()))
    })
    .catch(console.error)
    return () => { editorRef.current?.destroy(); editorRef.current=null }
  }, [])

  return <div className='prose max-w-none bg-white text-black rounded-xl p-4' ref={ref}/>
}
