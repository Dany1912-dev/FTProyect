const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

async function request<T>(
  path: string,
  options: RequestInit = {},
  isRetry = false,
): Promise<T> {
  const isFormData = options.body instanceof FormData

  const res = await fetch(`${BASE_URL}${path}`, {
    ...options,
    credentials: 'include', // el navegador adjunta las cookies automaticamente
    headers: {
      ...(!isFormData ? { 'Content-Type': 'application/json' } : {}),
      ...options.headers,
    },
  })

  // Access token expirado → intentar refresh una sola vez
  if (res.status === 401 && !isRetry && path !== '/auth/refresh') {
    try {
      const refreshRes = await fetch(`${BASE_URL}/auth/refresh`, {
        method: 'POST',
        credentials: 'include',
      })

      if (!refreshRes.ok) throw new Error('Refresh fallido')

      // Refresh exitoso → reintentar la peticion original
      return request<T>(path, options, true)
    } catch {
      // Refresh fallido → sesion expirada, notificar a la app
      window.dispatchEvent(new Event('auth:logout'))
      throw new Error('Sesion expirada, inicia sesion de nuevo')
    }
  }

  if (!res.ok) {
    const error = await res.json().catch(() => ({ message: 'Error desconocido' }))
    throw new Error(error.message || `Error ${res.status}`)
  }

  // 204 No Content u otras respuestas sin body
  const contentType = res.headers.get('content-type')
  if (!contentType || !contentType.includes('application/json')) {
    return undefined as T
  }

  return res.json()
}

export const api = {
  get: <T>(path: string) =>
    request<T>(path),

  post: <T>(path: string, body?: unknown) =>
    request<T>(path, {
      method: 'POST',
      body: body !== undefined ? JSON.stringify(body) : undefined,
    }),

  put: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'PUT', body: JSON.stringify(body) }),

  delete: <T>(path: string) =>
    request<T>(path, { method: 'DELETE' }),

  // Para subir archivos (el browser setea el Content-Type con boundary automaticamente)
  upload: <T>(path: string, formData: FormData) =>
    request<T>(path, { method: 'POST', body: formData }),
}
