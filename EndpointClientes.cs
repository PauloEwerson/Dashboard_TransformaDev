using DesafioFinal.BancoDeDados;
using DesafioFinal.BancoDeDados.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DesafioFinal
{
    public static class EndpointClientes
    {
        public static void MapClientesEndpoint(this WebApplication app)
        {
            app.MapPost("/clientes", async (InMemoryContext context) =>
            {
                var clientes = await context.Clientes
                    .OrderBy(c => c.first_name)
                    .ThenBy(c => c.last_name)
                    .Take(100)
                    .Select(c => new
                    {
                        nome_completo = c.first_name + " " + c.last_name,
                        email = c.email,
                        endereco = c.address,
                        cidade = c.city,
                        estado = c.state,
                        pais = c.country
                    })
                    .ToListAsync();

                return new { clientes };
            });

            app.MapPost("/clientes/resumo", async (InMemoryContext context) =>
            {
                var clientes = await context.Clientes.ToListAsync();

                var paisesComMaisClientes = clientes
                    .GroupBy(c => c.country == "-" ? "desconhecido" : c.country)
                    .ToDictionary(g => g.Key, g => g.Count());

                var dominios = clientes
                    .Select(c => c.email.Split('@').Last())
                    .GroupBy(d => d)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .ToDictionary(g => g.Key, g => g.Count());

                return new
                {
                    paisesComMaisClientes,
                    dominios
                };
            });

        }

    }
}
