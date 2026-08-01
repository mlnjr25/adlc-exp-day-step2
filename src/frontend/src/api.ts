import type {
  ConversionListItem,
  ConversionRequestPayload,
  ConversionResponse
} from './types'

function getApiBaseUrlFromMeta(): string {
  const meta = document.querySelector('meta[name="api-base-url"]') as HTMLMetaElement | null
  return (meta?.content ?? '').trim()
}

const rawBase = typeof document === 'undefined' ? '' : getApiBaseUrlFromMeta()
const normalizedBase = rawBase.replace(/\/$/, '')

function apiUrl(path: string): string {
  return `${normalizedBase}/api${path.startsWith('/') ? path : '/' + path}`.replace(/\/+api\//, '/api/')
}

export async function createConversion(payload: ConversionRequestPayload): Promise<ConversionResponse> {
  const res = await fetch(apiUrl('/conversions'), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  })

  if (!res.ok) {
    const problem = await res.json().catch(() => null)
    const detail = problem?.detail ?? `Request failed with status ${res.status}`
    throw new Error(detail)
  }

  return (await res.json()) as ConversionResponse
}

export async function listConversions(limit: number): Promise<ConversionListItem[]> {
  const res = await fetch(apiUrl(`/conversions?limit=${encodeURIComponent(String(limit))}`))
  if (!res.ok) {
    const problem = await res.json().catch(() => null)
    const detail = problem?.detail ?? `Request failed with status ${res.status}`
    throw new Error(detail)
  }
  return (await res.json()) as ConversionListItem[]
}

export async function getConversionById(auditId: string): Promise<ConversionResponse> {
  const res = await fetch(apiUrl(`/conversions/${encodeURIComponent(auditId)}`))
  if (!res.ok) {
    const problem = await res.json().catch(() => null)
    const detail = problem?.detail ?? `Request failed with status ${res.status}`
    throw new Error(detail)
  }
  return (await res.json()) as ConversionResponse
}
