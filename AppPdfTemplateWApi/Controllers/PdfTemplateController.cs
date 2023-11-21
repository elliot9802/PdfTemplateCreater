using DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Newtonsoft.Json;
using Services;

namespace AppPdfTemplateWApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PdfTemplateController : ControllerBase
    {

        private readonly ILogger<PdfTemplateController> _logger;
        private readonly IFileService _fileService;
        private readonly ICreationService _pdfService;
        private readonly IPdfTemplateService _pdfTemplateService;

        public PdfTemplateController(ICreationService pdfService, IPdfTemplateService pdfTemplateService, IFileService fileService, ILogger<PdfTemplateController> logger)
        {
            _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
            _pdfTemplateService = pdfTemplateService ?? throw new ArgumentNullException(nameof(pdfTemplateService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("CreateOrUpdateTemplate")]
        public async Task<IActionResult> CreateOrUpdateTemplate(
        [FromForm] TicketHandling ticketHandling,
        IFormFile bgFile,
        [FromForm] string customTextElementsJson, // Accept JSON string
        bool saveToDb = false)
        {
            // Manually deserialize the JSON string to List<CustomTextElement>
            if (!string.IsNullOrEmpty(customTextElementsJson))
            {
                ticketHandling.CustomTextElements =
                    JsonConvert.DeserializeObject<List<CustomTextElement>>(customTextElementsJson);
            }

            var tempBgFilePath = Path.GetTempFileName();
            string outputPath = _pdfService.GetTemporaryPdfFilePath();

            if (bgFile == null || bgFile.Length == 0)
            {
                _logger.LogWarning("Background file missing or empty.");
                return BadRequest("Please provide a background image file.");
            }
            try
            {
                // Generate the PDF Preview
                using (var stream = new FileStream(tempBgFilePath, FileMode.Create))
                {
                    await bgFile.CopyToAsync(stream);
                }

                var templateData = await _pdfTemplateService.GetTicketDataAsync(ticketHandling);
                if (templateData == null)
                {
                    _logger.LogWarning($"No template data found for ticket creation");
                    return NotFound("Ticket Id Not Found");
                }

                await _pdfTemplateService.CreatePdfAsync(outputPath, templateData, ticketHandling, tempBgFilePath);

                // Step 2: If the user wants to save the template, save the details to the database
                if (saveToDb)
                {
                    TemplateCUdto templateDetails = _pdfTemplateService.MapTicketHandlingToTemplateCUdto(ticketHandling);
                    var createdTemplate = await _pdfTemplateService.CreateTemplateAsync(templateDetails);

                    // Return the created template details
                    return Ok(createdTemplate);
                }
                else
                {
                    var pdfBytes = await _fileService.ReadAllBytesAsync(outputPath);
                    var fileName = $"{Guid.NewGuid()}.pdf";

                    _logger.LogInformation($"PDF created successfully. FileName: {fileName}");
                    return File(pdfBytes, "application/pdf", fileName);

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured while creating PDF");
                return StatusCode(500, new
                {
                    message = "Error creating PDF",
                    error = ex.Message
                });
            }
            finally
            {
                _logger.LogInformation($"Cleaning up temporary files.");
                await _fileService.DeleteAsync(tempBgFilePath);
                if (_fileService.Exists(outputPath))
                {
                    await _fileService.DeleteAsync(outputPath);
                }
            }
        }

        [HttpPost()]
        [ActionName("CreateTicketTemplate")]
        [ProducesResponseType(200, Type = typeof(TemplateCUdto))]
        [ProducesResponseType(400, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> CreateTicketTemplate([FromBody] TemplateCUdto _src)
        {
            // Generate a correlation Id for the current operation
            var correlationId = Guid.NewGuid().ToString();
            _logger.LogInformation("CreateTicketTemplate invoked with correlation id: {CorrelationId}", correlationId);
            // Extract and deserialize the TicketHandlingJson to a TicketHandling object
            // This assumes the TemplateCUdto class in the API has a TicketHandlingJson property of type string
            if (!string.IsNullOrEmpty(_src.TicketHandlingJson))
            {
                _src.TicketsHandling = JsonConvert.DeserializeObject<TicketHandling>(_src.TicketHandlingJson);
            }
            try
            {
                var createdTemplate = await _pdfTemplateService.CreateTemplateAsync(_src);
                return Ok(createdTemplate);
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "Argument exception in CreateTicketTemplate with correlation id: {CorrelationId} and source: {@TemplateCUdto}", correlationId, _src);
                return BadRequest(new ProblemDetails { Title = "Invalid argument", Detail = argEx.Message, Instance = correlationId });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update exception in CreateTicketTemplate with correlation id: {CorrelationId} and source: {@TemplateCUdto}", correlationId, _src);
                // You might want to return a 500 error for database-related issues
                return StatusCode(500, new ProblemDetails { Title = "Database error", Detail = "Error occurred while updating the database.", Instance = correlationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in CreateTicketTemplate with correlation id: {CorrelationId} and source: {@TemplateCUdto}", correlationId, _src);
                return StatusCode(500, new ProblemDetails { Title = "Server error", Detail = "An unexpected error occurred.", Instance = correlationId });
            }
        }

        [HttpPost()]
        [ActionName("createPdf")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> CreatePdf([FromForm] TicketHandling ticketRequest, IFormFile bgFile)
        {
            if (bgFile == null || bgFile.Length == 0)
            {
                _logger.LogWarning("Background file missing or empty.");
                return BadRequest("Please provide a background image file.");
            }
            var tempBgFilePath = Path.GetTempFileName();
            string outputPath = _pdfService.GetTemporaryPdfFilePath();

            try
            {
                using (var stream = new FileStream(tempBgFilePath, FileMode.Create))
                {
                    await bgFile.CopyToAsync(stream);
                }

                var templateData = await _pdfTemplateService.GetTicketDataAsync(ticketRequest);
                if (templateData == null)
                {
                    _logger.LogWarning($"No template data found for ticket creation");
                    return NotFound("Ticket Id Not Found");
                }

                await _pdfTemplateService.CreatePdfAsync(outputPath, templateData, ticketRequest, tempBgFilePath);


                var pdfBytes = await _fileService.ReadAllBytesAsync(outputPath);
                var fileName = $"{Guid.NewGuid()}.pdf";

                _logger.LogInformation($"PDF created successfully. FileName: {fileName}");
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured while creating PDF");
                return StatusCode(500, new { message = "Error creating PDF", error = ex.Message });
            }
            finally
            {
                _logger.LogInformation($"Cleaning up temporary files.");
                await _fileService.DeleteAsync(tempBgFilePath);
                if (_fileService.Exists(outputPath))
                {
                    await _fileService.DeleteAsync(outputPath);
                }
            }
        }

        [HttpDelete("{id}")]
        [ActionName("DeleteTicketTemplate")]
        [ProducesResponseType(200, Type = typeof(TicketTemplateDbM))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> DeleteTicketTemplate(string id)
        {
            try
            {
                var _id = Guid.Parse(id);
                var template = await _pdfTemplateService.DeleteTemplateAsync(_id);
                return Ok(template);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message);
            }
        }

        //// PUT: /PdfTemplate/Update/{templateName}
        //[HttpPut("Update/{templateName}")]
        //public ActionResult UpdateTemplate(string templateName, [FromBody] csPdfTemplate updatedTemplate)
        //{
        //    var template = InMemoryTemplateStore.Templates.FirstOrDefault(t => t.TemplateName == templateName);
        //    if (template == null)
        //    {
        //        return NotFound("Template not found.");
        //    }

        //    // Update logic
        //    template.Columns = updatedTemplate.Columns;

        //    return Ok("Template updated successfully.");
        //}


        //[HttpGet("Get/{templateName}")]
        //public ActionResult<csPdfTemplate> GetTemplate(string templateName)
        //{
        //    var template = InMemoryTemplateStore.Templates.FirstOrDefault(t => t.TemplateName == templateName);
        //    if (template == null)
        //    {
        //        return NotFound("Template not found.");
        //    }

        //    return Ok(template);
        //}


        //// GET: /PdfTemplate/List
        //[HttpGet("List")]
        //public ActionResult<IEnumerable<csPdfTemplate>> ListTemplates()
        //{
        //    return Ok(InMemoryTemplateStore.Templates);
        //}
    }
}