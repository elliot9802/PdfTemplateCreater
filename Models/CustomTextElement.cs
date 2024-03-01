using Newtonsoft.Json;

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

        public CustomTextElement()
        {
        }

        public string? Color { get; set; }
        public float? FontSize { get; set; } = 10;
        public float? PositionX { get; set; }

        public float? PositionY { get; set; }

        public string? Text { get; set; }

        [JsonIgnore]
        public bool IsInEditMode { get; set; } = true;
    }
}