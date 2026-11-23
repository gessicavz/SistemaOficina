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

[Authorize]
public class EquipamentosController : Controller
{
    private readonly AppDbContext _context;
    private readonly MovimentacaoService _mov;
    public EquipamentosController(AppDbContext context, MovimentacaoService mov)
    {
        _context = context;
        _mov = mov;
    }

    private void CarregarListas(int? responsavel = null)
    {
        ViewBag.FuncionarioId = new SelectList(_context.Funcionarios.Where(f => f.Ativo || f.Id == responsavel).OrderBy(f => f.Nome), "Id", "Nome", responsavel);
        ViewBag.Tipos = new SelectList(TiposEquipamento.Lista);
        ViewBag.Situacoes = Enum.GetValues<SituacaoEquipamento>().Select(s => new SelectListItem(s.Texto(), s.ToString())).ToList();
    }

    private static string Descrever(Equipamento e) => $"equipamento: {e.Nome} (série {e.NumeroSerie})";

    // Filtros: nome/série, tipo, localização, técnico responsável e situação
    public async Task<IActionResult> Index(string busca, string tipo, string localizacao, int? funcionarioId, string situacao)
    {
        var consulta = _context.Equipamentos.Include(e => e.Funcionario).AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca)) consulta = consulta.Where(e => e.Nome.Contains(busca) || e.NumeroSerie.Contains(busca));
        if (!string.IsNullOrWhiteSpace(tipo)) consulta = consulta.Where(e => e.Tipo == tipo);
        if (!string.IsNullOrWhiteSpace(localizacao)) consulta = consulta.Where(e => e.Localizacao.Contains(localizacao));
        if (funcionarioId.HasValue) consulta = consulta.Where(e => e.FuncionarioId == funcionarioId);
        if (Enum.TryParse<SituacaoEquipamento>(situacao, out var filtro)) consulta = consulta.Where(e => e.Situacao == filtro);

        ViewBag.Funcionarios = new SelectList(_context.Funcionarios.OrderBy(f => f.Nome), "Id", "Nome", funcionarioId);
        ViewBag.Tipos = new SelectList(TiposEquipamento.Lista, tipo);
        ViewBag.Busca = busca;
        ViewBag.Localizacao = localizacao;
        ViewBag.Situacao = situacao;
        return View(await consulta.OrderBy(e => e.Nome).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Equipamentos.Include(e => e.Funcionario).FirstOrDefaultAsync(e => e.Id == id);
        return item == null ? NotFound() : View(item);
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Create()
    {
        CarregarListas();
        return View(new Equipamento());
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(Equipamento equipamento)
    {
        if (await _context.Equipamentos.AnyAsync(e => e.NumeroSerie == equipamento.NumeroSerie))
            ModelState.AddModelError(nameof(Equipamento.NumeroSerie), "Já existe um equipamento com este número de série.");
        if (!ModelState.IsValid) { CarregarListas(equipamento.FuncionarioId); return View(equipamento); }
        _context.Add(equipamento);
        await _context.SaveChangesAsync();
        await _mov.RegistrarAsync("Cadastro", "Cadastro de " + Descrever(equipamento));
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Equipamentos.FindAsync(id);
        if (item == null) return NotFound();
        CarregarListas(item.FuncionarioId);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int id, Equipamento equipamento)
    {
        if (id != equipamento.Id) return NotFound();
        if (await _context.Equipamentos.AnyAsync(e => e.NumeroSerie == equipamento.NumeroSerie && e.Id != id))
            ModelState.AddModelError(nameof(Equipamento.NumeroSerie), "Já existe um equipamento com este número de série.");
        if (!ModelState.IsValid) { CarregarListas(equipamento.FuncionarioId); return View(equipamento); }
        try
        {
            _context.Update(equipamento);
            await _context.SaveChangesAsync();
            await _mov.RegistrarAsync("Atualização", "Atualização de " + Descrever(equipamento));
        }
        catch (DbUpdateConcurrencyException) when (!_context.Equipamentos.Any(e => e.Id == id)) { return NotFound(); }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Equipamentos.Include(e => e.Funcionario).FirstOrDefaultAsync(e => e.Id == id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Equipamentos.FindAsync(id);
        if (item != null)
        {
            _context.Remove(item);
            await _context.SaveChangesAsync();
            await _mov.RegistrarAsync("Exclusão", "Exclusão de " + Descrever(item));
        }
        return RedirectToAction(nameof(Index));
    }
}
