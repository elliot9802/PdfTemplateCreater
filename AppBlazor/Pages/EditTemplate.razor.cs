using Microsoft.AspNetCore.Components;
using Models;
using Newtonsoft.Json;
using Services;

namespace AppBlazor.Pages
{
    public partial class EditTemplate
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
        [Parameter]
        public Guid TemplateId { get; set; }

        private TicketHandling ticketHandling = new();

        private readonly string pdfBase64 = string.Empty;

        private int showEventInfo;

        private string templateName = string.Empty;

        public string? ErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadTemplateData(TemplateId);
        }

        private async Task LoadTemplateData(Guid templateId)
        {
            try
            {
                var requestUri = ConfigService!.GetApiUrl($"/api/PdfTemplate/ReadTemplate?id={templateId}");
                var response = await HttpClient.GetAsync(requestUri);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var templateDTO = JsonConvert.DeserializeObject<TemplateCUdto>(jsonString);

                    if (templateDTO != null)
                    {
                        ticketHandling = !string.IsNullOrWhiteSpace(templateDTO.TicketHandlingJson)
                            ? JsonConvert.DeserializeObject<TicketHandling>(templateDTO.TicketHandlingJson) ?? new TicketHandling()
                            : new TicketHandling();
                        showEventInfo = templateDTO.ShowEventInfo;
                        templateName = templateDTO.Name;
                    }
                    else
                    {
                        ErrorMessage = "Template data could not be loaded.";
                    }
                }
                else
                {
                    ErrorMessage = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
        }
    }
}