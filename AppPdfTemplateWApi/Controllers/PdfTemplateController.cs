using Microsoft.AspNetCore.Mvc;
using Models;
using Newtonsoft.Json;
using Services;

namespace AppPdfTemplateWApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PdfTemplateController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<PdfTemplateController> _logger;
        private readonly IPdfTemplateService _pdfService;

        public PdfTemplateController(IFileService fileService,
                                     ILogger<PdfTemplateController> logger,
                                     IPdfTemplateService pdfTemplateService)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pdfService = pdfTemplateService ?? throw new ArgumentNullException(nameof(pdfTemplateService));
        }

        [HttpPost("CreateTemplate")]
        public async Task<IActionResult> CreateTemplate([FromForm] TicketHandling ticketHandling,
                                                                IFormFile bgFile,
                                                                [FromForm] string? customTextElementsJson,
                                                                int ticketId,
                                                                bool saveToDb = false)
        {
            if (!TryDeserializeCustomTextElements(customTextElementsJson, out var customTextElements))
            {
                return BadRequestResponse("Invalid format for custom text elements.");
            }

            ticketHandling.CustomTextElements = customTextElements ?? new List<CustomTextElement>();

            if (bgFile == null || bgFile.Length == 0)
            {
                return BadRequestResponse("Background file missing or empty.");
            }

            try
            {
                var templateProcessResult = await ProcessTemplateCreation(ticketHandling, bgFile, ticketId, saveToDb);
                return saveToDb ? Ok(templateProcessResult) : FileResultFromBytes(templateProcessResult, $"{Guid.NewGuid()}.pdf");
            }
            catch (Exception ex)
            {
                return InternalServerErrorResponse(ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketTemplate(string id)
        {
            try
            {
                var template = await _pdfService.DeleteTemplateAsync(Guid.Parse(id));
                return Ok(template);
            }
            catch (ArgumentException ex)
            {
                return NotFoundResponse(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerErrorResponse(ex);
            }
        }

        //POST: api/pdftemplate/GetPredefinedTemplate/{showEventInfo}
        [HttpPost("GetPredefinedTemplate")]
        public async Task<IActionResult> GetPredefinedTemplate(int showEventInfo, int ticketId, IFormFile bgFile)
        {
            if (bgFile == null || bgFile.Length == 0)
            {
                return BadRequestResponse("Background file missing or empty.");
            }

            try
            {
                var templateProcessResult = await ProcessPredefinedTemplate(showEventInfo, ticketId, bgFile);
                return FileResultFromBytes(templateProcessResult, $"{Guid.NewGuid()}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFoundResponse(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerErrorResponse(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTicketTemplate()
        {
            try
            {
                var showEventInfoList = await _pdfService.ReadTemplatesAsync();
                return Ok(showEventInfoList);
            }
            catch (Exception ex)
            {
                return InternalServerErrorResponse(ex);
            }
        }

        // Helper methods
        private IActionResult BadRequestResponse(string message)
        {
            _logger.LogWarning(message);
            return BadRequest(new { message });
        }

        private async Task CleanUpFiles(params string[] filePaths)
        {
            foreach (var filePath in filePaths)
            {
                if (_fileService.Exists(filePath))
                {
                    await _fileService.DeleteAsync(filePath);
                }
            }
        }

        private FileResult FileResultFromBytes(byte[] fileBytes, string fileName)
        {
            _logger.LogInformation($"File {fileName} created successfully");
            return File(fileBytes, "application/pdf", fileName);
        }

        private IActionResult InternalServerErrorResponse(Exception ex)
        {
            _logger.LogError(ex, "An error occurred");
            return StatusCode(500, new { message = "An error occurred", error = ex.InnerException });
        }

        private IActionResult NotFoundResponse(string message)
        {
            _logger.LogWarning(message);
            return NotFound(new { message });
        }

        private async Task<byte[]> ProcessPredefinedTemplate(int showEventInfo, int ticketId, IFormFile bgFile)
        {
            string tempBgFilePath = Path.GetTempFileName();
            using (var stream = new FileStream(tempBgFilePath, FileMode.Create))
            {
                await bgFile.CopyToAsync(stream);
            }

            var ticketHandlingData = await _pdfService.GetPredefinedTicketHandlingAsync(showEventInfo);
            if (ticketHandlingData == null)
            {
                _logger.LogWarning($"No predefined TicketHandling found for ShowEventInfo: {showEventInfo}");
                await CleanUpFiles(tempBgFilePath);
                throw new KeyNotFoundException($"Predefined TicketHandling with ShowEventInfo {showEventInfo} not found.");
            }

            var ticketData = await _pdfService.GetTicketDataAsync(ticketId, showEventInfo);
            var outputPath = _pdfService.GetTemporaryPdfFilePath();
            await _pdfService.CreatePdfAsync(outputPath, ticketData, ticketHandlingData, tempBgFilePath);

            var pdfBytes = await _fileService.ReadAllBytesAsync(outputPath);
            _logger.LogInformation($"PDF created successfully. FileName: {Path.GetFileName(outputPath)}");

            await CleanUpFiles(outputPath, tempBgFilePath);
            return pdfBytes;
        }

        private async Task<byte[]> ProcessTemplateCreation(TicketHandling ticketHandling, IFormFile bgFile, int ticketId, bool saveToDb)
        {
            string tempBgFilePath = Path.GetTempFileName();
            using (var stream = new FileStream(tempBgFilePath, FileMode.Create))
            {
                await bgFile.CopyToAsync(stream);
            }

            var ticketData = await _pdfService.GetTicketDataAsync(ticketId, null);
            if (ticketData == null)
            {
                _logger.LogWarning($"No template data found for ticket creation");
                await CleanUpFiles(tempBgFilePath);
                throw new KeyNotFoundException($"Ticket Id {ticketId} Not Found");
            }

            var outputPath = _pdfService.GetTemporaryPdfFilePath();
            await _pdfService.CreatePdfAsync(outputPath, ticketData, ticketHandling, tempBgFilePath);

            byte[] pdfBytes = Array.Empty<byte>();
            if (saveToDb)
            {
                TemplateCUdto templateDetails = _pdfService.MapTicketHandlingToTemplateCUdto(ticketHandling);
                await _pdfService.CreateTemplateAsync(templateDetails);
            }
            else
            {
                pdfBytes = await _fileService.ReadAllBytesAsync(outputPath);
                _logger.LogInformation($"PDF created successfully. FileName: {Path.GetFileName(outputPath)}");
            }

            await CleanUpFiles(outputPath, tempBgFilePath);
            return pdfBytes;
        }

        private bool TryDeserializeCustomTextElements(string? json, out List<CustomTextElement>? elements)
        {
            elements = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                elements = new List<CustomTextElement>();
                return true;
            }
            try
            {
                elements = JsonConvert.DeserializeObject<List<CustomTextElement>>(json);
                elements ??= new List<CustomTextElement>();
                return true;
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "Error deserializing customTextElementsJson");
                return false;
            }
        }
    }
}