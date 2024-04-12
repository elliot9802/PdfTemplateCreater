namespace Models.DTO
{
    public class OptionsDto
    {
        public TicketHandling TicketHandling { get; set; }
        public string CustomTextElementsJson { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool SaveToDb { get; set; }

        public OptionsDto()
        {
            TicketHandling = new TicketHandling();
        }

        public OptionsDto(TicketHandling ticketHandling, string customTextElementsJson, string name, bool saveToDb)
        {
            TicketHandling = ticketHandling ?? throw new ArgumentNullException(nameof(ticketHandling));
            CustomTextElementsJson = customTextElementsJson ?? throw new ArgumentNullException(nameof(customTextElementsJson));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            SaveToDb = saveToDb;
        }
    }
}