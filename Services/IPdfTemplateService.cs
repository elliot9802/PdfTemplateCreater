using DbModels;
using Models;

namespace Services
{
    public interface IPdfTemplateService
    {
        Task<byte[]> CreateCombinedPdfAsync(Guid webbUid, int showEventInfo);

        Task<byte[]> CreatePdfAsync(TicketHandling ticketHandling, byte[] bgFileData, string bgFileName, string? name, bool saveToDb);

        Task<ITicketTemplate> DeleteTemplateAsync(Guid id);

        Task<byte[]> GeneratePredefinedPdfAsync(int showEventInfo);

        Task<TicketTemplateDto> GetTemplateByIdAsync(Guid ticketTemplateId);

        TemplateCUdto MapTicketHandlingToTemplateCUdto(TicketHandling ticketHandling);

        Task<TicketTemplateDto> UpdateTemplateAsync(TicketTemplateDto templateDto);

        Task<List<TicketTemplateDto>> ReadTemplatesAsync();
    }
}