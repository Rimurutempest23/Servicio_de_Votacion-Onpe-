using EF_POO_II.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EF_POO_II.Data.Repositories;

public class CandidatoRepository : ICandidatoRepository
{
    private readonly SistemaVotacionContext _context;

    public CandidatoRepository(SistemaVotacionContext context)
    {
        _context = context;
    }

    public async Task<List<Candidato>> GetAllAsync()
    {
        return await _context.Candidatos.OrderBy(c => c.Nombre).ToListAsync();
    }

    public async Task<PagedResult<Candidato>> BuscarPaginadoAsync(string? filtro, int page, int pageSize)
    {
        var result = new PagedResult<Candidato>
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
            await using var cmd = new SqlCommand("sp_ListarCandidatosPaginado", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@Filtro", SqlDbType.VarChar, 100).Value = string.IsNullOrWhiteSpace(filtro) ? DBNull.Value : filtro;
            cmd.Parameters.Add("@Page", SqlDbType.Int).Value = page;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Items.Add(new Candidato
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                    ImagenUrl = reader.IsDBNull(reader.GetOrdinal("ImagenUrl")) ? null : reader.GetString(reader.GetOrdinal("ImagenUrl")),
                    TotalVotos = reader.GetInt32(reader.GetOrdinal("TotalVotos"))
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

    public async Task<Candidato?> GetByIdAsync(int id)
    {
        return await _context.Candidatos.FindAsync(id);
    }

    public async Task AddAsync(Candidato candidato)
    {
        // Usar transacción para insertar
        var connection = (SqlConnection)_context.Database.GetDbConnection();
        var close = connection.State != System.Data.ConnectionState.Open;
        if (close) await connection.OpenAsync();

        await using var tx = (SqlTransaction)await connection.BeginTransactionAsync();
        try
        {
            await using var cmd = new SqlCommand("sp_InsertCandidato", connection, tx);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Nombre", candidato.Nombre);
            cmd.Parameters.AddWithValue("@ImagenUrl", (object?)candidato.ImagenUrl ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
        finally
        {
            if (close) await connection.CloseAsync();
        }
    }

    public async Task UpdateAsync(Candidato candidato)
    {
        // Call stored procedure sp_UpdateCandidato
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_UpdateCandidato @Id = {0}, @Nombre = {1}, @ImagenUrl = {2}",
            candidato.Id,
            candidato.Nombre,
            (object?)candidato.ImagenUrl ?? DBNull.Value);
    }

    public async Task DeleteAsync(int id)
    {
        await _context.Database.ExecuteSqlRawAsync("EXEC sp_DeleteCandidato @Id = {0}", id);
    }
}
