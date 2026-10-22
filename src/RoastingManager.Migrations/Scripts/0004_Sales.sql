-- Funcionalidad 3: Ventas (RF-14 a RF-18)

-- RF-14/RF-17/RF-18: venta de café tostado. roasted_coffee_kg_sold,
-- roasted_coffee_cost_per_kg_at_sale_time, associated_costs y
-- profit_margin_percentage quedan fijos al momento del registro (AC-17,
-- y por decisión consistente con AC-12, también AC-18): un reporte
-- histórico no cambia porque después cambie el costo del pool.
CREATE TABLE sales (
    id serial PRIMARY KEY,
    customer_id integer NOT NULL REFERENCES customers(id),
    presentation_id integer NOT NULL REFERENCES sale_presentations(id),
    units_sold integer NOT NULL CHECK (units_sold > 0),
    unit_sale_price numeric(12,2) NOT NULL CHECK (unit_sale_price > 0),
    sale_date date NOT NULL,
    roasted_coffee_kg_sold numeric(12,2) NOT NULL,
    roasted_coffee_cost_per_kg_at_sale_time numeric(12,2) NOT NULL,
    associated_costs numeric(12,2) NOT NULL,
    profit_margin_percentage numeric(6,2) NOT NULL
);
