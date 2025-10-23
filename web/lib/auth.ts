export function getBearer() {
  const token = process.env.NEXT_PUBLIC_MOCK_BEARER
  return token ? `Bearer ${token}` : ''
}
