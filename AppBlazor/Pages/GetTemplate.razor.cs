using Microsoft.AspNetCore.Components;
using Models;
using Newtonsoft.Json;
using Services;

namespace AppBlazor.Pages
{
    public partial class GetTemplate
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
        private List<TemplateCUdto>? templates;

        private bool isLoading;

        private bool isPredefined = true;

        private string? pdfBase64;

        private int? selectedShowEventInfo;

        private Guid? selectedTemplateId;

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadTemplatesAsync();
        }

        private async Task DeleteTemplate()
        {
            try
            {
                if (!isPredefined && selectedTemplateId.HasValue)
                {
                    var requestUri = ConfigService!.GetApiUrl($"/api/PdfTemplate/DeleteTicketTemplate/{selectedTemplateId}");
                    var response = await HttpClient.DeleteAsync(requestUri);
                    if (response.IsSuccessStatusCode)
                    {
                        await LoadTemplatesAsync();
                        pdfBase64 = null;
                        selectedShowEventInfo = null;
                        selectedTemplateId = null;
                        SuccessMessage = "Mallen har tagits bort.";
                    }
                    else
                    {
                        ErrorMessage = "Failed to delete the template. Please try again.";
                    }
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
        }

        private void EditTemplate()
        {
            if (selectedShowEventInfo.HasValue)
            {
                NavigationManager.NavigateTo($"/edit-template/{selectedTemplateId}");
            }
        }

        private async Task LoadTemplatesAsync()
        {
            var requestUri = ConfigService!.GetApiUrl("/api/PdfTemplate/ReadTemplatesDto");

            var response = await HttpClient.GetAsync(requestUri);
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                templates = JsonConvert.DeserializeObject<List<TemplateCUdto>>(jsonString);
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
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;
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
            SuccessMessage = string.Empty;

            if (selectedShowEventInfo.HasValue && selectedShowEventInfo > 0)
            {
                var requestUri = ConfigService!.GetApiUrl($"/api/PdfTemplate/GetPredefinedTemplate/?showEventInfo={selectedShowEventInfo}");
                var emptyContent = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");
                try
                {
                    var response = await HttpClient.PostAsync(requestUri, emptyContent);
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
    }
}