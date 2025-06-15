using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        string password = "Test123!";
        
        // Step 1: Client-side SHA256 hash
        string clientHash = GetSHA256Hash(password);
        Console.WriteLine($"Client-side SHA256 hash of '{password}': {clientHash}");
        Console.WriteLine();
        
        // Generate hashes for multiple users with different salts
        var users = new[] { "beginner_user", "intermediate_user", "advanced_user", "demo", "admin" };
        
        foreach (var user in users)
        {
            var (hash, salt) = HashPassword(clientHash);
            Console.WriteLine($"User: {user}");
            Console.WriteLine($"Salt (Base64): {salt}");
            Console.WriteLine($"Hash (Base64): {hash}");
            
            // Verify the hash
            bool isValid = VerifyPassword(clientHash, hash, salt);
            Console.WriteLine($"Verification: {(isValid ? "PASS" : "FAIL")}");
            Console.WriteLine();
        }
    }
    
    static string GetSHA256Hash(string input)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hash = sha256.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
    
    static (string Hash, string Salt) HashPassword(string clientHashedPassword)
    {
        // Generate random salt (256 bits)
        var saltBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        var salt = Convert.ToBase64String(saltBytes);
        
        // Combine client hash + salt and SHA256
        var combinedBytes = Encoding.UTF8.GetBytes(clientHashedPassword + salt);
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(combinedBytes);
        var hash = Convert.ToBase64String(hashBytes);
        
        return (hash, salt);
    }
    
    static bool VerifyPassword(string clientHashedPassword, string storedHash, string storedSalt)
    {
        try
        {
            // Combine client hash + salt and SHA256
            var combinedBytes = Encoding.UTF8.GetBytes(clientHashedPassword + storedSalt);
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(combinedBytes);
            var computedHash = Convert.ToBase64String(hashBytes);
            
            return computedHash == storedHash;
        }
        catch
        {
            return false;
        }
    }
}