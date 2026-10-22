import { useEffect, useState } from 'react'
import { salePresentationsApi, suppliesApi } from '../../api/catalogs'
import { CatalogPanel } from './CatalogPanel'

// RF-05 / AC-05a / AC-05b
export function SalePresentationsPanel() {
  const [supplies, setSupplies] = useState([])

  useEffect(() => {
    suppliesApi.list().then(setSupplies)
  }, [])

  return (
    <CatalogPanel
      title="Presentaciones de venta"
      api={salePresentationsApi}
      fields={[
        { name: 'name', label: 'Nombre', type: 'text', required: true },
        {
          name: 'roastedCoffeeGrams',
          label: 'Cantidad de café tostado (g)',
          type: 'number',
          required: true,
        },
        { name: 'supplyIds', label: 'Insumos asociados', type: 'multiselect', options: supplies },
      ]}
      columns={[
        { key: 'name', label: 'Nombre' },
        { key: 'roastedCoffeeGrams', label: 'Gramos' },
        {
          key: 'supplies',
          label: 'Insumos',
          render: (item) => item.supplies.map((s) => s.name).join(', ') || '—',
        },
      ]}
    />
  )
}
