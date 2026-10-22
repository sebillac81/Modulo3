import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import App from './App'

describe('App', () => {
  it('renders the app title and the catalogs tabs', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        status: 200,
        json: () => Promise.resolve([]),
      }),
    )

    render(<App />)

    expect(await screen.findByText('RoastingManager')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Proveedores de café' })).toBeInTheDocument()
  })
})
