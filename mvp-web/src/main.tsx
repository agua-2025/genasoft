import React from 'react'
import ReactDOM from 'react-dom/client'
import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import './index.css'
import DashboardPage from './pages/Dashboard'
import DocumentViewPage from './pages/DocumentView'

const router = createBrowserRouter([
  { path: '/', element: <DashboardPage /> },
  { path: '/documents/:id', element: <DocumentViewPage /> },
])

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <RouterProvider router={router} />
  </React.StrictMode>,
)
