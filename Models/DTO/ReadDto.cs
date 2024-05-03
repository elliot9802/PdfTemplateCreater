namespace Models.DTO
{
    public class ReadDto
    {
        public Guid TicketTemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ShowEventInfo { get; set; }
    }

    public class WebbUidInfo
    {
        public Guid? WebbUid { get; set; }
        public string? Name { get; set; } = "Empty";
    }
}