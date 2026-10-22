import { useEffect, useState } from 'react'
import './App.css'
import { CatalogsPage } from './features/catalogs/CatalogsPage'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5079'

function App() {
  const [health, setHealth] = useState({ status: 'checking...' })

  useEffect(() => {
    fetch(`${API_URL}/api/health`)
      .then((res) => res.json())
      .then(setHealth)
      .catch(() => setHealth({ status: 'unreachable' }))
  }, [])

  return (
    <>
      <header className="app-header">
        <h1>RoastingManager</h1>
        <span className="health-badge">
          API: {health.status}
          {health.database ? ` · DB: ${health.database}` : ''}
        </span>
      </header>
      <main>
        <CatalogsPage />
      </main>
    </>
  )
}

export default App
