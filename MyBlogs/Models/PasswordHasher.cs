using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Cryptography;

public class PasswordHasher : IPasswordHasher<string>
{
    private const int SaltSize = 128 / 8;
    private const int KeySize = 256 / 8;
    private const int Iterations = 10000;
    private static readonly KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA256;

    public string HashPassword(string user,string password)
    {
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: Prf,
            iterationCount: Iterations,
            numBytesRequested: KeySize));

        return $"{Convert.ToBase64String(salt)}.{hashedPassword}";
    }

    public PasswordVerificationResult VerifyHashedPassword(string user,string hashedPassword, string providedPassword)
    {
        var parts = hashedPassword.Split('.');
        if (parts.Length != 2)
        {
            return PasswordVerificationResult.Failed;
        }

        var salt = Convert.FromBase64String(parts[0]);
        var storedHash = parts[1];

        var hashedProvidedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: providedPassword,
            salt: salt,
            prf: Prf,
            iterationCount: Iterations,
            numBytesRequested: KeySize));

        return hashedProvidedPassword == storedHash
            ? PasswordVerificationResult.Success
            : PasswordVerificationResult.Failed;
    }
}
