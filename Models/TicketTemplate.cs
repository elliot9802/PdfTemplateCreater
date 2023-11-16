namespace Models
{

    public class TicketTemplate : ITicketTemplate
    {
        public virtual Guid TicketTemplateId { get; set; }

        public virtual TicketHandling TicketsHandling { get; set; }

        public virtual int ShowEventInfo {  get; set; }

        public TicketTemplate() { }

        public TicketTemplate(TemplateCUdto dto)
        {
            TicketTemplateId = dto.TicketTemplateId;
            TicketsHandling = dto.TicketsHandling;
            ShowEventInfo = dto.ShowEventInfo;
        }
    }
}
