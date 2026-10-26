using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoastingManager.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<RoastingManagerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("RoastingManager"))
           .UseSnakeCaseNamingConvention());

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Cualquier excepción no manejada por un controller se convierte en un
// ProblemDetails limpio, nunca en un 500 con stack trace expuesto. Una
// DbUpdateException (overflow numérico, constraint violada, etc.) es un
// dato de entrada inválido que no llegamos a validar antes de guardar, no
// un error del servidor: se reporta como 400.
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var statusCode = exception is DbUpdateException
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode == StatusCodes.Status400BadRequest
                ? "No se pudo completar la operación: verificá los valores ingresados."
                : "Ocurrió un error inesperado.",
        });
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Expuesta para que RoastingManager.Api.Tests pueda usar WebApplicationFactory<Program>.
public partial class Program { }
