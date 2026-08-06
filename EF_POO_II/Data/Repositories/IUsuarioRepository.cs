using EF_POO_II.Models;

namespace EF_POO_II.Data.Repositories
{
    public interface IUsuarioRepository
    {
        Task<List<Usuario>> GetAllAsync();
        Task<PagedResult<Usuario>> BuscarPaginadoAsync(string? filtro, int page, int pageSize);
        Task<Usuario?> GetByUsernameAsync(string username);
        Task AddAsync(Usuario usuario);
        Task<Usuario?> GetByIdAsync(int id);
        Task UpdateAsync(Usuario usuario);
        Task DeleteAsync(int id);
    }
}
