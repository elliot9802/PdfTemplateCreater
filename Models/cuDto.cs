namespace Models
{
    public class TemplateCUdto
    {
        public Guid TicketTemplateId { get; set; }

        public TicketHandling TicketsHandling { get; set; }

        public int ShowEventInfo { get; set; }

        public TemplateCUdto() { }

        public TemplateCUdto(ITicketTemplate org)
        {
            TicketTemplateId = org.TicketTemplateId;
            TicketsHandling = org.TicketsHandling;
            ShowEventInfo = org.ShowEventInfo;
        }
    }
}
