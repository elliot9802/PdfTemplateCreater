using Configuration;
using DbModels;
using DbRepos;
using Microsoft.Extensions.Logging;
using Models;
using Models.DTO;
using Newtonsoft.Json;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Barcode;
using Syncfusion.Pdf.Graphics;
using System.Text.RegularExpressions;

namespace Services
{
    public class PdfTemplateService : IPdfTemplateService
    {
        #region constructor logic

        private readonly float pdfWidth = 1024f;
        private readonly float pdfHeight = 364f;
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

        public Task<IEnumerable<WebbUidInfo>> GetAllWebbUidsAsync() => _repo.GetAllWebbUidsAsync();

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

        public static TicketsDataView GenerateMockTicketData()
        {
            Random random = new();

            return new TicketsDataView
            {
                anamn = "Anläggning",
                Artikelnamn = "Artikel namn",
                Beskrivning = "En beskrivande text som beskriver artikel",
                namn = "Sektion",
                namn1 = "Eventnamn",
                namn2 = "Eventundernamn",
                Ingang = "Ingång",
                stolsnr = $"{random.Next(1, 20)}",
                Rutbokstav = "RB",
                ArtikelNr = random.Next(10000, 99999).ToString(),
                BokningsNr = random.Next(4000, 7999),
                Datum = DateTime.Now.AddDays(random.Next(-30, 30)).ToString("yyyy-MM-dd-ss"),
                datumStart = DateTime.Now.AddDays(random.Next(-30, 30)),
                eMail = "användare@exempel.se",
                KontaktPerson = "Kontakt Person",
                logorad1 = "Logorad1",
                logorad2 = "Logorad2",
                Pris = random.Next(50, 500) + 0.00m,
                reklam1 = "Reklam, kan vara en bild eller form av text",
                serviceavgift1_kr = random.Next(10, 30) + 0.00m,
                stolsrad = $"{random.Next(1, 5)}",
                wbarticleinfo = "Artikle Info",
                wbeventinfo = "Event Info",
                Webbcode = "123456A",
                webbkod = "W78910BC"
            };
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
            const string bgFileName = "Predefined";

            return await CreatePdfAsync(ticketHandlingData, bgFileData, bgFileName, null, false);
        }

        public async Task<byte[]> CreatePdfAsync(TicketHandling ticketHandling, byte[] bgFileData, string bgFileName, string? name, bool saveToDb)
        {
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
                var ticketData = GenerateMockTicketData();

                using PdfDocument document = new();
                PdfPage page = document.Pages.Add();
                await DrawPageContent(page, bgFilePath, ticketData, ticketHandling);

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
                        continue;
                    }

