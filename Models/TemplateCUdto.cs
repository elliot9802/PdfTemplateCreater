namespace Models
{
    public class TemplateCUdto
    {
        public int? ShowEventInfo { get; set; }
        public string? TicketHandlingJson { get; set; }
        public TicketHandling TicketsHandling { get; set; } = new TicketHandling();
        public string Name { get; set; } = string.Empty;
        public Guid? TicketTemplateId { get; set; }
        public int? FileStorageID { get; set; }

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
            Name = org.Name;
            FileStorageID = org.FileStorageID;
        }
    }
}