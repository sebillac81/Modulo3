import { useEffect, useState } from 'react'

function defaultValue(field) {
  return field.type === 'multiselect' ? [] : ''
}

function initialForm(fields) {
  return Object.fromEntries(fields.map((f) => [f.name, defaultValue(f)]))
}

function toPayloadValue(field, raw) {
  if (field.type === 'number' || field.type === 'select') {
    return raw === '' ? null : Number(raw)
  }
  if (field.type === 'multiselect') {
    return raw.map(Number)
  }
  return raw
}

// Panel genérico de catálogo: formulario de alta + listado. Reutilizado por
// cada entidad de RF-01 a RF-06 y por los batches de tueste (RF-07), que
// solo difieren en los campos y columnas. `onCreated` es opcional y permite
// refrescar datos relacionados (p.ej. el stock de café tostado) tras un alta.
export function CatalogPanel({ title, api, fields, columns, onCreated }) {
  const [items, setItems] = useState([])
  const [form, setForm] = useState(() => initialForm(fields))
  const [error, setError] = useState(null)
  const [loading, setLoading] = useState(false)

  const load = () => api.list().then(setItems).catch((err) => setError(err.message))

  useEffect(() => {
    load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const handleChange = (name, value) => setForm((prev) => ({ ...prev, [name]: value }))

  const handleSubmit = async (event) => {
    event.preventDefault()
    setError(null)
    setLoading(true)
    try {
      const payload = Object.fromEntries(
        fields.map((field) => [field.name, toPayloadValue(field, form[field.name])]),
      )
      await api.create(payload)
      setForm(initialForm(fields))
      await load()
      await onCreated?.()
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <section className="catalog-panel">
      <h2>{title}</h2>
      <form onSubmit={handleSubmit} className="catalog-form">
        {fields.map((field) => (
          <label key={field.name}>
            {field.label}
            {field.type === 'select' && (
              <select
                value={form[field.name]}
                onChange={(e) => handleChange(field.name, e.target.value)}
                required={field.required}
              >
                <option value="">Seleccionar...</option>
                {field.options.map((opt) => (
                  <option key={opt.id} value={opt.id}>
                    {opt.name}
                  </option>
                ))}
              </select>
            )}
            {field.type === 'multiselect' && (
              <select
                multiple
                value={form[field.name]}
                onChange={(e) =>
                  handleChange(field.name, Array.from(e.target.selectedOptions, (o) => o.value))
                }
              >
                {field.options.map((opt) => (
                  <option key={opt.id} value={opt.id}>
                    {opt.name}
                  </option>
                ))}
              </select>
            )}
            {(field.type === 'text' || field.type === 'number' || field.type === 'date') && (
              <input
                type={field.type}
                step={field.type === 'number' ? '0.01' : undefined}
                value={form[field.name]}
                onChange={(e) => handleChange(field.name, e.target.value)}
                required={field.required}
              />
            )}
          </label>
        ))}
        <button type="submit" disabled={loading}>
          Guardar
        </button>
      </form>
      {error && (
        <p className="error" role="alert">
          {error}
        </p>
      )}
      <table>
        <thead>
          <tr>
            {columns.map((col) => (
              <th key={col.key}>{col.label}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {items.map((item) => (
            <tr key={item.id}>
              {columns.map((col) => (
                <td key={col.key}>{col.render ? col.render(item) : item[col.key]}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </section>
  )
}
