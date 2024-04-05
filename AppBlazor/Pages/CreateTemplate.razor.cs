using AppBlazor.Components;
using Microsoft.AspNetCore.Components;
using Models;
using Services;

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
        private TicketHandling ticketHandling = new();

        private ModalsComponent? successModal;

        private string? pdfBase64;
        public string? ErrorMessage { get; set; }

        // Lifecycle Methods
        private void InitializeState()
        {
            ErrorMessage = null;
            pdfBase64 = null;
            ticketHandling = new TicketHandling();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            InitializeState();
        }

        private void OnTicketTypeChanged(ChangeEventArgs e)
        {
            var selectedTicketType = e.Value?.ToString();

            ticketHandling = selectedTicketType switch
            {
                "Numrerad" => TicketHandling.CreateNumreradTicketHandling(),
                "Onumrerad" => TicketHandling.CreateOnumreradTicketHandling(),
                "Presentkort" => TicketHandling.CreatePresentkortTicketHandling(),
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