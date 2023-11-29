namespace Models
{
    public class TicketTemplate : ITicketTemplate
    {
        public virtual Guid TicketTemplateId { get; set; }

        public virtual TicketHandling TicketsHandling { get; set; } = new TicketHandling();

        public virtual int ShowEventInfo {  get; set; }

        public TicketTemplate() { }

        public TicketTemplate(TemplateCUdto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "TemplateCUdto cannot be null.");
            }

            TicketTemplateId = dto.TicketTemplateId;
            TicketsHandling = dto.TicketsHandling ?? TicketsHandling;
            ShowEventInfo = dto.ShowEventInfo;
        }
    }
}
