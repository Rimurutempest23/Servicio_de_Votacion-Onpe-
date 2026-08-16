using System.Security.Cryptography;
using System.Text;
using EF_POO_II.Models;
using Microsoft.AspNetCore.Identity;

namespace EF_POO_II.Helpers
{
    public static class PasswordHelper
    {
        private static readonly PasswordHasher<Usuario> Hasher = new();

        public static string HashPassword(string password)
        {
            return Hasher.HashPassword(new Usuario(), password);
        }

        public static bool VerifyPassword(string hash, string password)
        {
            var result = Hasher.VerifyHashedPassword(new Usuario(), hash, password);
            if (result != PasswordVerificationResult.Failed)
            {
                return true;
            }

            return hash == HashPasswordLegacy(password);
        }

        public static bool NeedsRehash(string hash, string password)
        {
            return hash == HashPasswordLegacy(password);
        }

        private static string HashPasswordLegacy(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
