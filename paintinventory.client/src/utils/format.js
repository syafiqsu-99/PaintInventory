export function formatDateTime(value) {
  if (!value) return ''
  return new Date(value).toLocaleString()
}

export function formatDate(value) {
  if (!value) return ''
  return new Date(value).toLocaleDateString()
}

export function formatNumber(value) {
  if (value === null || value === undefined) return ''
  return Number(value).toLocaleString(undefined, { maximumFractionDigits: 2 })
}
