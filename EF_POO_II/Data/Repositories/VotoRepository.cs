using EF_POO_II.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EF_POO_II.Data.Repositories;

public class VotoRepository : IVotoRepository
{
    private readonly SistemaVotacionContext _context;

    public VotoRepository(SistemaVotacionContext context)
    {
        _context = context;
    }

    public Task<bool> ExisteCandidatoAsync(int candidatoId)
    {
        return _context.Candidatos.AnyAsync(c => c.Id == candidatoId);
    }

    public async Task<IReadOnlyList<CandidatoResultado>> ListarResultadosAsync()
    {
        return await _context.Candidatos
            .OrderBy(c => c.Nombre)
            .Select(c => new CandidatoResultado
            {
                Id = c.Id,
                Nombre = c.Nombre,
                ImagenUrl = c.ImagenUrl,
                Total = c.TotalVotos
            })
            .ToListAsync();
    }

    public async Task<PagedResult<CandidatoResultado>> ListarResultadosPaginadosAsync(string? filtro, int page, int pageSize)
    {
        var result = new PagedResult<CandidatoResultado>
        {
            Page = page,
            PageSize = pageSize
        };

        var connection = (SqlConnection)_context.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;

        if (close)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var cmd = new SqlCommand("sp_ListarResultadosPaginado", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@Filtro", SqlDbType.VarChar, 100).Value = string.IsNullOrWhiteSpace(filtro) ? DBNull.Value : filtro;
            cmd.Parameters.Add("@Page", SqlDbType.Int).Value = page;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Items.Add(new CandidatoResultado
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                    ImagenUrl = reader.IsDBNull(reader.GetOrdinal("ImagenUrl")) ? null : reader.GetString(reader.GetOrdinal("ImagenUrl")),
                    Total = reader.GetInt32(reader.GetOrdinal("TotalVotos"))
                });
                result.TotalRegistros = reader.GetInt32(reader.GetOrdinal("TotalRegistros"));
            }
        }
        finally
        {
            if (close)
            {
                await connection.CloseAsync();
            }
        }

        return result;
    }

    public async Task RegistrarVotoConTransaccionAsync(int candidatoId, int cantidad)
    {
        var connection = (SqlConnection)_context.Database.GetDbConnection();
        var debeCerrarConexion = connection.State != ConnectionState.Open;

        if (debeCerrarConexion)
        {
            await connection.OpenAsync();
        }

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            await using var insertarVoto = new SqlCommand(
                "INSERT INTO Votos (CandidatoId, Cantidad, Fecha) VALUES (@candidatoId, @cantidad, GETDATE())",
                connection,
                transaction);

            insertarVoto.Parameters.Add("@candidatoId", SqlDbType.Int).Value = candidatoId;
            insertarVoto.Parameters.Add("@cantidad", SqlDbType.Int).Value = cantidad;
            await insertarVoto.ExecuteNonQueryAsync();

            await using var actualizarCandidato = new SqlCommand(
                "UPDATE Candidatos SET TotalVotos = TotalVotos + @cantidad WHERE Id = @candidatoId",
                connection,
                transaction);

            actualizarCandidato.Parameters.Add("@cantidad", SqlDbType.Int).Value = cantidad;
            actualizarCandidato.Parameters.Add("@candidatoId", SqlDbType.Int).Value = candidatoId;
            await actualizarCandidato.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            if (debeCerrarConexion)
            {
                await connection.CloseAsync();
            }
        }
    }
}
