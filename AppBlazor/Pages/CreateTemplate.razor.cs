using AppBlazor.Components;
using Microsoft.AspNetCore.Components;
using Models;
using Services;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AppBlazor.Pages
{
    public partial class CreateTemplate
    {
        private ByteArrayContent? bgFileContent;

        private List<CustomTextElement> customTexts = new();

        private string? pdfBase64;
        private ModalsComponent? successModal;
        private string templateName = string.Empty;
        private TicketHandling ticketHandling = new();

        private bool isPreviewLoading;
        private bool isSaveLoading;

        [Inject]
        public ConfigService configService { get; set; }

        public IConfiguration? Configuration { get; set; }
        public string? ErrorMessage { get; set; }

        // Lifecycle Methods
        protected override void OnInitialized()
        {
            base.OnInitialized();
            InitializeState();
        }

        private async Task CreatePdf(bool saveToDb)
        {
            if (saveToDb)
            {
                isSaveLoading = true;
            }
            else
            {
                isPreviewLoading = true;
            }

            if (bgFileContent == null)
            {
                ErrorMessage = "Please select a background file before proceeding";
                return;
            }

            var fileName = bgFileContent.Headers?.ContentDisposition?.FileName?.Trim('"');
            if (string.IsNullOrEmpty(fileName))
            {
                ErrorMessage = "The file name was not provided.";
                return;
            }

            const string ticketId = "15612";
            var requestUri = configService.GetApiUrl($"/api/PdfTemplate/CreateTemplate?ticketId={ticketId}&saveToDb={saveToDb}");
            var content = new MultipartFormDataContent
            {
                { bgFileContent, "bgFile", fileName }
            };

            if (saveToDb && !string.IsNullOrWhiteSpace(templateName))
            {
                content.Add(new StringContent(templateName), "name");
            }

            foreach (PropertyInfo property in ticketHandling.GetType().GetProperties())
            {
                var value = property.GetValue(ticketHandling, null)?.ToString();
                if (value != null)
                {
                    content.Add(new StringContent(value), property.Name);
                }
            }

            var customTextElementsJson = JsonSerializer.Serialize(customTexts);

            content.Add(new StringContent(customTextElementsJson, Encoding.UTF8, "application/json"), "customTextElementsJson");

            try
            {
                var response = await HttpClient.PostAsync(requestUri, content);
                if (response.IsSuccessStatusCode)
                {
                    if (saveToDb)
                    {
                        successModal?.Show();
                    }
                    else
                    {
                        var pdfData = await response.Content.ReadAsByteArrayAsync();
                        pdfBase64 = Convert.ToBase64String(pdfData);
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to create PDF. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while calling CreatePdf API: {ex.Message}");
            }
            finally
            {
                if (saveToDb)
                {
                    isSaveLoading = false;
                }
                else
                {
                    isPreviewLoading = false;
                }
            }
        }

        private void HandleFileUploaded(ByteArrayContent fileContent)
        {
            bgFileContent = fileContent;
            ErrorMessage = null;
        }

        // Methods to handle user selection after saving
        private Task HandleContinueFromCurrent()
        {
            successModal?.Hide();
            return Task.CompletedTask;
        }

        private Task HandleSelectExistingTemplates()
        {
            NavigationManager.NavigateTo("/existing-templates");
            successModal?.Hide();
            return Task.CompletedTask;
        }

        private Task HandleStartFresh()
        {
            successModal?.Hide();
            InitializeState();
            return Task.CompletedTask;
        }

        private void InitializeState()
        {
            ErrorMessage = null;
            pdfBase64 = null;
            ticketHandling = new TicketHandling();
            customTexts = new List<CustomTextElement>();
        }
    }
}