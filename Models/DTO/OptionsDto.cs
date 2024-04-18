namespace Models.DTO
{
    public class OptionsDto
    {
        private string _customTextElementsJson = string.Empty;
        private string _ticketHandlingJson = string.Empty;
        private string _name = string.Empty;

        public string CustomTextElementsJson
        {
            get => _customTextElementsJson;
            set => _customTextElementsJson = value ?? string.Empty;
        }

        public string TicketHandlingJson
        {
            get => _ticketHandlingJson;
            set => _ticketHandlingJson = value ?? string.Empty;
        }

        public string Name
        {
            get => _name;
            set => _name = value ?? string.Empty;
        }

        public bool SaveToDb { get; set; }

        public OptionsDto()
        { }

        public OptionsDto(string ticketHandlingJson, string customTextElementsJson, string name, bool saveToDb)
        {
            TicketHandlingJson = ticketHandlingJson ?? string.Empty;
            CustomTextElementsJson = customTextElementsJson ?? string.Empty;
            Name = name ?? string.Empty;
            SaveToDb = saveToDb;
        }
    }
}