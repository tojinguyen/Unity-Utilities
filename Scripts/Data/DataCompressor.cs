using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.Data
{
    /// <summary>
    /// Compression statistics for monitoring performance
    /// </summary>
    public class CompressionStats
    {
        public long OriginalSize { get; set; }
        public long CompressedSize { get; set; }
        public double CompressionRatio => OriginalSize > 0 ? (double)CompressedSize / OriginalSize : 0;
        public double SpaceSavedPercent => OriginalSize > 0 ? (1.0 - CompressionRatio) * 100 : 0;
        public TimeSpan CompressionTime { get; set; }
        public TimeSpan DecompressionTime { get; set; }
    }

    /// <summary>
    /// Compression result containing data and statistics
    /// </summary>
    public class CompressionResult
    {
        public byte[] Data { get; set; }
        public CompressionStats Stats { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// Decompression result containing data and statistics
    /// </summary>
    public class DecompressionResult
    {
        public byte[] Data { get; set; }
        public CompressionStats Stats { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// Data compression utility with multiple algorithms and performance monitoring
    /// </summary>
    public static class DataCompressor
    {
        /// <summary>
        /// Available compression algorithms
        /// </summary>
        public enum CompressionAlgorithm
        {
            GZip,
            Deflate,
            Brotli
        }

        /// <summary>
        /// Compression levels for algorithms that support it
        /// </summary>
        public enum CompressionLevel
        {
            Optimal = 0,
            Fastest = 1,
            NoCompression = 2,
            SmallestSize = 3
        }

        /// <summary>
        /// Compress string data using specified algorithm
        /// </summary>
        public static async Task<CompressionResult> CompressStringAsync(
            string data,
            CompressionAlgorithm algorithm = CompressionAlgorithm.GZip,
            CompressionLevel level = CompressionLevel.Optimal)
        {
            if (string.IsNullOrEmpty(data))
            {
                return new CompressionResult
                {
                    Success = false,
                    Error = "Input data is null or empty"
                };
            }

            try
            {
                var inputBytes = Encoding.UTF8.GetBytes(data);
                return await CompressBytesAsync(inputBytes, algorithm, level);
            }
            catch (Exception ex)
            {
                return new CompressionResult
                {
                    Success = false,
                    Error = $"String compression failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Compress byte array using specified algorithm
        /// </summary>
        public static async Task<CompressionResult> CompressBytesAsync(
            byte[] data,
            CompressionAlgorithm algorithm = CompressionAlgorithm.GZip,
            CompressionLevel level = CompressionLevel.Optimal)
        {
            if (data == null || data.Length == 0)
            {
                return new CompressionResult
                {
                    Success = false,
                    Error = "Input data is null or empty"
                };
            }

            var startTime = DateTime.UtcNow;
            var stats = new CompressionStats
            {
                OriginalSize = data.Length
            };

            try
            {
                byte[] compressedData;

                using (var output = new MemoryStream())
                {
                    Stream compressionStream = algorithm switch
                    {
                        CompressionAlgorithm.GZip => new GZipStream(output, GetCompressionLevel(level)),
                        CompressionAlgorithm.Deflate => new DeflateStream(output, GetCompressionLevel(level)),
                        CompressionAlgorithm.Brotli => new BrotliStream(output, GetCompressionLevel(level)),
                        _ => throw new ArgumentException($"Unsupported algorithm: {algorithm}")
                    };

                    using (compressionStream)
                    {
                        await compressionStream.WriteAsync(data, 0, data.Length);
                    }

                    compressedData = output.ToArray();
                }

                stats.CompressedSize = compressedData.Length;
                stats.CompressionTime = DateTime.UtcNow - startTime;

#if DATA_LOG
                Debug.Log($"[DataCompressor] Compressed {stats.OriginalSize} bytes to {stats.CompressedSize} bytes " +
                         $"({stats.SpaceSavedPercent:F1}% saved) using {algorithm} in {stats.CompressionTime.TotalMilliseconds:F2}ms");
#endif

                return new CompressionResult
                {
                    Data = compressedData,
                    Stats = stats,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new CompressionResult
                {
                    Stats = stats,
                    Success = false,
                    Error = $"Compression failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Decompress data to string using specified algorithm
        /// </summary>
        public static async Task<DecompressionResult> DecompressToStringAsync(
            byte[] compressedData,
            CompressionAlgorithm algorithm = CompressionAlgorithm.GZip)
        {
            var result = await DecompressBytesAsync(compressedData, algorithm);
            
            if (!result.Success)
                return new DecompressionResult
                {
                    Stats = result.Stats,
                    Success = false,
                    Error = result.Error
                };

            try
            {
                var stringData = Encoding.UTF8.GetString(result.Data);
                return new DecompressionResult
                {
                    Data = Encoding.UTF8.GetBytes(stringData), // Return as bytes for consistency
                    Stats = result.Stats,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new DecompressionResult
                {
                    Stats = result.Stats,
                    Success = false,
                    Error = $"String conversion failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Decompress byte array using specified algorithm
        /// </summary>
        public static async Task<DecompressionResult> DecompressBytesAsync(
            byte[] compressedData,
            CompressionAlgorithm algorithm = CompressionAlgorithm.GZip)
        {
            if (compressedData == null || compressedData.Length == 0)
            {
                return new DecompressionResult
                {
                    Success = false,
                    Error = "Compressed data is null or empty"
                };
            }

            var startTime = DateTime.UtcNow;
            var stats = new CompressionStats
            {
                CompressedSize = compressedData.Length
            };

            try
            {
                byte[] decompressedData;

                using (var input = new MemoryStream(compressedData))
                {
                    Stream decompressionStream = algorithm switch
                    {
                        CompressionAlgorithm.GZip => new GZipStream(input, CompressionMode.Decompress),
                        CompressionAlgorithm.Deflate => new DeflateStream(input, CompressionMode.Decompress),
                        CompressionAlgorithm.Brotli => new BrotliStream(input, CompressionMode.Decompress),
                        _ => throw new ArgumentException($"Unsupported algorithm: {algorithm}")
                    };

                    using (decompressionStream)
                    using (var output = new MemoryStream())
                    {
                        await decompressionStream.CopyToAsync(output);
                        decompressedData = output.ToArray();
                    }
                }

                stats.OriginalSize = decompressedData.Length;
                stats.DecompressionTime = DateTime.UtcNow - startTime;

#if DATA_LOG
                Debug.Log($"[DataCompressor] Decompressed {stats.CompressedSize} bytes to {stats.OriginalSize} bytes " +
                         $"using {algorithm} in {stats.DecompressionTime.TotalMilliseconds:F2}ms");
#endif

                return new DecompressionResult
                {
                    Data = decompressedData,
                    Stats = stats,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new DecompressionResult
                {
                    Stats = stats,
                    Success = false,
                    Error = $"Decompression failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Compress JSON string with optimal settings for JSON data
        /// </summary>
        public static async Task<CompressionResult> CompressJsonAsync(string jsonData)
        {
            // JSON typically compresses well with GZip at optimal level
            return await CompressStringAsync(jsonData, CompressionAlgorithm.GZip, CompressionLevel.Optimal);
        }

        /// <summary>
        /// Decompress JSON data back to string
        /// </summary>
        public static async Task<string> DecompressJsonAsync(byte[] compressedJsonData)
        {
            var result = await DecompressToStringAsync(compressedJsonData, CompressionAlgorithm.GZip);
            
            if (!result.Success)
            {
                Debug.LogError($"[DataCompressor] JSON decompression failed: {result.Error}");
                return null;
            }

            return Encoding.UTF8.GetString(result.Data);
        }

        /// <summary>
        /// Test compression efficiency for given data with different algorithms
        /// </summary>
        public static async Task<Dictionary<CompressionAlgorithm, CompressionStats>> TestCompressionEfficiencyAsync(byte[] data)
        {
            var results = new Dictionary<CompressionAlgorithm, CompressionStats>();

            foreach (CompressionAlgorithm algorithm in Enum.GetValues(typeof(CompressionAlgorithm)))
            {
                try
                {
                    var result = await CompressBytesAsync(data, algorithm, CompressionLevel.Optimal);
                    if (result.Success)
                    {
                        results[algorithm] = result.Stats;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[DataCompressor] Failed to test {algorithm}: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// Get recommended compression algorithm for given data size
        /// </summary>
        public static CompressionAlgorithm GetRecommendedAlgorithm(long dataSize)
        {
            // Small data: Use Deflate for speed
            if (dataSize < 1024) // < 1KB
                return CompressionAlgorithm.Deflate;
            
            // Medium data: Use GZip for balance
            if (dataSize < 1024 * 1024) // < 1MB
                return CompressionAlgorithm.GZip;
            
            // Large data: Use Brotli for best compression
            return CompressionAlgorithm.Brotli;
        }

        /// <summary>
        /// Check if compression would be beneficial for given data
        /// </summary>
        public static bool ShouldCompress(byte[] data, double minCompressionRatio = 0.8)
        {
            if (data == null || data.Length < 100) // Don't compress very small data
                return false;

            // Quick heuristic: Check entropy of first 1KB
            var sampleSize = Math.Min(1024, data.Length);
            var entropy = CalculateEntropy(data, sampleSize);
            
            // High entropy data (like already compressed or encrypted) won't compress well
            return entropy < 7.5; // Threshold for compressible data
        }

        /// <summary>
        /// Calculate Shannon entropy for data sample
        /// </summary>
        private static double CalculateEntropy(byte[] data, int sampleSize)
        {
            var frequency = new int[256];
            
            for (int i = 0; i < sampleSize; i++)
            {
                frequency[data[i]]++;
            }

            double entropy = 0;
            for (int i = 0; i < 256; i++)
            {
                if (frequency[i] > 0)
                {
                    var probability = (double)frequency[i] / sampleSize;
                    entropy -= probability * Math.Log2(probability);
                }
            }

            return entropy;
        }

        /// <summary>
        /// Convert compression level enum to system compression level
        /// </summary>
        private static System.IO.Compression.CompressionLevel GetCompressionLevel(CompressionLevel level)
        {
            return level switch
            {
                CompressionLevel.Optimal => System.IO.Compression.CompressionLevel.Optimal,
                CompressionLevel.Fastest => System.IO.Compression.CompressionLevel.Fastest,
                CompressionLevel.NoCompression => System.IO.Compression.CompressionLevel.NoCompression,
                CompressionLevel.SmallestSize => System.IO.Compression.CompressionLevel.SmallestSize,
                _ => System.IO.Compression.CompressionLevel.Optimal
            };
        }

        /// <summary>
        /// Format file size for human-readable display
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
