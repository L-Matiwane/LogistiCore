export const money = (amount: number) => `R ${amount.toFixed(2)}`

export const kg = (value: number) => `${value.toFixed(2)} kg`

export function dateTime(iso: string): string {
  return new Date(iso).toLocaleString('en-ZA', { dateStyle: 'medium', timeStyle: 'short' })
}

export function statusLabel(status: string): string {
  return status.replace(/([a-z])([A-Z])/g, '$1 $2')
}

export const handlingLabels: Record<string, string> = {
  Fragile: 'Fragile',
  KeepUpright: 'Keep upright',
  Perishable: 'Perishable',
  SignatureRequired: 'Signature required',
}
