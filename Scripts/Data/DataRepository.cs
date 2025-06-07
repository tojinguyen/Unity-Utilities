using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Data
{
    /// <summary>
    /// Generic data repository interface for CRUD operations
    /// </summary>
    public interface IDataRepository
    {
        UniTask SaveAllAsync();
    }
    
    /// <summary>
    /// Generic data repository interface for specific data type
    /// </summary>
    public interface IDataRepository<T> : IDataRepository where T : class, IDataModel<T>
    {
        UniTask<T> LoadAsync(string key);
        UniTask<bool> SaveAsync(string key, T data);
        UniTask<bool> DeleteAsync(string key);
        UniTask<bool> ExistsAsync(string key);
        UniTask<IEnumerable<string>> GetAllKeysAsync();
    }
    
    /// <summary>
    /// File-based implementation of data repository
    /// </summary>
    public class FileDataRepository<T> : IDataRepository<T> where T : class, IDataModel<T>, new()
    {
        private readonly string _basePath;
        private readonly bool _useEncryption;
        private readonly bool _useCompression;
        private readonly string _encryptionKey;
        private readonly string _encryptionIv;
        
        public FileDataRepository(string basePath = null, bool useEncryption = false, bool useCompression = false, 
            string encryptionKey = null, string encryptionIv = null)
        {
            _basePath = basePath ?? UnityEngine.Application.persistentDataPath;
            _useEncryption = useEncryption;
            _useCompression = useCompression;
            _encryptionKey = encryptionKey ?? "1234567890123456";
            _encryptionIv = encryptionIv ?? "6543210987654321";
        }
        
        public async UniTask<T> LoadAsync(string key)
        {
            try
            {
                var filePath = GetFilePath(key);
                
                if (!System.IO.File.Exists(filePath))
                {
                    return null;
                }
                
                var data = await System.IO.File.ReadAllBytesAsync(filePath);
                
                if (_useEncryption)
                {
                    var keyBytes = System.Text.Encoding.UTF8.GetBytes(_encryptionKey);
                    var ivBytes = System.Text.Encoding.UTF8.GetBytes(_encryptionIv);
                    var json = DataEncryptor.Decrypt(data, keyBytes, ivBytes);
                    
                    if (_useCompression)
                    {
                        json = DataCompressor.Decompress(json);
                    }
                    
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
                }
                else
                {
                    var json = System.Text.Encoding.UTF8.GetString(data);
                    
                    if (_useCompression)
                    {
                        json = DataCompressor.Decompress(json);
                    }
                    
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[FileDataRepository] Failed to load {key}: {ex.Message}");
                return null;
            }
        }
        
        public async UniTask<bool> SaveAsync(string key, T data)
        {
            try
            {
                var filePath = GetFilePath(key);
                var directory = System.IO.Path.GetDirectoryName(filePath);
                
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                
                if (_useCompression)
                {
                    json = DataCompressor.Compress(json);
                }
                
                if (_useEncryption)
                {
                    var keyBytes = System.Text.Encoding.UTF8.GetBytes(_encryptionKey);
                    var ivBytes = System.Text.Encoding.UTF8.GetBytes(_encryptionIv);
                    var encryptedData = DataEncryptor.Encrypt(json, keyBytes, ivBytes);
                    await System.IO.File.WriteAllBytesAsync(filePath, encryptedData);
                }
                else
                {
                    var dataBytes = System.Text.Encoding.UTF8.GetBytes(json);
                    await System.IO.File.WriteAllBytesAsync(filePath, dataBytes);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[FileDataRepository] Failed to save {key}: {ex.Message}");
                return false;
            }
        }
        
        public async UniTask<bool> DeleteAsync(string key)
        {
            try
            {
                var filePath = GetFilePath(key);
                
                if (System.IO.File.Exists(filePath))
                {
                    await UniTask.Run(() => System.IO.File.Delete(filePath));
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[FileDataRepository] Failed to delete {key}: {ex.Message}");
                return false;
            }
        }
        
        public async UniTask<bool> ExistsAsync(string key)
        {
            var filePath = GetFilePath(key);
            return await UniTask.Run(() => System.IO.File.Exists(filePath));
        }
        
        public async UniTask<IEnumerable<string>> GetAllKeysAsync()
        {
            try
            {
                var typeFolder = System.IO.Path.Combine(_basePath, typeof(T).Name);
                
                if (!System.IO.Directory.Exists(typeFolder))
                {
                    return new string[0];
                }
                
                var files = await UniTask.Run(() => System.IO.Directory.GetFiles(typeFolder, "*.json"));
                var keys = new List<string>();
                
                foreach (var file in files)
                {
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                    keys.Add(fileName);
                }
                
                return keys;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[FileDataRepository] Failed to get all keys: {ex.Message}");
                return new string[0];
            }
        }
        
        public async UniTask SaveAllAsync()
        {
            // For file repository, individual saves are already persistent
            await UniTask.CompletedTask;
        }
        
        private string GetFilePath(string key)
        {
            var sanitizedKey = SanitizeFileName(key);
            return System.IO.Path.Combine(_basePath, typeof(T).Name, $"{sanitizedKey}.json");
        }
        
        private string SanitizeFileName(string fileName)
        {
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var sanitized = fileName;
            
            foreach (var invalidChar in invalidChars)
            {
                sanitized = sanitized.Replace(invalidChar, '_');
            }
            
            return sanitized;
        }
    }
    
    /// <summary>
    /// Memory-based implementation of data repository (for testing or temporary data)
    /// </summary>
    public class MemoryDataRepository<T> : IDataRepository<T> where T : class, IDataModel<T>
    {
        private readonly Dictionary<string, T> _data = new();
        
        public async UniTask<T> LoadAsync(string key)
        {
            await UniTask.CompletedTask;
            return _data.TryGetValue(key, out var data) ? data : null;
        }
        
        public async UniTask<bool> SaveAsync(string key, T data)
        {
            await UniTask.CompletedTask;
            _data[key] = data;
            return true;
        }
        
        public async UniTask<bool> DeleteAsync(string key)
        {
            await UniTask.CompletedTask;
            return _data.Remove(key);
        }
        
        public async UniTask<bool> ExistsAsync(string key)
        {
            await UniTask.CompletedTask;
            return _data.ContainsKey(key);
        }
        
        public async UniTask<IEnumerable<string>> GetAllKeysAsync()
        {
            await UniTask.CompletedTask;
            return _data.Keys;
        }
        
        public async UniTask SaveAllAsync()
        {
            await UniTask.CompletedTask;
            // Memory repository doesn't need to save
        }
    }
}
