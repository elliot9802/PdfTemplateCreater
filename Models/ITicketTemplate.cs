namespace Models
{
    public interface ITicketTemplate
    {
        Guid TicketTemplateId { get; set; }
        TicketHandling TicketsHandling { get; set; }
        int ShowEventInfo { get; set; }
    }
}
