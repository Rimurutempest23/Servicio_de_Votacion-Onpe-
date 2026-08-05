using EF_POO_II.Models;

namespace EF_POO_II.Repositories
{
    public interface IUsuarioRepository
    {
        Task<List<Usuario>> GetAllAsync();
        Task<Usuario?> GetByUsernameAsync(string username);
        Task AddAsync(Usuario usuario);
    }
}