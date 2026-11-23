using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOficina.Data;
using SistemaOficina.Models;
using SistemaOficina.Services;

namespace SistemaOficina.Controllers;

[Authorize(Roles = "Administrador")]
public class FuncionariosController : Controller
{
    private readonly AppDbContext _context;
    private readonly MovimentacaoService _mov;
    public FuncionariosController(AppDbContext context, MovimentacaoService mov)
    {
        _context = context;
        _mov = mov;
    }

    // Filtros: nome/CPF, cargo e situação
    public async Task<IActionResult> Index(string busca, string cargo, string situacao)
    {
        var consulta = _context.Funcionarios.AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
            consulta = consulta.Where(f => f.Nome.Contains(busca) || f.CPF.Contains(busca));
        if (!string.IsNullOrWhiteSpace(cargo))
            consulta = consulta.Where(f => f.Cargo.Contains(cargo));
        if (situacao == "Ativo") consulta = consulta.Where(f => f.Ativo);
        else if (situacao == "Inativo") consulta = consulta.Where(f => !f.Ativo);

        ViewBag.Busca = busca;
        ViewBag.Cargo = cargo;
        ViewBag.Situacao = situacao;
        return View(await consulta.OrderBy(f => f.Nome).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Funcionarios.Include(f => f.Pagamentos).Include(f => f.Equipamentos).FirstOrDefaultAsync(f => f.Id == id);
        return item == null ? NotFound() : View(item);
    }

    public IActionResult Create() => View(new Funcionario());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Funcionario funcionario)
    {
        if (await _context.Funcionarios.AnyAsync(f => f.CPF == funcionario.CPF))
            ModelState.AddModelError(nameof(Funcionario.CPF), "Já existe um funcionário com este CPF.");
        if (!ModelState.IsValid) return View(funcionario);
        _context.Add(funcionario);
        await _context.SaveChangesAsync();
        await _mov.RegistrarAsync("Cadastro", $"Cadastro de funcionário: {funcionario.Nome}");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Funcionarios.FindAsync(id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Funcionario funcionario)
    {
        if (id != funcionario.Id) return NotFound();
        if (await _context.Funcionarios.AnyAsync(f => f.CPF == funcionario.CPF && f.Id != id))
            ModelState.AddModelError(nameof(Funcionario.CPF), "Já existe um funcionário com este CPF.");
        if (!ModelState.IsValid) return View(funcionario);
        try
        {
            _context.Update(funcionario);
            await _context.SaveChangesAsync();
            await _mov.RegistrarAsync("Atualização", $"Atualização de funcionário: {funcionario.Nome}");
        }
        catch (DbUpdateConcurrencyException) when (!_context.Funcionarios.Any(f => f.Id == id)) { return NotFound(); }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Funcionarios.FindAsync(id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Funcionarios.FindAsync(id);
        if (item == null) return RedirectToAction(nameof(Index));
        if (await _context.Pagamentos.AnyAsync(p => p.FuncionarioId == id))
        {
            TempData["Erro"] = "Não é possível excluir um funcionário que possui pagamentos. Marque-o como inativo.";
            return RedirectToAction(nameof(Index));
        }
        if (await _context.Equipamentos.AnyAsync(e => e.FuncionarioId == id))
        {
            TempData["Erro"] = "Não é possível excluir um funcionário que é responsável por equipamentos. Marque-o como inativo.";
            return RedirectToAction(nameof(Index));
        }
        _context.Remove(item);
        await _context.SaveChangesAsync();
        await _mov.RegistrarAsync("Exclusão", $"Exclusão de funcionário: {item.Nome}");
        return RedirectToAction(nameof(Index));
    }
}
