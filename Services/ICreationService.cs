using DbModels;
using Models;

namespace Services
{
    public interface ICreationService
    {
        string GetTemporaryPdfFilePath();
        Task<byte[]> CreateAndSavePdfAsync(TicketsDataDto ticketData, TicketHandling ticketDetails, string backgroundImagePath);

    }
}
