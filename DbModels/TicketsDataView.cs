namespace DbModels
{
#pragma warning disable
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
#pragma warning restore

    public class TicketsDataSeeder
    {
        private readonly Random random = new();

        private readonly List<string> emailOptions = new() { "user@example.com", "visitor@example.org", "guest@example.net" };
        private readonly List<string> logoradOptions = new() { "Sponsor 1", "Sponsor 2", "Sponsor 3" };
        private readonly List<string> reklamOptions = new() { "Ad Content 1", "Ad Content 2", "Ad Content 3" };
        private readonly List<string> webbcodeOptions = new() { "WEB123", "WEB456", "WEB789" };
        private readonly List<string> webbkodOptions = new() { "BK123", "BK456", "BK789" };

        public TicketsDataView GenerateMockData(string ticketType)
        {
            var ticketData = new TicketsDataView
            {
                BokningsNr = random.Next(10000, 99999),
                Datum = DateTime.Now.AddDays(random.Next(-30, 30)).ToString("yyyy-MM-dd"),
                datumStart = DateTime.Now.AddDays(random.Next(-30, 30)),
                eMail = PickRandom(emailOptions),
                eventdatum_id = random.Next(1, 100),
                KontaktPerson = "Contact Person",
                logorad1 = PickRandom(logoradOptions),
                logorad2 = PickRandom(logoradOptions),
                platsbokad_id = random.Next(1, 1000),
                Pris = random.Next(50, 500) + 0.00m,
                reklam1 = PickRandom(reklamOptions),
                serviceavgift1_kr = random.Next(10, 30) + 0.00m,
                stolsrad = $"{random.Next(1, 5)}",
                StrukturArtikel = random.NextDouble() > 0.5,
                wbarticleinfo = "Article Info",
                wbeventinfo = "Event Info",
                Webbcode = PickRandom(webbcodeOptions),
                webbkod = PickRandom(webbkodOptions),
                WebbUid = Guid.NewGuid()
            };

            switch (ticketType)
            {
                case "Numrerad":
                    ticketData.anamn = "Konsertsal";
                    ticketData.Artikelnamn = "Ordinarie";
                    ticketData.namn = "Parkett";
                    ticketData.namn1 = "Johan Glans";
                    ticketData.namn2 = "World tour of skåne";
                    ticketData.Ingang = "VÄ";
                    ticketData.stolsnr = $"{random.Next(1, 20)}";
                    ticketData.showEventInfo = 1;
                    break;
                case "Onumrerad":
                    ticketData.anamn = "ActorBadet";
                    ticketData.Artikelnamn = "Babysim";
                    ticketData.namn = "Utbildningsbassäng stor";
                    ticketData.namn1 = "Simskola Babysim";
                    ticketData.showEventInfo = 2;
                    break;
                case "Presentkort":
                    ticketData.anamn = "Presentkort_anamn";
                    ticketData.Artikelnamn = "Presentkort_Artikelnamn";
                    ticketData.namn1 = "Presentkort_namn1";
                    ticketData.Rutbokstav = "PK";
                    ticketData.namn = null;
                    ticketData.stolsrad = string.Empty;
                    ticketData.showEventInfo = 3;
                    break;
                default:
                    throw new ArgumentException($"Unsupported ticket type: {ticketType}", nameof(ticketType));
            }

            return ticketData;
        }

        private T PickRandom<T>(List<T> options)
        {
            return options[random.Next(options.Count)];
        }
    }
}