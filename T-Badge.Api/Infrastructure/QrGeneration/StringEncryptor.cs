using System.Security.Cryptography;
using System.Text;

namespace T_Badge.Infrastructure.QrGeneration;

public class StringEncryptor(byte[] key, byte[] iv)
{
    public byte[] Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        // aes.Padding = PaddingMode.None;
        
        var encryptor = aes.CreateEncryptor(
            aes.Key,
            aes.IV);

        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        
        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
        cryptoStream.FlushFinalBlock();

        return memoryStream.ToArray();
    }

    public string Decrypt(byte[] cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        // aes.Padding = PaddingMode.None;

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using var memoryStream = new MemoryStream(cipherText);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var decryptedStream = new MemoryStream();
        
        cryptoStream.CopyTo(decryptedStream);

        return Encoding.UTF8.GetString(decryptedStream.ToArray());
    }

    public static (byte[] key, byte[] iv) GenerateSalts()
    {
        var ivHashed = RandomNumberGenerator.GetBytes(16);
        var keyHashed = RandomNumberGenerator.GetBytes(32);
        
        return (keyHashed, ivHashed);
    }
}