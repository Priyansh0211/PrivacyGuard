using System;
using System.Security.Cryptography;
using System.Text;

namespace PrivacyGuard.Helpers
{
    public static class PasswordHelper
    {
        private const string Prefix = "PBKDF2";
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 600_000;

        // Format: PBKDF2$<iterations>$<saltBase64>$<hashBase64>
        public static string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

            return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
        }

        public static bool IsHashed(string? stored)
        {
            return !string.IsNullOrEmpty(stored) && stored.StartsWith(Prefix + "$", StringComparison.Ordinal);
        }

        // needsRehash = true tab hoga jab:
        //  (a) stored value purana plain text tha aur password sahi nikla, ya
        //  (b) hash purane (kam) iterations se bana tha.
        public static bool Verify(string? stored, string? password, out bool needsRehash)
        {
            needsRehash = false;

            if (string.IsNullOrEmpty(stored) || string.IsNullOrEmpty(password))
                return false;

            // Legacy plain-text password (migration support)
            if (!IsHashed(stored))
            {
                bool legacyMatch = CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(stored),
                    Encoding.UTF8.GetBytes(password));

                needsRehash = legacyMatch;
                return legacyMatch;
            }

            var parts = stored.Split('$');
            if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations) || iterations <= 0)
                return false;

            try
            {
                var salt = Convert.FromBase64String(parts[2]);
                var expected = Convert.FromBase64String(parts[3]);

                var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);

                bool match = CryptographicOperations.FixedTimeEquals(actual, expected);
                needsRehash = match && iterations < Iterations;
                return match;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}