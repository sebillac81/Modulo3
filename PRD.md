# PRD-001: RoastingManager — Gestión de Tuestes y Stock de Café

## Contexto y Problema

Actualmente no existe una forma centralizada de registrar el costo de compra del café verde, controlar el stock disponible y calcular el costo real del café tostado.

Durante el proceso de tueste se produce una merma natural, por lo que el costo del café tostado no depende únicamente del precio de compra del café verde, sino también de otros costos asociados como bolsas, etiquetas y materiales de empaque.

El objetivo de la aplicación es centralizar esta información para conocer en todo momento el stock disponible, el costo de producción de cada batch y la rentabilidad de cada venta.

### Personas

**Sebastián (Administrador)**

* Registra proveedores, cafés verdes, insumos y presentaciones.
* Registra ventas de café tostado a clientes.
* Analiza costos y rentabilidad.

**Tostador**

* Registra los batches de tueste.
* Selecciona el café a utilizar.
* Indica la cantidad de café verde utilizada y la cantidad de café tostado obtenida.
* Registra ventas de café tostado a clientes.


---

## Objetivos

* Mantener actualizado el stock de café verde y café tostado.
* Registrar los batches realizados.
* Registrar las ventas de café tostado realizadas a los clientes.
* Calcular automáticamente la merma de cada tueste.
* Calcular el costo del café tostado considerando materia prima y costos asociados.
* Mostrar el margen de ganancia según el precio de venta definido por el usuario.

---

## Requerimientos Funcionales

### RF-01 Registrar proveedores de café

El sistema deberá permitir registrar proveedores de café.

---

### RF-02 Registrar proveedores de insumos

El sistema deberá permitir registrar proveedores de insumos.

---

### RF-03 Registrar cafés verdes

El sistema deberá permitir registrar cafés verdes, indicando entre sus datos el precio de compra por kilogramo.

---

### RF-04 Registrar insumos

El sistema deberá permitir registrar insumos, indicando su precio por unidad.

---

### RF-05 Registrar presentaciones de venta

El sistema deberá permitir registrar presentaciones de venta, indicando la cantidad de café tostado que representa cada unidad y los insumos asociados (por ejemplo, tipo de bolsa y, opcionalmente, etiqueta).

---

### RF-06 Registrar clientes

El sistema deberá permitir registrar clientes.

---

### RF-07 Registrar batch de tueste

El sistema deberá permitir registrar un batch indicando:

* Café verde utilizado.
* Cantidad de café verde.
* Cantidad de café tostado obtenida.
* Fecha del tueste.
* Datos relevantes del proceso (por ejemplo humedad, densidad y porcentaje de desarrollo).

---

### RF-08 Descontar stock de café verde al registrar un batch

Al registrar un batch, el sistema deberá descontar automáticamente la cantidad utilizada del stock de café verde.

---

### RF-09 Incrementar stock de café tostado al registrar un batch

Al registrar un batch, el sistema deberá incrementar el stock de café tostado obtenido.

---

### RF-10 Impedir registrar un batch sin stock suficiente

El sistema deberá impedir registrar un batch cuando no exista stock suficiente de café verde.

---

### RF-11 Calcular el porcentaje de merma del batch

El sistema deberá calcular automáticamente el porcentaje de merma del batch.

---

### RF-12 Calcular el costo del café tostado

El sistema deberá calcular automáticamente el costo del café tostado considerando la merma.

---

### RF-13 Actualizar el costo promedio ponderado del café tostado al registrar un batch

Al registrar un batch, el sistema deberá recalcular automáticamente el costo promedio ponderado del stock de café tostado, combinando el costo del stock existente con el costo del café tostado obtenido en el nuevo batch.

---

### RF-14 Registrar venta de café tostado

El sistema deberá permitir registrar una venta indicando:

* Cliente.
* Presentación de venta.
* Cantidad de unidades vendidas.
* Precio de venta.
* Fecha de la venta.

---

### RF-15 Descontar stock de café tostado al registrar una venta

Al registrar una venta, el sistema deberá descontar del stock de café tostado la cantidad correspondiente, según la presentación y la cantidad de unidades vendidas.

---

### RF-16 Impedir registrar una venta sin stock de café tostado suficiente

El sistema deberá impedir registrar una venta cuando no exista stock suficiente de café tostado.

---

### RF-17 Calcular los costos asociados a una venta

El sistema deberá calcular automáticamente los costos asociados a una venta, según los insumos definidos en la presentación utilizada (por ejemplo bolsas y etiquetas).

---

### RF-18 Calcular el margen de ganancia de una venta

