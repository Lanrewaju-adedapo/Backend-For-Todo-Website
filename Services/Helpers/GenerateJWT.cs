using System;
using System.Security.Cryptography;

// Run this once to generate a key, then copy it to your config
public partial class Program
{
    public static void Main()
    {
        // Generate a 256-bit (32-byte) key and convert to Base64
        var keyBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(keyBytes);
        }

        string base64Key = Convert.ToBase64String(keyBytes);
        Console.WriteLine($"Your JWT Secret Key: {base64Key}");
        Console.WriteLine("Copy this to your appsettings.json JwtSettings:SecretKey");
    }
}