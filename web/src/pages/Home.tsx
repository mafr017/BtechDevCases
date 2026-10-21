import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { clearSession, getCachedWelcome, getMe, isUnauthorized } from '../api'
import { useSessionKeepAlive } from '../useSessionKeepAlive'
import Wallet from '../Wallet'

function useOnline() {
  const [online, setOnline] = useState(navigator.onLine)
  useEffect(() => {
    const update = () => setOnline(navigator.onLine)
    window.addEventListener('online', update)
    window.addEventListener('offline', update)
    return () => {
      window.removeEventListener('online', update)
      window.removeEventListener('offline', update)
    }
  }, [])
  return online
}

export default function Home() {
  const navigate = useNavigate()
  const online = useOnline()
  const [message, setMessage] = useState(getCachedWelcome() ?? '')

  const logout = useCallback(() => {
    clearSession()
    navigate('/login', { replace: true })
  }, [navigate])

  useSessionKeepAlive(logout)

  useEffect(() => {
    if (!online) return
    getMe()
      .then((me) => setMessage(me.message))
      .catch((err) => {
        if (isUnauthorized(err)) logout()
      })
  }, [online, logout])

  return (
    <main>
      {!online && <p className="offline">You are offline. Showing the last saved data.</p>}
      <h1>{message || 'Loading…'}</h1>
      <Wallet online={online} onUnauthorized={logout} />
      <button onClick={logout}>Logout</button>
    </main>
  )
}
