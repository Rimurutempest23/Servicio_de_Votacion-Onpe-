using EF_POO_II.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EF_POO_II.Controllers
{
    // 🔐 Solo Admin puede acceder
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly SistemaVotacionContext _context;

        public UsuariosController(SistemaVotacionContext context)
        {
            _context = context;
        }

        // 🔐 HASH
        private string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (usuario == null)
            {
                ViewBag.Error = "Usuario no existe";
                return View();
            }

            // 🔐 COMPARAR PASSWORD CON HASH
            var passwordValida = Convert.ToBase64String(
                SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password))
            ) == usuario.PasswordHash;

            if (!passwordValida)
            {
                ViewBag.Error = "Contraseña incorrecta";
                return View();
            }

            // ✅ LOGIN EXITOSO (sesión simple)
            HttpContext.Session.SetString("User", usuario.Username);
            HttpContext.Session.SetString("Role", usuario.Rol.Nombre);

            return RedirectToAction("Index", "Home");
        }
        // =========================
        // LISTA DE USUARIOS
        // =========================
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .ToListAsync();

            return View(usuarios);
        }

        // =========================
        // GET: CREAR USUARIO
        // =========================
        public IActionResult Create()
        {
            ViewBag.Roles = _context.Roles.ToList();
            return View();
        }

        // =========================
        // POST: CREAR USUARIO
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string username, string password, int rolId)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Todos los campos son obligatorios";
                ViewBag.Roles = _context.Roles.ToList();
                return View();
            }

            // Verificar si ya existe
            var existe = await _context.Usuarios.AnyAsync(u => u.Username == username);
            if (existe)
            {
                ViewBag.Error = "El usuario ya existe";
                ViewBag.Roles = _context.Roles.ToList();
                return View();
            }

            var nuevoUsuario = new Usuario
            {
                Username = username,
                PasswordHash = HashPassword(password), // 🔥 HASH
                RolId = rolId
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Usuario creado correctamente";

            return RedirectToAction(nameof(Index));
        }
    }
}