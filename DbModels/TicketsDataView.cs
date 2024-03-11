namespace DbModels
{
    public class TicketsDataView
    {
        public string? anamn { get; set; }
        public string? Artikelnamn { get; set; }
        public string? ArtikelNr { get; set; }
        public int? ArtNotText { get; set; }
        public string? Beskrivning { get; set; }
        public decimal? BokningsNr { get; set; }
        public string? Datum { get; set; }
        public DateTime? datumStart { get; set; }
        public string? eMail { get; set; }
        public int? eventdatum_id { get; set; }
        public string Ingang { get; set; }
        public string? KontaktPerson { get; set; }
        public string logorad1 { get; set; }
        public string logorad2 { get; set; }
        public string? namn { get; set; }
        public string? namn1 { get; set; }
        public string? namn2 { get; set; }
        public string plats { get; set; }
        public int? platsbokad_id { get; set; }
        public decimal? Pris { get; set; }
        public string reklam1 { get; set; }
        public string Rutbokstav { get; set; }
        public decimal serviceavgift1_kr { get; set; }
        public int showEventInfo { get; set; }
        public string stolsnr { get; set; }
        public string stolsrad { get; set; }
        public bool StrukturArtikel { get; set; }
        public string? wbarticleinfo { get; set; }
        public string? wbeventinfo { get; set; }
        public string Webbcode { get; set; }
        public string webbkod { get; set; }
        public Guid WebbUid { get; set; }
    }
}