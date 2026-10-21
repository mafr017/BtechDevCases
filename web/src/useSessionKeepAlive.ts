import { useEffect } from 'react'
import { clearSession, getSession, refresh } from './api'

const ACTIVITY_EVENTS = ['click', 'keydown', 'mousemove', 'scroll'] as const
const CHECK_INTERVAL_MS = 30_000

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
        refresh().catch(() => {
          active = true
        })
      }
    }, CHECK_INTERVAL_MS)

    return () => {
      clearInterval(timer)
      ACTIVITY_EVENTS.forEach((e) => window.removeEventListener(e, markActive))
    }
  }, [onExpired])
}
