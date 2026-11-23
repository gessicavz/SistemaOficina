using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOficina.Data;
using SistemaOficina.Models;

namespace SistemaOficina.Controllers;

[Authorize(Roles = "Administrador")]
public class PendenciasController : Controller
{
    private readonly AppDbContext _context;
    public PendenciasController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index() => View(await _context.Pagamentos
        .Include(p => p.Funcionario)
        .Where(p => p.Situacao != SituacaoPagamento.Pago)
        .OrderBy(p => p.DataVencimento).ToListAsync());
}
