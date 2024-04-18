namespace Models
{
    public class TextElement
    {
        public bool Include { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }
        public string? FontColor { get; set; }
        public float? FontSize { get; set; }
        public FontStyle FontStyle { get; set; } = FontStyle.Regular;
    }
}