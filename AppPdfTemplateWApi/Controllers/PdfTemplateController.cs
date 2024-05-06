using Microsoft.AspNetCore.Mvc;
using Models.DTO;
using Services;

namespace AppPdfTemplateWApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PdfTemplateController : ControllerBase
    {
        private readonly ILogger<PdfTemplateController> _logger;
        private readonly IPdfTemplateService _pdfService;

        public PdfTemplateController(ILogger<PdfTemplateController> logger,
                                     IPdfTemplateService pdfTemplateService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pdfService = pdfTemplateService ?? throw new ArgumentNullException(nameof(pdfTemplateService));
        }

        //POST: api/PdfTemplate/CreateCombinedPdf?webbUid={webbUid}&showEventInfo={showEventInfo}
        [HttpPost]
        public async Task<IActionResult> CreateCombinedPdf(Guid webbUid, int? showEventInfo)
        {
            if (webbUid == Guid.Empty)
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

        //POST: api/PdfTemplate/CreateTemplate?ticketId={ticketId}&saveToDb={saveToDb}
        [HttpPost]
        public async Task<IActionResult> CreateTemplate([FromForm] OptionsDto optionsDto, IFormFile bgFile)
        {
            _logger.LogInformation("Processing template creation, Name: {Name}, Save to DB: {SaveToDb}", optionsDto.Name, optionsDto.SaveToDb);
            if (optionsDto.SaveToDb && string.IsNullOrWhiteSpace(optionsDto.Name))
            {
                _logger.LogError("Template name is required when saving to the database.");
                return BadRequest("Template name is required when saving to the database.");
            }

            var ticketHandling = _pdfService.DeserializeTextElements(optionsDto.TicketHandlingJson);

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

            try
            {
                var pdfBytes = await _pdfService.CreatePdfAsync(ticketHandling, bgFileData, bgFile.FileName, optionsDto.Name, optionsDto.SaveToDb);
                return optionsDto.SaveToDb ? Ok("Template created and saved to database.") : File(pdfBytes, "application/pdf", $"{Guid.NewGuid()}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the template creation");
                return StatusCode(500, "An internal server error occurred.");
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

        //GET: api/PdfTemplate/GetAllWebbUids
        [HttpGet]
        public async Task<IActionResult> GetAllWebbUids()
        {
            try
            {
                var webbUids = await _pdfService.GetAllWebbUidsAsync();
                return Ok(webbUids);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Webb UIDs: {ErrorMessage}", ex.Message);
                return StatusCode(500, "Internal Server Error: Unable to retrieve Webb UIDs.");
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

        //GET: api/PdfTemplate/ReadTemplate?id={id}
        [HttpGet]
        public async Task<IActionResult> ReadTemplate(string id)
        {
            if (!Guid.TryParse(id, out Guid _id))
            {
                return BadRequest("Guid format error");
            }

            var template = await _pdfService.GetTemplateByIdAsync(_id);
            if (template == null)
            {
                _logger.LogWarning("Template With ID: {Id} not found.", _id);
                return NotFound($"Template with ID {_id} not found.");
            }
            return Ok(template);
        }

        //GET: api/PdfTemplate/ReadTemplatesDto
        [HttpGet]
        public async Task<IActionResult> ReadTemplatesDto()
        {
            try
            {
                var templates = await _pdfService.ReadTemplatesAsync();
                if (templates == null)
                {
                    _logger.LogWarning("Templates not found");
                    return NotFound("Templates not found.");
                }
                var dtoList = templates.ConvertAll(t => new TemplateCUdto(t));

                return Ok(dtoList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while retrieving the template(s).");
                return StatusCode(500, "An internal server error occured.");
            }
        }

        //PUT: api/PdfTemplate/UpdateTemplate
        [HttpPut]
        public async Task<IActionResult> UpdateTemplate([FromForm] TemplateCUdto templateDto, IFormFile bgFile)
        {
            if (templateDto == null || templateDto.TicketTemplateId == Guid.Empty)
            {
                _logger.LogWarning("Invalid template data received.");
                return BadRequest("Invalid template data.");
            }

            var json = templateDto.TicketHandlingJson ?? string.Empty;
            templateDto.TicketsHandling = _pdfService.DeserializeTextElements(json);

            byte[] bgFileData;
            await using (var ms = new MemoryStream())
            {
                await bgFile.CopyToAsync(ms);
                bgFileData = ms.ToArray();
            }

            try
            {
                var updatedTemplate = await _pdfService.UpdateTemplateAsync(templateDto, bgFileData, bgFile.FileName);
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