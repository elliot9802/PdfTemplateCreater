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

        Task<TemplateCUdto> GetTemplateByIdAsync(Guid ticketTemplateId);

        TemplateCUdto MapTicketHandlingToTemplateCUdto(TicketHandling ticketHandling);

        Task<ITicketTemplate> UpdateTemplateAsync(TemplateCUdto templateDto, byte[]? bgFileData, string? bgFileName);

        Task<List<ITicketTemplate>> ReadTemplatesAsync();
    }
}