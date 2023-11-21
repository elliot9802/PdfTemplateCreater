using Models;
using DbModels;

namespace Services
{
    public interface IPdfTemplateService
    {
        Task<TicketsDataDto> GetTicketDataAsync(TicketHandling templateData);

        Task<ITicketTemplate> CreateTemplateAsync(TemplateCUdto _src);
        Task<ITicketTemplate> DeleteTemplateAsync(Guid id);

        Task CreatePdfAsync(string outputPath, TicketsDataDto ticketData, TicketHandling ticketDetails, string backgroundImagePath);

        TemplateCUdto MapTicketHandlingToTemplateCUdto(TicketHandling ticketHandling);

    }
}
