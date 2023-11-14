using Models;
using DbModels;

namespace Services
{
    public interface IPdfTemplateService
    {
        Task CreatePdfAsync(string outputPath, TicketsDataDto ticketData, TicketHandling ticketDetails, string backgroundImagePath);

        Task<TicketsDataDto> GetTicketDataAsync(TicketHandling templateData);
    }
}
