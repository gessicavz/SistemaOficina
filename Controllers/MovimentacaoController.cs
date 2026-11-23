using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOficina.Data;

namespace SistemaOficina.Controllers;

[Authorize(Roles = "Administrador")]
public class MovimentacaoController : Controller
{
    private readonly AppDbContext _context;
    public MovimentacaoController(AppDbContext context) => _context = context;

    // Histórico das operações (mais recentes primeiro), com filtro opcional por tipo
    public async Task<IActionResult> Index(string tipo)
    {
        var consulta = _context.Movimentacoes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(tipo)) consulta = consulta.Where(m => m.Tipo == tipo);
        ViewBag.Tipo = tipo;
        return View(await consulta.OrderByDescending(m => m.DataMovimentacao).Take(500).ToListAsync());
    }
}
