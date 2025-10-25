export default function LoginPage() {
  return (
    <div className='mx-auto max-w-md space-y-6'>
      <header>
        <h1 className='text-2xl font-semibold text-neutral-900'>Acesso ao sistema</h1>
        <p className='text-sm text-neutral-500'>Autentique-se para continuar.</p>
      </header>
      <div className='card p-6'>
        <p className='text-sm text-neutral-600'>
          Em desenvolvimento, utilize um token de acesso para testar as APIs.
        </p>
        <div className='mt-4 space-y-2 text-sm'>
          <p>
            Crie o arquivo <code>.env.local</code> na raiz do projeto com a variável
            <code>NEXT_PUBLIC_DEV_BEARER</code> contendo o token.
          </p>
          <p className='text-neutral-500'>
            Em produção, este fluxo será substituído pelo provedor institucional.
          </p>
        </div>
        <div className='mt-6 flex gap-3'>
          <a href='/documents' className='btn-primary'>Entrar</a>
          <a href='/' className='btn-secondary'>Voltar</a>
        </div>
      </div>
    </div>
  )
}
