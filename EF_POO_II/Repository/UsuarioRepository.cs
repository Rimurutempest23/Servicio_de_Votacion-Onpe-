using EF_POO_II.Models;
using Microsoft.EntityFrameworkCore;

namespace EF_POO_II.Repositories
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
            return await _context.Usuarios.Include(u => u.Rol).ToListAsync();
        }

        public async Task<Usuario?> GetByUsernameAsync(string username)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task AddAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
        }
    }
}