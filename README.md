# GenaSoft

Monorepo para os serviços da GenaSoft.

## Como rodar local
- API (.NET): após criar o projeto, execute dotnet restore, dotnet build e dotnet run. Utilize dotnet format antes de enviar alterações.
- Web (Next.js): após criar o aplicativo, instale dependências com 
pm install ou pnpm install e inicie com 
pm run dev. Rode 
pm run lint e 
pm run format antes de abrir PR.

## Estrutura de pastas
- pi/: hospedará os serviços e utilitários em .NET 8.
- web/: conterá a aplicação Next.js 14.
- Arquivos de configuração na raiz valem para o repositório inteiro.

## Boas práticas
- Mantenha formatação automática (dotnet format, 
pm run format) sempre atualizada.
- Acompanhe resultados dos linters e trate avisos antes de enviar.
- Escreva testes automatizados para regressões críticas.
- Não registre informações sensíveis ou PII em logs ou monitoramento.
- Abra issues para discutir mudanças arquiteturais antes de implementá-las.
