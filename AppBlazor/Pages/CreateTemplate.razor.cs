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
        // Dependency Injection Properties
        private ConfigService? _configService;

        [Inject]
        public ConfigService? ConfigService
        {
            get => _configService ?? throw new InvalidOperationException("ConfigService is not configured.");
            set => _configService = value;
        }

        // Component State Properties
        private List<CustomTextElement> customTexts = new();

        private TicketHandling ticketHandling = new();
        private ByteArrayContent? bgFileContent;
        private ModalsComponent? successModal;

        private bool isPreviewLoading;
        private bool isSaveLoading;
        private string? pdfBase64;
        private string templateName = string.Empty;

        public string? ErrorMessage { get; set; }

        // Lifecycle Methods
        private void InitializeState()
        {
            ErrorMessage = null;
            pdfBase64 = null;
            ticketHandling = new TicketHandling();
            customTexts = new List<CustomTextElement>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            InitializeState();
        }

        private async Task CreatePdf(bool saveToDb)
        {
            isSaveLoading = saveToDb;
            isPreviewLoading = !saveToDb;

            if (bgFileContent == null)
            {
                SetErrorMessageAndResetLoading("Vänligen välj en bakgrundsbild innan du fortsätter");
                return;
            }

            var fileName = bgFileContent.Headers?.ContentDisposition?.FileName?.Trim('"');
            if (string.IsNullOrEmpty(fileName))
            {
                SetErrorMessageAndResetLoading("The file name was not provided.");
                return;
            }

            if (saveToDb && string.IsNullOrWhiteSpace(templateName))
            {
                SetErrorMessageAndResetLoading("Vänligen välj ett namn för din mall innan du fortsätter");
                return;
            }

            const string ticketId = "16835";
            var requestUri = ConfigService!.GetApiUrl($"/api/PdfTemplate/CreateTemplate?ticketId={ticketId}&saveToDb={saveToDb}");
            var content = CreateMultipartFormDataContent(bgFileContent, fileName);

            try
            {
                var response = await HttpClient.PostAsync(requestUri, content);
                await HandleResponse(response, saveToDb);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while calling CreatePdf API: {ex.Message}");
            }
            finally
            {
                isSaveLoading = false;
                isPreviewLoading = false;
            }
        }

        // Helper Methods
        private void SetErrorMessageAndResetLoading(string message)
        {
            ErrorMessage = message;
            isPreviewLoading = false;
            isSaveLoading = false;
        }

        private MultipartFormDataContent CreateMultipartFormDataContent(ByteArrayContent bgFileContent, string fileName)
        {
            var content = new MultipartFormDataContent
            {
                { bgFileContent, "bgFile", fileName }
            };

            if (!string.IsNullOrWhiteSpace(templateName))
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

            return content;
        }

        private async Task HandleResponse(HttpResponseMessage response, bool saveToDb)
        {
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

        private void HandleFileUploaded(ByteArrayContent fileContent)
        {
            bgFileContent = fileContent;
            ErrorMessage = null;
        }

        // SaveToDb User Interaction Handlers
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
    }
}