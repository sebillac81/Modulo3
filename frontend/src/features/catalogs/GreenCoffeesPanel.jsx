import { useEffect, useState } from 'react'
import { coffeeSuppliersApi, greenCoffeesApi } from '../../api/catalogs'
import { CatalogPanel } from './CatalogPanel'

// RF-03 / AC-03a / AC-03b
export function GreenCoffeesPanel() {
  const [suppliers, setSuppliers] = useState([])

  useEffect(() => {
    coffeeSuppliersApi.list().then(setSuppliers)
  }, [])

  return (
    <CatalogPanel
      title="Cafés verdes"
      api={greenCoffeesApi}
      fields={[
        { name: 'name', label: 'Nombre', type: 'text', required: true },
        { name: 'supplierId', label: 'Proveedor', type: 'select', options: suppliers, required: true },
        {
          name: 'purchasePricePerKg',
          label: 'Precio de compra por kg',
          type: 'number',
          required: true,
        },
        { name: 'stockKg', label: 'Stock inicial (kg)', type: 'number', required: true },
      ]}
      columns={[
        { key: 'name', label: 'Nombre' },
        { key: 'supplierName', label: 'Proveedor' },
        { key: 'purchasePricePerKg', label: 'Precio/kg' },
        { key: 'stockKg', label: 'Stock (kg)' },
      ]}
    />
  )
}
