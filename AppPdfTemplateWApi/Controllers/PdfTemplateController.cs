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

            if (!TryDeserializeCustomTextElements(customTextElementsJson, out var customTextElements))
            {
                _logger.LogWarning("Invalid format for custom text elements.");
                return BadRequest("Invalid format for custom text elements.");
            }

            if (bgFile == null || bgFile.Length == 0)
            {
                _logger.LogWarning("Background file missing or empty.");
                return BadRequest("Background file missing or empty.");
            }

            byte[] bgFileData;
            await using (var ms = new MemoryStream())
            {
                await bgFile.CopyToAsync(ms);
                bgFileData = ms.ToArray();
            }

            ticketHandling.CustomTextElements = customTextElements ?? new List<CustomTextElement>();

            try
            {
                await _pdfService.CreatePdfAsync(ticketHandling, bgFileData, bgFile.FileName, name, saveToDb);
                return saveToDb ? Ok("Template created and saved to database.") : Ok("PDF created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the template creation");
                return StatusCode(500, "An internal server error occurred.");
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

        //DELETE: api/PdfTemplate/DeleteTicketTemplate/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketTemplate(string id)
        {
            if (!Guid.TryParse(id, out Guid parseId))
            {
                return BadRequest("Guid format error");
            }

            try
            {
                await _pdfService.DeleteTemplateAsync(parseId);
                return Ok($"Template with ID {id} has been deleted successfully.");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                return NotFound($"Template not found. Details: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while attempting to delete the template: {ErrorMessage}", ex.Message);
                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }

        //POST: api/PdfTemplate/GetPredefinedTemplate?showEventInfo={showEventInfo}
        [HttpPost]
        public async Task<IActionResult> GetPredefinedTemplate(int showEventInfo)
        {
            try
            {
                var pdfBytes = await _pdfService.GeneratePredefinedPdfAsync(showEventInfo);
                return File(pdfBytes, "application/pdf", $"{Guid.NewGuid()}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Predefined template not found.");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the predefined template.");
                return StatusCode(500, "An internal server error occurred.");
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
                    if (template == null)
                    {
                        _logger.LogWarning("Template With ID: {Id} not found.", ticketTemplateId);
                        return NotFound($"Template with ID {ticketTemplateId} not found.");
                    }
                    return Ok(template);
                }
                else
                {
                    var templates = await _pdfService.ReadTemplatesAsync();
                    return Ok(templates);
                }
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
                if (updatedTemplate == null)
                {
                    _logger.LogWarning("Template with ID {Id} not found.", templateDto.TicketTemplateId);
                    return NotFound($"Template with ID {templateDto.TicketTemplateId} not found.");
                }
                return Ok(updatedTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while updating the template.");
                return StatusCode(500, "An internal server error occured.");
            }
        }

        //POST: api/PdfTemplate/CreateCombinedPdf?webbUid={webbUid}
        [HttpPost]
        public async Task<IActionResult> CreateCombinedPdf(Guid webbUid, int showEventInfo)
        {
            if (webbUid == Guid.Empty || showEventInfo <= 0)
            {
                _logger.LogWarning("CreateCombinedPdf called with invalid parameters. WebbUid: {WebbUid}, ShowEventInfo: {ShowEventInfo}", webbUid, showEventInfo);
                return BadRequest("Invalid request parameters.");
            }

            try
            {
                var pdfBytes = await _pdfService.CreateCombinedPdfAsync(webbUid, showEventInfo);
                return File(pdfBytes, "application/pdf", $"Tickets_{webbUid}.pdf");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error creating combined PDF due to invalid argument: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Specified data not found: {ErrorMessage}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating combined PDF: {ErrorMessage}", ex.Message);
                return StatusCode(500, "An internal server error occurred.");
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
    }
}