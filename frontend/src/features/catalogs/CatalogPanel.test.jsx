import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { CatalogPanel } from './CatalogPanel'

const fields = [{ name: 'name', label: 'Nombre', type: 'text', required: true }]
const columns = [{ key: 'name', label: 'Nombre' }]

describe('CatalogPanel', () => {
  it('lista los items existentes al montar', async () => {
    const api = {
      list: vi.fn().mockResolvedValue([{ id: 1, name: 'Finca La Esperanza' }]),
      create: vi.fn(),
    }

    render(<CatalogPanel title="Proveedores" api={api} fields={fields} columns={columns} />)

    expect(await screen.findByText('Finca La Esperanza')).toBeInTheDocument()
  })

  it('AC-01: guarda un nuevo registro y refresca el listado', async () => {
    const api = {
      list: vi
        .fn()
        .mockResolvedValueOnce([])
        .mockResolvedValueOnce([{ id: 1, name: 'Nuevo Proveedor' }]),
      create: vi.fn().mockResolvedValue({ id: 1, name: 'Nuevo Proveedor' }),
    }

    render(<CatalogPanel title="Proveedores" api={api} fields={fields} columns={columns} />)

    fireEvent.change(screen.getByLabelText('Nombre'), { target: { value: 'Nuevo Proveedor' } })
    fireEvent.click(screen.getByRole('button', { name: 'Guardar' }))

    await waitFor(() => expect(api.create).toHaveBeenCalledWith({ name: 'Nuevo Proveedor' }))
    expect(await screen.findByText('Nuevo Proveedor')).toBeInTheDocument()
  })

  it('AC-03b: muestra el mensaje de error devuelto por la API al rechazar la operación', async () => {
    const api = {
      list: vi.fn().mockResolvedValue([]),
      create: vi
        .fn()
        .mockRejectedValue(new Error('El precio de compra debe ser mayor a cero.')),
    }

    render(<CatalogPanel title="Cafés verdes" api={api} fields={fields} columns={columns} />)

    fireEvent.change(screen.getByLabelText('Nombre'), { target: { value: 'Colombia Huila' } })
    fireEvent.click(screen.getByRole('button', { name: 'Guardar' }))

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'El precio de compra debe ser mayor a cero.',
    )
  })
})
