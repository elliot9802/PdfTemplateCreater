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
using System.Net;
using System.Text.RegularExpressions;

namespace Services
{
    public class PdfTemplateService : IPdfTemplateService
    {
        #region constructor logic
        private readonly ILogger<PdfTemplateService> _logger;
        private readonly string _dbLogin;
        private readonly string _backgroundImagePath;
        private readonly string _scissorsLineImagePath;
        //private readonly string _adImagePath;

        public PdfTemplateService(ILogger<PdfTemplateService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var dbLoginDetail = AppConfig.GetDbLoginDetails("DbLogins");
            _dbLogin = dbLoginDetail.DbConnectionString ?? throw new InvalidOperationException("Database connection string is not configured.");

            var imagePaths = AppConfig.GetImagePaths();
            _backgroundImagePath = imagePaths.BackgroundImagePath ?? throw new KeyNotFoundException("BackgroundImagePath configuration is missing.");
            _scissorsLineImagePath = imagePaths.ScissorsLineImagePath ?? throw new KeyNotFoundException("ScissorsLineImagePath configuration is missing.");
            //_adImagePath = imagePaths.AdImagePath ?? throw new KeyNotFoundException("AdImagePath configuration is missing.");
        }
        #endregion

        #region database methods
        public async Task<TicketsDataDto> GetTicketDataAsync(int? ticketId, int? showEventInfo)
        {
            using var db = csMainDbContext.DbContext(_dbLogin);
            var query = db.Vy_ShowTickets.AsNoTracking();

            if (ticketId.HasValue)
            {
                // If a specific ticket ID is provided, use it to filter the data.
                query = query.Where(t => t.platsbokad_id == ticketId.Value);
            }
            else if (showEventInfo.HasValue)
            {
                // If a ShowEventInfo value is provided, use it to filter the data.
                query = query.Where(t => t.showEventInfo == showEventInfo.Value);
            }
            else
            {
                // If neither is provided, log a warning and return null.
                _logger.LogWarning("GetTicketDataAsync requires either a ticketId or showEventInfo to filter the data.");
                return null;
            }

            // Execute the query and return the first matching record.
            var templateData = await query.FirstOrDefaultAsync();
            if (templateData == null)
            {
                _logger.LogWarning("No matching ticket data found.");
            }
            return templateData;
        }

        public async Task<TicketHandling> GetPredefinedTicketHandlingAsync(int showEventInfo)
        {
            using (var db = csMainDbContext.DbContext(_dbLogin))
            {
                var predefinedTemplate = await db.TicketTemplate
                    .AsNoTracking()
                    .Where(t => t.ShowEventInfo == showEventInfo)
                    .FirstOrDefaultAsync();

                return predefinedTemplate.TicketsHandling; // This now contains TicketHandling data.
            }
        }
        #endregion

        #region Creating Pdf methods
        #region database methods
        public TemplateCUdto MapTicketHandlingToTemplateCUdto(TicketHandling ticketHandling)
        {
            // Instantiate a new TemplateCUdto object
            var templateCUdto = new TemplateCUdto
            {
                //TicketTemplateId = Guid.NewGuid(), // Assign a new Guid or obtain it from somewhere else if updating
                TicketsHandling = ticketHandling, // Directly assign the TicketHandling object
                                                  // The TicketHandlingJson is redundant if you're directly assigning the object above, unless you need it for some other operation
                TicketHandlingJson = JsonConvert.SerializeObject(ticketHandling), // Serialize the TicketHandling to JSON
                                                                                  // The ShowEventInfo property's value will depend on whether the layout is predefined or custom
                                                                                  //ShowEventInfo = IsPredefinedLayout(ticketHandling) ? DeterminePredefinedLayout(ticketHandling) : 4
            };

            return templateCUdto;
        }

        public async Task<TicketHandling> CreateTemplateAsync(TemplateCUdto _src)
        {
            using (var db = csMainDbContext.DbContext(_dbLogin))
            {
                TicketHandling ticketHandling = null;
                // Convert TicketsHandling to JSON before saving
                if (_src.ShowEventInfo >= 1 && _src.ShowEventInfo <= 3)
                {
                    var predefinedTemplate = await db.TicketTemplate
                        .FirstOrDefaultAsync(t => t.ShowEventInfo == _src.ShowEventInfo);
                    if (predefinedTemplate != null)
                    {
                        ticketHandling = predefinedTemplate.TicketsHandling;
                    }
                }
                else
                {
                    var ticketTemplateDbM = new TicketTemplateDbM
                    {
                        TicketTemplateId = _src.TicketTemplateId,
                        TicketsHandlingJson = JsonConvert.SerializeObject(_src.TicketsHandling),
                        ShowEventInfo = 4
                    };

                    db.TicketTemplate.Add(ticketTemplateDbM);
                    await db.SaveChangesAsync();
                    ticketHandling = ticketTemplateDbM.TicketsHandling;

                }
                return ticketHandling;
            }
        }

