import { customersApi } from '../../api/catalogs'
import { CatalogPanel } from './CatalogPanel'

// RF-06 / AC-06
export function CustomersPanel() {
  return (
    <CatalogPanel
      title="Clientes"
      api={customersApi}
      fields={[{ name: 'name', label: 'Nombre', type: 'text', required: true }]}
      columns={[{ key: 'name', label: 'Nombre' }]}
    />
  )
}
