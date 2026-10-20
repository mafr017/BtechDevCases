import { useEffect } from 'react'
import { clearSession, getSession, refresh } from './api'

const ACTIVITY_EVENTS = ['click', 'keydown', 'mousemove', 'scroll'] as const
const CHECK_INTERVAL_MS = 30_000

// While the user is active the token is refreshed (sliding 15-minute window).
// With no activity nothing refreshes it, so it expires and the user is logged out.
export function useSessionKeepAlive(onExpired: () => void) {
  useEffect(() => {
    let active = false
    const markActive = () => {
      active = true
    }
    ACTIVITY_EVENTS.forEach((e) => window.addEventListener(e, markActive))

    const timer = setInterval(() => {
      const session = getSession()
      if (!session || new Date(session.expiresAt) <= new Date()) {
        clearSession()
        onExpired()
        return
      }
      if (active) {
        active = false
        refresh().catch(() => {}) // a 401 already cleared the session; the next tick logs out
      }
    }, CHECK_INTERVAL_MS)

    return () => {
      clearInterval(timer)
      ACTIVITY_EVENTS.forEach((e) => window.removeEventListener(e, markActive))
    }
  }, [onExpired])
}
