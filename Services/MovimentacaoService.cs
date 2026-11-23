using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SistemaOficina.Data;
using SistemaOficina.Models;

namespace SistemaOficina.Services;

/// <summary>Registra o histórico das operações feitas pelos usuários.</summary>
public class MovimentacaoService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _http;

    public MovimentacaoService(AppDbContext context, IHttpContextAccessor http)
    {
        _context = context;
        _http = http;
    }

    public async Task RegistrarAsync(string tipo, string descricao)
    {
        _context.Movimentacoes.Add(new Movimentacao
        {
            Tipo = tipo,
            Descricao = descricao.Length > 300 ? descricao[..300] : descricao,
            DataMovimentacao = DateTime.Now,
            Usuario = _http.HttpContext?.User?.Identity?.Name ?? "Sistema"
        });
        await _context.SaveChangesAsync();
    }
}
