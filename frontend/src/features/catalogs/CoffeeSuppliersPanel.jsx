import { coffeeSuppliersApi } from '../../api/catalogs'
import { CatalogPanel } from './CatalogPanel'

// RF-01 / AC-01
export function CoffeeSuppliersPanel() {
  return (
    <CatalogPanel
      title="Proveedores de café"
      api={coffeeSuppliersApi}
      fields={[{ name: 'name', label: 'Nombre', type: 'text', required: true }]}
      columns={[{ key: 'name', label: 'Nombre' }]}
    />
  )
}