                    var fileData = await GetFileDataAsync(ticketTemplate.FileStorageID);
                    if (fileData == null)
                    {
                        _logger.LogWarning("Background image data not found for FileStorageID: {FileStorageID}", ticketTemplate.FileStorageID);
                        continue;
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
                    await DrawPageContent(page, bgFilePath, ticketData, ticketHandling);
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

        private async Task DrawPageContent(PdfPage page, string backgroundImagePath, TicketsDataView ticketData, TicketHandling ticketHandling)
        {
            float scaleFactor = Math.Min(page.GetClientSize().Width / pdfWidth, 1);
            PointF ticketOrigin = new((page.GetClientSize().Width - (pdfWidth * scaleFactor)) / 2, 0);

            DrawBackgroundImage(page, backgroundImagePath, ticketOrigin, scaleFactor);
            DrawTextContent(page.Graphics, ticketOrigin, scaleFactor, ticketData, ticketHandling);
            DrawCustomTextElements(page.Graphics, ticketHandling, ticketOrigin, scaleFactor);
            DrawBarcodeOrQRCode(page, ticketOrigin, scaleFactor, ticketHandling, ticketData);
            DrawScissorsLine(page.Graphics, ticketOrigin, scaleFactor, ticketHandling);
            DrawVitec(page.Graphics, scaleFactor);
            if (ticketHandling.IncludeAd)
            {
                await DrawAd(page.Graphics, page, ticketData.reklam1, ticketOrigin, scaleFactor, ticketHandling);
            }
        }

        private static async Task DrawAd(PdfGraphics graphics, PdfPage page, string htmlContent, PointF origin, float scale, TicketHandling ticketHandling)
        {
            string? imageUrl = ExtractImageUrl(htmlContent);
            htmlContent = HandleHtmlEntities(htmlContent);
            htmlContent = Regex.Replace(htmlContent, "<img.+?>", "", RegexOptions.IgnoreCase);

            PointF adPosition = new(
                origin.X + (ticketHandling.AdPositionX * scale),
                origin.Y + (ticketHandling.AdPositionY * scale)
            );
            if (!string.IsNullOrEmpty(imageUrl))
            {
                await DrawAdImage(graphics, imageUrl, adPosition, scale);
            }
            DrawHtmlContent(graphics, page, htmlContent, new PdfStandardFont(PdfFontFamily.Helvetica, 8), adPosition);
        }

        private void DrawBackgroundImage(PdfPage page, string imagePath, PointF origin, float scale)
        {
            using FileStream imageStream = new(imagePath, FileMode.Open, FileAccess.Read);
            PdfBitmap background = new(imageStream);

            page.Graphics.DrawImage(background, origin.X, origin.Y, pdfWidth * scale, pdfHeight * scale);
        }

        private static void DrawBarcodeOrQRCode(PdfPage page, PointF origin, float scale, TicketHandling ticketHandling, TicketsDataView ticketData)
        {
            PointF position = new(
                origin.X + (ticketHandling.BarcodePositionX * scale),
                origin.Y + (ticketHandling.BarcodePositionY * scale)
            );

            if (ticketHandling.UseQRCode)
            {
                DrawQRCode(page.Graphics, position, scale, ticketHandling, ticketData);
            }
            else
            {
                DrawBarcode(page.Graphics, position, scale, ticketHandling, ticketData);
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
                PdfFont customFont = GetCustomFont(fontSize, customTextElement.FontStyle);
                PdfBrush customBrush = GetCustomBrush(customTextElement.FontColor);

                PointF position = new(
                    origin.X + ((customTextElement.PositionX ?? 0) * scale),
                    origin.Y + ((customTextElement.PositionY ?? 0) * scale)
                );

                graphics.DrawString(customTextElement.Text, customFont, customBrush, position);
            }
        }

        private static PdfFont GetCustomFont(float? fontSize, Models.FontStyle fontStyle)
        {
            float resolvedFontSize = fontSize ?? 8;
            PdfFontStyle pdfStyle = fontStyle switch
            {
                Models.FontStyle.Bold => PdfFontStyle.Bold,
                Models.FontStyle.Italic => PdfFontStyle.Italic,
                Models.FontStyle.Underline => PdfFontStyle.Underline,
                Models.FontStyle.Strikeout => PdfFontStyle.Strikeout,
                _ => PdfFontStyle.Regular,
            };
            return new PdfStandardFont(PdfFontFamily.Helvetica, resolvedFontSize, pdfStyle);
        }

        private static PdfBrush GetCustomBrush(string? hexColor)
        {
            if (string.IsNullOrEmpty(hexColor) || !hexColor.StartsWith('#') || hexColor.Length != 7)
            {
                return new PdfSolidBrush(new PdfColor(0, 0, 0));
            }

            try
            {
                return new PdfSolidBrush(new PdfColor(
                    Convert.ToByte(hexColor.Substring(1, 2), 16),
                    Convert.ToByte(hexColor.Substring(3, 2), 16),
                    Convert.ToByte(hexColor.Substring(5, 2), 16)
                ));
            }
            catch
            {
                return new PdfSolidBrush(new PdfColor(0, 0, 0));
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
                origin.Y + (pdfHeight * scale) + (10 * scale)
                );
                SizeF scissorsSize = new(pdfWidth * scale, scissorsLineImage.Height * scale);

                graphics.DrawImage(scissorsLineImage, scissorsPosition, scissorsSize);
            }
        }

        private static void DrawTextContent(PdfGraphics graphics, PointF origin, float scale, TicketsDataView ticketData, TicketHandling ticketHandling)
        {
            foreach (var config in ticketHandling.TextConfigs.Select(configEntry => configEntry.Value))
            {
                if (config.Style.Include)
                {
                    var propertyInfo = ticketData.GetType().GetProperty(config.DataViewPropertyName);
                    var dataValue = propertyInfo?.GetValue(ticketData)?.ToString();
                    if (!string.IsNullOrEmpty(dataValue))
                    {
                        PdfFont font = GetCustomFont(config.Style.FontSize, config.Style.FontStyle);
                        PdfBrush brush = GetCustomBrush(config.Style.FontColor);
                        PointF adjustedOrigin = new(
                            origin.X + ((config.Style.PositionX ?? 0) * scale),
                            origin.Y + ((config.Style.PositionY ?? 0) * scale)
                        );
                        graphics.DrawString(dataValue, font, brush, adjustedOrigin);
                    }
                }
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
                Size = new SizeF(ticketHandling.BarcodeWidth * scale, ticketHandling.BarcodeHeight * scale),
                TextDisplayLocation = ticketHandling.HideBarcodeText ? TextLocation.None : TextLocation.Bottom
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

        private static void DrawQRCode(PdfGraphics graphics, PointF barcodePosition, float scale, TicketHandling ticketHandling, TicketsDataView ticketData)
        {
            PdfQRBarcode qrCode = new()
            {
                Text = string.IsNullOrEmpty(ticketData.webbkod) ? ticketData.Webbcode : ticketData.webbkod,
                Size = new SizeF(ticketHandling.QRSize * scale, ticketHandling.QRSize * scale)
            };
            qrCode.Draw(graphics, barcodePosition);
        }

        private static void DrawHtmlContent(PdfGraphics graphics, PdfPage page, string htmlContent, PdfFont font, PointF adPosition)
        {
            RectangleF rect = new(adPosition.X, adPosition.Y, page.GetClientSize().Width, page.GetClientSize().Height);
            PdfHTMLTextElement htmlTextElement = new(htmlContent, font, PdfBrushes.Black);
            htmlTextElement.Draw(graphics, rect);
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

        public TicketHandling DeserializeTextElements(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new TicketHandling();
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

                var elements = JsonConvert.DeserializeObject<TicketHandling>(json, settings);
                return elements ?? new TicketHandling();
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "Error deserializing ticketHandlingJson: {ErrorMessage}", ex.Message);
                return new TicketHandling();
            }
        }

        #endregion Helper methods
    }
}