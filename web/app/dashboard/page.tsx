export default function DashboardPage() {
  return (
    <div className='grid gap-4 md:grid-cols-3'>
      <div className='card p-5'>
        <div className='text-sm opacity-70'>Minutas</div>
        <div className='text-3xl font-bold'>—</div>
      </div>
      <div className='card p-5'>
        <div className='text-sm opacity-70'>Publicados</div>
        <div className='text-3xl font-bold'>—</div>
      </div>
      <div className='card p-5'>
        <div className='text-sm opacity-70'>Pendentes de assinatura</div>
        <div className='text-3xl font-bold'>—</div>
      </div>
    </div>
  )
}
