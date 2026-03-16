using System.Security.Cryptography;
using System.Text;
using AiChat.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AiChat.Infrastructure.Security;

public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public AesEncryptionService(IConfiguration configuration)
    {
        // 从配置读取密钥，或使用默认密钥（生产环境必须配置）
        var keyString = configuration["Encryption:Key"] ?? "AiChatDefaultEncryptionKey32Bit!";
        var ivString = configuration["Encryption:IV"] ?? "AiChatDefaultIV!";

        // 确保密钥长度为 32 字节（256 位）
        _key = Encoding.UTF8.GetBytes(keyString.PadRight(32).Substring(0, 32));
        // 确保 IV 长度为 16 字节
        _iv = Encoding.UTF8.GetBytes(ivString.PadRight(16).Substring(0, 16));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            var buffer = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
        catch
        {
            // 解密失败返回原文（可能是未加密的旧数据）
            return cipherText;
        }
    }
}
