using Configuration;
using DbContext;
using DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        public async Task<ITicketTemplate> CreateTemplateAsync(TemplateCUdto _src)
        {
            using (var db = csMainDbContext.DbContext(_dbLogin))
            {
                var _item = new TicketTemplateDbM(_src);

                db.TicketTemplate.Add(_item);

                await db.SaveChangesAsync();
                return _item;
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
            PdfFont regularFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
            PdfFont boldFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
            PdfPage page = document.Pages.Add();

            backgroundImagePath ??= _backgroundImagePath;
            DrawPageContent(page, backgroundImagePath, ticketData, ticketDetails, regularFont, boldFont);

            await SaveDocumentAsync(document, outputPath);
        }

        private void DrawPageContent(PdfPage page, string backgroundImagePath, TicketsDataDto ticketData, TicketHandling ticketDetails, PdfFont regularFont, PdfFont boldFont)
        {
            float scaleFactor = Math.Min(page.GetClientSize().Width / 1024f, 1);
            PointF ticketOrigin = new PointF((page.GetClientSize().Width - (1024 * scaleFactor)) / 2, 0);

            DrawBackgroundImage(page, backgroundImagePath, ticketOrigin, scaleFactor);
            DrawTextContent(page.Graphics, ticketOrigin, scaleFactor, ticketData, ticketDetails, regularFont, boldFont);
            DrawBarcode(page, ticketOrigin, scaleFactor, ticketDetails);
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

        private void DrawTextContent(PdfGraphics graphics, PointF origin, float scale, TicketsDataDto ticketData, TicketHandling ticketDetails, PdfFont regularFont, PdfFont boldFont)
        {
            // Log method entry
            Console.WriteLine("DrawTextContent method entered");

            // Log parameters
            Console.WriteLine($"Origin: {origin}, Scale: {scale}");
            Console.WriteLine($"TicketDetails: {JsonConvert.SerializeObject(ticketDetails)}");
            Console.WriteLine($"TicketData: {JsonConvert.SerializeObject(ticketData)}");

            DrawTextIfCondition(graphics, ticketDetails.IncludeEmail, ticketData.eMail, origin, scale, ticketDetails.EmailPositionX, ticketDetails.EmailPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeContactPerson, ticketData.KontaktPerson, origin, scale, ticketDetails.NamePositionX, ticketDetails.NamePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeServiceFee, ticketData.serviceavgift1_kr, origin, scale, ticketDetails.ServiceFeePositionX, ticketDetails.ServiceFeePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeWebBookingNr, ticketData.webbkod, origin, scale, ticketDetails.WebBookingNumberPositionX, ticketDetails.WebBookingNumberPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeBookingNr, ticketData.BokningsNr, origin, scale, ticketDetails.BookingNrPositionX, ticketDetails.BookingNrPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeEventName, ticketData.namn1, origin, scale, ticketDetails.EventNamePositionX, ticketDetails.EventNamePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeSubEventName, ticketData.namn, origin, scale, ticketDetails.SubEventNamePositionX, ticketDetails.SubEventNamePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeEventDate, ticketData.datumStart, origin, scale, ticketDetails.EventDatePositionX, ticketDetails.EventDatePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludePrice, ticketData.Pris, origin, scale, ticketDetails.PricePositionX, ticketDetails.PricePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludePBookId, ticketData.platsbokad_id, origin, scale, ticketDetails.PBookIdPositionX, ticketDetails.PBookIdPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeChairNr, ticketData.ArtikelNr, origin, scale, ticketDetails.ArtNrPositionX, ticketDetails.ArtNrPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeArtName, ticketData.Artikelnamn, origin, scale, ticketDetails.ArtNamePositionX, ticketDetails.ArtNamePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeChairRow, ticketData.stolsrad, origin, scale, ticketDetails.ChairRowPositionX, ticketDetails.ChairRowPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeChairNr, ticketData.stolsnr, origin, scale, ticketDetails.ChairNrPositionX, ticketDetails.ChairNrPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeEventDateId, ticketData.eventdatum_id, origin, scale, ticketDetails.EventDateIdPositionX, ticketDetails.EventDateIdPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeLogorad1, ticketData.logorad1, origin, scale, ticketDetails.Logorad1PositionX, ticketDetails.Logorad1PositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeLogorad2, ticketData.logorad2, origin, scale, ticketDetails.Logorad2PositionX, ticketDetails.Logorad2PositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeLocation, ticketData.plats, origin, scale, ticketDetails.LocationPositionX, ticketDetails.LocationPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeLocationName, ticketData.namn, origin, scale, ticketDetails.LocationNamePositionX, ticketDetails.LocationNamePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeFacilityName, ticketData.anamn, origin, scale, ticketDetails.FacilityNamePositionX, ticketDetails.FacilityNamePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeAd, ticketData.reklam1, origin, scale, ticketDetails.AdPositionX, ticketDetails.AdPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeStrukturArtikel, ticketData.StrukturArtikel, origin, scale, ticketDetails.StrukturArtikelPositionX, ticketDetails.StrukturArtikelPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeDescription, ticketData.Beskrivning, origin, scale, ticketDetails.DescriptionPositionX, ticketDetails.DescriptionPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeArtNotText, ticketData.ArtNotText, origin, scale, ticketDetails.ArtNotTextPositionX, ticketDetails.ArtNotTextPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeDatum, ticketData.Datum, origin, scale, ticketDetails.DatumPositionX, ticketDetails.DatumPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeEntrance, ticketData.Ingang, origin, scale, ticketDetails.EntrancePositionX, ticketDetails.EntrancePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketDetails.IncludeRutBokstav, ticketData.Rutbokstav, origin, scale, ticketDetails.RutBokstavPositionX, ticketDetails.RutBokstavPositionY, regularFont);

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

        private void DrawBarcode(PdfPage page, PointF origin, float scale, TicketHandling ticketDetails)
        {
            PointF barcodePosition = new PointF(
            origin.X + (ticketDetails.BarcodePositionX.HasValue ? ticketDetails.BarcodePositionX.Value * scale : 0),
            origin.Y + (ticketDetails.BarcodePositionY.HasValue ? ticketDetails.BarcodePositionY.Value * scale : 0)
        );

            if (ticketDetails.UseQRCode)
            {
                // Draw QR code
                PdfQRBarcode qrCode = new PdfQRBarcode();
                qrCode.Text = ticketDetails.BarcodeContent;
                qrCode.Size = new SizeF(450 * scale, 225 * scale);
                qrCode.Draw(page.Graphics, barcodePosition
                );
            }
            else
            {
                // Draw barcode
                PdfCode39Barcode barcode = new PdfCode39Barcode();
                barcode.Text = ticketDetails.BarcodeContent;
                barcode.Size = new SizeF(330 * scale, 120 * scale);

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
