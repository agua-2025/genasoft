import './globals.css'
import Link from 'next/link'
import type { Metadata } from 'next'
import { Inter } from 'next/font/google'
const inter = Inter({ subsets: ['latin'], display: 'swap' })

export const metadata: Metadata = {
  title: 'GenaSoft • Atos',
  description: 'Gestão de atos oficiais',
}

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="pt-br">
      <body className={`${inter.className}`}>
        <header className="app-header">
          <nav className="container flex items-center gap-6 py-3 text-white">
            <div className="flex items-center gap-3 mr-6">
              <div className="h-8 w-8 rounded-lg bg-white/10 ring-1 ring-white/20" aria-hidden="true"></div>
              <span className="font-semibold tracking-wide">GenaSoft • Atos</span>
            </div>
            <Link href="/dashboard" className="nav-link">Dashboard</Link>
            <Link href="/documents" className="nav-link">Documentos</Link>
            <div className="ml-auto">
              <Link href="/login" className="btn btn-primary">Entrar</Link>
            </div>
          </nav>
        </header>
        <main className="container py-8">{children}</main>
      </body>
    </html>
  )
}
