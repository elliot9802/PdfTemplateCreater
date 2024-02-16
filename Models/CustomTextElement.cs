namespace Models
{
    public class CustomTextElement
    {
        public CustomTextElement(string? text, float? x, float? y, float? fontSize, string? color)
        {
            Text = text;
            PositionX = x;
            PositionY = y;
            FontSize = fontSize;
            Color = color;
        }

        public string? Color { get; set; }
        public float? FontSize { get; set; } = 10;
        public float? PositionX { get; set; }

        public float? PositionY { get; set; }

        public string? Text { get; set; }
    }
}

/*[ { "Text": "Sektion", "PositionX": 398, "PositionY": 185, "FontSize": 9, "Color": "#7a7979" },
{ "Text": "Plats", "PositionX": 640, "PositionY": 185, "FontSize": 9, "Color": "#7a7979" },
{ "Text": "Serviceavgift", "PositionX": 250, "PositionY": 185, "FontSize": 8, "Color": "#000000" },
{ "Text": "- Köpt biljett återlöses ej -", "PositionX": 120, "PositionY": 265, "FontSize": 8, "Color": "#000000" }]*/