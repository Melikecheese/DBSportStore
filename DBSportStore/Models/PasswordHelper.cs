using System;
using System.Security.Cryptography;
using System.Text;

namespace DBSportStore.Models
{
    public static class PasswordHelper
    {
        public static string CreateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        public static string Hash(string password, string salt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (salt == null) throw new ArgumentNullException("salt");

            byte[] input = Encoding.UTF8.GetBytes(password + salt);
            using (SHA256 sha = SHA256.Create())
            {
                return Convert.ToBase64String(sha.ComputeHash(input));
            }
        }

        public static bool Verify(string password, string salt, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(salt) ||
                string.IsNullOrEmpty(storedHash))
                return false;

            byte[] actual = Convert.FromBase64String(Hash(password, salt));
            byte[] expected;
            try
            {
                expected = Convert.FromBase64String(storedHash);
            }
            catch (FormatException)
            {
                return false;
            }

            if (actual.Length != expected.Length) return false;
            int difference = 0;
            for (int i = 0; i < actual.Length; i++)
                difference |= actual[i] ^ expected[i];

            return difference == 0;
        }
    }
}