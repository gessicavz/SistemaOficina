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
public class AgruparController : Controller
{
    private readonly AppDbContext _context;
    public AgruparController(AppDbContext context) => _context = context;

    public async Task<IActionResult> PagamentosPorFuncionario()
    {
        var dados = await _context.Pagamentos
            .GroupBy(p => new { p.FuncionarioId, p.Funcionario.Nome })
            .Select(g => new PagamentoPorFuncionarioVM
            {
                Funcionario = g.Key.Nome,
                Total = g.Sum(p => p.Valor),
                Quantidade = g.Count(),
                Media = g.Average(p => p.Valor),
                Maior = g.Max(p => p.Valor),
                Menor = g.Min(p => p.Valor)
            })
            .OrderBy(x => x.Funcionario)
            .ToListAsync();
        return View(dados);
    }

    public async Task<IActionResult> PagamentosPorMesAno()
    {
        var pagamentos = await _context.Pagamentos.ToListAsync();
        var dados = pagamentos
            .GroupBy(p => new { p.AnoReferencia, p.MesReferencia })
            .Select(g => new PagamentoPorMesAnoVM
            {
                Ano = g.Key.AnoReferencia,
                Mes = g.Key.MesReferencia,
                Total = g.Sum(p => p.Valor),
                Quantidade = g.Count(),
                Media = g.Average(p => p.Valor)
            })
            .OrderBy(x => x.Ano).ThenBy(x => x.Mes)
            .ToList();
        return View(dados);
    }


    public async Task<IActionResult> EquipamentosPorFuncionario()
    {
        var dados = await _context.Equipamentos
            .GroupBy(e => new { e.FuncionarioId, e.Funcionario.Nome })
            .Select(g => new EquipamentoAgrupadoVM
            {
                Grupo = g.Key.Nome,
                Quantidade = g.Count(),
                EmManutencao = g.Count(e => e.Situacao == SituacaoEquipamento.Manutencao),
                Total = g.Sum(e => e.ValorAquisicao)
            })
            .OrderBy(x => x.Grupo)
            .ToListAsync();
        ViewBag.Titulo = "Equipamentos por técnico responsável";
        ViewBag.ColunaGrupo = "Técnico";
        return View("Equipamentos", dados);
    }

    public async Task<IActionResult> EquipamentosPorTipo()
    {
        var dados = await _context.Equipamentos
            .GroupBy(e => e.Tipo)
            .Select(g => new EquipamentoAgrupadoVM
            {
                Grupo = g.Key,
                Quantidade = g.Count(),
                EmManutencao = g.Count(e => e.Situacao == SituacaoEquipamento.Manutencao),
                Total = g.Sum(e => e.ValorAquisicao)
            })
            .OrderBy(x => x.Grupo)
            .ToListAsync();
        ViewBag.Titulo = "Equipamentos por tipo";
        ViewBag.ColunaGrupo = "Tipo";
        return View("Equipamentos", dados);
    }
}
