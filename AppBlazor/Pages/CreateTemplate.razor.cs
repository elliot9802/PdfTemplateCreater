using AppBlazor.Components;
using Microsoft.AspNetCore.Components;
using Models;
using Services;

namespace AppBlazor.Pages
{
    public partial class CreateTemplate
    {
        // Dependency Injection Properties
        [Inject]
        public ConfigService ConfigService { get; set; } = default!;

        [Inject]
        public HttpClient HttpClient { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public TicketHandlingService TicketService { get; set; } = default!;

        // Component State Properties
        private TicketHandling ticketHandling = new();

        private ModalsComponent? successModal;

        private string selectedTicketType = string.Empty;

        private string? pdfBase64;
        public string? ErrorMessage { get; set; }

        // Lifecycle Methods
        private void InitializeState()
        {
            ErrorMessage = null;
            pdfBase64 = null;
            ticketHandling = new TicketHandling();
            selectedTicketType = "";
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            InitializeState();
        }

        private void OnTicketTypeChanged(ChangeEventArgs e)
        {
            selectedTicketType = e.Value?.ToString() ?? string.Empty;

            ticketHandling = selectedTicketType switch
            {
                "Numrerad" => TicketService.CreateNumreradTicketHandling(),
                "Onumrerad" => TicketService.CreateOnumreradTicketHandling(),
                "Presentkort" => TicketService.CreatePresentkortTicketHandling(),
                _ => new TicketHandling(),
            };
        }

        private Task HandlePdfSaved(bool success)
        {
            if (success)
            {
                successModal?.Show();
            }
            else
            {
                ErrorMessage = "Failed to save the PDF template.";
            }

            return Task.CompletedTask;
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