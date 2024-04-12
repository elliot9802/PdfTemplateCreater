namespace Models
{
    public interface ITicketTemplate
    {
        int? ShowEventInfo { get; set; }
        TicketHandling TicketsHandling { get; set; }
        Guid? TicketTemplateId { get; set; }
        string Name { get; set; }
        int? FileStorageID { get; set; }
    }
}