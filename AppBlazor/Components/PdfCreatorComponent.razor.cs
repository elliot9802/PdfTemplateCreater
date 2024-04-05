using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Models;
using Newtonsoft.Json;
using Services;
using System.Reflection;
using System.Text;

namespace AppBlazor.Components
{
    public partial class PdfCreatorComponent
    {
        [Parameter] public EventCallback<bool> OnSave { get; set; }
        [Parameter] public bool IsEditMode { get; set; }
        [Parameter] public TicketHandling TicketHandling { get; set; } = new TicketHandling();
        [Parameter] public List<CustomTextElement> CustomTexts { get; set; } = new List<CustomTextElement>();
        [Parameter] public Guid? TemplateId { get; set; }
        [Parameter] public string? TemplateName { get; set; }
        [Parameter] public int? ShowEventInfo { get; set; }

        // Dependency Injection Properties
        private ConfigService? _configService;

        [Inject]
        public ConfigService? ConfigService
        {
            get => _configService ?? throw new InvalidOperationException("ConfigService is not configured.");
            set => _configService = value;
        }

        [Inject]
        private IJSRuntime? JSRuntime { get; set; }

        private string pdfBase64 = string.Empty;
        private bool isPreviewLoading;
        private bool isSaveLoading;
        private string? ErrorMessage = string.Empty;
        public string? SuccessMessage { get; set; }

        private readonly Dictionary<string, string> errorElementMap = new()
        {
    { "Vänligen välj en bakgrundsbild innan du fortsätter", "backgroundImageUpload" },
    { "Vänligen välj ett namn för din mall innan du fortsätter", "templateName" }
};

        private string currentFocusElementId = string.Empty;
        private bool shouldFocusOnError;

        private string templateName = string.Empty;

        private ByteArrayContent? bgFileContent;

        private enum Tab
        {
            Info,
            CustomText,
            PropertyGroup
        }

        private Tab selectedTab = Tab.Info;

        private void SelectTab(Tab tab)
        {
            selectedTab = tab;
        }

        //private async Task HandleFileUpload(InputFileChangeEventArgs e)
        //{
        //    const int maxAllowedFiles = 1;
        //    var file = e.GetMultipleFiles(maxAllowedFiles).FirstOrDefault();
        //    if (file != null)
        //    {
        //        var stream = file.OpenReadStream();
        //        var buffer = new byte[file.Size];
        //        await stream.ReadAsync(buffer);
        //        bgFileContent = new ByteArrayContent(buffer);
        //        bgFileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        //        bgFileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        //        {
        //            Name = "\"bgFile\"",
        //            FileName = $"\"{file.Name}\""
        //        };
        //        ErrorMessage = string.Empty;
        //    }
        //}

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

            foreach (PropertyInfo property in TicketHandling.GetType().GetProperties())
            {
                var value = property.GetValue(TicketHandling, null)?.ToString();
                if (value != null)
                {
                    content.Add(new StringContent(value), property.Name);
                }
            }

            var customTextElementsJson = System.Text.Json.JsonSerializer.Serialize(CustomTexts);
            content.Add(new StringContent(customTextElementsJson, Encoding.UTF8, "application/json"), "customTextElementsJson");

            return content;
        }

        private async Task HandleResponse(HttpResponseMessage response, bool saveToDb)
        {
            if (response.IsSuccessStatusCode)
            {
                if (saveToDb)
                {
                    await OnSave.InvokeAsync(true);
                }
                else
                {
                    var pdfData = await response.Content.ReadAsByteArrayAsync();
                    pdfBase64 = Convert.ToBase64String(pdfData);
                }
            }
            else
            {
                ErrorMessage = "Failed to generate PDF: " + await response.Content.ReadAsStringAsync();
            }
        }

        private void HandleFileUploaded(ByteArrayContent fileContent)
        {
            bgFileContent = fileContent;
            ErrorMessage = null;
        }

        private void SetErrorMessageAndResetLoading(string message)
        {
            ErrorMessage = message;
            isPreviewLoading = false;
            isSaveLoading = false;
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                SelectTab(Tab.Info);
                currentFocusElementId = errorElementMap.FirstOrDefault(x => ErrorMessage.Contains(x.Key)).Value ?? string.Empty;
                shouldFocusOnError = true;
                StateHasChanged();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (shouldFocusOnError && JSRuntime != null && !string.IsNullOrEmpty(currentFocusElementId))
            {
                shouldFocusOnError = false;
                try
                {
                    await JSRuntime.InvokeVoidAsync("focusOnElementById", currentFocusElementId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error invoking JS for focusing on file input: {ex.Message}");
                }
            }
        }

        private async Task CreatePdf(bool saveToDb)
        {
            isSaveLoading = saveToDb;
            isPreviewLoading = !saveToDb;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (bgFileContent == null)
            {
                SetErrorMessageAndResetLoading("Vänligen välj en bakgrundsbild innan du fortsätter");
                return;
            }

            var fileName = bgFileContent.Headers?.ContentDisposition?.FileName?.Trim('"');
            if (string.IsNullOrEmpty(fileName))
            {
                SetErrorMessageAndResetLoading("Filnamnet angavs inte.");
                return;
            }

            if (saveToDb && string.IsNullOrWhiteSpace(templateName))
            {
                SetErrorMessageAndResetLoading("Vänligen välj ett namn för din mall innan du fortsätter");
                return;
            }

            var requestUri = ConfigService!.GetApiUrl($"/api/PdfTemplate/CreateTemplate?saveToDb={saveToDb}");
            var content = CreateMultipartFormDataContent(bgFileContent, fileName);

            try
            {
                var response = await HttpClient.PostAsync(requestUri, content);
                await HandleResponse(response, saveToDb);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            }
            finally
            {
                isSaveLoading = false;
                isPreviewLoading = false;
            }
        }

        private async Task UpdateTemplate()
        {
            isSaveLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (!TemplateId.HasValue || !ShowEventInfo.HasValue)
            {
                Console.WriteLine("Template ID and/or Show Event Info are null.");
                isSaveLoading = false;
                return;
            }

            var templateDTO = new TicketTemplateDto
            {
                TicketTemplateId = TemplateId.Value,
                TicketHandlingJson = JsonConvert.SerializeObject(TicketHandling),
                ShowEventInfo = ShowEventInfo.Value,
                Name = templateName
            };

            var jsonContent = JsonConvert.SerializeObject(templateDTO);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var requestUri = ConfigService!.GetApiUrl("/api/PdfTemplate/UpdateTemplate");
                var response = await HttpClient.PutAsync(requestUri, content);
                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Template successfully updated!";
                    await OnSave.InvokeAsync(true);
                }
                else
                {
                    var statusCode = response.StatusCode;
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Failed to update the template: {statusCode} - {(string.IsNullOrEmpty(errorContent) ? "No additional error information provided." : errorContent)}";
                }
            }
            catch (HttpRequestException httpException)
            {
                ErrorMessage = "A network error occurred. Please check your connection and try again.";
                Console.WriteLine($"HttpRequestException: {httpException.Message}");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred. Please try again.";
                Console.WriteLine($"Exception: {ex.Message}");
            }
            finally
            {
                isSaveLoading = false;
            }
        }
    }
}