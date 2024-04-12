﻿using Configuration;
using DbModels;
using DbRepos;
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

        private readonly TemplateRepository _repo;
        private readonly ILogger<PdfTemplateService> _logger;
        private readonly string _scissorsLineImagePath;

        public PdfTemplateService(TemplateRepository repo, ILogger<PdfTemplateService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var imagePaths = AppConfig.ImagePathSettings;
            _scissorsLineImagePath = imagePaths.ScissorsLineImagePath ?? throw new KeyNotFoundException("ScissorsLineImagePath configuration is missing.");
        }

        #endregion constructor logic

        #region 1:1 calls

        public Task<int> SaveFileToDatabaseAsync(byte[] fileData, string description, string name) => _repo.SaveFileToDatabaseAsync(fileData, description, name);

        public Task<byte[]> GetFileDataAsync(int? fileStorageID) => _repo.GetFileDataAsync(fileStorageID);

        public Task<TicketHandling?> GetPredefinedTicketHandlingAsync(int showEventInfo) => _repo.GetPredefinedTicketHandlingAsync(showEventInfo);

        public Task<TicketTemplateDbM?> GetTicketTemplateByShowEventInfoAsync(int showEventInfo) => _repo.GetTicketTemplateByShowEventInfoAsync(showEventInfo);

        public Task<TemplateCUdto> GetTemplateByIdAsync(Guid ticketTemplateId) => _repo.GetTemplateByIdAsync(ticketTemplateId);

        public Task<List<ITicketTemplate>> ReadTemplatesAsync() => _repo.ReadTemplatesAsync();

        public Task<IEnumerable<TicketsDataView>> GetTicketsDataByWebbUidAsync(Guid webbUid) => _repo.GetTicketsDataByWebbUidAsync(webbUid);

        public Task<ITicketTemplate> CreateTemplateAsync(TemplateCUdto _src) => _repo.CreateTemplateAsync(_src);

        public Task<ITicketTemplate> UpdateTemplateAsync(TemplateCUdto templateDto, byte[]? bgFileData, string? bgFileName) => _repo.UpdateTemplateAsync(templateDto, bgFileData, bgFileName);

        public Task<ITicketTemplate> DeleteTemplateAsync(Guid id) => _repo.DeleteTemplateAsync(id);

        public Task<string> GetFilePathAsync(int fileId) => _repo.GetFilePathAsync(fileId);

        #endregion 1:1 calls

        #region Creating Pdf methods

        public static TicketsDataView GenerateMockTicketData(string ticketType)
        {
            TicketsDataSeeder seeder = new();
            try
            {
                return seeder.GenerateMockData(ticketType);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException($"Failed to generate mock ticket data: {ex.Message}", ex);
            }
        }

        public TemplateCUdto MapTicketHandlingToTemplateCUdto(TicketHandling ticketHandling)
        {
            return new TemplateCUdto
            {
                TicketsHandling = ticketHandling,
                TicketHandlingJson = JsonConvert.SerializeObject(ticketHandling),
            };
        }

        public async Task<byte[]> GeneratePredefinedPdfAsync(int showEventInfo)
        {
            var ticketHandlingData = await GetPredefinedTicketHandlingAsync(showEventInfo);
            if (ticketHandlingData == null)
            {
                _logger.LogWarning("No predefined TicketHandling found for ShowEventInfo: {ShowEventInfo}", showEventInfo);
                throw new KeyNotFoundException($"Predefined TicketHandling with ShowEventInfo {showEventInfo} not found.");
            }

            var ticketTemplate = await GetTicketTemplateByShowEventInfoAsync(showEventInfo);
            if (ticketTemplate == null)
            {
                _logger.LogWarning("No Template found with ShowEventInfo: {ShowEventInfo}", showEventInfo);
                throw new KeyNotFoundException($"Template with ShowEventInfo {showEventInfo} not found.");
            }

            byte[] bgFileData = await GetFileDataAsync(ticketTemplate.FileStorageID);
            const string bgFileName = "Predefined"; // TODO: dynamically determine this based on the actual file

            return await CreatePdfAsync(ticketHandlingData, bgFileData, bgFileName, null, false);
        }

        public async Task<byte[]> CreatePdfAsync(TicketHandling ticketHandling, byte[] bgFileData, string bgFileName, string? name, bool saveToDb)
        {
            if (saveToDb && string.IsNullOrWhiteSpace(name))
            {
                _logger.LogError("Template name is required when saving to the database.");
                throw new ArgumentException("Template name is required when saving to the database.");
            }

            int bgFileId = 0;
            string bgFilePath;
            string outputPath = GetTemporaryPdfFilePath();
            List<string> tempFiles = new() { outputPath };

            if (saveToDb)
            {
                bgFileId = await SaveFileToDatabaseAsync(bgFileData, "Background Image for " + name, bgFileName);
                bgFilePath = await GetFilePathAsync(bgFileId);
            }
            else
            {
                bgFilePath = SaveTempImage(bgFileData);
                tempFiles.Add(bgFilePath);
            }

            try
            {
                var ticketType = ticketHandling.DetermineTicketType();
                var ticketData = GenerateMockTicketData(ticketType);

                using PdfDocument document = new();
                PdfFont regularFont = new PdfStandardFont(PdfFontFamily.Helvetica, 8);
                PdfFont boldFont = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                PdfFont customFont = new PdfStandardFont(PdfFontFamily.Helvetica, 9);

                PdfPage page = document.Pages.Add();
                await DrawPageContent(page, bgFilePath, ticketData, ticketHandling, regularFont, boldFont, customFont);

                await SaveDocumentAsync(document, outputPath);
                _logger.LogInformation("PDF created successfully at {OutputPath}", outputPath);
                if (!File.Exists(outputPath))
                {
                    throw new InvalidOperationException($"Expected PDF at {outputPath} was not found after creation.");
                }
                if (saveToDb)
                {
                    TemplateCUdto templateDetails = MapTicketHandlingToTemplateCUdto(ticketHandling);
                    templateDetails.Name = name!;
                    templateDetails.FileStorageID = bgFileId;
                    await CreateTemplateAsync(templateDetails);
                }

                return await File.ReadAllBytesAsync(outputPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while creating a PDF at outputPath: {OutputPath}.", outputPath);
                throw new InvalidOperationException("Failed to create a PDF. See inner exception for details.", ex);
            }
            finally
            {
                foreach (var tempFile in tempFiles)
                {
                    try { File.Delete(tempFile); _logger.LogInformation("Deleted temporary file: {TempFile}", tempFile); }
                    catch (Exception ex) { _logger.LogError(ex, "Failed to delete temporary file: {TempFile}", tempFile); }
                }
            }
        }

        public async Task<byte[]> CreateCombinedPdfAsync(Guid webbUid, int showEventInfo)
        {
            string outputPath = GetTemporaryPdfFilePath();
            List<string> tempFiles = new() { outputPath };

            try
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
                    var ticketTemplate = await GetTicketTemplateByShowEventInfoAsync(showEventInfo);
                    if (ticketTemplate == null)
                    {
                        _logger.LogWarning("Ticket template not found for ShowEventInfo: {ShowEventInfo}", showEventInfo);
                        continue; // Skip this ticket if its template cannot be retrieved
                    }

                    var fileData = await GetFileDataAsync(ticketTemplate.FileStorageID);
                    if (fileData == null)
                    {
                        _logger.LogWarning("Background image data not found for FileStorageID: {FileStorageID}", ticketTemplate.FileStorageID);
                        continue; // Skip this ticket if background image cannot be retrieved
                    }

                    string bgFilePath = SaveTempImage(fileData);
                    tempFiles.Add(bgFilePath);

                    var ticketHandling = await GetPredefinedTicketHandlingAsync(showEventInfo);
                    if (ticketHandling == null)
                    {
                        _logger.LogWarning("No TicketHandling found for ShowEventInfo: {ShowEventInfo}, skipping ticket.", showEventInfo);
                        continue;
                    }

                    var page = document.Pages.Add();
                    await DrawPageContent(page, bgFilePath, ticketData, ticketHandling, new PdfStandardFont(PdfFontFamily.Helvetica, 8), new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold), new PdfStandardFont(PdfFontFamily.Helvetica, 9));
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while creating a combined PDF for WebbUid: {WebbUid} at outputPath: {OutputPath}.", webbUid, outputPath);
                throw new InvalidOperationException($"Failed to create combined PDF for WebbUid: {webbUid}. See inner exception for details.", ex);
            }
            finally
            {
                foreach (var tempFile in tempFiles)
                {
                    try { File.Delete(tempFile); }
                    catch (Exception ex) { _logger.LogError(ex, "Failed to delete temporary file: {TempFile}", tempFile); }
                }
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
                if (customTextElement == null || string.IsNullOrEmpty(customTextElement.Text))
                {
                    continue;
                }

                float fontSize = customTextElement.FontSize ?? 10f;
                PdfFont customFont = new PdfStandardFont(PdfFontFamily.Helvetica, fontSize);

                PdfColor color = new(0, 0, 0);

                if (!string.IsNullOrEmpty(customTextElement.Color) && customTextElement.Color.StartsWith('#') && customTextElement.Color.Length == 7)
                {
                    color = new PdfColor(
                        Convert.ToByte(customTextElement.Color.Substring(1, 2), 16),
                        Convert.ToByte(customTextElement.Color.Substring(3, 2), 16),
                        Convert.ToByte(customTextElement.Color.Substring(5, 2), 16));
                }

                PdfBrush customBrush = new PdfSolidBrush(color);

                PointF position = new(
                    origin.X + ((customTextElement.PositionX ?? 0) * scale),
                    origin.Y + ((customTextElement.PositionY ?? 0) * scale)
                );

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
                origin.X,
                origin.Y + (364 * scale) + (10 * scale)
                );
                SizeF scissorsSize = new(1024 * scale, scissorsLineImage.Height * scale);

                graphics.DrawImage(scissorsLineImage, scissorsPosition, scissorsSize);
            }
        }

        private static void DrawTextContent(PdfGraphics graphics, PointF origin, float scale, TicketsDataView ticketData, TicketHandling ticketHandling, PdfFont regularFont, PdfFont boldFont, PdfFont customFont)
        {
            PdfFont rutbokstavFont = new PdfStandardFont(PdfFontFamily.Helvetica, 24, PdfFontStyle.Bold);

            DrawTextIfCondition(graphics, ticketHandling.IncludeArtName, ticketData.Artikelnamn, origin, scale, ticketHandling.ArtNamePositionX, ticketHandling.ArtNamePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeArtNr, ticketData.ArtikelNr, origin, scale, ticketHandling.ArtNrPositionX, ticketHandling.ArtNrPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeArtNotText, ticketData.ArtNotText, origin, scale, ticketHandling.ArtNotTextPositionX, ticketHandling.ArtNotTextPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeBookingNr, "Bokningsnr: " + ticketData.BokningsNr, origin, scale, ticketHandling.BookingNrPositionX, ticketHandling.BookingNrPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeChairNr, ticketData.stolsnr, origin, scale, ticketHandling.ChairNrPositionX, ticketHandling.ChairNrPositionY, customFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeChairRow, ticketData.stolsrad, origin, scale, ticketHandling.ChairRowPositionX, ticketHandling.ChairRowPositionY, customFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeContactPerson, ticketData.KontaktPerson, origin, scale, ticketHandling.ContactPersonPositionX, ticketHandling.ContactPersonPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeDatum, ticketData.Datum, origin, scale, ticketHandling.DatumPositionX, ticketHandling.DatumPositionY, customFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeDescription, ticketData.Beskrivning, origin, scale, ticketHandling.DescriptionPositionX, ticketHandling.DescriptionPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeEmail, ticketData.eMail, origin, scale, ticketHandling.EmailPositionX, ticketHandling.EmailPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeEntrance, ticketData.Ingang, origin, scale, ticketHandling.EntrancePositionX, ticketHandling.EntrancePositionY, customFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeEventDate, "Köpdatum: " + ticketData.datumStart, origin, scale, ticketHandling.EventDatePositionX, ticketHandling.EventDatePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeEventName, ticketData.namn1, origin, scale, ticketHandling.EventNamePositionX, ticketHandling.EventNamePositionY, boldFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeFacilityName, ticketData.anamn, origin, scale, ticketHandling.FacilityNamePositionX, ticketHandling.FacilityNamePositionY, customFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeLogorad1, ticketData.logorad1, origin, scale, ticketHandling.Logorad1PositionX, ticketHandling.Logorad1PositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeLogorad2, ticketData.logorad2, origin, scale, ticketHandling.Logorad2PositionX, ticketHandling.Logorad2PositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludePrice, ticketData.Pris, origin, scale, ticketHandling.PricePositionX, ticketHandling.PricePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeRutBokstav, ticketData.Rutbokstav, origin, scale, ticketHandling.RutBokstavPositionX, ticketHandling.RutBokstavPositionY, rutbokstavFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeSection, ticketData.namn, origin, scale, ticketHandling.SectionPositionX, ticketHandling.SectionPositionY, customFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeServiceFee, ticketData.serviceavgift1_kr, origin, scale, ticketHandling.ServiceFeePositionX, ticketHandling.ServiceFeePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeStrukturArtikel, ticketData.StrukturArtikel, origin, scale, ticketHandling.StrukturArtikelPositionX, ticketHandling.StrukturArtikelPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeSubEventName, ticketData.namn2, origin, scale, ticketHandling.SubEventNamePositionX, ticketHandling.SubEventNamePositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeWebBookingNr, "Webbokningsnr: " + ticketData.webbkod, origin, scale, ticketHandling.WebBookingNrPositionX, ticketHandling.WebBookingNrPositionY, regularFont);

            DrawTextIfCondition(graphics, ticketHandling.IncludeWebbcode, ticketData.Webbcode, origin, scale, ticketHandling.WebbcodePositionX, ticketHandling.WebbcodePositionY, regularFont);
        }

        private static void DrawTextIfCondition(PdfGraphics graphics, bool condition, object? value, PointF origin, float scale, float? positionX, float? positionY, PdfFont font, string? format = null)
        {
            if (condition)
            {
                PointF position = new(
                    origin.X + ((positionX ?? 0) * scale),
                    origin.Y + ((positionY ?? 0) * scale));
                string text = value is IFormattable formattableValue
                    ? formattableValue.ToString(format, CultureInfo.InvariantCulture)
                    : value?.ToString() ?? string.Empty;
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
                (pageSize.Width - bottomTxtSize.Width) / 2,
                pageSize.Height - bottomTxtSize.Height - (30 * scale)
            );
            graphics.DrawString(bottomTxt, bottomTxtFont, PdfBrushes.Black, bottomTxtPosition);
        }

        private async Task SaveDocumentAsync(PdfDocument document, string outputPath)
        {
            try
            {
                await using FileStream stream = new(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await Task.Run(() => document.Save(stream));
                _logger.LogInformation("PDF Ticket Creation succeeded and saved to {OutputPath}", outputPath);
            }
            catch (IOException ioEx)
            {
                _logger.LogError(ioEx, "PDF Ticket creation IO error: {ErrorMessage}", ioEx.Message);
                throw new InvalidOperationException($"An IO error occurred while saving the PDF to {outputPath}.", ioEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PDF Ticket creation failed: {ErrorMessage}", ex.Message);
                throw new InvalidOperationException("An unexpected error occurred while saving the PDF document.", ex);
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

        private static string? ExtractImageUrl(string htmlContent)
        {
            var match = Regex.Match(htmlContent, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }

        private static string HandleHtmlEntities(string htmlContent)
        {
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
            return Regex.Replace(htmlContent, "<img.+?>", "", RegexOptions.IgnoreCase);
        }

        private static string SaveTempImage(byte[] fileData)
        {
            string tempImagePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".png");
            File.WriteAllBytes(tempImagePath, fileData);
            return tempImagePath;
        }

        public string GetTemporaryPdfFilePath()
        {
            string tempDirectory = Path.GetTempPath();
            string fileName = Guid.NewGuid().ToString("N") + ".pdf";
            string fullPath = Path.Combine(tempDirectory, fileName);
            _logger.LogInformation("Generated temporary PDF file path: {FullPath}", fullPath);
            return fullPath;
        }

        #endregion Helper methods
    }
}