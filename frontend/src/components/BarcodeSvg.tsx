import { useEffect, useRef } from 'react'
import JsBarcode from 'jsbarcode'

export default function BarcodeSvg({ value }: { value: string }) {
  const ref = useRef<SVGSVGElement>(null)

  useEffect(() => {
    if (ref.current) {
      JsBarcode(ref.current, value, {
        format: 'CODE128',
        displayValue: false,
        height: 56,
        width: 2,
        margin: 0,
      })
    }
  }, [value])

  return <svg ref={ref} className="barcode" role="img" aria-label={`Barcode for ${value}`} />
}
