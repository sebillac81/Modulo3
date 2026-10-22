import { useEffect, useState } from 'react'
import './App.css'

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
    <main>
      <h1>RoastingManager</h1>
      <p>
        API: <strong>{health.status}</strong>
        {health.database ? ` · DB: ${health.database}` : ''}
      </p>
    </main>
  )
}

export default App
