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
using SistemaOficina.Services;

namespace SistemaOficina.Controllers;

[Authorize(Roles = "Administrador")]
public class PagamentosController : Controller
{
    private readonly AppDbContext _context;
    private readonly MovimentacaoService _mov;
    public PagamentosController(AppDbContext context, MovimentacaoService mov)
    {
        _context = context;
        _mov = mov;
    }

    private void CarregarFuncionarios(int? selecionado = null) =>
        ViewBag.FuncionarioId = new SelectList(_context.Funcionarios.Where(f => f.Ativo || f.Id == selecionado).OrderBy(f => f.Nome), "Id", "Nome", selecionado);

    private async Task AtualizarAtrasados()
    {
        var atrasados = await _context.Pagamentos.Where(p => p.Situacao == SituacaoPagamento.Pendente && p.DataVencimento < DateTime.Today).ToListAsync();
        foreach (var item in atrasados) item.Situacao = SituacaoPagamento.Atrasado;
        if (atrasados.Count > 0) await _context.SaveChangesAsync();
    }

    private static SituacaoPagamento CalcularSituacao(Pagamento p) =>
        p.DataPagamento.HasValue ? SituacaoPagamento.Pago :
        p.DataVencimento.Date < DateTime.Today ? SituacaoPagamento.Atrasado : SituacaoPagamento.Pendente;

    private static string Descrever(Pagamento p, string funcionario) =>
        $"pagamento de {funcionario} ({p.Competencia}) - {p.Valor:C}";

    // Filtros: funcionário, mês, ano, período de vencimento e situação
    public async Task<IActionResult> Index(int? funcionarioId, int? mes, int? ano, DateTime? inicio, DateTime? fim, string situacao)
    {
        await AtualizarAtrasados();
        var consulta = _context.Pagamentos.Include(p => p.Funcionario).AsQueryable();
        if (funcionarioId.HasValue) consulta = consulta.Where(p => p.FuncionarioId == funcionarioId);
        if (mes.HasValue && ano.HasValue) { var comp = $"{mes:00}/{ano}"; consulta = consulta.Where(p => p.Competencia == comp); }
        else if (mes.HasValue) { var prefixo = $"{mes:00}/"; consulta = consulta.Where(p => p.Competencia.StartsWith(prefixo)); }
        else if (ano.HasValue) { var sufixo = $"/{ano}"; consulta = consulta.Where(p => p.Competencia.EndsWith(sufixo)); }
        if (inicio.HasValue) consulta = consulta.Where(p => p.DataVencimento >= inicio.Value);
        if (fim.HasValue) consulta = consulta.Where(p => p.DataVencimento <= fim.Value);
        if (Enum.TryParse<SituacaoPagamento>(situacao, out var filtro)) consulta = consulta.Where(p => p.Situacao == filtro);

        ViewBag.Funcionarios = new SelectList(_context.Funcionarios.OrderBy(f => f.Nome), "Id", "Nome", funcionarioId);
        ViewBag.Mes = mes;
        ViewBag.Ano = ano;
        ViewBag.Inicio = inicio?.ToString("yyyy-MM-dd");
        ViewBag.Fim = fim?.ToString("yyyy-MM-dd");
        ViewBag.Situacao = situacao;
        return View(await consulta.OrderBy(p => p.DataVencimento).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Pagamentos.Include(p => p.Funcionario).FirstOrDefaultAsync(p => p.Id == id);
        return item == null ? NotFound() : View(item);
    }

    public IActionResult Create()
    {
        CarregarFuncionarios();
        return View(new Pagamento { Competencia = DateTime.Today.ToString("MM/yyyy") });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Pagamento pagamento)
    {
        if (!ModelState.IsValid) { CarregarFuncionarios(pagamento.FuncionarioId); return View(pagamento); }
        pagamento.Situacao = CalcularSituacao(pagamento);
        _context.Add(pagamento);
        await _context.SaveChangesAsync();
        var nome = (await _context.Funcionarios.FindAsync(pagamento.FuncionarioId))?.Nome;
        await _mov.RegistrarAsync("Cadastro", "Registro de " + Descrever(pagamento, nome));
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Pagamentos.FindAsync(id);
        if (item == null) return NotFound();
        CarregarFuncionarios(item.FuncionarioId);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Pagamento pagamento)
    {
        if (id != pagamento.Id) return NotFound();
        if (!ModelState.IsValid) { CarregarFuncionarios(pagamento.FuncionarioId); return View(pagamento); }
        pagamento.Situacao = CalcularSituacao(pagamento);
        _context.Update(pagamento);
        await _context.SaveChangesAsync();
        var nome = (await _context.Funcionarios.FindAsync(pagamento.FuncionarioId))?.Nome;
        await _mov.RegistrarAsync("Atualização", "Atualização de " + Descrever(pagamento, nome));
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarPago(int id)
    {
        var item = await _context.Pagamentos.Include(p => p.Funcionario).FirstOrDefaultAsync(p => p.Id == id);
        if (item != null)
        {
            item.Situacao = SituacaoPagamento.Pago;
            item.DataPagamento = DateTime.Today;
            await _context.SaveChangesAsync();
            await _mov.RegistrarAsync("Atualização", "Baixa de " + Descrever(item, item.Funcionario?.Nome));
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Pagamentos.Include(p => p.Funcionario).FirstOrDefaultAsync(p => p.Id == id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Pagamentos.Include(p => p.Funcionario).FirstOrDefaultAsync(p => p.Id == id);
        if (item != null)
        {
            _context.Remove(item);
            await _context.SaveChangesAsync();
            await _mov.RegistrarAsync("Exclusão", "Exclusão de " + Descrever(item, item.Funcionario?.Nome));
        }
        return RedirectToAction(nameof(Index));
    }
}
