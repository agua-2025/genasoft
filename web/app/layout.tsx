import './globals.css'
import Link from 'next/link'
import type { Metadata } from 'next'

export const metadata: Metadata = {
  title: 'GenaSoft • Atos',
  description: 'Gestão de atos oficiais',
}

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="pt-br">
      <body>
        <header className="border-b border-neutral-800">
          <nav className="container flex items-center gap-4 py-3">
            <Link href="/dashboard" className="btn">Dashboard</Link>
            <Link href="/documents" className="btn">Documentos</Link>
            <div className="ml-auto">
              <Link href="/login" className="btn">Login</Link>
            </div>
          </nav>
        </header>
        <main className="container py-6">{children}</main>
      </body>
    </html>
  )
}