El sistema deberá calcular automáticamente el margen de ganancia de una venta, considerando el costo del café tostado, los costos asociados y el precio de venta definido por el usuario.

---

### RF-19 Consultar stock disponible

El sistema deberá permitir consultar el stock disponible de cada café.

---

### RF-20 Consultar historial de batches

El sistema deberá permitir consultar el historial de batches realizados.

---

### RF-21 Consultar historial de ventas

El sistema deberá permitir consultar el historial de ventas realizadas.

---

### RF-22 Consultar costo de producción por batch

El sistema deberá permitir consultar el costo de producción del café tostado de cada batch.

---

### RF-23 Consultar costos asociados de una venta

El sistema deberá permitir consultar los costos asociados de cada venta.

---

### RF-24 Consultar rentabilidad de una venta

El sistema deberá permitir consultar la rentabilidad de cada venta.

---

## Requerimientos No Funcionales

* RNF-01: El tiempo de respuesta para las operaciones principales deberá ser inferior a 3 segundos en el percentil 95 (p95), bajo una carga de hasta 5 usuarios concurrentes.
* RNF-02: El stock de café verde y de café tostado nunca deberá ser menor a 0 (cero).

---

## Restricciones Técnicas

* RT-01: El sistema deberá almacenar la información en una base de datos PostgreSQL.

---

## Criterios de Aceptación

### AC-01 (RF-01) Registrar proveedor de café

**Dado** que el usuario ingresa los datos de un proveedor de café,

**Cuando** el usuario guarda el registro,

**Entonces** el sistema deberá registrar el proveedor de café.

---

### AC-02 (RF-02) Registrar proveedor de insumos

**Dado** que el usuario ingresa los datos de un proveedor de insumos,

**Cuando** el usuario guarda el registro,

**Entonces** el sistema deberá registrar el proveedor de insumos.

---

### AC-03 (RF-03) Registrar café verde

**Dado** que el usuario ingresa los datos de un café verde, incluyendo su precio de compra por kilogramo,

**Cuando** el usuario guarda el registro,

**Entonces** el sistema deberá registrar el café verde.

---

### AC-04 (RF-04) Registrar insumo

**Dado** que el usuario ingresa los datos de un insumo, incluyendo su precio por unidad,

**Cuando** el usuario guarda el registro,

**Entonces** el sistema deberá registrar el insumo.

---

### AC-05 (RF-05) Registrar presentación de venta

**Dado** que el usuario ingresa los datos de una presentación de venta, incluyendo la cantidad de café tostado que representa y los insumos asociados,

**Cuando** el usuario guarda el registro,

**Entonces** el sistema deberá registrar la presentación de venta.

---

### AC-06 (RF-06) Registrar cliente

**Dado** que el usuario ingresa los datos de un cliente,

**Cuando** el usuario guarda el registro,

**Entonces** el sistema deberá registrar el cliente.

---

### AC-07 (RF-07) Registrar batch de tueste

**Dado** que existe stock suficiente de un café verde,

**Cuando** el tostador registra un nuevo batch indicando café verde, cantidades, fecha y datos del proceso,

**Entonces** el sistema deberá guardar el batch con los datos ingresados.

---

### AC-08 (RF-08) Descontar stock de café verde al registrar un batch

**Dado** que existe stock suficiente de un café verde,

**Cuando** el tostador registra un nuevo batch,

**Entonces** el sistema deberá descontar del stock de café verde la cantidad utilizada.

---

### AC-09 (RF-09) Incrementar stock de café tostado al registrar un batch

**Dado** que el tostador registra un nuevo batch,

**Cuando** el batch se guarda,

**Entonces** el sistema deberá incrementar el stock de café tostado en la cantidad obtenida.

---

### AC-10 (RF-10) Impedir registrar un batch sin stock suficiente

**Dado** que el stock disponible de café verde es menor a la cantidad que se desea tostar,

**Cuando** el usuario intenta registrar el batch,

**Entonces** el sistema deberá impedir la operación e informar que no existe stock suficiente.

---

### AC-11 (RF-11) Calcular el porcentaje de merma del batch

**Dado** un batch con cantidad de café verde y cantidad de café tostado,

**Cuando** el batch es registrado,

**Entonces** el sistema deberá calcular automáticamente el porcentaje de merma.

---

### AC-12 (RF-12) Calcular el costo del café tostado

**Dado** un batch con su cantidad de café verde utilizada, el precio de compra del café verde y la cantidad de café tostado obtenida,

**Cuando** el batch es registrado,

**Entonces** el sistema deberá calcular el costo del café tostado como (precio de compra del café verde × cantidad de café verde utilizada) / cantidad de café tostado obtenida.

---

