using Configuration;
using DbContext;
using DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Barcode;
using Syncfusion.Pdf.Graphics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Services
{
    public class PdfTemplateService : IPdfTemplateService
    {
        #region constructor logic

        private readonly string _backgroundImagePath;
        private readonly string _dbLogin;
        private readonly ILogger<PdfTemplateService> _logger;
        private readonly string _scissorsLineImagePath;

        public PdfTemplateService(ILogger<PdfTemplateService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var dbLoginDetail = AppConfig.DbLoginDetails("DbLogins");
            _dbLogin = dbLoginDetail.DbConnectionString ?? throw new InvalidOperationException("Database connection string is not configured.");

            var imagePaths = AppConfig.ImagePathSettings;
            _backgroundImagePath = imagePaths.BackgroundImagePath ?? throw new KeyNotFoundException("BackgroundImagePath configuration is missing.");
            _scissorsLineImagePath = imagePaths.ScissorsLineImagePath ?? throw new KeyNotFoundException("ScissorsLineImagePath configuration is missing.");
        }

        #endregion constructor logic

        #region database methods

        //public async Task<int> SaveFileToDatabaseAsync(byte[] fileData, int fileTypeID, int fileCategoryID, string description, string name)
        //{
        //    if (fileData == null || fileData.Length == 0)
        //    {
        //        _logger.LogError("Attempted to save an empty or null file data.");
        //        throw new ArgumentException("The file data cannot be empty or null.");
        //    }

        //    await using var db = TicketTemplateDbContext.Create(_dbLogin);
        //    var fileStorage = new FileStorage
        //    {
        //        Data = fileData,
        //        CreateTime = DateTime.UtcNow,
        //        ValidState = 1 // Assuming 1 is for valid
        //    };

        //    try
        //    {
        //        await db.FileStorage.AddAsync(fileStorage);
        //        await db.SaveChangesAsync();

        //        var fileDescription = new FileDescription
        //        {
        //            FileStorageID = fileStorage.FileStorageID,
        //            FileTypeID = fileTypeID,
        //            FileCategoryID = fileCategoryID,
        //            Description = description,
        //            Name = name,
        //            CreateTime = DateTime.UtcNow,
        //            ValidState = 1
        //        };

        //        await db.FileDescription.AddAsync(fileDescription);
        //        await db.SaveChangesAsync();

        //        return fileStorage.FileStorageID; // Return the ID of the saved file for linking purposes
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error saving file to database. Details: {Message}", ex.Message);
        //        throw new InvalidOperationException("Failed to save the file to the database.", ex);
        //    }
        //}

        //public async Task<byte[]> GetFileDataAsync(int fileStorageID)
        //{
        //    await using var db = TicketTemplateDbContext.Create(_dbLogin);

        //    try
        //    {
        //        var fileStorage = await db.FileStorage.FindAsync(fileStorageID);

        //        if (fileStorage == null)
        //        {
        //            _logger.LogWarning("File with ID {FileStorageID} not found.", fileStorageID);
        //            throw new KeyNotFoundException($"File with ID {fileStorageID} not found.");
        //        }

        //        return fileStorage.Data;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving file data for FileStorageID {FileStorageID}. Details: {Message}", fileStorageID, ex.Message);
        //        throw new InvalidOperationException($"Failed to retrieve the file data for FileStorageID {fileStorageID}.", ex);
        //    }
        //}

        public async Task<TicketHandling?> GetPredefinedTicketHandlingAsync(int showEventInfo)
        {
            try
            {
                await using var db = TicketTemplateDbContext.Create(_dbLogin);
                var predefinedTemplate = await db.TicketTemplate
                                                 .AsNoTracking()
                                                 .FirstOrDefaultAsync(t => t.ShowEventInfo == showEventInfo);

                if (predefinedTemplate == null)
                {
                    _logger.LogWarning("No predefined TicketHandling found for ShowEventInfo: {ShowEventInfo}", showEventInfo);
                    return null;
                }

                return predefinedTemplate.TicketsHandling;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPredefinedTicketHandlingAsync: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<TicketTemplateDto> GetTemplateByIdAsync(Guid ticketTemplateId)
        {
            await using var db = TicketTemplateDbContext.Create(_dbLogin);
            var template = await db.TicketTemplate
                .Where(t => t.TicketTemplateId == ticketTemplateId)
                .Select(t => new TicketTemplateDto
                {
                    TicketTemplateId = t.TicketTemplateId,
                    ShowEventInfo = t.ShowEventInfo,
                    TicketHandlingJson = t.TicketsHandlingJson,
                    Name = t.Name
                }).FirstOrDefaultAsync();

            return template ?? throw new KeyNotFoundException($"Template with ID {ticketTemplateId} not found.");
        }

        public async Task<TicketsDataView?> GetTicketDataAsync(int? ticketId, int? showEventInfo)
        {
            try
            {
                await using var db = TicketTemplateDbContext.Create(_dbLogin);
                var query = db.Vy_ShowTickets.AsNoTracking();

                if (ticketId.HasValue)
                    query = query.Where(t => t.platsbokad_id == ticketId.Value);
                else
                    _logger.LogWarning("GetTicketDataAsync requires either a ticketId or showEventInfo to filter the data.");

                var templateData = await query.FirstOrDefaultAsync();
                if (templateData == null)
                    _logger.LogWarning("No matching ticket data found.");
                return templateData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTicketDataAsync: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<List<TicketTemplateDto>> ReadTemplatesAsync()
        {
            try
            {
                await using var db = TicketTemplateDbContext.Create(_dbLogin);
                var templates = await db.TicketTemplate.Select(t => new TicketTemplateDto
                {
                    TicketTemplateId = t.TicketTemplateId,
                    ShowEventInfo = t.ShowEventInfo,
                    TicketHandlingJson = t.TicketsHandlingJson,
                    Name = t.Name
                }).ToListAsync();

                _logger.LogInformation("Retrieved {TemplateCount} templates", templates.Count);
                return templates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReadTemplatesAsync: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<TicketsDataView>> GetTicketsDataByWebbUidAsync(Guid webbUid)
        {
            await using var db = TicketTemplateDbContext.Create(_dbLogin);

            var tickets = await db.Vy_ShowTickets
                                  .Where(ticket => ticket.WebbUid == webbUid)
                                  .ToListAsync();

            _logger.LogInformation("{ticketsCount} tickets retrieved for WebbUid: {webbUid}", tickets.Count, webbUid);
            return tickets;
        }

        public async Task<byte[]> CreateCombinedPdfAsync(Guid webbUid, string outputPath)
        {
            var ticketsData = await GetTicketsDataByWebbUidAsync(webbUid);
            if (!ticketsData.Any())
            {
                _logger.LogWarning("No tickets found for WebbUid: {WebbUid}", webbUid);
                throw new KeyNotFoundException($"No tickets found for WebbUid: {webbUid}.");
            }

            using var document = new PdfDocument();
            foreach (var ticketData in ticketsData)
            {
                var ticketHandling = await GetPredefinedTicketHandlingAsync(ticketData.showEventInfo);
                if (ticketHandling == null)
                {
                    _logger.LogWarning("No TicketHandling found for ShowEventInfo: {ShowEventInfo}, skipping ticket.", ticketData.showEventInfo);
                    continue;
                }

                var page = document.Pages.Add();
                await DrawPageContent(page, _backgroundImagePath, ticketData, ticketHandling, new PdfStandardFont(PdfFontFamily.Helvetica, 8), new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold), new PdfStandardFont(PdfFontFamily.Helvetica, 9));
            }

            if (document.Pages.Count == 0)
            {
                _logger.LogWarning("No pages created in the document for WebbUid: {WebbUid}", webbUid);
                throw new InvalidOperationException("Failed to create any pages in the PDF document.");
            }

            await SaveDocumentAsync(document, outputPath);
            _logger.LogInformation("Combined PDF created successfully with {PageCount} pages at {OutputPath}", document.Pages.Count, outputPath);

            return await File.ReadAllBytesAsync(outputPath);
        }

        #endregion database methods

        #region Creating Pdf methods

        #region database methods

        public async Task<TicketHandling> CreateTemplateAsync(TemplateCUdto _src)
        {
            try
            {
                await using var db = TicketTemplateDbContext.Create(_dbLogin);

                var maxShowEventInfo = await db.TicketTemplate.MaxAsync(t => (int?)t.ShowEventInfo) ?? 0;
                _src.ShowEventInfo = maxShowEventInfo + 1;

                var newTicketTemplate = new TicketTemplateDbM(_src)
                {
                    ShowEventInfo = _src.ShowEventInfo
                };

                await db.TicketTemplate.AddAsync(newTicketTemplate);
                await db.SaveChangesAsync();

                _logger.LogInformation("New template created with ShowEventInfo {newShowEventInfo}", newTicketTemplate.ShowEventInfo);
                return newTicketTemplate.TicketsHandling;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateTemplateAsync: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<TicketTemplateDto> UpdateTemplateAsync(TicketTemplateDto templateDto)
        {
            await using var db = TicketTemplateDbContext.Create(_dbLogin);
            var templateToUpdate = await db.TicketTemplate.FindAsync(templateDto.TicketTemplateId) ?? throw new KeyNotFoundException($"Template with ID {templateDto.TicketTemplateId} not found.");
            if (templateDto.Name != null)
            {
                templateToUpdate.Name = templateDto.Name;
            }
            else
            {
                _logger.LogWarning("Attempted to update a template with a null name for template ID {id}.", templateDto.TicketTemplateId);
                templateToUpdate.Name = "Default Name";
            }
            if (!string.IsNullOrWhiteSpace(templateDto.TicketHandlingJson))
            {
                var ticketHandlingFromDto = JsonConvert.DeserializeObject<TicketHandling>(templateDto.TicketHandlingJson);
                if (ticketHandlingFromDto != null)
                {
                    templateToUpdate.TicketsHandling = ticketHandlingFromDto;
                }
            }

            await db.SaveChangesAsync();

            _logger.LogInformation("Template with ID {id} has been updated.", templateDto.TicketTemplateId);
            return templateDto;
        }

        public async Task<ITicketTemplate> DeleteTemplateAsync(Guid id)
        {
            try
            {
                await using var db = TicketTemplateDbContext.Create(_dbLogin);
                var templateToDelete = await db.TicketTemplate.FindAsync(id);

                if (templateToDelete == null)
                {
                    _logger.LogWarning("Template with ID {id} not found.", id);
                    throw new ArgumentException($"Item with id {id} does not exist");
                }

                db.TicketTemplate.Remove(templateToDelete);
                await db.SaveChangesAsync();

                _logger.LogInformation("Template with ID {id} has been deleted.", id);
                return templateToDelete;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteTemplateAsync: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public string GetTemporaryPdfFilePath()
        {
            string tempDirectory = Path.GetTempPath();
            string fileName = Guid.NewGuid().ToString("N") + ".pdf";
            return Path.Combine(tempDirectory, fileName);
        }

        public TemplateCUdto MapTicketHandlingToTemplateCUdto(TicketHandling ticketHandling)
        {
            return new TemplateCUdto
            {
                TicketsHandling = ticketHandling,
                TicketHandlingJson = JsonConvert.SerializeObject(ticketHandling),
            };
        }

        #endregion database methods

        public async Task CreatePdfAsync(string outputPath, TicketsDataView ticketData, TicketHandling ticketHandling, string? backgroundImagePath)
        {
            try
            {
                using PdfDocument document = new();
                PdfFont regularFont = new PdfStandardFont(PdfFontFamily.Helvetica, 8);
                PdfFont boldFont = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                PdfFont customFont = new PdfStandardFont(PdfFontFamily.Helvetica, 9);

                PdfPage page = document.Pages.Add();
                backgroundImagePath ??= _backgroundImagePath;
                await DrawPageContent(page, backgroundImagePath, ticketData, ticketHandling, regularFont, boldFont, customFont);

                await SaveDocumentAsync(document, outputPath);
                _logger.LogInformation("PDF created successfully at {outputPath}", outputPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreatePdfAsync: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        private async Task DrawPageContent(PdfPage page, string backgroundImagePath, TicketsDataView ticketData, TicketHandling ticketHandling, PdfFont regularFont, PdfFont boldFont, PdfFont customFont)
        {
            float scaleFactor = Math.Min(page.GetClientSize().Width / 1024f, 1);
            PointF ticketOrigin = new((page.GetClientSize().Width - (1024 * scaleFactor)) / 2, 0);

            DrawBackgroundImage(page, backgroundImagePath, ticketOrigin, scaleFactor);
            DrawTextContent(page.Graphics, ticketOrigin, scaleFactor, ticketData, ticketHandling, regularFont, boldFont, customFont);
            DrawCustomTextElements(page.Graphics, ticketHandling, ticketOrigin, scaleFactor);
            DrawBarcodeOrQRCode(page, ticketOrigin, scaleFactor, ticketHandling, ticketData);
            DrawScissorsLine(page.Graphics, ticketOrigin, scaleFactor, ticketHandling);
            DrawVitec(page.Graphics, scaleFactor);
            if (ticketHandling.IncludeAd)
            {
                await DrawAd(page.Graphics, page, ticketData.reklam1, ticketOrigin, scaleFactor, regularFont, ticketHandling);
            }
        }

        private static async Task DrawAd(PdfGraphics graphics, PdfPage page, string htmlContent, PointF origin, float scale, PdfFont font, TicketHandling ticketHandling)
        {
            // Extract the URL from the <img> tag, handle HTML entities and remove the <img> tag
            string? imageUrl = ExtractImageUrl(htmlContent);
            htmlContent = HandleHtmlEntities(htmlContent);
            htmlContent = RemoveImageTag(htmlContent);

            PointF adPosition = CalculateAdPosition(origin, scale, ticketHandling);

            if (!string.IsNullOrEmpty(imageUrl))
            {
                await DrawAdImage(graphics, imageUrl, adPosition, scale);
            }
            DrawHtmlContent(graphics, page, htmlContent, font, adPosition);
        }

        private static void DrawBackgroundImage(PdfPage page, string imagePath, PointF origin, float scale)
        {
            using FileStream imageStream = new(imagePath, FileMode.Open, FileAccess.Read);
            PdfBitmap background = new(imageStream);

            page.Graphics.DrawImage(background, origin.X, origin.Y, 1024 * scale, 364 * scale);
        }

        private static void DrawBarcodeOrQRCode(PdfPage page, PointF origin, float scale, TicketHandling ticketHandling, TicketsDataView ticketData)
        {
            PointF barCodePosition = CalculateBarcodePosition(origin, scale, ticketHandling);
            if (ticketHandling.UseQRCode)
            {
                DrawQRCode(page.Graphics, barCodePosition, scale, ticketData);
            }
            else
            {
                DrawBarcode(page.Graphics, barCodePosition, scale, ticketHandling, ticketData);
            }
        }

        private static void DrawCustomTextElements(PdfGraphics graphics, TicketHandling ticketHandling, PointF origin, float scale)
        {
            if (ticketHandling.CustomTextElements == null)
            {
                return;
            }
            foreach (var customTextElement in ticketHandling.CustomTextElements)
            {
                // Skip if the current CustomTextElement or its Text property is null or empty
                if (customTextElement == null || string.IsNullOrEmpty(customTextElement.Text))
                {
                    continue;
                }

                // Create a font for the custom text
                float fontSize = customTextElement.FontSize ?? 10f;
                PdfFont customFont = new PdfStandardFont(PdfFontFamily.Helvetica, fontSize);

                // Initialize default color as black in case of null or invalid color string
                PdfColor color = new(0, 0, 0);

                // Check if the color string is properly formatted before attempting to convert it
                if (!string.IsNullOrEmpty(customTextElement.Color) && customTextElement.Color.StartsWith('#') && customTextElement.Color.Length == 7)
                {
                    color = new PdfColor(
                        Convert.ToByte(customTextElement.Color.Substring(1, 2), 16),
                        Convert.ToByte(customTextElement.Color.Substring(3, 2), 16),
                        Convert.ToByte(customTextElement.Color.Substring(5, 2), 16));
                }

                // Create a brush with the color
                PdfBrush customBrush = new PdfSolidBrush(color);

                // Calculate position based on scale and origin, providing a default value if the nullable float is null
                PointF position = new(
                    origin.X + ((customTextElement.PositionX ?? 0) * scale), // if PositionX is null, default to 0
                    origin.Y + ((customTextElement.PositionY ?? 0) * scale)  // if PositionY is null, default to 0
                );

                // Draw the text
                graphics.DrawString(customTextElement.Text, customFont, customBrush, position);
            }
        }

        private void DrawScissorsLine(PdfGraphics graphics, PointF origin, float scale, TicketHandling ticketHandling)
        {
            if (ticketHandling.AddScissorsLine)
            {
                using FileStream scissorsImageStream = new(_scissorsLineImagePath, FileMode.Open, FileAccess.Read);
                PdfBitmap scissorsLineImage = new(scissorsImageStream);

                PointF scissorsPosition = new(
                origin.X, // Aligned with the left edge of the ticket
                origin.Y + (364 * scale) + (10 * scale) // Just below the ticket, 10 units of space
                );
                SizeF scissorsSize = new(1024 * scale, scissorsLineImage.Height * scale); // Full width and scaled height

                graphics.DrawImage(scissorsLineImage, scissorsPosition, scissorsSize);
            }
        }

        private void DrawTextContent(PdfGraphics graphics, PointF origin, float scale, TicketsDataView ticketData, TicketHandling ticketHandling, PdfFont regularFont, PdfFont boldFont, PdfFont customFont)
        {
            PdfFont rutbokstavFont = new PdfStandardFont(PdfFontFamily.Helvetica, 24, PdfFontStyle.Bold);

            //// Log method entry
            //_logger.LogInformation("DrawTextContent method entered");

            //// Log parameters
            //_logger.LogInformation("Origin: {origin}, Scale: {scale}", origin, scale);
            //_logger.LogInformation("TicketDetails: {ticketHandling}", JsonConvert.SerializeObject(ticketHandling));
            //_logger.LogInformation("TicketDetails: {ticketData}", JsonConvert.SerializeObject(ticketData));

            DrawTextIfCondition(graphics, ticketHandling.IncludeEmail, ticketData.eMail, origin, scale, ticketHandling.EmailPositionX, ticketHandling.EmailPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeContactPerson, ticketData.KontaktPerson, origin, scale, ticketHandling.ContactPersonPositionX, ticketHandling.ContactPersonPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeServiceFee, ticketData.serviceavgift1_kr, origin, scale, ticketHandling.ServiceFeePositionX, ticketHandling.ServiceFeePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeWebBookingNr, "Webbokningsnr: " + ticketData.webbkod, origin, scale, ticketHandling.WebBookingNrPositionX, ticketHandling.WebBookingNrPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeBookingNr, "Bokningsnr: " + ticketData.BokningsNr, origin, scale, ticketHandling.BookingNrPositionX, ticketHandling.BookingNrPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeEventName, ticketData.namn1, origin, scale, ticketHandling.EventNamePositionX, ticketHandling.EventNamePositionY, boldFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeSubEventName, ticketData.namn2, origin, scale, ticketHandling.SubEventNamePositionX, ticketHandling.SubEventNamePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeEventDate, "Köpdatum: " + ticketData.datumStart, origin, scale, ticketHandling.EventDatePositionX, ticketHandling.EventDatePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludePrice, ticketData.Pris, origin, scale, ticketHandling.PricePositionX, ticketHandling.PricePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeArtNr, ticketData.ArtikelNr, origin, scale, ticketHandling.ArtNrPositionX, ticketHandling.ArtNrPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeArtName, ticketData.Artikelnamn, origin, scale, ticketHandling.ArtNamePositionX, ticketHandling.ArtNamePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeChairRow, ticketData.stolsrad, origin, scale, ticketHandling.ChairRowPositionX, ticketHandling.ChairRowPositionY, customFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeChairNr, ticketData.stolsnr, origin, scale, ticketHandling.ChairNrPositionX, ticketHandling.ChairNrPositionY, customFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeLogorad1, ticketData.logorad1, origin, scale, ticketHandling.Logorad1PositionX, ticketHandling.Logorad1PositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeLogorad2, ticketData.logorad2, origin, scale, ticketHandling.Logorad2PositionX, ticketHandling.Logorad2PositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeSection, ticketData.namn, origin, scale, ticketHandling.SectionPositionX, ticketHandling.SectionPositionY, customFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeFacilityName, ticketData.anamn, origin, scale, ticketHandling.FacilityNamePositionX, ticketHandling.FacilityNamePositionY, customFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeStrukturArtikel, ticketData.StrukturArtikel, origin, scale, ticketHandling.StrukturArtikelPositionX, ticketHandling.StrukturArtikelPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeDescription, ticketData.Beskrivning, origin, scale, ticketHandling.DescriptionPositionX, ticketHandling.DescriptionPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeArtNotText, ticketData.ArtNotText, origin, scale, ticketHandling.ArtNotTextPositionX, ticketHandling.ArtNotTextPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeDatum, ticketData.Datum, origin, scale, ticketHandling.DatumPositionX, ticketHandling.DatumPositionY, customFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeEntrance, ticketData.Ingang, origin, scale, ticketHandling.EntrancePositionX, ticketHandling.EntrancePositionY, customFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeRutBokstav, ticketData.Rutbokstav, origin, scale, ticketHandling.RutBokstavPositionX, ticketHandling.RutBokstavPositionY, rutbokstavFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeWebbcode, ticketData.Webbcode, origin, scale, ticketHandling.WebbcodePositionX, ticketHandling.WebbcodePositionY, regularFont);

            //_logger.LogInformation("DrawTextContent method exited");
        }

        private static void DrawTextIfCondition(PdfGraphics graphics, bool condition, object? value, PointF origin, float scale, float? positionX, float? positionY, PdfFont font, string? format = null)
        {
            if (condition && value != null)
            {
                PointF position = new(
                    origin.X + ((positionX ?? 0) * scale),
                    origin.Y + ((positionY ?? 0) * scale));
                string text = value is IFormattable formattableValue
                    ? formattableValue.ToString(format, CultureInfo.InvariantCulture)
#pragma warning disable S2589 // Suppressing S2589: 'value?.ToString() ?? string.Empty' handles nulls safely for dynamic 'value'.
                    : value?.ToString() ?? string.Empty;
#pragma warning restore S2589 // Necessary to ensure string representation for any object type, including handling of null ToString().
                graphics.DrawString(text, font, PdfBrushes.Black, position);
            }
        }

        private static void DrawVitec(PdfGraphics graphics, float scale)
        {
            const string bottomTxt = "Powered by Vitec Smart Visitor System AB";
            SizeF pageSize = graphics.ClientSize;
            PdfFont bottomTxtFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Italic);
            SizeF bottomTxtSize = bottomTxtFont.MeasureString(bottomTxt);
            PointF bottomTxtPosition = new(
                (pageSize.Width - bottomTxtSize.Width) / 2, // Centered horizontally
                pageSize.Height - bottomTxtSize.Height - (30 * scale) // 30 units from the bottom
            );
            graphics.DrawString(bottomTxt, bottomTxtFont, PdfBrushes.Black, bottomTxtPosition);
        }

        private static string? ExtractImageUrl(string htmlContent)
        {
            // Extracts URL from <img> tag
            var match = Regex.Match(htmlContent, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }

        private static string HandleHtmlEntities(string htmlContent)
        {
            // Handles common HTML entities
            Dictionary<string, string> entities = new()
            {
                {"&nbsp;", "\u00A0"}, {"&auml;", "ä"}, {"&aring;", "å"}, {"</p>", "</p><br/>"}
            };
            foreach (var entity in entities)
            {
                htmlContent = htmlContent.Replace(entity.Key, entity.Value);
            }
            return htmlContent;
        }

        private static string RemoveImageTag(string htmlContent)
        {
            // Removes <img> tag from HTML content
            return Regex.Replace(htmlContent, "<img.+?>", "", RegexOptions.IgnoreCase);
        }

        private async Task SaveDocumentAsync(PdfDocument document, string outputPath)
        {
            try
            {
                await using FileStream stream = new(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
                document.Save(stream);
                _logger.LogInformation("PDF Ticket Creation succeeded and saved to {outputPath}", outputPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PDF Ticket creation failed: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        #endregion Creating Pdf methods

        #region Helper methods

        private static PointF CalculateAdPosition(PointF origin, float scale, TicketHandling ticketHandling)
        {
            return new PointF(
                origin.X + ((ticketHandling.AdPositionX ?? 0) * scale),
                origin.Y + ((ticketHandling.AdPositionY ?? 500) * scale)
            );
        }

        private static PointF CalculateBarcodePosition(PointF origin, float scale, TicketHandling ticketHandling)
        {
            return new PointF(
                origin.X + ((ticketHandling.BarcodePositionX ?? 825) * scale),
                origin.Y + ((ticketHandling.BarcodePositionY ?? 320) * scale)
            );
        }

        private static async Task DrawAdImage(PdfGraphics graphics, string imageUrl, PointF adPosition, float scale)
        {
            using HttpClient httpClient = new();
            byte[] bytes = await httpClient.GetByteArrayAsync(imageUrl);
            await using MemoryStream ms = new(bytes);
            PdfBitmap image = new(ms);

            SizeF imageSize = new(image.Width * scale, image.Height * scale);
            PointF imagePosition = new(adPosition.X, adPosition.Y);
            graphics.DrawImage(image, imagePosition, imageSize);
        }

        private static void DrawBarcode(PdfGraphics graphics, PointF barcodePosition, float scale, TicketHandling ticketHandling, TicketsDataView ticketData)
        {
            PdfCode39Barcode barcode = new()
            {
                Text = string.IsNullOrEmpty(ticketData.webbkod) ? ticketData.Webbcode : ticketData.webbkod,
                Size = new SizeF(270 * scale, 90 * scale)
            };

            if (ticketHandling.FlipBarcode)
            {
                barcode.Draw(graphics, barcodePosition);
            }
            else
            {
                graphics.Save();
                graphics.TranslateTransform(barcodePosition.X + barcode.Size.Height, barcodePosition.Y);
                graphics.RotateTransform(-90);
                barcode.Draw(graphics, PointF.Empty);
                graphics.Restore();
            }
        }

        private static void DrawHtmlContent(PdfGraphics graphics, PdfPage page, string htmlContent, PdfFont font, PointF adPosition)
        {
            RectangleF rect = new(adPosition.X, adPosition.Y, page.GetClientSize().Width, page.GetClientSize().Height);
            PdfHTMLTextElement htmlTextElement = new(htmlContent, font, PdfBrushes.Black);
            htmlTextElement.Draw(graphics, rect);
        }

        private static void DrawQRCode(PdfGraphics graphics, PointF barcodePosition, float scale, TicketsDataView ticketData)
        {
            PdfQRBarcode qrCode = new()
            {
                Text = string.IsNullOrEmpty(ticketData.webbkod) ? ticketData.Webbcode : ticketData.webbkod,
                Size = new SizeF(450 * scale, 205 * scale)
            };
            qrCode.Draw(graphics, barcodePosition);
        }

        #endregion Helper methods
    }
}