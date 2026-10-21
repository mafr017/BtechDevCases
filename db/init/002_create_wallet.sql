ALTER TABLE users ADD COLUMN IF NOT EXISTS balance numeric(18, 2) NOT NULL DEFAULT 1000000.00;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_users_balance_non_negative') THEN
        ALTER TABLE users ADD CONSTRAINT ck_users_balance_non_negative CHECK (balance >= 0);
    END IF;
END $$;

CREATE TABLE IF NOT EXISTS transfers (
    id              uuid           PRIMARY KEY DEFAULT gen_random_uuid(),
    sender_id       uuid           NOT NULL REFERENCES users (id),
    recipient_id    uuid           NOT NULL REFERENCES users (id),
    amount          numeric(18, 2) NOT NULL CHECK (amount > 0),
    notes           varchar(200),
    idempotency_key uuid           NOT NULL,
    created_at      timestamptz    NOT NULL DEFAULT now(),
    CHECK (sender_id <> recipient_id)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_transfers_sender_idempotency_key ON transfers (sender_id, idempotency_key);
CREATE INDEX IF NOT EXISTS ix_transfers_sender_id ON transfers (sender_id);
CREATE INDEX IF NOT EXISTS ix_transfers_recipient_id ON transfers (recipient_id);
