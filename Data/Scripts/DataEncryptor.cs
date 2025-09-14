using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class DataEncryptor
{
    private const int IvSizeBytes = 16;
    private static byte[] _key;
    
    private static byte[] GetKey()
    {
        if (_key != null) return _key;

        const string salt = "your_unique_salt_here_1a2b3c";
        
        var deviceId = SystemInfo.deviceUniqueIdentifier;
        
        var rfc2898 = new Rfc2898DeriveBytes(deviceId, Encoding.UTF8.GetBytes(salt), 10000, HashAlgorithmName.SHA256);
        _key = rfc2898.GetBytes(32); 

        return _key;
    }
    
    private static readonly byte[] DefaultKey = Encoding.UTF8.GetBytes("1234567890123456"); // Key 16 bytes
    private static readonly byte[] DefaultIv = Encoding.UTF8.GetBytes("6543210987654321"); // IV 16 bytes

    public static byte[] Encrypt(byte[] dataToEncrypt)
    {
        using var aes = Aes.Create();
        aes.Key = GetKey();
        aes.GenerateIV();
        var iv = aes.IV;

        using var encryptor = aes.CreateEncryptor(aes.Key, iv);
        using var ms = new MemoryStream();
        
        // Ghi IV vào đầu stream
        ms.Write(iv, 0, iv.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cs.Write(dataToEncrypt, 0, dataToEncrypt.Length);
        }
        return ms.ToArray();
    }

    public static byte[] Decrypt(byte[] dataToDecrypt)
    {
        if (dataToDecrypt == null || dataToDecrypt.Length <= IvSizeBytes)
        {
            throw new ArgumentException("Dữ liệu cần giải mã không hợp lệ.");
        }

        using var aes = Aes.Create();
        aes.Key = GetKey();

        var iv = new byte[IvSizeBytes];
        Array.Copy(dataToDecrypt, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(new MemoryStream(dataToDecrypt, IvSizeBytes, dataToDecrypt.Length - IvSizeBytes), decryptor, CryptoStreamMode.Read))
        {
            cs.CopyTo(ms);
        }
        return ms.ToArray();
    }
}