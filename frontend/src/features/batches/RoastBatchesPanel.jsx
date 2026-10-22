import { useEffect, useState } from 'react'
import { roastBatchesApi, roastedCoffeeStockApi } from '../../api/batches'
import { greenCoffeesApi } from '../../api/catalogs'
import { CatalogPanel } from '../catalogs/CatalogPanel'

// RF-07 a RF-13 / RF-19 / RF-20 / RF-22
export function RoastBatchesPanel() {
  const [greenCoffees, setGreenCoffees] = useState([])
  const [roastedStock, setRoastedStock] = useState(null)

  const loadGreenCoffees = () => greenCoffeesApi.list().then(setGreenCoffees)
  const loadRoastedStock = () => roastedCoffeeStockApi.get().then(setRoastedStock)

  useEffect(() => {
    loadGreenCoffees()
    loadRoastedStock()
  }, [])

  // El batch descuenta stock de café verde e incrementa el de café tostado
  // (RF-08/RF-09): refrescamos ambos luego de un alta exitosa.
  const handleCreated = async () => {
    await Promise.all([loadGreenCoffees(), loadRoastedStock()])
  }

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
        title="Batches de tueste"
        api={roastBatchesApi}
        onCreated={handleCreated}
        fields={[
          {
            name: 'greenCoffeeId',
            label: 'Café verde',
            type: 'select',
            required: true,
            options: greenCoffees.map((c) => ({ id: c.id, name: `${c.name} (stock: ${c.stockKg}kg)` })),
          },
          { name: 'greenCoffeeUsedKg', label: 'Café verde usado (kg)', type: 'number', required: true },
          {
            name: 'roastedCoffeeObtainedKg',
            label: 'Café tostado obtenido (kg)',
            type: 'number',
            required: true,
          },
          { name: 'roastDate', label: 'Fecha del tueste', type: 'date', required: true },
          { name: 'humidityPercentage', label: 'Humedad (%, opcional)', type: 'number' },
          { name: 'density', label: 'Densidad (opcional)', type: 'number' },
          { name: 'developmentPercentage', label: 'Desarrollo (%, opcional)', type: 'number' },
        ]}
        columns={[
          { key: 'roastDate', label: 'Fecha' },
          { key: 'greenCoffeeName', label: 'Café verde' },
          { key: 'greenCoffeeUsedKg', label: 'Verde usado (kg)' },
          { key: 'roastedCoffeeObtainedKg', label: 'Tostado obtenido (kg)' },
          { key: 'shrinkagePercentage', label: 'Merma (%)' },
          { key: 'costPerKg', label: 'Costo/kg' },
        ]}
      />
    </div>
  )
}
