using System.Security.Cryptography;
using System.Text;

namespace AccountingProject.Infrastructure
{
    /// <summary>
    /// Builds a 256-bit signing key from configured key material (SHA-256 hash when the string is shorter than 32 bytes).
    /// </summary>
    public static class JwtSigningKey
    {
        public static byte[] GetKeyBytes(string keyMaterial)
        {
            var bytes = Encoding.UTF8.GetBytes(keyMaterial);
            if (bytes.Length >= 32)
                return bytes;
            return SHA256.HashData(bytes);
        }
    }
}
