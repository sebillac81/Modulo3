import { supplySuppliersApi } from '../../api/catalogs'
import { CatalogPanel } from './CatalogPanel'

// RF-02 / AC-02
export function SupplySuppliersPanel() {
  return (
    <CatalogPanel
      title="Proveedores de insumos"
      api={supplySuppliersApi}
      fields={[{ name: 'name', label: 'Nombre', type: 'text', required: true }]}
      columns={[{ key: 'name', label: 'Nombre' }]}
    />
  )
}
