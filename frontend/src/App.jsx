import { useEffect, useState } from 'react'
import './App.css'
import { RoastBatchesPanel } from './features/batches/RoastBatchesPanel'
import { CatalogsPage } from './features/catalogs/CatalogsPage'
import { SalesPanel } from './features/sales/SalesPanel'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5079'

const SECTIONS = [
  { key: 'catalogs', label: 'Catálogos', Component: CatalogsPage },
  { key: 'batches', label: 'Batches de tueste', Component: RoastBatchesPanel },
  { key: 'sales', label: 'Ventas', Component: SalesPanel },
]

function App() {
  const [health, setHealth] = useState({ status: 'checking...' })
  const [activeSection, setActiveSection] = useState(SECTIONS[0].key)

  useEffect(() => {
    fetch(`${API_URL}/api/health`)
      .then((res) => res.json())
      .then(setHealth)
      .catch(() => setHealth({ status: 'unreachable' }))
  }, [])

  const ActiveSection = SECTIONS.find((s) => s.key === activeSection).Component

  return (
    <>
      <header className="app-header">
        <h1>RoastingManager</h1>
        <span className="health-badge">
          API: {health.status}
          {health.database ? ` · DB: ${health.database}` : ''}
        </span>
      </header>
      <nav className="section-nav">
        {SECTIONS.map((section) => (
          <button
            key={section.key}
            type="button"
            className={section.key === activeSection ? 'active' : ''}
            onClick={() => setActiveSection(section.key)}
          >
            {section.label}
          </button>
        ))}
      </nav>
      <main>
        <ActiveSection />
      </main>
    </>
  )
}

export default App
