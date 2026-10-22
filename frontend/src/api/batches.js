import { apiGet, apiPost } from './client'

export const roastBatchesApi = {
  list: () => apiGet('/api/roast-batches'),
  create: (data) => apiPost('/api/roast-batches', data),
}

export const roastedCoffeeStockApi = {
  get: () => apiGet('/api/roasted-coffee-stock'),
}
