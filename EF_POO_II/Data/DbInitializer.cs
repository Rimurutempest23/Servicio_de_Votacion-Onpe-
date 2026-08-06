using EF_POO_II.Helpers;
using EF_POO_II.Models;
using Microsoft.EntityFrameworkCore;

namespace EF_POO_II.Data;

public static class DbInitializer
{
    public static async Task InicializarAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SistemaVotacionContext>();

        await context.Database.MigrateAsync();

        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new Role { Nombre = "Administrador" },
                new Role { Nombre = "Operador" });
        }

        if (!await context.Candidatos.AnyAsync())
        {
            context.Candidatos.AddRange(
                new Candidato { Nombre = "Lista A", ImagenUrl = "https://placehold.co/120x120/0056b3/ffffff?text=A", TotalVotos = 0 },
                new Candidato { Nombre = "Lista B", ImagenUrl = "https://placehold.co/120x120/198754/ffffff?text=B", TotalVotos = 0 },
                new Candidato { Nombre = "Lista C", ImagenUrl = "https://placehold.co/120x120/dc3545/ffffff?text=C", TotalVotos = 0 });
        }

        await context.SaveChangesAsync();

        var rolAdministrador = await context.Roles.FirstAsync(r => r.Nombre == "Administrador");
        if (!await context.Usuarios.AnyAsync(u => u.Username == "admin"))
        {
            context.Usuarios.Add(new Usuario
            {
                Username = "admin",
                PasswordHash = PasswordHelper.HashPassword("admin123"),
                RolId = rolAdministrador.Id
            });

            await context.SaveChangesAsync();
        }
    }
}