### AC-13 (RF-13) Actualizar el costo promedio ponderado del café tostado

**Dado** un stock de café tostado existente con su costo promedio ponderado, y un nuevo batch con su cantidad y costo de café tostado,

**Cuando** el batch es registrado,

**Entonces** el sistema deberá recalcular el costo promedio ponderado como ((stock existente × costo promedio actual) + (cantidad del nuevo batch × costo del nuevo batch)) / (stock existente + cantidad del nuevo batch).

---

### AC-14 (RF-14) Registrar venta de café tostado

**Dado** que existe stock suficiente de café tostado,

**Cuando** el usuario registra una venta indicando cliente, presentación, cantidad de unidades, precio de venta y fecha,

**Entonces** el sistema deberá guardar la venta con los datos ingresados.

---

### AC-15 (RF-15) Descontar stock de café tostado al registrar una venta

**Dado** que existe stock suficiente de café tostado,

**Cuando** el usuario registra una venta,

**Entonces** el sistema deberá descontar del stock de café tostado la cantidad correspondiente a la presentación y cantidad vendida.

---

### AC-16 (RF-16) Impedir registrar una venta sin stock de café tostado suficiente

**Dado** que el stock disponible de café tostado es menor a la cantidad que se desea vender,

**Cuando** el usuario intenta registrar la venta,

**Entonces** el sistema deberá impedir la operación e informar que no existe stock suficiente.

---

### AC-17 (RF-17) Calcular los costos asociados a una venta

**Dado** una venta con su cantidad de unidades vendidas y la presentación utilizada, con sus insumos y precio unitario,

**Cuando** el usuario consulta la información de la venta,

**Entonces** el sistema deberá calcular los costos asociados como la cantidad de unidades vendidas multiplicada por la suma de los precios unitarios de los insumos de la presentación.

---

### AC-18 (RF-18) Calcular el margen de ganancia de una venta

**Dado** el costo promedio ponderado del café tostado vigente, la cantidad de café tostado vendida, los costos asociados y el precio de venta de una venta,

**Cuando** el usuario consulta la información de la venta,

**Entonces** el sistema deberá calcular el margen de ganancia como ((precio de venta × cantidad de unidades vendidas) − (costo promedio ponderado × cantidad de café tostado vendida + costos asociados)) / (precio de venta × cantidad de unidades vendidas) × 100.

---

### AC-19 (RF-19) Consultar stock disponible

**Dado** que existen cafés registrados con stock,

**Cuando** el usuario consulta el stock,

**Entonces** el sistema deberá mostrar el stock disponible de cada café.

---

### AC-20 (RF-20) Consultar historial de batches

**Dado** que existen batches registrados,

**Cuando** el usuario consulta el historial,

**Entonces** el sistema deberá mostrar el listado de batches realizados.

---

### AC-21 (RF-21) Consultar historial de ventas

**Dado** que existen ventas registradas,

**Cuando** el usuario consulta el historial,

**Entonces** el sistema deberá mostrar el listado de ventas realizadas.

---

### AC-22 (RF-22) Consultar costo de producción por batch

**Dado** un batch registrado con su costo de producción calculado,

**Cuando** el usuario consulta la información del batch,

**Entonces** el sistema deberá mostrar el costo de producción del café tostado obtenido.

---

### AC-23 (RF-23) Consultar costos asociados de una venta

**Dado** una venta registrada con sus costos asociados calculados,

**Cuando** el usuario consulta la información de la venta,

**Entonces** el sistema deberá mostrar los costos asociados de la venta.

---

### AC-24 (RF-24) Consultar rentabilidad de una venta

**Dado** una venta registrada con su margen de ganancia calculado,

**Cuando** el usuario consulta la información de la venta,

**Entonces** el sistema deberá mostrar la rentabilidad de la venta.

---

## Fuera de Alcance

Esta primera versión no contempla:

* Gestión comercial (CRM).
* Facturación.
* Comercio electrónico.
* Gestión de compras (órdenes de compra, historial de compras). El precio de compra registrado en café verde e insumos (RF-03, RF-04) es un atributo de catálogo, no una transacción de compra.
* Soporte para múltiples sucursales o empresas (multitenancy).
* Control de acceso por rol (restricción de datos/funciones entre Administrador y Tostador). Ambas personas operan sobre la misma información sin restricciones de visibilidad.
* Inventario de café tostado por lote (FIFO). El costeo del stock de café tostado se maneja como pool único mediante costo promedio ponderado (RF-13).

---

## Riesgos y Dependencias

* Disponibilidad de una base de datos PostgreSQL.
* Correcta carga de los costos e insumos para obtener cálculos de rentabilidad precisos.
