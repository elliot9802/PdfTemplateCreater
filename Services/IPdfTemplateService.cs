using Models;
using DbModels;

namespace Services
{
    public interface IPdfTemplateService
    {
        Task<TicketsDataDto> GetTicketDataAsync(int? ticketId, int? showEventInfo);
        //Task<TicketTemplateDbM> GetPredefinedTemplateAsync(int templateLayout, TicketsDataDto ticketData, TicketTemplateDbM ticketDbM);

        //Task<TicketHandling> GetPredefinedTemplateAsync(int templateLayout);
        Task<TicketHandling> GetPredefinedTicketHandlingAsync(int showEventInfo);
        Task<TicketHandling> CreateTemplateAsync(TemplateCUdto _src);
        Task<ITicketTemplate> DeleteTemplateAsync(Guid id);

        Task CreatePdfAsync(string outputPath, TicketsDataDto ticketData, TicketHandling ticketDetails, string backgroundImagePath);

        TemplateCUdto MapTicketHandlingToTemplateCUdto(TicketHandling ticketHandling);
    }
}
