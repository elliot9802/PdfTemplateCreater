using Microsoft.Extensions.Logging;
using Models;
using DbModels;
namespace Services
{
    public class CreationService : ICreationService
    {
        private readonly IFileService _fileService;
        private readonly IPdfTemplateService _pdfTemplateService;
        private readonly ILogger<CreationService> _logger;

        public CreationService(IFileService fileService, IPdfTemplateService pdfUtility, ILogger<CreationService> logger)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _pdfTemplateService = pdfUtility ?? throw new ArgumentNullException(nameof(fileService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string GetTemporaryPdfFilePath()
        {
            string tempDirectory = Path.GetTempPath();
            string fileName = Guid.NewGuid().ToString("N") + ".pdf";
            return Path.Combine(tempDirectory, fileName);
        }

        public async Task<byte[]> CreateAndSavePdfAsync(TicketsDataDto ticketData, TicketHandling ticketDetails, string backgroundImagePath)
        {
            string outputPath = GetTemporaryPdfFilePath();
            try
            {
                _logger.LogInformation("Starting PDF creation.");
                await _pdfTemplateService.CreatePdfAsync(outputPath, ticketData, ticketDetails, backgroundImagePath);
                byte[] pdfBytes = await _fileService.ReadAllBytesAsync(outputPath);
                _logger.LogInformation("Pdf Creation completed.");
                return pdfBytes;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to create PDF: {ex.Message}";
                _logger.LogError(errorMessage, ex);
                throw new PdfCreationException(errorMessage, ex);
            }
            finally
            {
                if (_fileService.Exists(outputPath))
                {
                    await _fileService.DeleteAsync(outputPath);
                }
            }
        }
    }
}

/// <summary>
/// Represents errors that occur during PDF conversion.
/// </summary>
public class PdfCreationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfCreationException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public PdfCreationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}