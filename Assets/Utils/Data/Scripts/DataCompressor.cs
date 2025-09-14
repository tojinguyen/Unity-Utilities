using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.Data
{
    public class CompressionStats
    {
        public long OriginalSize { get; set; }
        public long CompressedSize { get; set; }
        public double CompressionRatio => OriginalSize > 0 ? (double)CompressedSize / OriginalSize : 0;
        public double SpaceSavedPercent => OriginalSize > 0 ? (1.0 - CompressionRatio) * 100 : 0;
        public TimeSpan CompressionTime { get; internal set; }
        public TimeSpan DecompressionTime { get; internal set; }
    }

    public class CompressionResult
    {
        public byte[] Data { get; internal set; }
        public CompressionStats Stats { get; set; }
        public bool Success { get; set; }
        public string Error { get; internal set; }
    }

    public class DecompressionResult
    {
        public byte[] Data { get; set; }
        public CompressionStats Stats { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    public static class DataCompressor
    {
        public enum CompressionAlgorithm
        {
            GZip,
            Deflate,
            Brotli
        }

        public enum CompressionLevel
        {
            Optimal = 0,
            Fastest = 1,
            NoCompression = 2,
            MaxCompression = 3
        }

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
                    Data = Encoding.UTF8.GetBytes(stringData),
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

        public static async Task<CompressionResult> CompressJsonAsync(string jsonData)
        {
            return await CompressStringAsync(jsonData);
        }

        public static async Task<string> DecompressJsonAsync(byte[] compressedJsonData)
        {
            var result = await DecompressToStringAsync(compressedJsonData);
            
            if (!result.Success)
            {
                Debug.LogError($"[DataCompressor] JSON decompression failed: {result.Error}");
                return null;
            }

            return Encoding.UTF8.GetString(result.Data);
        }

        public static async Task<Dictionary<CompressionAlgorithm, CompressionStats>> TestCompressionEfficiencyAsync(byte[] data)
        {
            var results = new Dictionary<CompressionAlgorithm, CompressionStats>();

            foreach (CompressionAlgorithm algorithm in Enum.GetValues(typeof(CompressionAlgorithm)))
            {
                try
                {
                    var result = await CompressBytesAsync(data, algorithm);
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

        public static CompressionAlgorithm GetRecommendedAlgorithm(long dataSize)
        {
            if (dataSize < 1024)
                return CompressionAlgorithm.Deflate;
            
            if (dataSize < 1024 * 1024)
                return CompressionAlgorithm.GZip;
            
            return CompressionAlgorithm.Brotli;
        }

        public static bool ShouldCompress(byte[] data, double minCompressionRatio = 0.8)
        {
            if (data == null || data.Length < 100) 
                return false;

            var sampleSize = Math.Min(1024, data.Length);
            var entropy = CalculateEntropy(data, sampleSize);
            
            return entropy < 7.5;
        }

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
                    entropy -= probability * Math.Log(probability) / Math.Log(2);
                }
            }

            return entropy;
        }

        private static System.IO.Compression.CompressionLevel GetCompressionLevel(CompressionLevel level)
        {
            return level switch
            {
                CompressionLevel.Optimal => System.IO.Compression.CompressionLevel.Optimal,
                CompressionLevel.Fastest => System.IO.Compression.CompressionLevel.Fastest,
                CompressionLevel.NoCompression => System.IO.Compression.CompressionLevel.NoCompression,
                CompressionLevel.MaxCompression => System.IO.Compression.CompressionLevel.Optimal,
                _ => System.IO.Compression.CompressionLevel.Optimal
            };
        }

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
