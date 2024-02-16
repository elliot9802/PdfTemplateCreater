namespace Services
{
    /// <summary>
    /// Provides functionalities related to file operations.
    /// </summary>
    public interface IFileService
    {
        Task DeleteAsync(string path);

        bool Exists(string path);

        Task<byte[]> ReadAllBytesAsync(string path);
    }
}