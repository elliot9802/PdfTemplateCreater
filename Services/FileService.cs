using Microsoft.Extensions.Logging;

namespace Services
{
    /// <summary>
    /// Provides functionalities related to file operations.
    /// </summary>
    public interface IFileService
    {
        Task DeleteAsync(string path);
        Task WriteAllBytesAsync(string path, byte[] bytes);
        Task<byte[]> ReadAllBytesAsync(string path);
        bool Exists(string path);
    }

    /// <summary>
    /// Service responsible for file operations.
    /// </summary>
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;
        public FileService(ILogger<FileService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task DeleteAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path), "Path cannot be null or empty.");
            }
            if (File.Exists(path))
            {

                try
                {
                    _logger.LogInformation($"Deleting file at path: {path}");
                    await Task.Run(() => File.Delete(path));
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, $"Failed to delete file at path: {path}");
                    throw;
                }
            }
            else
            {
                _logger.LogWarning($"File at path: {path} does not exist. Delete operation skipped");
            }
        }

        public async Task WriteAllBytesAsync(string path, byte[] bytes)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path), "Path cannot be null or empty.");
            }
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _logger.LogWarning($"Writing to file at path: {path}");
                await File.WriteAllBytesAsync(path, bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to write bytes to file at path: {path}");
                throw;
            }
        }

        public async Task<byte[]> ReadAllBytesAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path), "Path cannot be null or empty.");
            }
            try
            {
                _logger.LogInformation($"Reading all bytes from file at path {path}");
                return await File.ReadAllBytesAsync(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to write bytes to file at path: {path}");
                throw;
            }
        }

        public bool Exists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path), "Path cannot be null or empty.");
            }

            return File.Exists(path);
        }
    }
}
