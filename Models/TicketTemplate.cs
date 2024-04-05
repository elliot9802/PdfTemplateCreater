namespace Models
{
    public class TicketTemplate : ITicketTemplate
    {
        public virtual int ShowEventInfo { get; set; }
        public virtual TicketHandling TicketsHandling { get; set; } = new TicketHandling();
        public virtual Guid TicketTemplateId { get; set; }
        public virtual string Name { get; set; } = string.Empty;
        public virtual int? FileStorageID { get; set; }

        public TicketTemplate()
        { }

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