namespace Models
{
    public class TicketHandling
    {
        public int Id { get; set; }

        // Properties for customization options
        #region Include
        public bool IncludePBookId { get; set; }
        public bool IncludeArtNr { get; set; }
        public bool IncludePrice { get; set; }
        public bool IncludeServiceFee { get; set; }
        public bool IncludeArtName { get; set; }
        public bool IncludeChairRow { get; set; }
        public bool IncludeChairNr { get; set; }
        public bool IncludeEventDateId { get; set; }
        public bool IncludeEventDate { get; set; }
        public bool IncludeEventName { get; set; }
        public bool IncludeSubEventName { get; set; }
        public bool IncludeLogorad1 { get; set; }
        public bool IncludeLogorad2 { get; set; }
        public bool IncludeLocation { get; set; }
        public bool IncludeLocationName { get; set; }
        public bool IncludeBookingNr { get; set; }
        public bool IncludeWebBookingNr { get; set; }
        public bool IncludeFacilityName { get; set; }
        public bool IncludeAd { get; set; }
        //public bool IncludeshowEventInfo { get; set; }
        public bool IncludeStrukturArtikel { get; set; }
        public bool IncludeDescription { get; set; }
        public bool IncludeArtNotText { get; set; }
        public bool IncludeContactPerson { get; set; }
        public bool IncludeEmail { get; set; }
        public bool IncludeDatum { get; set; }
        public bool IncludeEntrance { get; set; }
        //public bool Includewbeventinfo { get; set; }
        public bool IncludeRutBokstav { get; set; }
        public bool IncludeWebbcode { get; set; }
        public bool IncludeScissorsLine { get; set; }

        #endregion

        #region Position 
        // Properties for positioning elements on the ticket
        public float? PBookIdPositionX { get; set; }
        public float? PBookIdPositionY { get; set; }

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

        public float? EventDateIdPositionX { get; set; }
        public float? EventDateIdPositionY { get; set; }

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

        public float? LocationNamePositionX { get; set; }
        public float? LocationNamePositionY { get; set; }

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

        //public float wbeventinfoPositionX { get; set; }
        //public float wbeventinfoPositionY { get; set; }

        public float? RutBokstavPositionX { get; set; }
        public float? RutBokstavPositionY { get; set; }

        public float? WebbcodePositionX { get; set; }
        public float? WebbcodePositionY { get; set; }
        #endregion

        public float? BarcodePositionX { get; set; }
        public float? BarcodePositionY { get; set; }

        // Property to choose between QR code and Barcode
        public bool UseQRCode { get; set; }
    }
}