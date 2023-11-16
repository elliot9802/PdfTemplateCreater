using DbModels;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace AppPdfTemplateWApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        [HttpPost()]
        [ActionName("CreateTicketTemplate")]
        [ProducesResponseType(200, Type = typeof(TemplateCUdto))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> CreateTicketTemplate([FromBody] TemplateCUdto _src)
        {
            try
            {
                //if (_src.TicketTemplateId != null)
                //    throw new ArgumentException($"{nameof(_src.TicketTemplateId)} must be null when creating a new ticket template");
                var pn = await _pdfTemplateService.CreateTemplateAsync(_src);

                return Ok(pn);
            }
            catch (Exception ex)
            {
                return BadRequest($"Could not create Ticket Template. Error {ex.InnerException}");
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

        [HttpPost("createPdf")]
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