        public async Task<ITicketTemplate> DeleteTemplateAsync(Guid id)
        {
            using (var db = csMainDbContext.DbContext(_dbLogin))
            {
                var tt = await db.TicketTemplate
                    .FirstOrDefaultAsync(tt => tt.TicketTemplateId == id);
                if (tt == null)
                {
                    throw new ArgumentException($"Item with id {id} does not exist");
                }
                else
                {
                    db.TicketTemplate.Remove(tt);

                    await db.SaveChangesAsync();

                    return tt;
                }
            }
        }
        #endregion
        public async Task CreatePdfAsync(string outputPath, TicketsDataDto ticketData, TicketHandling ticketHandling, string backgroundImagePath = null)
        {
            using PdfDocument document = new PdfDocument();
            PdfFont regularFont = new PdfStandardFont(PdfFontFamily.Helvetica, 8);
            PdfFont boldFont = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
            PdfFont customFont = new PdfStandardFont(PdfFontFamily.Helvetica, 9);

            PdfPage page = document.Pages.Add();

            backgroundImagePath ??= _backgroundImagePath;
            DrawPageContent(page, backgroundImagePath, ticketData, ticketHandling, regularFont, boldFont, customFont);

            await SaveDocumentAsync(document, outputPath);
        }

        private void DrawPageContent(PdfPage page, string backgroundImagePath, TicketsDataDto ticketData, TicketHandling ticketHandling, PdfFont regularFont, PdfFont boldFont, PdfFont customFont)
        {
            float scaleFactor = Math.Min(page.GetClientSize().Width / 1024f, 1);
            PointF ticketOrigin = new PointF((page.GetClientSize().Width - (1024 * scaleFactor)) / 2, 0);

            DrawBackgroundImage(page, backgroundImagePath, ticketOrigin, scaleFactor);
            DrawTextContent(page.Graphics, ticketOrigin, scaleFactor, ticketData, ticketHandling, regularFont, boldFont, customFont);
            DrawCustomTextElements(page.Graphics, ticketHandling, ticketOrigin, scaleFactor, regularFont);
            DrawBarcode(page, ticketOrigin, scaleFactor, ticketHandling, ticketData);
            DrawScissorsLine(page.Graphics, ticketOrigin, scaleFactor, ticketHandling);
            if (ticketHandling.IncludeAd && !string.IsNullOrEmpty(ticketData.reklam1))
            {
                DrawAd(page.Graphics, page, ticketData.reklam1, ticketOrigin, scaleFactor, regularFont, ticketHandling);
            }
        }

        private void DrawBackgroundImage(PdfPage page, string imagePath, PointF origin, float scale)
        {
            using FileStream imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            PdfBitmap background = new PdfBitmap(imageStream);

            page.Graphics.DrawImage(background, origin.X, origin.Y, 1024 * scale, 364 * scale);
        }

        private void DrawTextIfCondition(PdfGraphics graphics, bool condition, object value, PointF origin, float scale, float? positionX, float? positionY, PdfFont font, string format = null)
        {
            if (condition && value != null)
            {
                PointF position = new PointF(
                    origin.X + (positionX ?? 0) * scale,
                    origin.Y + (positionY ?? 0) * scale);
                string text = value is IFormattable formattableValue ? formattableValue.ToString(format, CultureInfo.InvariantCulture) : value.ToString();
                graphics.DrawString(text, font, PdfBrushes.Black, position);
            }
        }

