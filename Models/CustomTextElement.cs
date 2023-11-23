namespace Models
{
    public class CustomTextElement
    {
        public string? Text { get; set; } // The text content of the element
        public float? PositionX { get; set; } // X-coordinate position on the ticket
        public float? PositionY { get; set; } // Y-coordinate position on the ticket
        public float? FontSize { get; set; } = 10; // Default Font Size
        public string? Color { get; set; }

        // Constructor with parameters to quickly create a populated element
        public CustomTextElement(string text, float x, float y, float fontSize, string color)
        {
            Text = text;
            PositionX = x;
            PositionY = y;
            FontSize = fontSize;
            Color = color;
        }
    }
}

/*[ { "Text": "Sektion", "PositionX": 398, "PositionY": 185, "FontSize": 9, "Color": "#7a7979" },   
{ "Text": "Plats", "PositionX": 640, "PositionY": 185, "FontSize": 9, "Color": "#7a7979" },
{ "Text": "Serviceavgift", "PositionX": 250, "PositionY": 185, "FontSize": 8, "Color": "#000000" },
{ "Text": "- Köpt biljett återlöses ej -", "PositionX": 120, "PositionY": 265, "FontSize": 8, "Color": "#000000" }]*/