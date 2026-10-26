-- Corrección: el tueste siempre pierde masa, nunca gana. Sin esta
-- restricción, un batch con más café tostado obtenido que verde utilizado
-- generaba merma negativa y un costo/kg falso que contaminaba el costo
-- promedio ponderado del pool (y, con él, el margen de todas las ventas
-- posteriores). La validación principal vive en el controller
-- (RoastBatchesController); este CHECK es defensa en profundidad.
ALTER TABLE roast_batches
    ADD CONSTRAINT roast_batches_merma_no_negativa
    CHECK (roasted_coffee_obtained_kg <= green_coffee_used_kg);
