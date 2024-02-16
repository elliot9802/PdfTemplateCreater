namespace Models
{
    public class TemplateCUdto
    {
        public Guid TicketTemplateId { get; set; }
        public TicketHandling TicketsHandling { get; set; } = new TicketHandling();
        public string? TicketHandlingJson { get; set; }
        public int ShowEventInfo { get; set; }

        public TemplateCUdto()
        { }

        public TemplateCUdto(ITicketTemplate org)
        {
            if (org == null)
            {
                throw new ArgumentNullException(nameof(org), "Provided ITicketTemplate is null.");
            }
            TicketTemplateId = org.TicketTemplateId;
            TicketsHandling = org.TicketsHandling ?? new TicketHandling();
            ShowEventInfo = org.ShowEventInfo;
        }
    }
}