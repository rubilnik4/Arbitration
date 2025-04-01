CREATE TABLE IF NOT EXISTS prices (
    id UUID PRIMARY KEY,
    asset TEXT NOT NULL,
    price NUMERIC NOT NULL,
    time TIMESTAMPTZ NOT NULL,
    UNIQUE (asset, time)
    );

CREATE INDEX IF NOT EXISTS idx_prices_asset_time ON prices (asset, time DESC);

-- spread key — нормализованная пара
CREATE TABLE IF NOT EXISTS spreads (
    id UUID PRIMARY KEY,
    price_a_id UUID NOT NULL REFERENCES prices(id),
    price_b_id UUID NOT NULL REFERENCES prices(id),
    asset_spread_id TEXT NOT NULL,
    spread_value NUMERIC NOT NULL,
    spread_time TIMESTAMPTZ NOT NULL
    UNIQUE (asset_spread_id, spread_time)
    );

CREATE INDEX IF NOT EXISTS idx_spreads_unique ON spreads (asset_spread_id, spread_time);

