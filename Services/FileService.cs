using Microsoft.Extensions.Logging;

namespace Services
{
    /// <summary>
    /// Service responsible for file operations, providing an abstraction over file system interactions.
    /// </summary>
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;

        public FileService(ILogger<FileService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
        }

        public async Task DeleteAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path), "Path cannot be null or empty.");
            }
            if (!File.Exists(path))
            {
                _logger.LogWarning("File at path: {Path} does not exist. Delete operation skipped", path);
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Deleting file at path: {Path}.", path);
                    File.Delete(path);
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "Failed to delete file at path: {Path}.", path);
                    throw new IOException($"Failed to delete file at path: {path}.", ex);
                }
            });
        }

        public bool Exists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path), "Path cannot be null or empty.");
            }

            return File.Exists(path);
        }

        public async Task<byte[]> ReadAllBytesAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path), "Path cannot be null or empty.");
            }

            try
            {
                _logger.LogInformation("Reading all bytes from file at path {Path}.", path);
                return await File.ReadAllBytesAsync(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write bytes to file at path: {Path}", path);
                throw new IOException($"Failed to read bytes from file at path: {path}.", ex);
            }
        }

        public async Task WriteFileAsync(string path, byte[] data)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path), "Path cannot be null or empty.");
            }

            if (data == null || data.Length == 0)
            {
                throw new ArgumentNullException(nameof(data), "Data cannot be null or empty.");
            }

            var directory = Path.GetDirectoryName(path) ?? throw new InvalidOperationException("Could not determine the directory for the specified path.");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogInformation("Created directory at path: {Directory}.", directory);
            }

            try
            {
                _logger.LogInformation("Writing data to file at path: {Path}.", path);
                await File.WriteAllBytesAsync(path, data);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Failed to write bytes to file at path: {Path}.", path);
                throw new IOException($"Failed to write bytes to file at path: {path}.", ex);
            }
        }

        public class BasicFileInfo
        {
            public string FileName { get; set; }
            public Stream FileStream { get; set; }
        }

        public async Task<string> SaveTemporaryFileAsync(BasicFileInfo file)
        {
            var filePath = Path.Combine(Path.GetTempPath(), file.FileName);
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.FileStream.CopyToAsync(stream);
            }
            return filePath;
        }
    }
}