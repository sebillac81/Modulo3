using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace RoastingManager.Api.Domain;

public static class DbExceptionHelpers
{
    // SqlState 23505 = unique_violation en Postgres.
    public static bool IsUniqueViolation(this DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: "23505" };
}
