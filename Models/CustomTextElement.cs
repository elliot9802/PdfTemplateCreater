using Newtonsoft.Json;

namespace Models
{
    public class CustomTextElement
    {
        public Guid CustomTextId { get; set; } = Guid.NewGuid();
        public string? Text { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }
        public string? FontColor { get; set; }
        public float? FontSize { get; set; } = 10;
        public FontStyle FontStyle { get; set; } = FontStyle.Regular;

        [JsonIgnore]
        public bool IsInEditMode { get; set; }

        public CustomTextElement(string? text, float? x, float? y, float? fontSize, string? color, FontStyle fontStyle = FontStyle.Regular)
        {
            Text = text;
            PositionX = x;
            PositionY = y;
            FontSize = fontSize;
            FontStyle = fontStyle;
            FontColor = color;
        }

        public CustomTextElement()
        { }
    }
}