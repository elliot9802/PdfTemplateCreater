using DbModels;
using Models;

namespace Services
{
    public interface IPdfTemplateService
    {
        Task<byte[]> CreateCombinedPdfAsync(Guid webbUid, string outputPath);

        Task CreatePdfAsync(string outputPath, TicketHandling ticketHandling, string backgroundImagePath);

        Task<TicketHandling> CreateTemplateAsync(TemplateCUdto _src);

        Task<ITicketTemplate> DeleteTemplateAsync(Guid id);

        TicketsDataView GenerateMockTicketData(string ticketType);

        Task<byte[]> GetFileDataAsync(int? fileStorageID);

        Task<string> GetFilePathAsync(int fileId);

        Task<TicketHandling?> GetPredefinedTicketHandlingAsync(int showEventInfo);

        Task<TicketTemplateDto> GetTemplateByIdAsync(Guid ticketTemplateId);

        string GetTemporaryPdfFilePath();

        Task<TicketsDataView?> GetTicketDataAsync(int? ticketId, int? showEventInfo);

        Task<IEnumerable<TicketsDataView>> GetTicketsDataByWebbUidAsync(Guid webbUid);

        TemplateCUdto MapTicketHandlingToTemplateCUdto(TicketHandling ticketHandling);

        Task<TicketTemplateDbM?> GetTicketTemplateByShowEventInfoAsync(int showEventInfo);

        Task<int> SaveFileToDatabaseAsync(byte[] fileData, string description, string name);

        Task<TicketTemplateDto> UpdateTemplateAsync(TicketTemplateDto templateDto);

        Task<List<TicketTemplateDto>> ReadTemplatesAsync();
    }
}