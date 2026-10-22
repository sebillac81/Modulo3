import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import App from './App'

describe('App', () => {
  it('renders the app title', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({ json: () => Promise.resolve({ status: 'ok' }) }),
    )

    render(<App />)

    expect(await screen.findByText('RoastingManager')).toBeInTheDocument()
  })
})
