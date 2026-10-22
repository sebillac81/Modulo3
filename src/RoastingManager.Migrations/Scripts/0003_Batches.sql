-- Funcionalidad 2: Batches de tueste (RF-07 a RF-13)

-- RF-08/RF-10/RNF-02: el café verde tiene stock, que se fija al registrar el
-- catálogo (no hay gestión de compras en el alcance) y se decrementa al
-- registrar batches.
ALTER TABLE green_coffees
    ADD COLUMN stock_kg numeric(12,2) NOT NULL DEFAULT 0 CHECK (stock_kg >= 0);

-- RF-09/RF-13/RNF-02: pool único de café tostado (sin FIFO), con costo
-- promedio ponderado. Fila única.
CREATE TABLE roasted_coffee_stock (
    id integer PRIMARY KEY,
    quantity_kg numeric(12,2) NOT NULL DEFAULT 0 CHECK (quantity_kg >= 0),
    weighted_average_cost_per_kg numeric(12,2) NOT NULL DEFAULT 0,
    CONSTRAINT roasted_coffee_stock_single_row CHECK (id = 1)
);
INSERT INTO roasted_coffee_stock (id, quantity_kg, weighted_average_cost_per_kg)
VALUES (1, 0, 0);

-- RF-07/RF-11/RF-12: batch de tueste. Usa un único café verde (sin blends,
-- ver Fuera de Alcance). Precio de compra, merma y costo quedan fijos al
-- momento del registro (AC-12).
CREATE TABLE roast_batches (
    id serial PRIMARY KEY,
    green_coffee_id integer NOT NULL REFERENCES green_coffees(id),
    green_coffee_used_kg numeric(12,2) NOT NULL CHECK (green_coffee_used_kg > 0),
    roasted_coffee_obtained_kg numeric(12,2) NOT NULL CHECK (roasted_coffee_obtained_kg > 0),
    roast_date date NOT NULL,
    humidity_percentage numeric(5,2) NULL,
    density numeric(8,2) NULL,
    development_percentage numeric(5,2) NULL,
    purchase_price_per_kg_at_batch_time numeric(12,2) NOT NULL,
    shrinkage_percentage numeric(5,2) NOT NULL,
    cost_per_kg numeric(12,2) NOT NULL
);
