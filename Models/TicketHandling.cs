namespace Models
{
    public enum Code
    {
        Barcode,
        QrCode
    }

    public class TicketHandling
    {
        public bool AddScissorsLine { get; set; }
        public bool IncludeAd { get; set; }
        public float AdPositionX { get; set; }
        public float AdPositionY { get; set; } = 500f;
        public float QRSize { get; set; } = 150f;
        public bool FlipBarcode { get; set; }
        public float BarcodePositionX { get; set; } = 825f;
        public float BarcodePositionY { get; set; } = 320f;
        public Code Code { get; set; } = Code.Barcode;
        public float BarcodeWidth { get; set; } = 270f;
        public float BarcodeHeight { get; set; } = 90f;
        public bool HideBarcodeText { get; set; }
        public List<CustomTextElement>? CustomTextElements { get; set; } = new();
        public Dictionary<string, TextElementConfig> TextConfigs { get; set; } = new();

        public TicketHandling()
        { InitializeTextConfigs(); }

        public void InitializeTextConfigs()
        {
            var properties = new Dictionary<string, (string PropertyName, float DefaultFontSize)>
            {
                {"Artikelnamn", ("Artikelnamn", 8)},
                {"ArtikelNr", ("ArtikelNr", 8)},
                {"BookingNr", ("BokningsNr", 8)},
                {"ChairNr", ("stolsnr", 9)},
                {"ChairRow", ("stolsrad", 9)},
                {"ContactPerson", ("KontaktPerson", 8)},
                {"Datum", ("Datum", 8)},
                {"Description", ("Beskrivning", 8)},
                {"Email", ("eMail", 8)},
                {"Entrance", ("Ingang", 9)},
                {"EventDate", ("datumStart", 9)},
                {"EventName", ("namn1", 9)},
                {"FacilityName", ("anamn", 9)},
                {"Logorad1", ("logorad1", 8)},
                {"Logorad2", ("logorad2", 8)},
                {"Price", ("Pris", 8)},
                {"RutBokstav", ("Rutbokstav", 24)},
                {"Section", ("namn", 9)},
                {"ServiceFee", ("serviceavgift1_kr", 8)},
                {"SubEventName", ("namn2", 8)},
                {"Webbcode", ("Webbcode", 8)},
                {"WebBookingNr", ("webbkod", 8)}
            };
            foreach (var property in properties)
            {
                TextConfigs[property.Key] = new TextElementConfig
                {
                    DataViewPropertyName = property.Value.PropertyName,
                    Style = new TextElement
                    {
                        FontSize = property.Value.DefaultFontSize
                    }
                };
            }
        }
    }
}