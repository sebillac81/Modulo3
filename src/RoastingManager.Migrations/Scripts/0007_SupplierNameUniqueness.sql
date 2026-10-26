-- Corrección: "Finca Kenia" y "FINCA KENIA" se registraban como dos
-- proveedores distintos. Un proveedor duplicado con otra capitalización es
-- casi siempre un error de tipeo sobre la misma entidad real (a diferencia
-- de un café verde o un cliente, donde un nombre repetido puede ser
-- legítimo). Único a nivel DB como defensa en profundidad; la validación
-- principal vive en los controllers.
CREATE UNIQUE INDEX coffee_suppliers_name_unique_ci ON coffee_suppliers (LOWER(name));
CREATE UNIQUE INDEX supply_suppliers_name_unique_ci ON supply_suppliers (LOWER(name));
