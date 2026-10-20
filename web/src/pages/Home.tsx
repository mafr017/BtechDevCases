import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { clearSession, getMe } from '../api'
import { useSessionKeepAlive } from '../useSessionKeepAlive'

export default function Home() {
  const navigate = useNavigate()
  const [message, setMessage] = useState('')

  const logout = useCallback(() => {
    clearSession()
    navigate('/login', { replace: true })
  }, [navigate])

  useSessionKeepAlive(logout)

  useEffect(() => {
    getMe()
      .then((me) => setMessage(me.message))
      .catch(logout)
  }, [logout])

  return (
    <main>
      <h1>{message || 'Loading…'}</h1>
      <button onClick={logout}>Logout</button>
    </main>
  )
}
