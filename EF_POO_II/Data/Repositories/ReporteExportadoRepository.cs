using EF_POO_II.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EF_POO_II.Data.Repositories;

public class ReporteExportadoRepository : IReporteExportadoRepository
{
    private readonly SistemaVotacionContext _context;

    public ReporteExportadoRepository(SistemaVotacionContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(ReporteExportado reporte)
    {
        var connection = (SqlConnection)_context.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;

        if (close)
        {
            await connection.OpenAsync();
        }

        await using var tx = (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            await using var cmd = new SqlCommand("sp_InsertReporteExportado", connection, tx)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 150).Value = reporte.Nombre;
            cmd.Parameters.Add("@RutaGuardado", SqlDbType.VarChar, 300).Value = reporte.RutaGuardado;

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
            if (close)
            {
                await connection.CloseAsync();
            }
        }
    }
}
