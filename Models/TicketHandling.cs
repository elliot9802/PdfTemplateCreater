namespace Models
{
    public class TicketHandling
    {
        #region Standard options

        public bool IncludeAd { get; set; }
        public float? AdPositionX { get; set; }
        public float? AdPositionY { get; set; }

        public bool IncludeArtName { get; set; }
        public float? ArtNamePositionX { get; set; }
        public float? ArtNamePositionY { get; set; }

        public bool IncludeArtNotText { get; set; }
        public float? ArtNotTextPositionX { get; set; }
        public float? ArtNotTextPositionY { get; set; }

        public bool IncludeArtNr { get; set; }
        public float? ArtNrPositionX { get; set; }
        public float? ArtNrPositionY { get; set; }

        public bool IncludeBookingNr { get; set; }
        public float? BookingNrPositionX { get; set; }
        public float? BookingNrPositionY { get; set; }

        public bool IncludeChairNr { get; set; }
        public float? ChairNrPositionX { get; set; }
        public float? ChairNrPositionY { get; set; }

        public bool IncludeChairRow { get; set; }
        public float? ChairRowPositionX { get; set; }
        public float? ChairRowPositionY { get; set; }

        public bool IncludeContactPerson { get; set; }
        public float? ContactPersonPositionX { get; set; }
        public float? ContactPersonPositionY { get; set; }

        public bool IncludeDatum { get; set; }
        public float? DatumPositionX { get; set; }
        public float? DatumPositionY { get; set; }

        public bool IncludeDescription { get; set; }
        public float? DescriptionPositionX { get; set; }
        public float? DescriptionPositionY { get; set; }

        public bool IncludeEmail { get; set; }
        public float? EmailPositionX { get; set; }
        public float? EmailPositionY { get; set; }

        public bool IncludeEntrance { get; set; }
        public float? EntrancePositionX { get; set; }
        public float? EntrancePositionY { get; set; }

        public bool IncludeEventDate { get; set; }
        public float? EventDatePositionX { get; set; }
        public float? EventDatePositionY { get; set; }

        public bool IncludeEventName { get; set; }
        public float? EventNamePositionX { get; set; }
        public float? EventNamePositionY { get; set; }

        public bool IncludeFacilityName { get; set; }
        public float? FacilityNamePositionX { get; set; }
        public float? FacilityNamePositionY { get; set; }

        public bool IncludeLogorad1 { get; set; }
        public float? Logorad1PositionX { get; set; }
        public float? Logorad1PositionY { get; set; }

        public bool IncludeLogorad2 { get; set; }
        public float? Logorad2PositionX { get; set; }
        public float? Logorad2PositionY { get; set; }

        public bool IncludePrice { get; set; }
        public float? PricePositionX { get; set; }
        public float? PricePositionY { get; set; }

        public bool IncludeRutBokstav { get; set; }
        public float? RutBokstavPositionX { get; set; }
        public float? RutBokstavPositionY { get; set; }

        public bool IncludeSection { get; set; }
        public float? SectionPositionX { get; set; }
        public float? SectionPositionY { get; set; }

        public bool IncludeServiceFee { get; set; }
        public float? ServiceFeePositionX { get; set; }
        public float? ServiceFeePositionY { get; set; }

        public bool IncludeStrukturArtikel { get; set; }
        public float? StrukturArtikelPositionX { get; set; }
        public float? StrukturArtikelPositionY { get; set; }

        public bool IncludeSubEventName { get; set; }
        public float? SubEventNamePositionX { get; set; }
        public float? SubEventNamePositionY { get; set; }

        public bool IncludeWebbcode { get; set; }
        public float? WebbcodePositionX { get; set; }
        public float? WebbcodePositionY { get; set; }

        public bool IncludeWebBookingNr { get; set; }
        public float? WebBookingNrPositionX { get; set; }
        public float? WebBookingNrPositionY { get; set; }

        #endregion Standard options

        public float? BarcodePositionX { get; set; }
        public float? BarcodePositionY { get; set; }

        public bool AddScissorsLine { get; set; }

        public bool FlipBarcode { get; set; }

        public bool UseQRCode { get; set; }
        public List<CustomTextElement>? CustomTextElements { get; set; } = new();

        public string DetermineTicketType()
        {
            if (!IncludeChairNr && IncludeSection)
            {
                return "Onumrerad";
            }
            else if (!IncludeChairNr && !IncludeSection)
            {
                return "Presentkort";
            }
            else
            {
                return "Numrerad";
            }
        }

        public static TicketHandling CreateNumreradTicketHandling()
        {
            return new TicketHandling
            {
                // Set up common inclusion flags
                IncludeAd = true,
                IncludeArtName = true,
                IncludeArtNotText = false,
                IncludeArtNr = false,
                IncludeBookingNr = true,
                IncludeChairNr = true,
                IncludeChairRow = true,
                IncludeContactPerson = true,
                IncludeDatum = true,
                IncludeDescription = false,
                IncludeEmail = true,
                IncludeEntrance = true,
                IncludeEventDate = true,
                IncludeEventName = true,
                IncludeFacilityName = true,
                IncludeLogorad1 = false,
                IncludeLogorad2 = false,
                IncludePrice = true,
                IncludeRutBokstav = false,
                IncludeSection = true,
                IncludeServiceFee = true,
                IncludeStrukturArtikel = false,
                IncludeSubEventName = true,
                IncludeWebbcode = false,
                IncludeWebBookingNr = true,
                AddScissorsLine = true,

                // Set up common positions
                ArtNamePositionX = 30.0f,
                ArtNamePositionY = 185.0f,
                BookingNrPositionX = 30.0f,
                BookingNrPositionY = 162.0f,
                ChairNrPositionX = 680.0f,
                ChairNrPositionY = 210.0f,
                ChairRowPositionX = 580.0f,
                ChairRowPositionY = 210.0f,
                ContactPersonPositionX = 30.0f,
                ContactPersonPositionY = 130.0f,
                DatumPositionX = 580.0f,
                DatumPositionY = 100.0f,
                EmailPositionX = 30.0f,
                EmailPositionY = 112.0f,
                EntrancePositionX = 788.0f,
                EntrancePositionY = 210.0f,
                EventDatePositionX = 30.0f,
                EventDatePositionY = 240.0f,
                EventNamePositionX = 570.0f,
                EventNamePositionY = 35.0f,
                FacilityNamePositionX = 585.0f,
                FacilityNamePositionY = 135.0f,
                PricePositionX = 30.0f,
                PricePositionY = 210.0f,
                SectionPositionX = 398.0f,
                SectionPositionY = 210.0f,
                ServiceFeePositionX = 250.0f,
                ServiceFeePositionY = 210.0f,
                SubEventNamePositionX = 560.0f,
                SubEventNamePositionY = 65.0f,
                WebBookingNrPositionX = 30.0f,
                WebBookingNrPositionY = 146.0f,
                CustomTextElements = new List<CustomTextElement>
                {
                    new("- Köpt biljett återlöses ej -", 120, 265, 8, null),
                    new("Serviceavgift", 250, 185, 8, null),
                    new("Sektion", 398, 185, 9, "#7a7979"),
                    new("Plats", 680, 185, 9, "#7a7979"),
                    new("Rad", 580, 185, 9, "#7a7979"),
                    new("Ingång", 788, 185, 9, "#7a7979")
                }
            };
        }

        public static TicketHandling CreateOnumreradTicketHandling()
        {
            var ticketHandling = CreateNumreradTicketHandling();
            ticketHandling.IncludeChairNr = false;
            ticketHandling.IncludeEntrance = false;

            ticketHandling.ChairRowPositionX = 640.0f;
            ticketHandling.EventNamePositionX = 545.0f;
            ticketHandling.CustomTextElements = new List<CustomTextElement>
            {
                new("- Köpt biljett återlöses ej -", 120, 265, 8, null),
                new("Serviceavgift", 250, 185, 8, null),
                new("Sektion", 398, 185, 9, "#7a7979"),
                new("Rad", 640, 185, 9, "#7a7979")
            };
            return ticketHandling;
        }

        public static TicketHandling CreatePresentkortTicketHandling()
        {
            var ticketHandling = CreateOnumreradTicketHandling();
            ticketHandling.IncludeDatum = false;
            ticketHandling.IncludeEntrance = false;
            ticketHandling.IncludeFacilityName = false;
            ticketHandling.IncludeSection = false;
            ticketHandling.IncludeSubEventName = false;
            ticketHandling.IncludeRutBokstav = true;

            ticketHandling.RutBokstavPositionX = 825.0f;
            ticketHandling.RutBokstavPositionY = 280.0f;
            ticketHandling.CustomTextElements = new List<CustomTextElement>
            {
                new("- Köpt biljett återlöses ej -", 120, 265, 8, null),
                new("Serviceavgift", 250, 185, 8, null),
            };
            return ticketHandling;
        }
    }
}