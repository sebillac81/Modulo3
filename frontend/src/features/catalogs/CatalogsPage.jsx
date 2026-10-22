import { useState } from 'react'
import { CoffeeSuppliersPanel } from './CoffeeSuppliersPanel'
import { CustomersPanel } from './CustomersPanel'
import { GreenCoffeesPanel } from './GreenCoffeesPanel'
import { SalePresentationsPanel } from './SalePresentationsPanel'
import { SuppliesPanel } from './SuppliesPanel'
import { SupplySuppliersPanel } from './SupplySuppliersPanel'

const TABS = [
  { key: 'coffee-suppliers', label: 'Proveedores de café', Component: CoffeeSuppliersPanel },
  { key: 'supply-suppliers', label: 'Proveedores de insumos', Component: SupplySuppliersPanel },
  { key: 'green-coffees', label: 'Cafés verdes', Component: GreenCoffeesPanel },
  { key: 'supplies', label: 'Insumos', Component: SuppliesPanel },
  { key: 'sale-presentations', label: 'Presentaciones de venta', Component: SalePresentationsPanel },
  { key: 'customers', label: 'Clientes', Component: CustomersPanel },
]

export function CatalogsPage() {
  const [activeKey, setActiveKey] = useState(TABS[0].key)
  const ActivePanel = TABS.find((tab) => tab.key === activeKey).Component

  return (
    <div className="catalogs-page">
      <nav className="catalog-tabs">
        {TABS.map((tab) => (
          <button
            key={tab.key}
            type="button"
            className={tab.key === activeKey ? 'active' : ''}
            onClick={() => setActiveKey(tab.key)}
          >
            {tab.label}
          </button>
        ))}
      </nav>
      <ActivePanel />
    </div>
  )
}
