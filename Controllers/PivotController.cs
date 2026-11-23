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
public class PivotController : Controller
{
    private readonly AppDbContext _context;
    public PivotController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index(int? ano)
    {
        var pagamentos = await _context.Pagamentos.Include(p => p.Funcionario).ToListAsync();
        var anos = pagamentos.Select(p => p.AnoReferencia).Where(a => a > 0).Distinct().OrderBy(a => a).ToList();
        int anoSelecionado = ano ?? (anos.Contains(DateTime.Today.Year) ? DateTime.Today.Year : (anos.Count > 0 ? anos.Last() : DateTime.Today.Year));

        var linhas = pagamentos
            .Where(p => p.AnoReferencia == anoSelecionado && p.MesReferencia is >= 1 and <= 12)
            .GroupBy(p => p.Funcionario.Nome)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var linha = new PivotLinhaVM { Funcionario = g.Key };
                foreach (var p in g) linha.Meses[p.MesReferencia - 1] += p.Valor;
                return linha;
            })
            .ToList();

        ViewBag.Anos = new SelectList(anos.Count > 0 ? anos : new List<int> { anoSelecionado }, anoSelecionado);
        ViewBag.Ano = anoSelecionado;
        return View(linhas);
    }
}
