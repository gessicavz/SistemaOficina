using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaOficina.Models;

namespace SistemaOficina.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Funcionario> Funcionarios { get; set; }
    public DbSet<Pagamento> Pagamentos { get; set; }
    public DbSet<Equipamento> Equipamentos { get; set; }
    public DbSet<Movimentacao> Movimentacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Funcionario>().HasIndex(f => f.CPF).IsUnique();
        modelBuilder.Entity<Equipamento>().HasIndex(e => e.NumeroSerie).IsUnique();
        modelBuilder.Entity<Equipamento>()
            .HasOne(e => e.Funcionario)
            .WithMany(f => f.Equipamentos)
            .HasForeignKey(e => e.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Pagamento>()
            .HasOne(p => p.Funcionario)
            .WithMany(f => f.Pagamentos)
            .HasForeignKey(p => p.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
