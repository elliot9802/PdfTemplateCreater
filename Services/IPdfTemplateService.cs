using DbModels;
using Models;

namespace Services
{
    public interface IPdfTemplateService
    {
        Task CreatePdfAsync(string outputPath, TicketsDataView ticketData, TicketHandling ticketHandling, string backgroundImagePath);

        Task<TicketHandling> CreateTemplateAsync(TemplateCUdto _src);

        Task<ITicketTemplate> DeleteTemplateAsync(Guid id);

        Task<TicketHandling?> GetPredefinedTicketHandlingAsync(int showEventInfo);

        Task<TicketTemplateDto> GetTemplateByIdAsync(Guid ticketTemplateId);

        string GetTemporaryPdfFilePath();

        Task<TicketsDataView?> GetTicketDataAsync(int? ticketId, int? showEventInfo);

        TemplateCUdto MapTicketHandlingToTemplateCUdto(TicketHandling ticketHandling);

        Task<TicketTemplateDto> UpdateTemplateAsync(TicketTemplateDto templateDto);

        Task<List<TicketTemplateDto>> ReadTemplatesAsync();

        Task<byte[]> CreateCombinedPdfAsync(Guid webbUid, string outputPath);

        Task<IEnumerable<TicketsDataView>> GetTicketsDataByWebbUidAsync(Guid webbUid);
    }
}