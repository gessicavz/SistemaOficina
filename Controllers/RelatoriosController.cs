using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaOficina.Data;
using SistemaOficina.Models;

namespace SistemaOficina.Controllers;

[Authorize(Roles = "Administrador")]
public class RelatoriosController : Controller
{
    private readonly AppDbContext _context;
    public RelatoriosController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index(int? funcionarioId, DateTime? inicio, DateTime? fim, string situacao)
    {
        var consulta = _context.Pagamentos.Include(p => p.Funcionario).AsQueryable();
        if (funcionarioId.HasValue) consulta = consulta.Where(p => p.FuncionarioId == funcionarioId);
        if (inicio.HasValue) consulta = consulta.Where(p => p.DataVencimento >= inicio.Value);
        if (fim.HasValue) consulta = consulta.Where(p => p.DataVencimento <= fim.Value);
        if (Enum.TryParse<SituacaoPagamento>(situacao, out var status)) consulta = consulta.Where(p => p.Situacao == status);
        ViewBag.Funcionarios = new SelectList(_context.Funcionarios.OrderBy(f => f.Nome), "Id", "Nome", funcionarioId);
        ViewBag.Inicio = inicio?.ToString("yyyy-MM-dd");
        ViewBag.Fim = fim?.ToString("yyyy-MM-dd");
        ViewBag.Situacao = situacao;
        return View(await consulta.OrderBy(p => p.DataVencimento).ToListAsync());
    }
}
