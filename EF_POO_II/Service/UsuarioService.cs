using EF_POO_II.Models;
using EF_POO_II.Repositories;
using EF_POO_II.Helpers;

namespace EF_POO_II.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepository _repo;

        public UsuarioService(IUsuarioRepository repo)
        {
            _repo = repo;
        }

        public async Task CrearUsuario(string username, string password, int rolId)
        {
            var usuario = new Usuario
            {
                Username = username,
                PasswordHash = PasswordHelper.HashPassword(password),
                RolId = rolId
            };

            await _repo.AddAsync(usuario);
        }

        public async Task<Usuario?> Login(string username)
        {
            return await _repo.GetByUsernameAsync(username);
        }
    }
}