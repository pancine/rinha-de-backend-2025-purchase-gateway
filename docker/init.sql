CREATE UNLOGGED TABLE purchase (
    requested_at TIMESTAMP NOT NULL,
    payment_gateway_used int NOT NULL,
    amount DECIMAL NOT NULL
);

CREATE INDEX purchase_rapgu_idx ON purchase (requested_at, payment_gateway_used);