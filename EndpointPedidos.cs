using DesafioFinal.BancoDeDados;
using DesafioFinal.BancoDeDados.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.RegularExpressions;

namespace DesafioFinal
{
  public static class EndpointPedidos
  {
    public static void MapPedidosEndpoint(this WebApplication app)
    {
      app.MapPost("/pedidos/resumo", async (InMemoryContext context) =>
      {
        var totalPedidos = await context.Pedidos
              .GroupBy(p => new
              {
                YearMonth = p.order_date.Substring(0, 7)
              })
              .Select(g => new
              {
                g.Key.YearMonth,
                Total = Math.Round(g.Sum(p => p.total_amount), 2)
              })
              .OrderByDescending(g => g.YearMonth)
              .ToDictionaryAsync(g => g.YearMonth, g => g.Total);

        var topClientes = await context.Pedidos
        .GroupBy(p => p.customer_id)
        .Select(g => new
        {
          CustomerId = g.Key,
          TotalValue = Math.Round(g.Sum(p => p.total_amount), 2)
        })
        .OrderByDescending(g => g.TotalValue)
        .Take(10)
        .ToListAsync();

        var topClientesNomes = topClientes
        .Join(context.Clientes,
              pedido => pedido.CustomerId,
              cliente => cliente.customer_id,
              (pedido, cliente) => new
              {
                NomeCompleto = cliente.first_name
                  + " "
                  + cliente.last_name,
                TotalValue = pedido.TotalValue
              })
        .ToDictionary(k => k.NomeCompleto, v => v.TotalValue);

        var totalPedidosPorQuinzena = await context.Pedidos
              .GroupBy(p => new
              {
                YearMonth = p.order_date.Substring(0, 7)
              })
              .Select(g => new
              {
                g.Key.YearMonth,
                PrimeiraQuinzena = Math.Round(g.Where(p => int.Parse(p.order_date.Substring(8, 2)) <= 15)
                                     .Sum(p => p.total_amount), 2),
                SegundaQuinzena = Math.Round(g.Where(p => int.Parse(p.order_date.Substring(8, 2)) > 15)
                                     .Sum(p => p.total_amount), 2)
              })
              .OrderByDescending(g => g.YearMonth)
              .ToDictionaryAsync(g => g.YearMonth, g => new
              {
                primeira = g.PrimeiraQuinzena, segunda = g.SegundaQuinzena
              });

        return new
        {
          totalPedidos,
          topClientes = topClientesNomes,
          totalPedidosPorQuinzena
        };
      });

      app.MapPost("/pedidos/mais_comprados", async (InMemoryContext context) =>
      {
        var produtosMaisCompradosPorValor = await context.ItensDePedidos
            .GroupBy(i => i.product_id)
            .Select(g => new
            {
              Produto = g.Key,
              Quantidade = g.Sum(i => i.quantity),
              Valor = Math.Round(g.Sum(i => i.price * i.quantity), 2)
            })
            .OrderByDescending(g => g.Valor)
            .Take(30)
            .ToListAsync();

        var produtosMaisCompradosPorQuantidade = await context.ItensDePedidos
            .GroupBy(i => i.product_id)
            .Select(g => new
            {
              Produto = g.Key,
              Quantidade = g.Sum(i => i.quantity),
              Valor = Math.Round(g.Sum(i => i.price * i.quantity), 2)
            })
            .OrderByDescending(g => g.Quantidade)
            .Take(30)
            .ToListAsync();

        var produtosPorValor = produtosMaisCompradosPorValor
            .Join(context.Produtos,
                pedido => pedido.Produto,
                produto => produto.product_id,
                (pedido, produto) => new
                {
                  nome = produto.product_name,
                  categoria = context.Categorias.First
                  (
                    c => c.category_id == produto.category_id
                  ).category_name,
                  quantidade = pedido.Quantidade,
                  valor = $"R$ {pedido.Valor}"
                });

        var produtosPorQuantidade = produtosMaisCompradosPorQuantidade
            .Join(context.Produtos,
                pedido => pedido.Produto,
                produto => produto.product_id,
                (pedido, produto) => new
                {
                  nome = produto.product_name,
                  categoria = context.Categorias.First
                  (
                    c => c.category_id == produto.category_id
                  ).category_name,
                  quantidade = pedido.Quantidade,
                  valor = $"R$ {pedido.Valor}"
                });

        return new
        {
          produtosMaisCompradosPorValor = produtosPorValor,
          produtosMaisCompradosPorQuantidade = produtosPorQuantidade
        };
      });

      app.MapPost("/pedidos/mais_comprados_por_categoria", async (InMemoryContext context) =>
    {
      var itensPedidosProdutosCategorias = await context.ItensDePedidos
          .Join(context.Produtos,
                ip => ip.product_id,
                p => p.product_id,
                (ip, p) => new { ip, p })
          .Join(context.Categorias,
                x => x.p.category_id,
                c => c.category_id,
                (x, c) => new { x.ip, x.p, c })
          .ToListAsync();

      var produtosMaisCompradosPorCategoria = itensPedidosProdutosCategorias
          .GroupBy(x => x.c.category_name)
          .SelectMany(g =>
          {
            var produtosPorValor = g.GroupBy
            (
              x => x.p.product_name
            ).Select(pg => new
            {
              Nome = pg.Key,
              Quantidade = pg.Sum(x => x.ip.quantity),
              Valor = $"R$ {pg.Sum(x => x.ip.price * x.ip.quantity):N2}"
            })
            .OrderByDescending(p => p.Valor)
            .Take(30)
            .ToList();

            var produtosPorQuantidade = g.GroupBy
            (
              x => x.p.product_name
            ).Select(pg => new
            {
              Nome = pg.Key,
              Quantidade = pg.Sum(x => x.ip.quantity),
              Valor = $"R$ {pg.Sum(x => x.ip.price * x.ip.quantity):N2}"
            })
            .OrderByDescending(p => p.Quantidade)
            .Take(30)
            .ToList();

            return new[]
              {
                new
                {
                    Key = Regex.Replace(g.Key, @"[^a-zA-Z0-9]+", "") + "PorValor",
                    Value = produtosPorValor
                },
                new
                {
                    Key = Regex.Replace(g.Key, @"[^a-zA-Z0-9]+", "") + "PorQuantidade",
                    Value = produtosPorQuantidade
                }
              };
          }).ToDictionary(g => g.Key, g => g.Value);

      return produtosMaisCompradosPorCategoria;
    });

      app.MapPost("/pedidos/mais_comprados_por_fornecedor", async (InMemoryContext context) =>
    {
      var itensPedidosProdutosFornecedoresCategorias = await context.ItensDePedidos
          .Join(context.Produtos,
                ip => ip.product_id,
                p => p.product_id,
                (ip, p) => new { ip, p })
          .Join(context.Categorias,
                x => x.p.category_id,
                c => c.category_id,
                (x, c) => new { x.ip, x.p, c })
          .Join(context.Fornecedores,
                x => x.p.supplier_id,
                f => f.supplier_id,
                (x, f) => new { x.ip, x.p, x.c, f })
          .ToListAsync();

      var produtosMaisCompradosPorFornecedor = itensPedidosProdutosFornecedoresCategorias
          .GroupBy(x => x.f.supplier_name)
          .ToDictionary(
              g => Regex.Replace(g.Key, @"[^a-zA-Z0-9]+", ""),
              g => g.GroupBy(x => x.p.product_id)
                    .Select(pg => new
                    {
                      Nome = pg.First().p.product_name,
                      Categoria = pg.First().c.category_name,
                      Quantidade = pg.Sum(x => x.ip.quantity),
                      Valor = $"R$ {pg.Sum(x => x.ip.price * x.ip.quantity):N2}"
                    })
                    .OrderBy(p => p.Nome)
                    .Take(30)
                    .ToList()
          );

      return produtosMaisCompradosPorFornecedor;
    });

    }
  }
}
