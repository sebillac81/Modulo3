import { apiGet, apiPost } from './client'

export const coffeeSuppliersApi = {
  list: () => apiGet('/api/coffee-suppliers'),
  create: (data) => apiPost('/api/coffee-suppliers', data),
}

export const supplySuppliersApi = {
  list: () => apiGet('/api/supply-suppliers'),
  create: (data) => apiPost('/api/supply-suppliers', data),
}

export const greenCoffeesApi = {
  list: () => apiGet('/api/green-coffees'),
  create: (data) => apiPost('/api/green-coffees', data),
}

export const suppliesApi = {
  list: () => apiGet('/api/supplies'),
  create: (data) => apiPost('/api/supplies', data),
}

export const salePresentationsApi = {
  list: () => apiGet('/api/sale-presentations'),
  create: (data) => apiPost('/api/sale-presentations', data),
}

export const customersApi = {
  list: () => apiGet('/api/customers'),
  create: (data) => apiPost('/api/customers', data),
}
