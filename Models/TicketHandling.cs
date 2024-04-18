namespace Models
{
    public class TicketHandling
    {
        #region Standard options for non-text elements

        public bool AddScissorsLine { get; set; }
        public bool IncludeAd { get; set; }
        public float? AdPositionX { get; set; }
        public float? AdPositionY { get; set; }
        public bool UseQRCode { get; set; }
        public bool FlipBarcode { get; set; }
        public float? BarcodePositionX { get; set; }
        public float? BarcodePositionY { get; set; }

        public List<CustomTextElement>? CustomTextElements { get; set; } = new();
        public Dictionary<string, TextElementConfig> TextConfigs { get; set; } = new();

        #endregion Standard options for non-text elements

        public TicketHandling()
        { InitializeTextConfigs(); }

        public void InitializeTextConfigs()
        {
            var properties = new Dictionary<string, string>
            {
                {"Artikelnamn", "Artikelnamn"},
                {"ArtikelNr", "ArtikelNr"},
                {"BookingNr", "BokningsNr"},
                {"ChairNr", "stolsnr"},
                {"ChairRow", "stolsrad"},
                {"ContactPerson", "KontaktPerson"},
                {"Datum", "Datum"},
                {"Description", "Beskrivning"},
                {"Email", "eMail"},
                {"Entrance", "Ingang"},
                {"EventDate", "datumStart"},
                {"EventName", "namn1"},
                {"FacilityName", "anamn"},
                {"Logorad1", "logorad1"},
                {"Logorad2", "logorad2"},
                {"Price", "Pris"},
                {"RutBokstav", "Rutbokstav"},
                {"Section", "namn"},
                {"ServiceFee", "serviceavgift1_kr"},
                {"SubEventName", "namn2"},
                {"Webbcode", "Webbcode"},
                {"WebBookingNr", "webbkod"}
            };
            foreach (var property in properties)
            {
                TextConfigs[property.Key] = new TextElementConfig
                {
                    DataViewPropertyName = property.Value,
                    Style = new TextElement()
                };
            }
        }
    }
}