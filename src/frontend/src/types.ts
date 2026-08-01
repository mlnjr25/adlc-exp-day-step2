export type ConversionRequestPayload = {
  amount: number
  fromCurrency: string
  toCurrency: string
}

export type ConversionResponse = {
  convertedAmount: number
  rate: number
  auditId: string
  providerDate: string
  backendExecutionTimestampUtc: string
  providerMarkers: Record<string, string>
}

// API list returns the same shape as get-by-id.
export type ConversionListItem = ConversionResponse