        private void DrawTextContent(PdfGraphics graphics, PointF origin, float scale, TicketsDataDto ticketData, TicketHandling ticketHandling, PdfFont regularFont, PdfFont boldFont, PdfFont customFont)
        {
            PdfFont rutbokstavFont = new PdfStandardFont(PdfFontFamily.Helvetica, 24, PdfFontStyle.Bold);

            // Log method entry
            _logger.LogInformation("DrawTextContent method entered");

            // Log parameters
            _logger.LogInformation($"Origin: {origin}, Scale: {scale}");
            _logger.LogInformation($"TicketDetails: {JsonConvert.SerializeObject(ticketHandling)}");
            _logger.LogInformation($"TicketData: {JsonConvert.SerializeObject(ticketData)}");

            DrawTextIfCondition(graphics, ticketHandling.IncludeEmail, ticketData.eMail, origin, scale, ticketHandling.EmailPositionX, ticketHandling.EmailPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeContactPerson, ticketData.KontaktPerson, origin, scale, ticketHandling.NamePositionX, ticketHandling.NamePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeServiceFee, ticketData.serviceavgift1_kr, origin, scale, ticketHandling.ServiceFeePositionX, ticketHandling.ServiceFeePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeWebBookingNr, "Webbokningsnr: " + ticketData.webbkod, origin, scale, ticketHandling.WebBookingNumberPositionX, ticketHandling.WebBookingNumberPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeBookingNr, "Bokningsnr: " + ticketData.BokningsNr, origin, scale, ticketHandling.BookingNrPositionX, ticketHandling.BookingNrPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeEventName, ticketData.namn1, origin, scale, ticketHandling.EventNamePositionX, ticketHandling.EventNamePositionY, boldFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeSubEventName, ticketData.namn2, origin, scale, ticketHandling.SubEventNamePositionX, ticketHandling.SubEventNamePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeEventDate, "Köpdatum: " + ticketData.datumStart, origin, scale, ticketHandling.EventDatePositionX, ticketHandling.EventDatePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludePrice, ticketData.Pris, origin, scale, ticketHandling.PricePositionX, ticketHandling.PricePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeChairNr, ticketData.ArtikelNr, origin, scale, ticketHandling.ArtNrPositionX, ticketHandling.ArtNrPositionY, regularFont);

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

            // Draw "Powered by Vitec Smart Visitor System AB" text at the bottom
            string bottomTxt = "Powered by Vitec Smart Visitor System AB";
            SizeF pageSize = graphics.ClientSize; // Assuming graphics is the PdfGraphics of the page
            PdfFont bottomTxtFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Italic);
            SizeF bottomTxtSize = bottomTxtFont.MeasureString(bottomTxt);
            PointF bottomTxtPosition = new PointF(
                (pageSize.Width - bottomTxtSize.Width) / 2, // Centered horizontally
                pageSize.Height - bottomTxtSize.Height - 30 * scale // 30 units from the bottom
            );
            graphics.DrawString(bottomTxt, bottomTxtFont, PdfBrushes.Black, bottomTxtPosition);

            // Draw scissors line below the ticket
            DrawScissorsLine(graphics, origin, scale, ticketHandling);

            _logger.LogInformation("DrawTextContent method exited");
        }

        private void DrawCustomTextElements(PdfGraphics graphics, TicketHandling ticketHandling, PointF origin, float scale, PdfFont regularFont)
        {
            foreach (var customTextElement in ticketHandling.CustomTextElements)
            {
                // Skip if the current CustomTextElement is null
                if (customTextElement == null) continue;

                // Skip if the text is null or empty
                if (string.IsNullOrEmpty(customTextElement.Text)) continue;

                // Create a font for the custom text
                float fontSize = customTextElement.FontSize.HasValue ? customTextElement.FontSize.Value : 10f;
                PdfFont customFont = new PdfStandardFont(PdfFontFamily.Helvetica, fontSize);

                // Initialize default color as black in case of null or invalid color string
                PdfColor color = new PdfColor(0, 0, 0);

                // Check if the color string is properly formatted before attempting to convert it
                if (!string.IsNullOrEmpty(customTextElement.Color) && customTextElement.Color.StartsWith("#") && customTextElement.Color.Length == 7)
                {
                    color = new PdfColor(
                        Convert.ToByte(customTextElement.Color.Substring(1, 2), 16),
                        Convert.ToByte(customTextElement.Color.Substring(3, 2), 16),
                        Convert.ToByte(customTextElement.Color.Substring(5, 2), 16));
                }

                // Create a brush with the color
                PdfBrush customBrush = new PdfSolidBrush(color);

                // Calculate position based on scale and origin, providing a default value if the nullable float is null
                PointF position = new PointF(
                    origin.X + (customTextElement.PositionX ?? 0) * scale, // if PositionX is null, default to 0
                    origin.Y + (customTextElement.PositionY ?? 0) * scale  // if PositionY is null, default to 0
                );

                // Draw the text
                graphics.DrawString(customTextElement.Text, customFont, customBrush, position);
            }
        }


        private void DrawScissorsLine(PdfGraphics graphics, PointF origin, float scale, TicketHandling ticketHandling)
        {
            if (ticketHandling.IncludeScissorsLine)
            {
                using FileStream scissorsImageStream = new FileStream(_scissorsLineImagePath, FileMode.Open, FileAccess.Read);
                PdfBitmap scissorsLineImage = new PdfBitmap(scissorsImageStream);

                PointF scissorsPosition = new PointF(
                origin.X, // Aligned with the left edge of the ticket
                origin.Y + 364 * scale + 10 * scale // Just below the ticket, 10 units of space
                );
                SizeF scissorsSize = new SizeF(1024 * scale, scissorsLineImage.Height * scale); // Full width and scaled height

                graphics.DrawImage(scissorsLineImage, scissorsPosition, scissorsSize);
            }
        }

