namespace Models
{
    public interface ITicketTemplate
    {
        int ShowEventInfo { get; set; }
        TicketHandling TicketsHandling { get; set; }
        Guid TicketTemplateId { get; set; }
    }
}