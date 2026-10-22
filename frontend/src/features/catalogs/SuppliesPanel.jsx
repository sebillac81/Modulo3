import { useEffect, useState } from 'react'
import { suppliesApi, supplySuppliersApi } from '../../api/catalogs'
import { CatalogPanel } from './CatalogPanel'

// RF-04 / AC-04a / AC-04b
export function SuppliesPanel() {
  const [suppliers, setSuppliers] = useState([])

  useEffect(() => {
    supplySuppliersApi.list().then(setSuppliers)
  }, [])

  return (
    <CatalogPanel
      title="Insumos"
      api={suppliesApi}
      fields={[
        { name: 'name', label: 'Nombre', type: 'text', required: true },
        { name: 'supplierId', label: 'Proveedor', type: 'select', options: suppliers, required: true },
        { name: 'unitPrice', label: 'Precio por unidad', type: 'number', required: true },
      ]}
      columns={[
        { key: 'name', label: 'Nombre' },
        { key: 'supplierName', label: 'Proveedor' },
        { key: 'unitPrice', label: 'Precio/u' },
      ]}
    />
  )
}
