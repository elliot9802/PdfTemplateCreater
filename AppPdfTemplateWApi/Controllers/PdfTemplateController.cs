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
            _logger.LogInformation("Processing template creation, Name: {Name}, Save to DB: {SaveToDb}", name, saveToDb);

            if (saveToDb && string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Template name is required when saving to the database.");
                return BadRequest("Template name is required when saving to the database.");
            }

            if (bgFile == null || bgFile.Length == 0)
            {
                _logger.LogWarning("Background file missing or empty.");
                return BadRequest("Background file missing or empty.");
            }

            if (!TryDeserializeCustomTextElements(customTextElementsJson, out var customTextElements))
            {
                _logger.LogWarning("Invalid format for custom text elements.");
                return BadRequest("Invalid format for custom text elements.");
            }
            ticketHandling.CustomTextElements = customTextElements ?? new List<CustomTextElement>();

            string outputPath = _pdfService.GetTemporaryPdfFilePath();
            List<string> filesToCleanUp = new() { outputPath };
            byte[] pdfBytes = Array.Empty<byte>();
            int bgFileId = 0;

            try
            {
                string bgFilePath;
                if (saveToDb)
                {
                    byte[] bgFileData;
                    await using (var ms = new MemoryStream())
                    {
                        await bgFile.CopyToAsync(ms);
                        bgFileData = ms.ToArray();
                    }
                    bgFileId = await _pdfService.SaveFileToDatabaseAsync(bgFileData, "Background Image for " + name, bgFile.FileName);
                    bgFilePath = await _pdfService.GetFilePathAsync(bgFileId);
                }
                else
                {
                    bgFilePath = Path.GetRandomFileName();
                    await using var stream = new FileStream(bgFilePath, FileMode.Create);
                    await bgFile.CopyToAsync(stream);
                    filesToCleanUp.Add(bgFilePath);
                }

                await _pdfService.CreatePdfAsync(outputPath, ticketHandling, bgFilePath);

                if (saveToDb)
                {
                    TemplateCUdto templateDetails = _pdfService.MapTicketHandlingToTemplateCUdto(ticketHandling);
                    templateDetails.Name = name!;
                    templateDetails.FileStorageID = bgFileId;
                    await _pdfService.CreateTemplateAsync(templateDetails);
                }
                else
                {
                    pdfBytes = await _fileService.ReadAllBytesAsync(outputPath);
                    _logger.LogInformation("PDF created successfully. FileName: {OutputPath}", Path.GetFileName(outputPath));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the template creation");
                return StatusCode(500, "An internal server error occurred.");
            }
            finally
            {
                await CleanUpFiles(filesToCleanUp.ToArray());
            }

            return saveToDb ? Ok("Template created and saved to database.") : File(pdfBytes, "application/pdf", $"{Guid.NewGuid()}.pdf");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while deleting the template.");
                if (ex is ArgumentException) return NotFound(ex.Message);
                return StatusCode(500, "An internal server error occured.");
            }
        }

        //POST: api/PdfTemplate/GetPredefinedTemplate?showEventInfo={showEventInfo}&ticketId={ticketId}
        [HttpPost]
        public async Task<IActionResult> GetPredefinedTemplate(int showEventInfo)
        {
            string outputPath = _pdfService.GetTemporaryPdfFilePath();
            List<string> filesToCleanUp = new() { outputPath };

            try
            {
                var ticketHandlingData = await _pdfService.GetPredefinedTicketHandlingAsync(showEventInfo);
                if (ticketHandlingData == null)
                {
                    _logger.LogWarning("No predefined TicketHandling found for ShowEventInfo: {ShowEventInfo}", showEventInfo);
                    throw new KeyNotFoundException($"Predefined TicketHandling with ShowEventInfo {showEventInfo} not found.");
                }

                var ticketTemplate = await _pdfService.GetTicketTemplateByShowEventInfoAsync(showEventInfo);
                if (ticketTemplate == null)
                {
                    _logger.LogWarning("No Template found with ShowEventInfo: {ShowEventInfo}", showEventInfo);
                    throw new KeyNotFoundException($"Template with ShowEventInfo {showEventInfo} not found.");
                }

                await using var stream = new MemoryStream(await _pdfService.GetFileDataAsync(ticketTemplate.FileStorageID));
                string tempBgFilePath = Path.GetRandomFileName();
                await using (var fileStream = new FileStream(tempBgFilePath, FileMode.Create))
                {
                    await stream.CopyToAsync(fileStream);
                }
                filesToCleanUp.Add(tempBgFilePath);

                await _pdfService.CreatePdfAsync(outputPath, ticketHandlingData, tempBgFilePath);

                var pdfBytes = await _fileService.ReadAllBytesAsync(outputPath);
                _logger.LogInformation("PDF created successfully. FileName: {OutputPath}", Path.GetFileName(outputPath));

                return File(pdfBytes, "application/pdf", $"{Guid.NewGuid()}.pdf");
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
            finally
            {
                await CleanUpFiles(filesToCleanUp.ToArray());
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
                _logger.LogWarning(ex, "Template with {Id} not found.", ticketTemplateId);
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
                return File(pdfBytes, "application/pdf", $"Tickets_{webbUid}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating combined PDF: {ErrorMessage}", ex.Message);
                if (ex is ArgumentException) return NotFound(ex.Message);
                return StatusCode(500, "An internal server error occured.");
            }
            finally
            {
                await CleanUpFiles(outputPath);
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
                    _logger.LogInformation("Attempting to delete file: {FilePath}", filePath);
                    await _fileService.DeleteAsync(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete file: {FilePath}. Continuing cleanup", filePath);
                }
            }
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