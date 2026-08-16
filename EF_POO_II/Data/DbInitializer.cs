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

        if (!await context.Elecciones.AnyAsync())
        {
            context.Elecciones.Add(new Eleccion
            {
                Nombre = "Eleccion General 2026",
                FechaInicio = new DateTime(2026, 4, 12, 8, 0, 0),
                FechaFin = new DateTime(2026, 4, 12, 16, 0, 0),
                Estado = "Abierta"
            });
        }

        if (!await context.MesasElectorales.AnyAsync())
        {
            context.MesasElectorales.AddRange(
                new MesaElectoral { CodigoMesa = "MESA-001", LocalVotacion = "IE Republica del Peru", Distrito = "Lima", Estado = "Pendiente" },
                new MesaElectoral { CodigoMesa = "MESA-002", LocalVotacion = "IE Santa Rosa", Distrito = "Ate", Estado = "Pendiente" },
                new MesaElectoral { CodigoMesa = "MESA-003", LocalVotacion = "IE San Martin", Distrito = "Comas", Estado = "Pendiente" });
        }

        await context.SaveChangesAsync();

        var rolAdministrador = await context.Roles.FirstAsync(r => r.Nombre == "Administrador");
        var rolOperador = await context.Roles.FirstAsync(r => r.Nombre == "Operador");
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

        if (!await context.Usuarios.AnyAsync(u => u.Username == "operador1"))
        {
            context.Usuarios.Add(new Usuario
            {
                Username = "operador1",
                PasswordHash = PasswordHelper.HashPassword("operador123"),
                RolId = rolOperador.Id
            });

            await context.SaveChangesAsync();
        }

        var operador = await context.Usuarios.FirstAsync(u => u.Username == "operador1");
        var mesaSinOperador = await context.MesasElectorales.FirstOrDefaultAsync(m => m.UsuarioId == null);
        if (mesaSinOperador != null)
        {
            mesaSinOperador.UsuarioId = operador.Id;
            if (!await context.OperadorMesaAsignaciones.AnyAsync(a => a.UsuarioId == operador.Id && a.MesaElectoralId == mesaSinOperador.Id && a.IsActiva))
            {
                context.OperadorMesaAsignaciones.Add(new OperadorMesaAsignacion
                {
                    UsuarioId = operador.Id,
                    MesaElectoralId = mesaSinOperador.Id,
                    FechaAsignacion = DateTime.Now,
                    IsActiva = true,
                    Observacion = "Asignacion automatica inicial"
                });
            }
            await context.SaveChangesAsync();
        }
    }
}
