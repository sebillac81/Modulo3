using System.Reflection;
using DbUp;

// Cadena de conexión: variable de entorno RM_CONNECTION_STRING o, por defecto,
// la misma base levantada por docker-compose.yml en la raíz del repo.
var connectionString = Environment.GetEnvironmentVariable("RM_CONNECTION_STRING")
    ?? "Host=localhost;Port=5432;Database=roasting_manager;Username=roasting_manager;Password=roasting_manager";

var upgrader = DeployChanges.To
    .PostgresqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(result.Error);
    Console.ResetColor();
    return -1;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Migraciones aplicadas correctamente.");
Console.ResetColor();
return 0;
