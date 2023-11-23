namespace Models
{
    public class TicketHandling
    {
        public int Id { get; set; }
        public int showEventInfo { get; set; }

        // Properties for customization options
        #region Include

        #region bara 0
        public bool IncludeStrukturArtikel { get; set; }
        #endregion

        #region helt null
        public bool IncludeDescription { get; set; }


        public bool IncludeArtNotText { get; set; }

        #endregion

        #region helt tom, inte null
        public bool IncludeRutBokstav { get; set; }
        #endregion

        public bool IncludeArtNr { get; set; } = true;
        public bool IncludePrice { get; set; } = true;
        public bool IncludeServiceFee { get; set; } = true;
        public bool IncludeArtName { get; set; } = true;
        public bool IncludeChairRow { get; set; } = true;
        public bool IncludeChairNr { get; set; } = true;
       

        public bool IncludeEventDate { get; set; } = true;
        public bool IncludeEventName { get; set; } = true;
        public bool IncludeSubEventName { get; set; } = true;
        public bool IncludeLogorad1 { get; set; } = true;
        public bool IncludeLogorad2 { get; set; } = true;


        public bool IncludeSection { get; set; } = true;
        public bool IncludeBookingNr { get; set; } = true;
        public bool IncludeWebBookingNr { get; set; } = true;
        public bool IncludeFacilityName { get; set; } = true;
        public bool IncludeAd { get; set; } = true;


        public bool IncludeContactPerson { get; set; } = true;
        public bool IncludeEmail { get; set; } = true;
        public bool IncludeDatum { get; set; } = true;
        public bool IncludeEntrance { get; set; } = true;


        public bool IncludeWebbcode { get; set; } = true;
        public bool IncludeScissorsLine { get; set; } = true;
        #endregion

        #region Position 
        // Properties for positioning elements on the ticket
        public float? ArtNrPositionX { get; set; }
        public float? ArtNrPositionY { get; set; }

        public float? PricePositionX { get; set; }
        public float? PricePositionY { get; set; }

        public float? ServiceFeePositionX { get; set; }
        public float? ServiceFeePositionY { get; set; }

        public float? ArtNamePositionX { get; set; }
        public float? ArtNamePositionY { get; set; }

        public float? ChairRowPositionX { get; set; }
        public float? ChairRowPositionY { get; set; }

        public float? ChairNrPositionX { get; set; }
        public float? ChairNrPositionY { get; set; }

        public float? EventDatePositionX { get; set; }
        public float? EventDatePositionY { get; set; }

        public float? EventNamePositionX { get; set; }
        public float? EventNamePositionY { get; set; }

        public float? SubEventNamePositionX { get; set; }
        public float? SubEventNamePositionY { get; set; }

        public float? Logorad1PositionX { get; set; }
        public float? Logorad1PositionY { get; set; }

        public float? Logorad2PositionX { get; set; }
        public float? Logorad2PositionY { get; set; }

        public float? LocationPositionX { get; set; }
        public float? LocationPositionY { get; set; }

        public float? SectionPositionX { get; set; }
        public float? SectionPositionY { get; set; }

        public float? BookingNrPositionX { get; set; }
        public float? BookingNrPositionY { get; set; }

        public float? WebBookingNumberPositionX { get; set; }
        public float? WebBookingNumberPositionY { get; set; }

        public float? FacilityNamePositionX { get; set; }
        public float? FacilityNamePositionY { get; set; }

        public float? AdPositionX { get; set; }
        public float? AdPositionY { get; set; }

        public float? StrukturArtikelPositionX { get; set; }
        public float? StrukturArtikelPositionY { get; set; }

        public float? DescriptionPositionX { get; set; }
        public float? DescriptionPositionY { get; set; }

        public float? ArtNotTextPositionX { get; set; }
        public float? ArtNotTextPositionY { get; set; }

        public float? NamePositionX { get; set; }
        public float? NamePositionY { get; set; }

        public float? EmailPositionX { get; set; }
        public float? EmailPositionY { get; set; }

        public float? DatumPositionX { get; set; }
        public float? DatumPositionY { get; set; }

        public float? EntrancePositionX { get; set; }
        public float? EntrancePositionY { get; set; }

        public float? RutBokstavPositionX { get; set; }
        public float? RutBokstavPositionY { get; set; }

        public float? WebbcodePositionX { get; set; }
        public float? WebbcodePositionY { get; set; }

        public float? BarcodePositionX { get; set; }
        public float? BarcodePositionY { get; set; }
        #endregion

        // Property to choose between QR code and Barcode
        public bool UseQRCode { get; set; }

        public bool FlipBarcode { get; set; }

        public List<CustomTextElement>? CustomTextElements { get; set; } = new List<CustomTextElement>();
        
    }
}