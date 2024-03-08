using Microsoft.AspNetCore.Components;
using Models;
using Newtonsoft.Json;
using Services;
using System.Text;

namespace AppBlazor.Pages
{
    public partial class EditTemplate
    {
        private readonly string pdfBase64 = string.Empty;

        private int showEventInfo;

        private string templateName = string.Empty;

        private TicketHandling ticketHandling = new();

        [Inject]
        public ConfigService configService { get; set; }

        public string? ErrorMessage { get; set; }

        [Parameter]
        public Guid TemplateId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadTemplateData(TemplateId);
        }

        // Helper Methods
        private async Task LoadTemplateData(Guid templateId)
        {
            try
            {
                var requestUri = configService.GetApiUrl($"/api/PdfTemplate/GetTicketTemplate?ticketTemplateId={templateId}");
                var response = await HttpClient.GetAsync(requestUri);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var templateDTO = JsonConvert.DeserializeObject<TicketTemplateDto>(jsonString);

                    if (templateDTO != null)
                    {
                        if (!string.IsNullOrWhiteSpace(templateDTO.TicketHandlingJson))
                        {
                            ticketHandling = JsonConvert.DeserializeObject<TicketHandling>(templateDTO.TicketHandlingJson) ?? new TicketHandling();
                        }
                        else
                        {
                            ticketHandling = new TicketHandling();
                        }
                        showEventInfo = templateDTO.ShowEventInfo;
                        templateName = templateDTO.Name ?? string.Empty;
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

        private async Task SaveTemplate()
        {
            var templateDTO = new TicketTemplateDto
            {
                TicketTemplateId = TemplateId,
                TicketHandlingJson = JsonConvert.SerializeObject(ticketHandling),
                ShowEventInfo = showEventInfo,
                Name = templateName
            };

            var jsonContent = JsonConvert.SerializeObject(templateDTO);
            Console.WriteLine(jsonContent);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var requestUri = configService.GetApiUrl("/api/PdfTemplate/UpdateTemplate");
                var response = await HttpClient.PutAsync(requestUri, content);
                if (response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Template updated successfully!";
                }
                else
                {
                    ErrorMessage = "Failed to update the template: " + await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            }
        }
    }
}