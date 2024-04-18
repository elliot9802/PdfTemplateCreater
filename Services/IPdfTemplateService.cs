using Models;
using Models.DTO;

namespace Services
{
    public interface IPdfTemplateService
    {
        Task<byte[]> CreateCombinedPdfAsync(Guid webbUid, int showEventInfo);

        Task<byte[]> CreatePdfAsync(TicketHandling ticketHandling, byte[] bgFileData, string bgFileName, string? name, bool saveToDb);

        Task<ITicketTemplate> DeleteTemplateAsync(Guid id);

        TicketHandling DeserializeTextElements(string json);

        Task<byte[]> GeneratePredefinedPdfAsync(int showEventInfo);

        Task<IEnumerable<WebbUidInfo>> GetAllWebbUidsAsync();

        Task<TemplateCUdto> GetTemplateByIdAsync(Guid ticketTemplateId);

        TemplateCUdto MapTicketHandlingToTemplateCUdto(TicketHandling ticketHandling);

        Task<ITicketTemplate> UpdateTemplateAsync(TemplateCUdto templateDto, byte[]? bgFileData, string? bgFileName);

        Task<List<ITicketTemplate>> ReadTemplatesAsync();
    }
}