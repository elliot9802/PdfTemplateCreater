using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Models;
using Newtonsoft.Json;
using Services;

namespace AppBlazor.Pages
{
    public partial class GetTemplate
    {
        private ByteArrayContent? bgFileContent;
        private bool isPredefined = true;
        private string? pdfBase64;
        private int? selectedShowEventInfo;
        private Guid? selectedTemplateId;
        private List<TicketTemplateDto>? templates;
        private bool isLoading;

        [Inject]
        public ConfigService configService { get; set; }

        public string? ErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadTemplatesAsync();
        }

        private async Task DeleteTemplate()
        {
            if (!isPredefined && selectedTemplateId.HasValue)
            {
                var requestUri = configService.GetApiUrl($"/api/PdfTemplate/DeleteTicketTemplate/{selectedTemplateId}");

                var response = await HttpClient.DeleteAsync(requestUri);
                if (response.IsSuccessStatusCode)
                {
                    await LoadTemplatesAsync();
                    pdfBase64 = null;
                    await ShowTemporaryMessage("Template successfully deleted.", 5000);
                }
                else
                {
                    ErrorMessage = "Failed to delete the template. Please try again.";
                }
            }
        }

        private void EditTemplate()
        {
            if (selectedShowEventInfo.HasValue)
            {
                NavigationManager.NavigateTo($"/edit-template/{selectedTemplateId}");
            }
        }

        private void HandleFileUploaded(ByteArrayContent fileContent)
        {
            bgFileContent = fileContent;
            ErrorMessage = null;
        }

        private async Task LoadTemplatesAsync()
        {
            var requestUri = configService.GetApiUrl("/api/PdfTemplate/GetTicketTemplate");

            var response = await HttpClient.GetAsync(requestUri);
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                templates = JsonConvert.DeserializeObject<List<TicketTemplateDto>>(jsonString);
            }
            else
            {
                ErrorMessage = "Failed to load templates.";
            }
        }

        private void OnTemplateSelected(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out var showEventInfo))
            {
                var template = templates?.Find(t => t.ShowEventInfo == showEventInfo);
                selectedShowEventInfo = showEventInfo;
                selectedTemplateId = template?.TicketTemplateId;
                isPredefined = showEventInfo <= 3;
                pdfBase64 = null;
            }
            else
            {
                selectedShowEventInfo = null;
                selectedTemplateId = null;
            }
        }

        private async Task PreviewTemplate()
        {
            isLoading = true;
            ErrorMessage = string.Empty;
            if (selectedShowEventInfo.HasValue && selectedShowEventInfo > 0)
            {
                if (bgFileContent == null)
                {
                    ErrorMessage = "Please select a background file before proceeding";
                    isLoading = false;
                    return;
                }

                var fileName = bgFileContent.Headers?.ContentDisposition?.FileName?.Trim('"');
                if (string.IsNullOrEmpty(fileName))
                {
                    ErrorMessage = "The file name was not provided.";
                    isLoading = false;
                    return;
                }

                const string ticketId = "15612";
                var requestUri = configService.GetApiUrl($"/api/PdfTemplate/GetPredefinedTemplate/?showEventInfo={selectedShowEventInfo}&ticketId={ticketId}");

                var content = new MultipartFormDataContent
                {
                    { bgFileContent, "bgFile", fileName }
                };

                try
                {
                    var response = await HttpClient.PostAsync(requestUri, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var pdfData = await response.Content.ReadAsByteArrayAsync();
                        pdfBase64 = Convert.ToBase64String(pdfData);
                    }
                    else
                    {
                        ErrorMessage = "Failed to preview the template.";
                        isLoading = false;
                        Console.WriteLine($"Failed to preview PDF. Status code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = "An error occurred while attempting to preview the template.";
                    isLoading = false;
                    Console.WriteLine($"Exception while calling PreviewTemplate API: {ex.Message}");
                }
                finally
                {
                    isLoading = false;
                }
            }
        }

        private async Task ShowTemporaryMessage(string message, int duration)
        {
            await JSRuntime.InvokeVoidAsync("showTemporaryMessage", message, duration);
        }
    }
}