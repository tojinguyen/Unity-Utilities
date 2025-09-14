using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace TirexGame.Utils.Data
{
    /// <summary>
    /// Backup metadata information
    /// </summary>
    [Serializable]
    public class BackupMetadata
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public List<string> DataTypes { get; set; } = new List<string>();
        public long SizeBytes { get; set; }
        public bool IsFullBackup { get; set; }
        public string CreatedBy { get; set; }
        public Dictionary<string, object> CustomData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Backup entry containing data and metadata
    /// </summary>
    [Serializable]
    public class BackupEntry
    {
        public string Key { get; set; }
        public string DataType { get; set; }
        public string JsonData { get; set; }
        public DateTime BackupTime { get; set; }
        public string Hash { get; set; }
    }

    /// <summary>
    /// Complete backup container
    /// </summary>
    [Serializable]
    public class DataBackup
    {
        public BackupMetadata Metadata { get; set; }
        public List<BackupEntry> Entries { get; set; } = new List<BackupEntry>();
    }

    /// <summary>
    /// Backup restoration result
    /// </summary>
    public class BackupRestoreResult
    {
        public bool Success { get; set; }
        public int ItemsRestored { get; set; }
        public int ItemsFailed { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// Data backup and migration manager with versioning support
    /// </summary>
    public class DataBackupManager
    {
        private readonly string _backupDirectory;
        private readonly int _maxBackups;
        private readonly bool _enableCompression;
        private readonly bool _enableEncryption;

        public DataBackupManager(
            string backupDirectory = null,
            int maxBackups = 10,
            bool enableCompression = true,
            bool enableEncryption = false)
        {
            _backupDirectory = backupDirectory ?? Path.Combine(Application.persistentDataPath, "Backups");
            _maxBackups = maxBackups;
            _enableCompression = enableCompression;
            _enableEncryption = enableEncryption;

            EnsureBackupDirectoryExists();
        }

        /// <summary>
        /// Create a backup for specific data
        /// </summary>
        public async UniTask<string> CreateBackupAsync<T>(string key, T data, string description = null) where T : class
        {
            try
            {
                var backupId = GenerateBackupId();
                var dataType = typeof(T).Name;
                var jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
                var hash = ComputeHash(jsonData);

                var backup = new DataBackup
                {
                    Metadata = new BackupMetadata
                    {
                        Id = backupId,
                        CreatedAt = DateTime.UtcNow,
                        Description = description ?? $"Backup of {key}",
                        Version = Application.version,
                        DataTypes = { dataType },
                        IsFullBackup = false,
                        CreatedBy = "DataBackupManager"
                    },
                    Entries = 
                    {
                        new BackupEntry
                        {
                            Key = key,
                            DataType = dataType,
                            JsonData = jsonData,
                            BackupTime = DateTime.UtcNow,
                            Hash = hash
                        }
                    }
                };

                await SaveBackupAsync(backup);
                await CleanupOldBackupsAsync();

#if DATA_LOG
                Debug.Log($"[DataBackupManager] Backup created: {backupId} for {key}");
#endif

                return backupId;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DataBackupManager] Failed to create backup: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create a full backup of all data from repositories
        /// </summary>
        public async UniTask<string> CreateFullBackupAsync(Dictionary<Type, IDataRepository> repositories, string description = null)
        {
            try
            {
                var backupId = GenerateBackupId();
                var backup = new DataBackup
                {
                    Metadata = new BackupMetadata
                    {
                        Id = backupId,
                        CreatedAt = DateTime.UtcNow,
                        Description = description ?? "Full system backup",
                        Version = Application.version,
                        IsFullBackup = true,
                        CreatedBy = "DataBackupManager"
                    }
                };

                foreach (var repoKvp in repositories)
                {
                    var dataType = repoKvp.Key;
                    var repository = repoKvp.Value;
                    backup.Metadata.DataTypes.Add(dataType.Name);

                    try
                    {
                        // Get all keys from repository using reflection
                        var getAllKeysMethod = repository.GetType().GetMethod("GetAllKeysAsync");
                        if (getAllKeysMethod != null)
                        {
                            var keysTask = getAllKeysMethod.Invoke(repository, null);
                            var keysResult = await ConvertToUniTask(keysTask);
                            var keys = keysResult as IEnumerable<string>;

                            foreach (var key in keys)
                            {
                                try
                                {
                                    // Load data using reflection
                                    var loadMethod = repository.GetType().GetMethod("LoadAsync");
                                    if (loadMethod != null)
                                    {
                                        var loadTask = loadMethod.Invoke(repository, new object[] { key });
                                        var taskResult = await ConvertToUniTask(loadTask);
                                        
                                        if (taskResult != null)
                                        {
                                            var jsonData = JsonConvert.SerializeObject(taskResult, Formatting.Indented);
                                            var hash = ComputeHash(jsonData);

                                            backup.Entries.Add(new BackupEntry
                                            {
                                                Key = key,
                                                DataType = dataType.Name,
                                                JsonData = jsonData,
                                                BackupTime = DateTime.UtcNow,
                                                Hash = hash
                                            });
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogWarning($"[DataBackupManager] Failed to backup {key}: {ex.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[DataBackupManager] Failed to backup repository {dataType.Name}: {ex.Message}");
                    }
                }

                await SaveBackupAsync(backup);
                await CleanupOldBackupsAsync();

#if DATA_LOG
                Debug.Log($"[DataBackupManager] Full backup created: {backupId} with {backup.Entries.Count} entries");
#endif

                return backupId;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DataBackupManager] Failed to create full backup: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Restore data from backup
        /// </summary>
        public async UniTask<BackupRestoreResult> RestoreFromBackupAsync(string backupId, Dictionary<Type, IDataRepository> repositories)
        {
            var result = new BackupRestoreResult();
            var startTime = DateTime.UtcNow;

            try
            {
                var backup = await LoadBackupAsync(backupId);
                if (backup == null)
                {
                    result.Errors.Add($"Backup not found: {backupId}");
                    return result;
                }

                foreach (var entry in backup.Entries)
                {
                    try
                    {
                        // Find the appropriate repository
                        var repository = repositories.Values.FirstOrDefault(r => 
                            r.GetType().GetGenericArguments().Any(t => t.Name == entry.DataType));

                        if (repository != null)
                        {
                            // Verify hash
                            var currentHash = ComputeHash(entry.JsonData);
                            if (currentHash != entry.Hash)
                            {
                                result.Errors.Add($"Hash mismatch for {entry.Key}");
                                result.ItemsFailed++;
                                continue;
                            }

                            // Deserialize data
                            var dataType = repository.GetType().GetGenericArguments()[0];
                            var data = JsonConvert.DeserializeObject(entry.JsonData, dataType);

                            // Save using reflection
                            var saveMethod = repository.GetType().GetMethod("SaveAsync");
                            if (saveMethod != null)
                            {
                                var saveTask = saveMethod.Invoke(repository, new object[] { entry.Key, data });
                                var saveResult = await ConvertToUniTask<bool>(saveTask);
                                
                                if (saveResult)
                                {
                                    result.ItemsRestored++;
                                }
                                else
                                {
                                    result.Errors.Add($"Failed to save restored data for {entry.Key}");
                                    result.ItemsFailed++;
                                }
                            }
                            else
                            {
                                result.Errors.Add($"Save method not found for {entry.DataType}");
                                result.ItemsFailed++;
                            }
                        }
                        else
                        {
                            result.Errors.Add($"Repository not found for {entry.DataType}");
                            result.ItemsFailed++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Failed to restore {entry.Key}: {ex.Message}");
                        result.ItemsFailed++;
                    }
                }

                result.Success = result.ItemsFailed == 0;
                result.Duration = DateTime.UtcNow - startTime;

#if DATA_LOG
                Debug.Log($"[DataBackupManager] Restore completed: {result.ItemsRestored} restored, {result.ItemsFailed} failed");
#endif

                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Restore failed: {ex.Message}");
                result.Duration = DateTime.UtcNow - startTime;
                Debug.LogError($"[DataBackupManager] Restore failed: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Get all available backups
        /// </summary>
        public async UniTask<List<BackupMetadata>> GetAvailableBackupsAsync()
        {
            var backups = new List<BackupMetadata>();

            try
            {
                if (!Directory.Exists(_backupDirectory))
                    return backups;

                var backupFiles = Directory.GetFiles(_backupDirectory, "*.backup");
                
                foreach (var file in backupFiles)
                {
                    try
                    {
                        var backup = await LoadBackupAsync(Path.GetFileNameWithoutExtension(file));
                        if (backup?.Metadata != null)
                        {
                            backup.Metadata.SizeBytes = new FileInfo(file).Length;
                            backups.Add(backup.Metadata);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[DataBackupManager] Failed to load backup metadata from {file}: {ex.Message}");
                    }
                }

                return backups.OrderByDescending(b => b.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DataBackupManager] Failed to get available backups: {ex.Message}");
                return backups;
            }
        }

        /// <summary>
        /// Delete a specific backup
        /// </summary>
        public async UniTask<bool> DeleteBackupAsync(string backupId)
        {
            try
            {
                var backupPath = GetBackupPath(backupId);
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
#if DATA_LOG
                    Debug.Log($"[DataBackupManager] Backup deleted: {backupId}");
#endif
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DataBackupManager] Failed to delete backup {backupId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clean up old backups based on max backup limit
        /// </summary>
        public async UniTask CleanupOldBackupsAsync()
        {
            try
            {
                var backups = await GetAvailableBackupsAsync();
                
                if (backups.Count <= _maxBackups)
                    return;

                var toDelete = backups
                    .OrderByDescending(b => b.CreatedAt)
                    .Skip(_maxBackups)
                    .ToList();

                foreach (var backup in toDelete)
                {
                    await DeleteBackupAsync(backup.Id);
                }

#if DATA_LOG
                Debug.Log($"[DataBackupManager] Cleaned up {toDelete.Count} old backups");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DataBackupManager] Failed to cleanup old backups: {ex.Message}");
            }
        }

        private async UniTask SaveBackupAsync(DataBackup backup)
        {
            var backupPath = GetBackupPath(backup.Metadata.Id);
            var jsonData = JsonConvert.SerializeObject(backup, Formatting.Indented);

            if (_enableEncryption)
            {
                // Simple encryption implementation - in production, use proper encryption
                jsonData = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonData));
            }

            if (_enableCompression)
            {
                // Simple compression - in production, use proper compression
                var bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
                using var compressed = new MemoryStream();
                using (var gzip = new System.IO.Compression.GZipStream(compressed, System.IO.Compression.CompressionMode.Compress))
                {
                    await gzip.WriteAsync(bytes, 0, bytes.Length);
                }
                jsonData = Convert.ToBase64String(compressed.ToArray());
            }

            await File.WriteAllTextAsync(backupPath, jsonData);
        }

        private async UniTask<DataBackup> LoadBackupAsync(string backupId)
        {
            var backupPath = GetBackupPath(backupId);
            
            if (!File.Exists(backupPath))
                return null;

            var jsonData = await File.ReadAllTextAsync(backupPath);

            if (_enableCompression)
            {
                var compressedBytes = Convert.FromBase64String(jsonData);
                using var compressed = new MemoryStream(compressedBytes);
                using var gzip = new System.IO.Compression.GZipStream(compressed, System.IO.Compression.CompressionMode.Decompress);
                using var result = new MemoryStream();
                await gzip.CopyToAsync(result);
                jsonData = System.Text.Encoding.UTF8.GetString(result.ToArray());
            }

            if (_enableEncryption)
            {
                jsonData = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(jsonData));
            }

            return JsonConvert.DeserializeObject<DataBackup>(jsonData);
        }

        private void EnsureBackupDirectoryExists()
        {
            if (!Directory.Exists(_backupDirectory))
            {
                Directory.CreateDirectory(_backupDirectory);
            }
        }

        private string GenerateBackupId()
        {
            return $"backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";
        }

        private string GetBackupPath(string backupId)
        {
            return Path.Combine(_backupDirectory, $"{backupId}.backup");
        }

        private string ComputeHash(string input)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private async UniTask<object> ConvertToUniTask(object task)
        {
            if (task is UniTask<object> uniTaskObj)
                return await uniTaskObj;
            
            // Handle other task types through reflection
            var taskType = task.GetType();
            if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(UniTask<>))
            {
                var resultProperty = taskType.GetProperty("Result");
                if (resultProperty != null)
                {
                    // For UniTask<T>, we need to await it first
                    var awaiterMethod = taskType.GetMethod("GetAwaiter");
                    var awaiter = awaiterMethod.Invoke(task, null);
                    var getResultMethod = awaiter.GetType().GetMethod("GetResult");
                    return getResultMethod.Invoke(awaiter, null);
                }
            }
            
            return null;
        }

        private async UniTask<T> ConvertToUniTask<T>(object task)
        {
            var result = await ConvertToUniTask(task);
            return result is T ? (T)result : default(T);
        }
    }
}
