import { type FormEvent, useEffect, useMemo, useState } from 'react'
import {
  createConversion,
  getConversionById,
  listConversions
} from './api'
import type { ConversionListItem, ConversionRequestPayload, ConversionResponse } from './types'
import './app.css'

export default function App() {
  const [amount, setAmount] = useState<string>('100.00')
  const [fromCurrency, setFromCurrency] = useState<string>('USD')
  const [toCurrency, setToCurrency] = useState<string>('EUR')

  const [submitting, setSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)
  const [result, setResult] = useState<ConversionResponse | null>(null)

  const [historyLoading, setHistoryLoading] = useState(false)
  const [historyError, setHistoryError] = useState<string | null>(null)
  const [history, setHistory] = useState<ConversionListItem[]>([])

  const [selectedAuditId, setSelectedAuditId] = useState<string | null>(null)
  const [selectedLoading, setSelectedLoading] = useState(false)
  const [selectedError, setSelectedError] = useState<string | null>(null)
  const [selected, setSelected] = useState<ConversionResponse | null>(null)

  const requestPayload: ConversionRequestPayload = useMemo(
    () => ({
      amount: Number(amount),
      fromCurrency,
      toCurrency
    }),
    [amount, fromCurrency, toCurrency]
  )

  async function refreshHistory() {
    setHistoryLoading(true)
    setHistoryError(null)
    try {
      const items = await listConversions(10)
      setHistory(items)
    } catch (e) {
      setHistoryError(e instanceof Error ? e.message : 'Failed to load audit history')
    } finally {
      setHistoryLoading(false)
    }
  }

  useEffect(() => {
    void refreshHistory()
  }, [])

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setSubmitError(null)
    try {
      const response = await createConversion(requestPayload)
      setResult(response)
      setSelectedAuditId(response.auditId)
      setSelected(response)
      await refreshHistory()
    } catch (err) {
      setSubmitError(err instanceof Error ? err.message : 'Conversion failed')
    } finally {
      setSubmitting(false)
    }
  }

  async function onSelectAudit(auditId: string) {
    setSelectedAuditId(auditId)
    setSelectedLoading(true)
    setSelectedError(null)
    try {
      const record = await getConversionById(auditId)
      setSelected(record)
    } catch (e) {
      setSelectedError(e instanceof Error ? e.message : 'Failed to load audit record')
    } finally {
      setSelectedLoading(false)
    }
  }

  return (
    <div className="page">
      <header className="header">
        <div>
          <h1>Currency Conversion</h1>
          <p className="subtitle">Immediate conversion results with an immutable audit trail.</p>
        </div>
      </header>

      <main className="grid">
        <section className="card">
          <h2>New Conversion</h2>
          <form onSubmit={onSubmit} className="form">
            <label className="field">
              <span>Amount</span>
              <input
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
                inputMode="decimal"
                placeholder="100.00"
              />
            </label>

            <div className="row">
              <label className="field">
                <span>From</span>
                <input value={fromCurrency} onChange={(e) => setFromCurrency(e.target.value)} />
              </label>
              <label className="field">
                <span>To</span>
                <input value={toCurrency} onChange={(e) => setToCurrency(e.target.value)} />
              </label>
            </div>

            <button disabled={submitting} className="primary" type="submit">
              {submitting ? 'Converting…' : 'Convert'}
            </button>
          </form>

          {submitError ? <p className="error">{submitError}</p> : null}

          {result ? (
            <div className="result">
              <h3>Converted Result</h3>
              <div className="kv">
                <div>
                  <span className="k">Rate</span>
                  <span className="v">{result.rate}</span>
                </div>
                <div>
                  <span className="k">Converted</span>
                  <span className="v">{result.convertedAmount}</span>
                </div>
                <div>
                  <span className="k">Audit ID</span>
                  <span className="v monospace">{result.auditId}</span>
                </div>
                <div>
                  <span className="k">Provider Date</span>
                  <span className="v">{result.providerDate}</span>
                </div>
                <div>
                  <span className="k">Backend Exec Time (UTC)</span>
                  <span className="v monospace">{result.backendExecutionTimestampUtc}</span>
                </div>
              </div>
            </div>
          ) : null}
        </section>

        <section className="card">
          <h2>Audit History</h2>

          {historyLoading ? <p>Loading…</p> : null}
          {historyError ? <p className="error">{historyError}</p> : null}

          <div className="list">
            {history.length === 0 ? (
              <p className="muted">No conversions yet.</p>
            ) : (
              history.map((item) => (
                <button
                  key={item.auditId}
                  className={item.auditId === selectedAuditId ? 'listItem active' : 'listItem'}
                  onClick={() => void onSelectAudit(item.auditId)}
                  type="button"
                >
                  <div className="liTop">
                    <span className="liAudit monospace">{item.auditId}</span>
                  </div>
                  <div className="liBottom">
                    <span className="liMeta">Rate: {item.rate}</span>
                    <span className="liMeta">Converted: {item.convertedAmount}</span>
                  </div>
                </button>
              ))
            )}
          </div>

          <div className="divider" />

          <h3>Selected Audit Record</h3>

          {selectedLoading ? <p>Loading…</p> : null}
          {selectedError ? <p className="error">{selectedError}</p> : null}
          {selected ? (
            <div className="result">
              <div className="kv">
                <div>
                  <span className="k">Audit ID</span>
                  <span className="v monospace">{selected.auditId}</span>
                </div>
                <div>
                  <span className="k">Rate</span>
                  <span className="v">{selected.rate}</span>
                </div>
                <div>
                  <span className="k">Converted Amount</span>
                  <span className="v">{selected.convertedAmount}</span>
                </div>
                <div>
                  <span className="k">Provider Date</span>
                  <span className="v">{selected.providerDate}</span>
                </div>
                <div>
                  <span className="k">Backend Exec Time (UTC)</span>
                  <span className="v monospace">{selected.backendExecutionTimestampUtc}</span>
                </div>
              </div>

              <div className="markers">
                <div className="markersTitle">Provider Markers</div>
                {Object.keys(selected.providerMarkers).length === 0 ? (
                  <p className="muted">No provider markers found.</p>
                ) : (
                  <ul>
                    {Object.entries(selected.providerMarkers).map(([k, v]) => (
                      <li key={k}>
                        <span className="monospace">{k}</span>: {v}
                      </li>
                    ))}
                  </ul>
                )}
              </div>
            </div>
          ) : (
            <p className="muted">Select an audit record from history.</p>
          )}
        </section>
      </main>
    </div>
  )
}
