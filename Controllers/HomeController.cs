using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOficina.Data;
using SistemaOficina.Models;

namespace SistemaOficina.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    public HomeController(AppDbContext context) => _context = context;

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var pagamentos = User.IsInRole("Administrador")
            ? await _context.Pagamentos.Include(p => p.Funcionario).ToListAsync()
            : new List<Pagamento>();
        foreach (var p in pagamentos.Where(p => p.Situacao == SituacaoPagamento.Pendente && p.DataVencimento.Date < DateTime.Today))
            p.Situacao = SituacaoPagamento.Atrasado;
        await _context.SaveChangesAsync();

        // Calcula e monta o ViewModel
        return View(new DashboardViewModel
        {
            FuncionariosAtivos = await _context.Funcionarios.CountAsync(f => f.Ativo),
            TotalPrevisto = pagamentos.Sum(p => p.Valor), //Soma todos os pagamentos
            TotalPago = pagamentos.Where(p => p.Situacao == SituacaoPagamento.Pago).Sum(p => p.Valor), //Filtra os pagos e soma
            TotalPendente = pagamentos.Where(p => p.Situacao == SituacaoPagamento.Pendente).Sum(p => p.Valor),
            TotalAtrasado = pagamentos.Where(p => p.Situacao == SituacaoPagamento.Atrasado).Sum(p => p.Valor),
            Equipamentos = await _context.Equipamentos.CountAsync(), //Qntd total de equipamentos
            EquipamentosEmManutencao = await _context.Equipamentos.CountAsync(e => e.Situacao == SituacaoEquipamento.Manutencao),
            PagamentosEmAberto = pagamentos.Count(p => p.Situacao != SituacaoPagamento.Pago),
            ProximosVencimentos = pagamentos.Where(p => p.Situacao != SituacaoPagamento.Pago && p.DataVencimento >= DateTime.Today).OrderBy(p => p.DataVencimento).Take(5).ToList()
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
