using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SistemaOficina.Data;

public static class DbInicializador
{
    public const string Administrador = "Administrador";

    public static async Task InicializarAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbInicializador");
        try
        {
            await services.GetRequiredService<AppDbContext>().Database.MigrateAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var role in new[] { Administrador })
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            await CriarUsuarioAsync(services, "admin@oficina.com", "Admin@123", Administrador);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Não foi possível preparar o banco. Execute 'dotnet ef migrations add Inicial' e reinicie o sistema.");
        }
    }

    private static async Task CriarUsuarioAsync(IServiceProvider services, string email, string senha, string role)
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            if (!(await userManager.CreateAsync(user, senha)).Succeeded) return;
        }
        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);
    }
}
