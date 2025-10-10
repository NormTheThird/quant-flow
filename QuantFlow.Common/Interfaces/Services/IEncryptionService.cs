namespace QuantFlow.Common.Interfaces.Services;

/// <summary>
/// Service for encrypting and decrypting sensitive data
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts a plaintext string
    /// </summary>
    /// <param name="plainText">The text to encrypt</param>
    /// <returns>Encrypted string</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts an encrypted string
    /// </summary>
    /// <param name="encryptedText">The text to decrypt</param>
    /// <returns>Decrypted plaintext string</returns>
    string Decrypt(string encryptedText);
}