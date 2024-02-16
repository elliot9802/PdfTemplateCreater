using DbModels;
using Models;

namespace Services
{
    public interface IPdfTemplateService
    {
        Task<List<int>> ReadTemplatesAsync();

        Task<TicketsDataDto> GetTicketDataAsync(int? ticketId, int? showEventInfo);

        Task<TicketHandling> GetPredefinedTicketHandlingAsync(int showEventInfo);

        Task<TicketHandling> CreateTemplateAsync(TemplateCUdto _src);

        Task<ITicketTemplate> DeleteTemplateAsync(Guid id);

        Task CreatePdfAsync(string outputPath, TicketsDataDto ticketData, TicketHandling ticketDetails, string backgroundImagePath);

        TemplateCUdto MapTicketHandlingToTemplateCUdto(TicketHandling ticketHandling);

        string GetTemporaryPdfFilePath();
    }
}