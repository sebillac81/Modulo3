import { apiGet, apiPost } from './client'

export const salesApi = {
  list: () => apiGet('/api/sales'),
  create: (data) => apiPost('/api/sales', data),
}
