interface StatCardProps {
  title: string
  value: number | string
}

export function StatCard({ title, value }: StatCardProps) {
  return (
    <div className="card p-4">
      <span className="text-xs uppercase tracking-wide text-neutral-500">{title}</span>
      <div className="mt-2 text-2xl font-semibold">{value}</div>
    </div>
  )
}