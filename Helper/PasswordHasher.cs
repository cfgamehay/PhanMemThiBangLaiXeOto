using System.Security.Cryptography;

namespace ApiThiBangLaiXeOto.Helper
{
    public static class PasswordHasher
    {
        // Tạo hash từ mật khẩu
        public static string HashPassword(string password)
        {
            // Sinh salt ngẫu nhiên
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            // Dùng PBKDF2 với 100,000 iterations
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            // Ghép salt + hash để lưu (dùng Base64 cho tiện)
            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        // Xác minh mật khẩu
        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] hash = Convert.FromBase64String(parts[1]);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hashToCompare = pbkdf2.GetBytes(32);

            return CryptographicOperations.FixedTimeEquals(hash, hashToCompare);
        }
    }
}
