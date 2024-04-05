namespace Models
{
    public class TicketTemplateDto
    {
        public int ShowEventInfo { get; set; }
        public string? TicketHandlingJson { get; set; }
        public Guid TicketTemplateId { get; set; }
        public string? Name { get; set; }
        public int FileStorageID { get; set; }
    }
}