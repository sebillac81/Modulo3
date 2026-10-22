-- Funcionalidad 1: Catálogos (RF-01 a RF-06)
-- Alta y consulta de proveedores, cafés verdes, insumos, presentaciones y clientes.
-- No contemplan edición ni eliminación (Fuera de Alcance del PRD).

CREATE TABLE coffee_suppliers (
    id serial PRIMARY KEY,
    name varchar(200) NOT NULL
);

CREATE TABLE supply_suppliers (
    id serial PRIMARY KEY,
    name varchar(200) NOT NULL
);

CREATE TABLE green_coffees (
    id serial PRIMARY KEY,
    name varchar(200) NOT NULL,
    supplier_id integer NOT NULL REFERENCES coffee_suppliers(id),
    purchase_price_per_kg numeric(12,2) NOT NULL CHECK (purchase_price_per_kg > 0)
);

CREATE TABLE supplies (
    id serial PRIMARY KEY,
    name varchar(200) NOT NULL,
    supplier_id integer NOT NULL REFERENCES supply_suppliers(id),
    unit_price numeric(12,2) NOT NULL CHECK (unit_price > 0)
);

CREATE TABLE sale_presentations (
    id serial PRIMARY KEY,
    name varchar(200) NOT NULL,
    roasted_coffee_grams numeric(12,2) NOT NULL CHECK (roasted_coffee_grams > 0)
);

-- Insumos asociados a una presentación (cero o más, p.ej. bolsa + etiqueta).
CREATE TABLE sale_presentation_supplies (
    presentation_id integer NOT NULL REFERENCES sale_presentations(id) ON DELETE CASCADE,
    supply_id integer NOT NULL REFERENCES supplies(id),
    PRIMARY KEY (presentation_id, supply_id)
);

CREATE TABLE customers (
    id serial PRIMARY KEY,
    name varchar(200) NOT NULL
);
