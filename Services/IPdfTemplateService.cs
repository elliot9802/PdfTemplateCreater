using DbModels;
using Models;

namespace Services
{
    public interface IPdfTemplateService
    {
        Task CreatePdfAsync(string outputPath, TicketsDataDto ticketData, TicketHandling ticketHandling, string backgroundImagePath);

        Task<TicketHandling> CreateTemplateAsync(TemplateCUdto _src);

        Task<ITicketTemplate> DeleteTemplateAsync(Guid id);

        Task<TicketHandling?> GetPredefinedTicketHandlingAsync(int showEventInfo);

        Task<TicketTemplateDto> GetTemplateByIdAsync(Guid ticketTemplateId);

        string GetTemporaryPdfFilePath();

        Task<TicketsDataDto?> GetTicketDataAsync(int? ticketId, int? showEventInfo);

        TemplateCUdto MapTicketHandlingToTemplateCUdto(TicketHandling ticketHandling);

        Task<TicketTemplateDto> UpdateTemplateAsync(TicketTemplateDto templateDto);

        Task<List<TicketTemplateDto>> ReadTemplatesAsync();
    }
}