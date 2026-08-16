using System.Data;
using System.Text.Json;
using EF_POO_II.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EF_POO_II.Data.Repositories;

public class ElectoralRepository : IElectoralRepository
{
    private readonly SistemaVotacionContext _context;

    public ElectoralRepository(SistemaVotacionContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Eleccion>> ListarEleccionesAsync()
    {
        return await _context.Elecciones
            .OrderByDescending(e => e.FechaInicio)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<MesaElectoral>> ListarMesasAsync()
    {
        return await _context.MesasElectorales
            .Include(m => m.Usuario)
            .Include(m => m.Actas)
            .OrderBy(m => m.Distrito)
            .ThenBy(m => m.CodigoMesa)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<MesaElectoral>> ListarMesasPorUsuarioAsync(string username)
    {
        return await _context.OperadorMesaAsignaciones
            .Include(a => a.MesaElectoral)
            .ThenInclude(m => m!.Usuario)
            .Where(a => a.IsActiva && a.Usuario != null && a.Usuario.Username == username)
            .Where(a => a.MesaElectoral != null && a.MesaElectoral.Estado != "Procesada")
            .OrderBy(a => a.MesaElectoral!.CodigoMesa)
            .Select(a => a.MesaElectoral!)
            .ToListAsync();
    }

    public async Task<MesaElectoral?> ObtenerMesaAsync(int mesaId)
    {
        return await _context.MesasElectorales
            .Include(m => m.Usuario)
            .FirstOrDefaultAsync(m => m.Id == mesaId);
    }

    public async Task<IReadOnlyList<ActaResumenDto>> ListarActasAsync()
    {
        return await _context.ActasElectorales
            .AsNoTracking()
            .OrderByDescending(a => a.FechaRegistro)
            .Select(a => new ActaResumenDto
            {
                Id = a.Id,
                Eleccion = a.Eleccion != null ? a.Eleccion.Nombre : string.Empty,
                CodigoMesa = a.MesaElectoral != null ? a.MesaElectoral.CodigoMesa : string.Empty,
                Distrito = a.MesaElectoral != null ? a.MesaElectoral.Distrito : string.Empty,
                Estado = a.Estado,
                UsuarioRegistro = a.UsuarioRegistro,
                FechaRegistro = a.FechaRegistro,
                TotalVotos = a.Detalles.Sum(d => d.Votos)
            })
            .ToListAsync();
    }

    public async Task<ActaElectoral?> ObtenerActaDetalleAsync(int id)
    {
        return await _context.ActasElectorales
            .AsNoTracking()
            .Include(a => a.Eleccion)
            .Include(a => a.MesaElectoral)
            .Include(a => a.Detalles)
                .ThenInclude(d => d.Candidato)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IReadOnlyList<Auditoria>> ListarAuditoriaAsync()
    {
        return await _context.Auditorias
            .AsNoTracking()
            .OrderByDescending(a => a.Fecha)
            .Take(50)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CandidatoResultado>> ListarResultadosPorEleccionAsync(int eleccionId)
    {
        return await _context.Candidatos
            .AsNoTracking()
            .Where(c => c.IsActivo)
            .OrderBy(c => c.Nombre)
            .Select(c => new CandidatoResultado
            {
                Id = c.Id,
                Nombre = c.Nombre,
                ImagenUrl = c.ImagenUrl,
                Total = _context.DetalleActas
                    .Where(d => d.CandidatoId == c.Id && d.ActaElectoral != null && d.ActaElectoral.EleccionId == eleccionId)
                    .Sum(d => (int?)d.Votos) ?? 0
            })
            .ToListAsync();
    }

    public async Task<int> ProcesarActaAsync(RegistrarActaRequest request, string usuario)
    {
        var connection = (SqlConnection)_context.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;

        if (close)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var cmd = new SqlCommand("sp_ProcesarActaElectoral", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@EleccionId", SqlDbType.Int).Value = request.EleccionId;
            cmd.Parameters.Add("@MesaElectoralId", SqlDbType.Int).Value = request.MesaElectoralId;
            cmd.Parameters.Add("@UsuarioRegistro", SqlDbType.VarChar, 50).Value = usuario;
            cmd.Parameters.Add("@Observaciones", SqlDbType.VarChar, 250).Value = string.IsNullOrWhiteSpace(request.Observaciones) ? DBNull.Value : request.Observaciones;
            cmd.Parameters.Add("@DetalleJson", SqlDbType.NVarChar).Value = JsonSerializer.Serialize(request.Detalles);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        finally
        {
            if (close)
            {
                await connection.CloseAsync();
            }
        }
    }
}
