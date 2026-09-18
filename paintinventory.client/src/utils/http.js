const BASE = '/api'

async function request(path, { method = 'GET', body, signal, headers } = {}) {
  const res = await fetch(`${BASE}${path}`, {
    method,
    signal,
    headers: {
      ...(body !== undefined ? { 'Content-Type': 'application/json' } : {}),
      ...headers
    },
    body: body !== undefined ? JSON.stringify(body) : undefined
  })

  if (!res.ok) {
    let payload = null
    let text = ''
    try { text = await res.text() } catch { /* body may be empty */ }
    if (text) { try { payload = JSON.parse(text) } catch { /* not JSON */ } }

    const message = payload?.error ?? payload?.detail ?? payload?.title ?? text ?? `Request failed (${res.status})`
    const error = new Error(message)
    error.status = res.status
    error.traceId = payload?.traceId
    throw error
  }

  return res.status === 204 ? null : res.json()
}

export default {
  get: (path, opts) => request(path, { ...opts, method: 'GET' }),
  post: (path, body, opts) => request(path, { ...opts, method: 'POST', body }),
  put: (path, body, opts) => request(path, { ...opts, method: 'PUT', body }),
  del: (path, opts) => request(path, { ...opts, method: 'DELETE' })
}
