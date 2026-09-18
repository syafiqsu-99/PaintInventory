const API_BASE = import.meta.env.VITE_API_BASE || '/api'

export async function getPaintByBarcode(barcode) {
  const res = await fetch(`${API_BASE}/paint/${encodeURIComponent(barcode)}`)
  if (res.status === 404) return null
  if (!res.ok) throw new Error('Failed to fetch paint')
  return res.json()
}

export async function recordScan(payload) {
  const res = await fetch(`${API_BASE}/scan`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })

  if (!res.ok) {
    const txt = await res.text()
    throw new Error(txt || 'Failed to record scan')
  }

  return res.json()
}

export default { getPaintByBarcode, recordScan }
