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

namespace Services
{
    public class PdfTemplateService : IPdfTemplateService
    {
        #region constructor logic
        private readonly ILogger<PdfTemplateService> _logger;
        private readonly string _dbLogin;
        private readonly string _backgroundImagePath;
        private readonly string _scissorsLineImagePath;
        private readonly string _adImagePath;

        public PdfTemplateService(ILogger<PdfTemplateService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var dbLoginDetail = AppConfig.GetDbLoginDetails("DbLogins");
            _dbLogin = dbLoginDetail.DbConnectionString ?? throw new InvalidOperationException("Database connection string is not configured.");

            var imagePaths = AppConfig.GetImagePaths();
            _backgroundImagePath = imagePaths.BackgroundImagePath ?? throw new KeyNotFoundException("BackgroundImagePath configuration is missing.");
            _scissorsLineImagePath = imagePaths.ScissorsLineImagePath ?? throw new KeyNotFoundException("ScissorsLineImagePath configuration is missing.");
            _adImagePath = imagePaths.AdImagePath ?? throw new KeyNotFoundException("AdImagePath configuration is missing.");
        }
        #endregion

        #region database methods
        public async Task<TicketsDataDto> GetTicketDataAsync(TicketHandling ticketDetails)
        {
            using var db = csMainDbContext.DbContext(_dbLogin);
            var templateData = await db.Vy_ShowTickets
                .FirstOrDefaultAsync(t => t.platsbokad_id == ticketDetails.Id);

            if (templateData == null)
            {
                _logger.LogWarning($"A ticket with ID {ticketDetails.Id} was not found.");
                return null;
            }
            return templateData;
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

        public async Task<ITicketTemplate> CreateTemplateAsync(TemplateCUdto _src)
        {
            using (var db = csMainDbContext.DbContext(_dbLogin))
            {
                // Convert TicketsHandling to JSON before saving
                if (_src.ShowEventInfo >= 1 && _src.ShowEventInfo <= 3)
                {
                    var predefinedTemplate = await db.TicketTemplate
                        .FirstOrDefaultAsync(t => t.ShowEventInfo == _src.ShowEventInfo);
                    if (predefinedTemplate != null)
                    {
                        return predefinedTemplate;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (_src.ShowEventInfo == 4)
                {

                    {
                        var ticketTemplateDbM = new TicketTemplateDbM
                        {
                            TicketTemplateId = _src.TicketTemplateId,
                            TicketsHandlingJson = JsonConvert.SerializeObject(_src.TicketsHandling),
                            ShowEventInfo = _src.ShowEventInfo
                        };
                        var _item = new TicketTemplateDbM(_src);

                        db.TicketTemplate.Add(_item);

                        await db.SaveChangesAsync();
                        return _item;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
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
        public async Task CreatePdfAsync(string outputPath, TicketsDataDto ticketData, TicketHandling ticketDetails, string backgroundImagePath = null)
        {
            using PdfDocument document = new PdfDocument();
            PdfFont regularFont = new PdfStandardFont(PdfFontFamily.Helvetica, 8);
            PdfFont boldFont = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
            PdfFont customFont = new PdfStandardFont(PdfFontFamily.Helvetica, 9);

            PdfPage page = document.Pages.Add();

            backgroundImagePath ??= _backgroundImagePath;
            DrawPageContent(page, backgroundImagePath, ticketData, ticketDetails, regularFont, boldFont, customFont);

            await SaveDocumentAsync(document, outputPath);
        }

        private void DrawPageContent(PdfPage page, string backgroundImagePath, TicketsDataDto ticketData, TicketHandling ticketDetails, PdfFont regularFont, PdfFont boldFont, PdfFont customFont)
        {
            float scaleFactor = Math.Min(page.GetClientSize().Width / 1024f, 1);
            PointF ticketOrigin = new PointF((page.GetClientSize().Width - (1024 * scaleFactor)) / 2, 0);

            DrawBackgroundImage(page, backgroundImagePath, ticketOrigin, scaleFactor);
            DrawTextContent(page.Graphics, ticketOrigin, scaleFactor, ticketData, ticketDetails, regularFont, boldFont, customFont);
            DrawCustomTextElements(page.Graphics, ticketDetails, ticketOrigin, scaleFactor, regularFont);
            DrawBarcode(page, ticketOrigin, scaleFactor, ticketDetails, ticketData);
            DrawScissorsLine(page.Graphics, ticketOrigin, scaleFactor, ticketDetails);
            DrawAd(page.Graphics, ticketOrigin, scaleFactor);
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

        private void DrawTextContent(PdfGraphics graphics, PointF origin, float scale, TicketsDataDto ticketData, TicketHandling ticketDetails, PdfFont regularFont, PdfFont boldFont, PdfFont customFont)
        {
            PdfFont rutbokstavFont = new PdfStandardFont(PdfFontFamily.Helvetica, 24, PdfFontStyle.Bold);

            // Log method entry
            _logger.LogInformation("DrawTextContent method entered");

            // Log parameters
            _logger.LogInformation($"Origin: {origin}, Scale: {scale}");
            _logger.LogInformation($"TicketDetails: {JsonConvert.SerializeObject(ticketDetails)}");
            //_logger.LogInformation($"TicketData: {JsonConvert.SerializeObject(ticketData)}");

            DrawTextIfCondition(graphics, ticketDetails.IncludeEmail, ticketData.eMail, origin, scale, ticketDetails.EmailPositionX, ticketDetails.EmailPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeContactPerson, ticketData.KontaktPerson, origin, scale, ticketDetails.NamePositionX, ticketDetails.NamePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeServiceFee, ticketData.serviceavgift1_kr, origin, scale, ticketDetails.ServiceFeePositionX, ticketDetails.ServiceFeePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeWebBookingNr, "Webbokningsnr: " + ticketData.webbkod, origin, scale, ticketDetails.WebBookingNumberPositionX, ticketDetails.WebBookingNumberPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeBookingNr, "Bokningsnr: " + ticketData.BokningsNr, origin, scale, ticketDetails.BookingNrPositionX, ticketDetails.BookingNrPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeEventName, ticketData.namn1, origin, scale, ticketDetails.EventNamePositionX, ticketDetails.EventNamePositionY, boldFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeSubEventName, ticketData.namn2, origin, scale, ticketDetails.SubEventNamePositionX, ticketDetails.SubEventNamePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeEventDate, "Köpdatum: " + ticketData.datumStart, origin, scale, ticketDetails.EventDatePositionX, ticketDetails.EventDatePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludePrice, ticketData.Pris, origin, scale, ticketDetails.PricePositionX, ticketDetails.PricePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeChairNr, ticketData.ArtikelNr, origin, scale, ticketDetails.ArtNrPositionX, ticketDetails.ArtNrPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeArtName, ticketData.Artikelnamn, origin, scale, ticketDetails.ArtNamePositionX, ticketDetails.ArtNamePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeChairRow, ticketData.stolsrad, origin, scale, ticketDetails.ChairRowPositionX, ticketDetails.ChairRowPositionY, customFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeChairNr, ticketData.stolsnr, origin, scale, ticketDetails.ChairNrPositionX, ticketDetails.ChairNrPositionY, customFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeLogorad1, ticketData.logorad1, origin, scale, ticketDetails.Logorad1PositionX, ticketDetails.Logorad1PositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeLogorad2, ticketData.logorad2, origin, scale, ticketDetails.Logorad2PositionX, ticketDetails.Logorad2PositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeSection, ticketData.namn, origin, scale, ticketDetails.SectionPositionX, ticketDetails.SectionPositionY, customFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeFacilityName, ticketData.anamn, origin, scale, ticketDetails.FacilityNamePositionX, ticketDetails.FacilityNamePositionY, customFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeAd, ticketData.reklam1, origin, scale, ticketDetails.AdPositionX, ticketDetails.AdPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeStrukturArtikel, ticketData.StrukturArtikel, origin, scale, ticketDetails.StrukturArtikelPositionX, ticketDetails.StrukturArtikelPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeDescription, ticketData.Beskrivning, origin, scale, ticketDetails.DescriptionPositionX, ticketDetails.DescriptionPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeArtNotText, ticketData.ArtNotText, origin, scale, ticketDetails.ArtNotTextPositionX, ticketDetails.ArtNotTextPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeDatum, ticketData.Datum, origin, scale, ticketDetails.DatumPositionX, ticketDetails.DatumPositionY, customFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeEntrance, ticketData.Ingang, origin, scale, ticketDetails.EntrancePositionX, ticketDetails.EntrancePositionY, customFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeRutBokstav, ticketData.Rutbokstav, origin, scale, ticketDetails.RutBokstavPositionX, ticketDetails.RutBokstavPositionY, rutbokstavFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeWebbcode, ticketData.Webbcode, origin, scale, ticketDetails.WebbcodePositionX, ticketDetails.WebbcodePositionY, regularFont);

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
            DrawScissorsLine(graphics, origin, scale, ticketDetails);

            _logger.LogInformation("DrawTextContent method exited");
        }

        private void DrawCustomTextElements(PdfGraphics graphics, TicketHandling ticketDetails, PointF origin, float scale, PdfFont regularFont)
        {
            foreach (var customTextElement in ticketDetails.CustomTextElements)
            {
                PdfFont customFont = new PdfStandardFont(PdfFontFamily.Helvetica, customTextElement.FontSize);
                PdfColor color = new PdfColor(Convert.ToByte(customTextElement.Color.Substring(1, 2), 16),
                                      Convert.ToByte(customTextElement.Color.Substring(3, 2), 16),
                                      Convert.ToByte(customTextElement.Color.Substring(5, 2), 16));
                PdfBrush customBrush = new PdfSolidBrush(color);

                // Calculate position based on scale and origin
                PointF position = new PointF(
                    origin.X + customTextElement.PositionX * scale,
                    origin.Y + customTextElement.PositionY * scale
                );

                // Draw the text
                graphics.DrawString(customTextElement.Text, customFont, customBrush, position);
            }
        }

        private void DrawScissorsLine(PdfGraphics graphics, PointF origin, float scale, TicketHandling ticketDetails)
        {
            if (ticketDetails.IncludeScissorsLine)
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

        private void DrawAd(PdfGraphics graphics, PointF origin, float scale)
        {
            using FileStream adImageStream = new FileStream(_adImagePath, FileMode.Open, FileAccess.Read);
            PdfBitmap adImage = new PdfBitmap(adImageStream);

            PointF adPosition = new PointF(
            origin.X, // Aligned with the left edge of the ticket
            origin.Y + 500 * scale + 10 * scale // Just below the ticket, 10 units of space
            );
            SizeF adSize = new SizeF(1024 * scale, 820 * scale); // Full width and scaled height

            graphics.DrawImage(adImage, adPosition, adSize);
        }

        private void DrawBarcode(PdfPage page, PointF origin, float scale, TicketHandling ticketDetails, TicketsDataDto ticketData)
        {
            PointF barcodePosition = new PointF(
            origin.X + (ticketDetails.BarcodePositionX.HasValue ? ticketDetails.BarcodePositionX.Value * scale : 0),
            origin.Y + (ticketDetails.BarcodePositionY.HasValue ? ticketDetails.BarcodePositionY.Value * scale : 0)
        );

            if (ticketDetails.UseQRCode)
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

                if (ticketDetails.FlipBarcode)
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
