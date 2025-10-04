using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Data
{
    public class DataAccessException : Exception
    {
        public DataAccessException(string message, Exception innerException) : base(message, innerException) { }
    }
    
    public interface IDataRepository
    {
        UniTask SaveAllAsync();
        Type GetDataType();
    }
    
    public interface IDataRepository<T> : IDataRepository where T : class, IDataModel<T>
    {
        UniTask<T> LoadAsync(string key);
        UniTask<bool> SaveAsync(string key, T data);
        UniTask<bool> DeleteAsync(string key);
        UniTask<bool> ExistsAsync(string key);
        UniTask<IEnumerable<string>> GetAllKeysAsync();
    }
    
    /// <summary>
    /// Synchronous data repository interface for lightweight operations that don't require async/await
    /// </summary>
    public interface IDataRepositorySync<T> where T : class, IDataModel<T>
    {
        T Load(string key);
        bool Save(string key, T data);
        bool Delete(string key);
        bool Exists(string key);
        IEnumerable<string> GetAllKeys();
    }
    
    public class FileDataRepository<T> : IDataRepository<T>, IDataRepositorySync<T> where T : class, IDataModel<T>, new()
    {
        private readonly string _basePath;
        private readonly bool _useEncryption;
        private readonly bool _useCompression;
        
        public FileDataRepository(string basePath = null, bool useEncryption = true, bool useCompression = true)
        {
            _basePath = basePath ?? UnityEngine.Application.persistentDataPath;
            _useEncryption = useEncryption;
            _useCompression = useCompression;
        }
        
        public Type GetDataType() => typeof(T);
        
        public async UniTask<T> LoadAsync(string key)
        {
            try
            {
                var filePath = GetFilePath(key);
                if (!System.IO.File.Exists(filePath))
                {
                    return null;
                }
                
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                
                // Flow Decode: File Bytes -> Decrypt -> Compressed Bytes -> Decompress -> JSON String
                var dataBytes = _useEncryption ? DataEncryptor.Decrypt(fileBytes) : fileBytes;
                string json;
                
                if (_useCompression)
                {
                    var decompressedResult = await DataCompressor.DecompressBytesAsync(dataBytes);
                    if (!decompressedResult.Success)
                    {
                        throw new Exception($"Decompression failed: {decompressedResult.Error}");
                    }
                    json = Encoding.UTF8.GetString(decompressedResult.Data);
                }
                else
                {
                    json = Encoding.UTF8.GetString(dataBytes);
                }
                
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[FileDataRepository] Failed to load {key}: {ex.Message}");
                throw new DataAccessException($"Failed to load data for key '{key}'.", ex);
            }
        }
        
        public async UniTask<bool> SaveAsync(string key, T data)
        {
            try
            {
                var filePath = GetFilePath(key);
                var directory = System.IO.Path.GetDirectoryName(filePath);
                
                if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                
                // Flow encode: JSON String -> Compress -> Compressed Bytes -> Encrypt -> File Bytes
                var dataBytes = Encoding.UTF8.GetBytes(json);
                
                if (_useCompression)
                {
                    var compressedResult = await DataCompressor.CompressBytesAsync(dataBytes);
                    if (!compressedResult.Success)
                    {
                        throw new Exception($"Compression failed: {compressedResult.Error}");
                    }
                    dataBytes = compressedResult.Data;
                }

                var fileBytes = _useEncryption ? DataEncryptor.Encrypt(dataBytes) : dataBytes;

                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[FileDataRepository] Failed to save {key}: {ex.Message}");
                throw new DataAccessException($"Failed to save data for key '{key}'.", ex);
            }
        }

        public async UniTask<bool> DeleteAsync(string key)
        {
            try
            {
                var filePath = GetFilePath(key);
                
                if (System.IO.File.Exists(filePath))
                {
                    await UniTask.RunOnThreadPool(() => System.IO.File.Delete(filePath));
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
            return await UniTask.RunOnThreadPool(() => System.IO.File.Exists(filePath));
        }
        
        public async UniTask<IEnumerable<string>> GetAllKeysAsync()
        {
            try
            {
                var typeFolder = System.IO.Path.Combine(_basePath, typeof(T).Name);
                
                if (!System.IO.Directory.Exists(typeFolder))
                {
                    return Enumerable.Empty<string>();
                }
                
                var files = await UniTask.RunOnThreadPool(() => System.IO.Directory.GetFiles(typeFolder, "*.dat"));
                return files.Select(System.IO.Path.GetFileNameWithoutExtension).ToList();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[FileDataRepository] Failed to get all keys: {ex.Message}");
                return Enumerable.Empty<string>();
            }
        }
        
        public async UniTask SaveAllAsync()
        {
            await UniTask.CompletedTask;
        }
        
        // Synchronous implementations for lightweight operations
        
        public T Load(string key)
        {
            try
            {
                var filePath = GetFilePath(key);
                if (!System.IO.File.Exists(filePath))
                {
                    return null;
                }
                
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                
                // Flow Decode: File Bytes -> Decrypt -> Compressed Bytes -> Decompress -> JSON String
                var dataBytes = _useEncryption ? DataEncryptor.Decrypt(fileBytes) : fileBytes;
                string json;
                
                if (_useCompression)
                {
                    var decompressedResult = DataCompressor.DecompressBytes(dataBytes);
                    if (!decompressedResult.Success)
                    {
                        throw new Exception($"Decompression failed: {decompressedResult.Error}");
                    }
                    json = Encoding.UTF8.GetString(decompressedResult.Data);
                }
                else
                {
                    json = Encoding.UTF8.GetString(dataBytes);
                }
                
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[FileDataRepository] Failed to load {key}: {ex.Message}");
                throw new DataAccessException($"Failed to load data for key '{key}'.", ex);
            }
        }
        
        public bool Save(string key, T data)
        {
            try
            {
                var filePath = GetFilePath(key);
                var directory = System.IO.Path.GetDirectoryName(filePath);
                
                if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                
                // Flow encode: JSON String -> Compress -> Compressed Bytes -> Encrypt -> File Bytes
                var dataBytes = Encoding.UTF8.GetBytes(json);
                
                if (_useCompression)
                {
                    var compressedResult = DataCompressor.CompressBytes(dataBytes);
                    if (!compressedResult.Success)
                    {
                        throw new Exception($"Compression failed: {compressedResult.Error}");
                    }
                    dataBytes = compressedResult.Data;
                }

                var fileBytes = _useEncryption ? DataEncryptor.Encrypt(dataBytes) : dataBytes;

                System.IO.File.WriteAllBytes(filePath, fileBytes);
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[FileDataRepository] Failed to save {key}: {ex.Message}");
                throw new DataAccessException($"Failed to save data for key '{key}'.", ex);
            }
        }

        public bool Delete(string key)
        {
            try
            {
                var filePath = GetFilePath(key);
                
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
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
        
        public bool Exists(string key)
        {
            var filePath = GetFilePath(key);
            return System.IO.File.Exists(filePath);
        }
        
        public IEnumerable<string> GetAllKeys()
        {
            try
            {
                var typeFolder = System.IO.Path.Combine(_basePath, typeof(T).Name);
                
                if (!System.IO.Directory.Exists(typeFolder))
                {
                    return Enumerable.Empty<string>();
                }
                
                var files = System.IO.Directory.GetFiles(typeFolder, "*.dat");
                return files.Select(System.IO.Path.GetFileNameWithoutExtension).ToList();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[FileDataRepository] Failed to get all keys: {ex.Message}");
                return Enumerable.Empty<string>();
            }
        }
        
        private string GetFilePath(string key)
        {
            var sanitizedKey = SanitizeFileName(key);
            return System.IO.Path.Combine(_basePath, typeof(T).Name, $"{sanitizedKey}.dat");
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
    
    public class MemoryDataRepository<T> : IDataRepository<T>, IDataRepositorySync<T> where T : class, IDataModel<T>
    {
        private readonly Dictionary<string, T> _data = new();
        
        public Type GetDataType() => typeof(T);
        
        public async UniTask<T> LoadAsync(string key)
        {
            await UniTask.CompletedTask;
            return _data.GetValueOrDefault(key);
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
        }
        
        // Synchronous implementations for lightweight operations
        
        public T Load(string key)
        {
            return _data.GetValueOrDefault(key);
        }
        
        public bool Save(string key, T data)
        {
            _data[key] = data;
            return true;
        }
        
        public bool Delete(string key)
        {
            return _data.Remove(key);
        }
        
        public bool Exists(string key)
        {
            return _data.ContainsKey(key);
        }
        
        public IEnumerable<string> GetAllKeys()
        {
            return _data.Keys;
        }
    }
}
