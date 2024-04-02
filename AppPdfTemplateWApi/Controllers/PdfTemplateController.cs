using DbModels;
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

        //POST: api/PdfTemplate/CreateTemplate?ticketId={ticketId}&saveToDb={saveToDb}
        [HttpPost]
        public async Task<IActionResult> CreateTemplate([FromForm] TicketHandling ticketHandling,
                                                                IFormFile bgFile,
                                                                [FromForm] string? customTextElementsJson,
                                                                [FromForm] string? name,
                                                                bool saveToDb = false)
        {
            _logger.LogInformation("Processing template creation, Name: {name}, Save to DB: {saveToDb}", name, saveToDb);

            if (saveToDb && string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Template name is required when saving to the database.");
                return BadRequest("Template name is required when saving to the database.");
            }

            if (!TryDeserializeCustomTextElements(customTextElementsJson, out var customTextElements))
            {
                _logger.LogWarning("Invalid format for custom text elements.");
                return BadRequest("Invalid format for custom text elements.");
            }

            ticketHandling.CustomTextElements = customTextElements ?? new List<CustomTextElement>();

            if (bgFile == null || bgFile.Length == 0)
            {
                _logger.LogWarning("Background file missing or empty.");
                return BadRequest("Background file missing or empty.");
            }

            try
            {
                var templateProcessResult = await ProcessTemplateCreation(ticketHandling, bgFile, name ?? "Template Name", saveToDb);
                return saveToDb ? Ok(templateProcessResult) : File(templateProcessResult, "application/pdf", $"{Guid.NewGuid()}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while processing the template creation");
                return StatusCode(500, "An internal server error occured.");
            }
        }

        //DELETE: api/PdfTemplate/DeleteTicketTemplate/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketTemplate(string id)
        {
            try
            {
                await _pdfService.DeleteTemplateAsync(Guid.Parse(id));
                return Ok($"Template with ID {id} has been deleted successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Template with ID {id} not found.", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while deleting the template.");
                return StatusCode(500, "An internal server error occured.");
            }
        }

        //POST: api/PdfTemplate/GetPredefinedTemplate?showEventInfo={showEventInfo}&ticketId={ticketId}
        [HttpPost]
        public async Task<IActionResult> GetPredefinedTemplate(int showEventInfo, IFormFile bgFile)
        {
            if (bgFile == null || bgFile.Length == 0)
            {
                _logger.LogWarning("Background file missing or empty.");
                return BadRequest("Background file missing or empty.");
            }

            try
            {
                var templateProcessResult = await ProcessPredefinedTemplate(showEventInfo, bgFile);
                return File(templateProcessResult, "application/pdf", $"{Guid.NewGuid()}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Predefined template not found.");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while getting the predefined template.");
                return StatusCode(500, "An internal server error occured.");
            }
        }

        //GET: api/PdfTemplate/GetTicketTemplate?ticketTemplateId={ticketTemplateId}
        [HttpGet]
        public async Task<IActionResult> GetTicketTemplate(Guid? ticketTemplateId = null)
        {
            try
            {
                if (ticketTemplateId.HasValue)
                {
                    var template = await _pdfService.GetTemplateByIdAsync(ticketTemplateId.Value);
                    return Ok(template);
                }
                else
                {
                    var templates = await _pdfService.ReadTemplatesAsync();
                    return Ok(templates);
                }
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Template with {id} not found.", ticketTemplateId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while retrieving the template(s).");
                return StatusCode(500, "An internal server error occured.");
            }
        }

        //PUT: api/PdfTemplate/UpdateTemplate
        [HttpPut]
        public async Task<IActionResult> UpdateTemplate([FromBody] TicketTemplateDto templateDto)
        {
            if (templateDto == null || templateDto.TicketTemplateId == Guid.Empty)
            {
                _logger.LogWarning("Invalid template data received.");
                return BadRequest("Invalid template data.");
            }

            try
            {
                var updatedTemplate = await _pdfService.UpdateTemplateAsync(templateDto);
                return Ok(updatedTemplate);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Template to update not found.");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while updating the template.");
                return StatusCode(500, "An internal server error occured.");
            }
        }

        //POST: api/PdfTemplate/CreateCombinedPdf?webbUid={webbUid}
        [HttpPost]
        public async Task<IActionResult> CreateCombinedPdf(Guid webbUid)
        {
            string outputPath = Path.Combine(Path.GetTempPath(), $"Tickets_{Guid.NewGuid()}.pdf");

            try
            {
                byte[] pdfBytes = await _pdfService.CreateCombinedPdfAsync(webbUid, outputPath);

                System.IO.File.Delete(outputPath);

                return File(pdfBytes, "application/pdf", $"Tickets_{webbUid}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Failed to find tickets: {ErrorMessage}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating combined PDF: {ErrorMessage}", ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> WarmUp()
        {
            try
            {
                await _pdfService.ReadTemplatesAsync();
                _logger.LogInformation("API and database warmed up successfully.");
                return Ok("Warm-up successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the warm-up process.");
                return StatusCode(500, "An internal server error occurred during warm-up.");
            }
        }

        // Helper methods
        private async Task CleanUpFiles(params string[] filePaths)
        {
            foreach (var filePath in filePaths.Where(filePath => _fileService.Exists(filePath)))
            {
                try
                {
                    _logger.LogInformation("Attempting to delete file: {filePath}", filePath);
                    await _fileService.DeleteAsync(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete file: {filePath}. Continuing cleanup", filePath);
                }
            }
        }

        private async Task<byte[]> ProcessPredefinedTemplate(int showEventInfo, IFormFile bgFile)
        {
            string tempBgFilePath = Path.GetRandomFileName();
            await using (var stream = new FileStream(tempBgFilePath, FileMode.Create))
            {
                await bgFile.CopyToAsync(stream);
            }

            var ticketHandlingData = await _pdfService.GetPredefinedTicketHandlingAsync(showEventInfo);
            if (ticketHandlingData == null)
            {
                _logger.LogWarning("No predefined TicketHandling found for ShowEventInfo: {showEventInfo}", showEventInfo);
                await CleanUpFiles(tempBgFilePath);
                throw new KeyNotFoundException($"Predefined TicketHandling with ShowEventInfo {showEventInfo} not found.");
            }

            //var ticketData = await _pdfService.GetTicketDataAsync(ticketId, showEventInfo);
            //if (ticketData == null)
            //{
            //    _logger.LogWarning("No template data found for ticket creation");
            //    await CleanUpFiles(tempBgFilePath);
            //    throw new KeyNotFoundException($"{showEventInfo} Not Found");
            //}
            var outputPath = _pdfService.GetTemporaryPdfFilePath();
            await _pdfService.CreatePdfAsync(outputPath, ticketHandlingData, tempBgFilePath);

            var pdfBytes = await _fileService.ReadAllBytesAsync(outputPath);
            _logger.LogInformation("PDF created successfully. FileName: {outputPath}", Path.GetFileName(outputPath));

            await CleanUpFiles(outputPath, tempBgFilePath);
            return pdfBytes;
        }

        private async Task<byte[]> ProcessTemplateCreation(TicketHandling ticketHandling, IFormFile bgFile, string name, bool saveToDb)
        {
            string tempBgFilePath = Path.GetRandomFileName();
            await using (var stream = new FileStream(tempBgFilePath, FileMode.Create))
            {
                await bgFile.CopyToAsync(stream);
            }

            var outputPath = _pdfService.GetTemporaryPdfFilePath();
            await _pdfService.CreatePdfAsync(outputPath, ticketHandling, tempBgFilePath);

            byte[] pdfBytes = Array.Empty<byte>();
            if (saveToDb)
            {
                TemplateCUdto templateDetails = _pdfService.MapTicketHandlingToTemplateCUdto(ticketHandling);
                templateDetails.Name = name;
                await _pdfService.CreateTemplateAsync(templateDetails);
            }
            else
            {
                pdfBytes = await _fileService.ReadAllBytesAsync(outputPath);
                _logger.LogInformation("PDF created successfully. FileName: {outputPath}", Path.GetFileName(outputPath));
            }

            await CleanUpFiles(outputPath, tempBgFilePath);
            return pdfBytes;
        }

        private bool TryDeserializeCustomTextElements(string? json, out List<CustomTextElement>? elements)
        {
            elements = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                return true;
            }
            try
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None,
                    Error = (sender, args) =>
                    {
                        _logger.LogError(args.ErrorContext.Error, "Deserialization error.");
                        args.ErrorContext.Handled = true;
                    }
                };

                elements = JsonConvert.DeserializeObject<List<CustomTextElement>>(json, settings);
                elements ??= new List<CustomTextElement>();
                return true;
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "Error deserializing customTextElementsJson: {ErrorMessage}", ex.Message);
                return false;
            }
        }
    }
}