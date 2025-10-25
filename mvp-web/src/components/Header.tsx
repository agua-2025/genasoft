export default function Header() {
  return (
    <header className="border-b bg-white">
      <div className="container flex h-14 items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="h-6 w-6 rounded-md bg-brand-600" aria-hidden />
          <span className="text-sm font-semibold">GenaSoft</span>
        </div>
        <div className="flex items-center gap-3">
          <span className="text-sm text-neutral-600">dev@local</span>
          <button className="btn btn-ghost">Sair</button>
        </div>
      </div>
    </header>
  )
}