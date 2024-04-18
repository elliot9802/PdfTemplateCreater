using Microsoft.AspNetCore.Components;
using Models;

namespace AppBlazor.Components
{
    public partial class PropertyGroupComponent
    {
        [Parameter]
        public TicketHandling? TicketHandling { get; set; }

        private void HandleIncludeChange(ChangeEventArgs e, TextElement style)
        {
            if (bool.TryParse(e.Value?.ToString(), out var include))
            {
                style.Include = include;
            }
        }

        private void HandlePositionChange(ChangeEventArgs e, TextElement style, bool isX)
        {
            if (float.TryParse(e.Value?.ToString(), out var position))
            {
                if (isX)
                    style.PositionX = position;
                else
                    style.PositionY = position;
            }
        }

        private void HandleFontSizeChange(ChangeEventArgs e, TextElement style)
        {
            if (float.TryParse(e.Value?.ToString(), out var fontSize))
            {
                style.FontSize = fontSize;
            }
        }

        private void HandleFontColorChange(ChangeEventArgs e, TextElement style)
        {
            style.FontColor = e.Value?.ToString() ?? "#000000";
        }

        private void HandleFontStyleChange(ChangeEventArgs e, TextElement style)
        {
            if (Enum.TryParse<FontStyle>(e.Value?.ToString(), out var fontStyle))
            {
                style.FontStyle = fontStyle;
            }
        }
    }
}