        private void DrawAd(PdfGraphics graphics, PdfPage page, string htmlContent, PointF origin, float scale, PdfFont font, TicketHandling ticketHandling)
        {
            //Extract the URL from the <img> tag
            var imageUrl = ExtractImageUrl(htmlContent);

            // Correctly handle HTML entities
            htmlContent = htmlContent.Replace("&nbsp;", "\u00A0");
            htmlContent = htmlContent.Replace("&auml;", "ä");
            htmlContent = htmlContent.Replace("&aring;", "å");
            htmlContent = htmlContent.Replace("</p>", "</p><br/>");

            PointF adPosition = new PointF(
                origin.X + (ticketHandling.AdPositionX.HasValue ? ticketHandling.AdPositionX.Value * scale : 0),
                origin.Y + (ticketHandling.AdPositionY.HasValue ? ticketHandling.AdPositionY.Value * scale : 500 * scale)
                );

            float xOffset = adPosition.X;
            float yOffset = adPosition.Y;

            // Remove the <img> tag from the htmlContent
            htmlContent = RemoveImageTag(htmlContent);

            // Rest of the HTML content
            PdfHTMLTextElement htmlTextElement = new PdfHTMLTextElement(htmlContent, font, PdfBrushes.Black);
            RectangleF rect = new RectangleF(xOffset, yOffset, page.GetClientSize().Width, page.GetClientSize().Height);
            htmlTextElement.Draw(graphics, rect);

            // If an image URL is found, draw it using PdfBitmap
            if (!string.IsNullOrEmpty(imageUrl))
            {
                WebClient wc = new WebClient();
                byte[] bytes = wc.DownloadData(imageUrl);
                MemoryStream ms = new MemoryStream(bytes);
                PdfBitmap image = new PdfBitmap(ms);

                // Set the size to the original image size
                SizeF imageSize = new SizeF(image.Width * scale, image.Height * scale);

                yOffset += imageSize.Height * scale;

                // Define the position for the image
                PointF imagePosition = new PointF(xOffset, yOffset);

                // Draw the image
                graphics.DrawImage(image, imagePosition, imageSize);

                // Dispose of WebClient and MemoryStream appropriately
                wc.Dispose();
                ms.Close();
            }
        }

        private string ExtractImageUrl(string htmlContent)
        {
            var match = Regex.Match(htmlContent, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }

        private string RemoveImageTag(string htmlContent)
        {
            return Regex.Replace(htmlContent, "<img.+?>", "", RegexOptions.IgnoreCase);
        }

        private void DrawBarcode(PdfPage page, PointF origin, float scale, TicketHandling ticketHandling, TicketsDataDto ticketData)
        {
            PointF barcodePosition = new PointF(
            origin.X + (ticketHandling.BarcodePositionX.HasValue ? ticketHandling.BarcodePositionX.Value * scale : 0),
            origin.Y + (ticketHandling.BarcodePositionY.HasValue ? ticketHandling.BarcodePositionY.Value * scale : 0)
            );

            if (ticketHandling.UseQRCode)
            {
                // Draw QR code
                PdfQRBarcode qrCode = new PdfQRBarcode();
                qrCode.Text = ticketData.webbkod;
                qrCode.Size = new SizeF(450 * scale, 205 * scale);
                qrCode.Draw(page.Graphics, barcodePosition);
            }
            else
            {
                // Draw barcode
                PdfCode39Barcode barcode = new PdfCode39Barcode();
                barcode.Text = ticketData.webbkod;
                barcode.Size = new SizeF(270 * scale, 90 * scale);

                if (ticketHandling.FlipBarcode)
                {
                    // Save the current state of the graphics object
                    page.Graphics.Save();
                    // Apply translation and rotation
                    page.Graphics.TranslateTransform(barcodePosition.X + barcode.Size.Height, barcodePosition.Y);
                    page.Graphics.RotateTransform(-90);
                    // Draw the barcode at the transformed position
                    barcode.Draw(page.Graphics, PointF.Empty);
                    // Restore the graphics state to its original configuration
                    page.Graphics.Restore();
                }
                else
                {
                    barcode.Draw(page.Graphics, barcodePosition);
                }
            }
        }
        private async Task SaveDocumentAsync(PdfDocument document, string outputPath)
        {
            try
            {
                await using FileStream stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
                document.Save(stream);
                _logger.LogInformation($"PDF Ticket Creation succeeded and saved to {outputPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"PDF Ticket creation failed: {ex.Message}");
                throw;
            }
        }
        #endregion
    }
}
