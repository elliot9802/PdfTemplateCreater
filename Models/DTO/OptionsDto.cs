namespace Models.DTO
{
    public class OptionsDto
    {
        public TicketHandling TicketHandling { get; set; }
        public string CustomTextElementsJson { get; set; }
        public string Name { get; set; }
        public bool SaveToDb { get; set; }

        public OptionsDto()
        { }

        public OptionsDto(TicketHandling ticketHandling, string customTextElementsJson, string name, bool saveToDb)
        {
            TicketHandling = ticketHandling;
            CustomTextElementsJson = customTextElementsJson;
            Name = name;
            SaveToDb = saveToDb;
        }
    }
}