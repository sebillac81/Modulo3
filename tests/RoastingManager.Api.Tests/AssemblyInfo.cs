using Xunit;

// Los tests de integración comparten una única base de datos de test
// (roasting_manager_test) y cada clase trunca sus tablas antes de correr.
// Deben ejecutarse secuencialmente para evitar que un truncado pise datos
// de otra clase corriendo en paralelo.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
