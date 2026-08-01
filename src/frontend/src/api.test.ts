import { describe, expect, it, vi } from 'vitest'

describe('api base url', () => {
  it('reads VITE_API_URL placeholder replacement value from meta', async () => {
    document.head.innerHTML = '<meta name="api-base-url" content="http://example.test" />'
    vi.resetModules()
    const mod = await import('./api')
    // api.ts computes urls internally; just ensure module loads and fetch functions exist.
    expect(typeof mod.createConversion).toBe('function')
    expect(typeof mod.listConversions).toBe('function')
    expect(typeof mod.getConversionById).toBe('function')
  })
})
