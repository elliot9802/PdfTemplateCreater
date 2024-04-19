using Models;

namespace Services
{
    public static class TicketHandlingService
    {
        public static TicketHandling CreateNumreradTicketHandling()
        {
            var ticketHandling = new TicketHandling();
            StandardTicketHandling(ticketHandling);

            ConfigureTextElement(ticketHandling, "EventName", positionX: 570, positionY: 35, fontColor: "#000000", fontStyle: FontStyle.Bold);
            ConfigureTextElement(ticketHandling, "EventDate", positionX: 540, positionY: 100, fontColor: "#000000");
            ConfigureTextElement(ticketHandling, "ChairNr", positionX: 680, positionY: 210, fontColor: "#000000");
            ConfigureTextElement(ticketHandling, "ChairRow", positionX: 580, positionY: 210, fontColor: "#000000");
            ConfigureTextElement(ticketHandling, "Entrance", positionX: 788, positionY: 210, fontColor: "#000000");
            ConfigureTextElement(ticketHandling, "FacilityName", positionX: 585, positionY: 135, fontColor: "#000000");
            ConfigureTextElement(ticketHandling, "Section", positionX: 398, positionY: 210, fontColor: "#000000");
            ConfigureTextElement(ticketHandling, "SubEventName", positionX: 560, positionY: 65, fontColor: "#000000");

            ticketHandling.CustomTextElements = new List<CustomTextElement>
                {
                    new("- Köpt biljett återlöses ej -", 120, 265, 8, null),
                    new("Serviceavgift", 250, 185, 8, null),
                    new("Sektion", 398, 185, 9, "#7a7979"),
                    new("Plats", 680, 185, 9, "#7a7979"),
                    new("Rad", 580, 185, 9, "#7a7979"),
                    new("Ingång", 788, 185, 9, "#7a7979"),
                    new("Bokningsnr:", 30, 162, 8, null),
                    new("Webbokningsnr:", 30, 146, 8, null),
                    new("Köpdatum:", 30, 240, 8, null)
                };

            return ticketHandling;
        }

        public static TicketHandling CreateOnumreradTicketHandling()
        {
            var ticketHandling = new TicketHandling();
            StandardTicketHandling(ticketHandling);

            ConfigureTextElement(ticketHandling, "EventName", positionX: 545, positionY: 35, fontColor: "#000000", fontStyle: FontStyle.Bold);
            ConfigureTextElement(ticketHandling, "EventDate", positionX: 540, positionY: 100, fontColor: "#000000");
            ConfigureTextElement(ticketHandling, "ChairRow", positionX: 680, positionY: 210, fontColor: "#000000");
            ConfigureTextElement(ticketHandling, "FacilityName", positionX: 585, positionY: 135, fontColor: "#000000");
            ConfigureTextElement(ticketHandling, "Section", positionX: 398, positionY: 210, fontColor: "#000000");
            ConfigureTextElement(ticketHandling, "SubEventName", positionX: 560, positionY: 65, fontColor: "#000000");

            ticketHandling.CustomTextElements = new List<CustomTextElement>
                {
                    new("- Köpt biljett återlöses ej -", 120, 265, 8, null),
                    new("Serviceavgift", 250, 185, 8, null),
                    new("Sektion", 398, 185, 9, "#7a7979"),
                    new("Rad", 680, 185, 9, "#7a7979"),
                    new("Bokningsnr:", 30, 162, 8, null),
                    new("Webbokningsnr:", 30, 146, 8, null),
                    new("Köpdatum:", 30, 240, 8, null)
                };
            return ticketHandling;
        }

        public static TicketHandling CreatePresentkortTicketHandling()
        {
            var ticketHandling = new TicketHandling();
            StandardTicketHandling(ticketHandling);

            ConfigureTextElement(ticketHandling, "EventName", positionX: 545, positionY: 35, fontColor: "#000000", fontStyle: FontStyle.Bold);
            ConfigureTextElement(ticketHandling, "RutBokstav", positionX: 825, positionY: 280, fontColor: "#000000", fontStyle: FontStyle.Bold);

            ticketHandling.CustomTextElements = new List<CustomTextElement>
            {
                new("- Köpt biljett återlöses ej -", 120, 265, 8, null),
                new("Serviceavgift", 250, 185, 8, null),
                new("Bokningsnr:", 30, 162, 8, null),
                new("Webbokningsnr:", 30, 146, 8, null),
                new("Köpdatum:", 30, 240, 8, null)
            };
            return ticketHandling;
        }

        private static void ConfigureTextElement(TicketHandling ticketHandling, string key, float? positionX = null, float? positionY = null,
                                                                  float? fontSize = null, string? fontColor = null, FontStyle? fontStyle = null, bool include = true)
        {
            if (ticketHandling.TextConfigs.ContainsKey(key))
            {
                var textElement = ticketHandling.TextConfigs[key].Style;
                if (positionX.HasValue)
                    textElement.PositionX = positionX;
                if (positionY.HasValue)
                    textElement.PositionY = positionY;
                if (fontSize.HasValue)
                    textElement.FontSize = fontSize;
                if (fontColor != null)
                    textElement.FontColor = fontColor;
                if (fontStyle.HasValue)
                    textElement.FontStyle = fontStyle.Value;
                textElement.Include = include;
            }
        }

        private static void StandardTicketHandling(TicketHandling ticketHandling)
        {
            ConfigureTextElement(ticketHandling, "Artikelnamn", positionX: 30, positionY: 185, fontColor: "#000000");
            ConfigureTextElement(ticketHandling, "BookingNr", positionX: 118, positionY: 162, fontColor: "#000000");
            ConfigureTextElement(ticketHandling, "ContactPerson", positionX: 30, positionY: 129, fontColor: "#000000");
            ConfigureTextElement(ticketHandling, "Email", positionX: 30, positionY: 112, fontColor: "#000000");
            ConfigureTextElement(ticketHandling, "Datum", positionX: 113, positionY: 240, fontColor: "#000000");
            ConfigureTextElement(ticketHandling, "Price", positionX: 30, positionY: 210, fontColor: "#000000");
            ConfigureTextElement(ticketHandling, "ServiceFee", positionX: 250, positionY: 210, fontColor: "#000000");
            ConfigureTextElement(ticketHandling, "WebBookingNr", positionX: 150, positionY: 146, fontColor: "#000000");

            ticketHandling.IncludeAd = true;
            ticketHandling.AdPositionY = 500;
            ticketHandling.AddScissorsLine = true;
        }
    }
}