using System.Security.Cryptography;

namespace GameExplorer.Api.Auth;

public static class Passwords
{
	const int Iterations = 600_000;
	static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

	public static string Hash(string password)
	{
		byte[] salt = RandomNumberGenerator.GetBytes(16);
		byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, 32);
		return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
	}

	public static bool Verify(string password, string stored)
	{
		string[] parts = stored.Split('.');
		byte[] salt = Convert.FromBase64String(parts[1]);
		byte[] expected = Convert.FromBase64String(parts[2]);
		byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, int.Parse(parts[0]), Algorithm, expected.Length);
		return CryptographicOperations.FixedTimeEquals(actual, expected);
	}
}
