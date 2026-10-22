import { useEffect, useState } from 'react'
import { roastedCoffeeStockApi } from '../../api/batches'
import { customersApi, salePresentationsApi } from '../../api/catalogs'
import { salesApi } from '../../api/sales'
import { CatalogPanel } from '../catalogs/CatalogPanel'

// RF-14 a RF-18 / RF-21 / RF-23 / RF-24
export function SalesPanel() {
  const [customers, setCustomers] = useState([])
  const [presentations, setPresentations] = useState([])
  const [roastedStock, setRoastedStock] = useState(null)

  const loadCustomers = () => customersApi.list().then(setCustomers)
  const loadPresentations = () => salePresentationsApi.list().then(setPresentations)
  const loadRoastedStock = () => roastedCoffeeStockApi.get().then(setRoastedStock)

  useEffect(() => {
    loadCustomers()
    loadPresentations()
    loadRoastedStock()
  }, [])

  // La venta descuenta stock de café tostado (RF-15): refrescamos el resumen
  // luego de un alta exitosa.
  const handleCreated = () => loadRoastedStock()

  return (
    <div>
      <section className="stock-summary">
        <h2>Stock de café tostado</h2>
        {roastedStock && (
          <p>
            <strong>{roastedStock.quantityKg} kg</strong> disponibles · costo promedio ponderado{' '}
            <strong>${roastedStock.weightedAverageCostPerKg}/kg</strong>
          </p>
        )}
      </section>

      <CatalogPanel
        title="Ventas"
        api={salesApi}
        onCreated={handleCreated}
        fields={[
          {
            name: 'customerId',
            label: 'Cliente',
            type: 'select',
            required: true,
            options: customers,
          },
          {
            name: 'presentationId',
            label: 'Presentación',
            type: 'select',
            required: true,
            options: presentations.map((p) => ({ id: p.id, name: `${p.name} (${p.roastedCoffeeGrams}g)` })),
          },
          { name: 'unitsSold', label: 'Cantidad de unidades', type: 'number', required: true },
          { name: 'unitSalePrice', label: 'Precio de venta por unidad', type: 'number', required: true },
          { name: 'saleDate', label: 'Fecha de la venta', type: 'date', required: true },
        ]}
        columns={[
          { key: 'saleDate', label: 'Fecha' },
          { key: 'customerName', label: 'Cliente' },
          { key: 'presentationName', label: 'Presentación' },
          { key: 'unitsSold', label: 'Unidades' },
          { key: 'unitSalePrice', label: 'Precio/u' },
          { key: 'associatedCosts', label: 'Costos asociados' },
          { key: 'profitMarginPercentage', label: 'Margen (%)' },
        ]}
      />
    </div>
  )
}
