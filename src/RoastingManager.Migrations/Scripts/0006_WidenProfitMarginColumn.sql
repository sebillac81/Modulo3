-- Corrección: una venta con precio válido (>0, AC-14b) pero muy bajo
-- frente a los costos genera un margen negativo de gran magnitud
-- (ej. -51150.00%), que desbordaba numeric(6,2) (máx. ±9999.99) y
-- rompía con un 500 sin control. profit_margin_percentage no tiene por
-- qué estar acotado a un rango "razonable": es la consecuencia aritmética
-- de precios de catálogo válidos, no un dato inválido en sí mismo.
ALTER TABLE sales
    ALTER COLUMN profit_margin_percentage TYPE numeric(14,2);
