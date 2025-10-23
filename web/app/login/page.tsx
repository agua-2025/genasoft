export default function LoginPage() {
  return (
    <div className='card p-6 space-y-2'>
      <h1 className='text-2xl font-semibold'>Login (mock)</h1>
      <p className='text-sm opacity-80'>
        Para testar em dev, defina o token no arquivo <code>.env.local</code>:
      </p>
      <pre className='bg-neutral-900 border border-neutral-800 rounded-xl p-3 text-xs overflow-auto'>
NEXT_PUBLIC_API_BASE=http://localhost:5110
NEXT_PUBLIC_MOCK_BEARER=eyJhbGciOi...
      </pre>
      <p className='text-sm opacity-70'>
        Esse token precisa ter a claim <code>roles</code> (ex.: <code>Admin</code>, <code>Redator</code> ou <code>Autoridade</code>).
      </p>
    </div>
  )
}
