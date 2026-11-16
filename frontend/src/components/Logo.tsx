type LogoProps = {
  size?: 'large' | 'small'
}

export default function Logo({ size = 'large' }: LogoProps) {
  return (
    <div className={`logo logo--${size}`} role="img" aria-label="LogistiCore Courier Service">
      <span className="logo__word">LOGISTICORE</span>
      <span className="logo__tag">COURIER SERVICE</span>
    </div>
  )
}
