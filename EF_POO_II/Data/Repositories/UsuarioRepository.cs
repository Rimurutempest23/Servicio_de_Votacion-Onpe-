using EF_POO_II.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EF_POO_II.Data.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly SistemaVotacionContext _context;

        public UsuarioRepository(SistemaVotacionContext context)
        {
            _context = context;
        }

        public async Task<List<Usuario>> GetAllAsync()
        {
            var usuarios = new List<Usuario>();
            var connection = (SqlConnection)_context.Database.GetDbConnection();
            var close = connection.State != ConnectionState.Open;

            if (close)
            {
                await connection.OpenAsync();
            }

            try
            {
                await using var cmd = new SqlCommand(@"
                    SELECT u.Id, u.Username, u.PasswordHash, u.RolId, r.Nombre AS RolNombre
                    FROM Usuarios u
                    INNER JOIN Roles r ON r.Id = u.RolId
                    WHERE u.IsActivo = 1
                    ORDER BY u.Username", connection);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    usuarios.Add(MapUsuario(reader));
                }
            }
            finally
            {
                if (close)
                {
                    await connection.CloseAsync();
                }
            }

            return usuarios;
        }

        public async Task<PagedResult<Usuario>> BuscarPaginadoAsync(string? filtro, int page, int pageSize)
        {
            var result = new PagedResult<Usuario>
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
                await using var cmd = new SqlCommand("sp_ListarUsuariosPaginado", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add("@Filtro", SqlDbType.VarChar, 100).Value = string.IsNullOrWhiteSpace(filtro) ? DBNull.Value : filtro;
                cmd.Parameters.Add("@Page", SqlDbType.Int).Value = page;
                cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Items.Add(MapUsuario(reader));
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

        public async Task<Usuario?> GetByUsernameAsync(string username)
        {
            var connection = (SqlConnection)_context.Database.GetDbConnection();
            var close = connection.State != ConnectionState.Open;

            if (close)
            {
                await connection.OpenAsync();
            }

            try
            {
                await using var cmd = new SqlCommand(@"
                    SELECT u.Id, u.Username, u.PasswordHash, u.RolId, r.Nombre AS RolNombre
                    FROM Usuarios u
                    INNER JOIN Roles r ON r.Id = u.RolId
                    WHERE u.Username = @username AND u.IsActivo = 1", connection);

                cmd.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = username;

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapUsuario(reader);
                }

                return null;
            }
            finally
            {
                if (close)
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            var connection = (SqlConnection)_context.Database.GetDbConnection();
            var close = connection.State != ConnectionState.Open;

            if (close)
            {
                await connection.OpenAsync();
            }

            try
            {
                await using var cmd = new SqlCommand(@"
                    SELECT u.Id, u.Username, u.PasswordHash, u.RolId, r.Nombre AS RolNombre
                    FROM Usuarios u
                    INNER JOIN Roles r ON r.Id = u.RolId
                    WHERE u.Id = @id AND u.IsActivo = 1", connection);

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapUsuario(reader);
                }

                return null;
            }
            finally
            {
                if (close)
                {
                    await connection.CloseAsync();
                }
            }
        }

        public Task AddAsync(Usuario usuario)
        {
            return EjecutarProcedureConTransaccionAsync("sp_InsertUsuario", cmd =>
            {
                cmd.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = usuario.Username;
                cmd.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255).Value = usuario.PasswordHash;
                cmd.Parameters.Add("@RolId", SqlDbType.Int).Value = usuario.RolId;
            });
        }

        public Task UpdateAsync(Usuario usuario)
        {
            return EjecutarProcedureConTransaccionAsync("sp_UpdateUsuario", cmd =>
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = usuario.Id;
                cmd.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = usuario.Username;
                cmd.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255).Value = usuario.PasswordHash;
                cmd.Parameters.Add("@RolId", SqlDbType.Int).Value = usuario.RolId;
            });
        }

        public Task DeleteAsync(int id)
        {
            return EjecutarProcedureConTransaccionAsync("sp_DeleteUsuario", cmd =>
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            });
        }

        private async Task EjecutarProcedureConTransaccionAsync(string procedure, Action<SqlCommand> configurarParametros)
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
                await using var cmd = new SqlCommand(procedure, connection, tx)
                {
                    CommandType = CommandType.StoredProcedure
                };

                configurarParametros(cmd);
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

        private static Usuario MapUsuario(SqlDataReader reader)
        {
            var rolId = reader.GetInt32(reader.GetOrdinal("RolId"));

            return new Usuario
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                RolId = rolId,
                Rol = new Role
                {
                    Id = rolId,
                    Nombre = reader.GetString(reader.GetOrdinal("RolNombre"))
                }
            };
        }
    }
}
