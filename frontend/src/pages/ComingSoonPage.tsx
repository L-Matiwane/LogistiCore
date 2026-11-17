export default function ComingSoonPage({ title }: { title: string }) {
  return (
    <div className="glass-card empty-card">
      <h1 className="page-title">
        {title}
        <span className="dot">.</span>
      </h1>
      <p className="muted">This module is planned for an upcoming sprint.</p>
    </div>
  )
}
