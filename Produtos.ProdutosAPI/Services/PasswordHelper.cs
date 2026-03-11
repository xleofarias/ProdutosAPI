using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace ProdutosAPI.Services
{
    public class PasswordHelper
    {
        public static string Hash(string password)
        {
            // Gera um Salt aleatório
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Gera o Hash
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            // Retorna no formato: salt.hash
            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        public static bool Verify(string hashCompleto, string password)
        {
            try
            {
                var parts = hashCompleto.Split('.');
                if (parts.Length != 2) return false;

                var salt = Convert.FromBase64String(parts[0]);
                var hashSalvo = parts[1];

                string hashGeradoAgora = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

                return hashSalvo == hashGeradoAgora;
            }
            catch
            {
                return false;
            }
        }
    }
}
