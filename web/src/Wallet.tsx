import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { getCachedWallet, getWallet, isNetworkError, isUnauthorized, transfer, type CachedWallet } from './api'

type Pending = { key: string; recipient: string; amount: string; notes: string }

const formatMoney = (value: number) =>
  value.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })

export default function Wallet({ online, onUnauthorized }: { online: boolean; onUnauthorized: () => void }) {
  const [wallet, setWallet] = useState<CachedWallet | null>(getCachedWallet)
  const [loadError, setLoadError] = useState('')
  const [recipient, setRecipient] = useState('')
  const [amount, setAmount] = useState('')
  const [notes, setNotes] = useState('')
  const [pending, setPending] = useState<Pending | null>(null)
  const [sending, setSending] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const fail = useCallback(
    (err: unknown, show: (message: string) => void) => {
      if (isUnauthorized(err)) onUnauthorized()
      else show((err as Error).message)
    },
    [onUnauthorized],
  )

  useEffect(() => {
    if (!online) return
    getWallet()
      .then((w) => {
        setWallet(w)
        setLoadError('')
      })
      .catch((err) => fail(err, setLoadError))
  }, [online, fail])

  async function submit(t: Pending) {
    setSending(true)
    setError('')
    setSuccess('')
    try {
      setWallet(await transfer(t.recipient, Number(t.amount), t.notes, t.key))
      setPending(null)
      setRecipient('')
      setAmount('')
      setNotes('')
      setSuccess(`Sent ${formatMoney(Number(t.amount))} to ${t.recipient}.`)
    } catch (err) {
      if (isNetworkError(err)) {
        setPending(t)
        setError('Connection lost, the transfer may not have gone through. Retrying is safe: it is never sent twice.')
      } else {
        setPending(null)
        fail(err, setError)
      }
    } finally {
      setSending(false)
    }
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    submit(pending ?? { key: crypto.randomUUID(), recipient, amount, notes })
  }

  function cancelPending() {
    setPending(null)
    setError('')
  }

  const locked = sending || pending !== null

  return (
    <section>
      <h2>Wallet</h2>
      <p className="balance">{wallet ? formatMoney(wallet.balance) : '…'}</p>
      {wallet && <p className="muted">Updated {new Date(wallet.updatedAt).toLocaleString()}</p>}
      {loadError && <p className="error">{loadError}</p>}

      <form onSubmit={handleSubmit}>
        <h3>Transfer</h3>
        <input type="email" placeholder="Recipient email" value={recipient} onChange={(e) => setRecipient(e.target.value)} disabled={locked} required />
        <input type="number" placeholder="Amount" min="0.01" step="0.01" value={amount} onChange={(e) => setAmount(e.target.value)} disabled={locked} required />
        <input placeholder="Notes (optional)" maxLength={200} value={notes} onChange={(e) => setNotes(e.target.value)} disabled={locked} />
        {error && <p className="error">{error}</p>}
        {success && <p className="success">{success}</p>}
        <button type="submit" disabled={sending}>
          {sending ? 'Sending…' : pending ? 'Retry transfer' : 'Send'}
        </button>
        {pending && !sending && (
          <button type="button" onClick={cancelPending}>
            Cancel
          </button>
        )}
      </form>

      <h3>History</h3>
      {wallet?.transfers.length ? (
        <ul className="history">
          {wallet.transfers.map((t) => (
            <li key={t.id}>
              <strong>
                {t.direction === 'sent' ? '−' : '+'}
                {formatMoney(t.amount)}
              </strong>{' '}
              {t.direction === 'sent' ? 'to' : 'from'} {t.counterparty}
              {t.notes && <> · {t.notes}</>}
              <br />
              <span className="muted">{new Date(t.createdAt).toLocaleString()}</span>
            </li>
          ))}
        </ul>
      ) : (
        <p className="muted">No transfers yet.</p>
      )}
    </section>
  )
}
