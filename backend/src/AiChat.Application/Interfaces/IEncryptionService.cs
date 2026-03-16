namespace AiChat.Application.Interfaces;

/// <summary>
/// 加密服务接口
/// </summary>
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
