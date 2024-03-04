using Microsoft.AspNetCore.Components;
using Models;

namespace AppBlazor.Components
{
    public partial class CustomTextComponent
    {
        // Parameters and Properties
        [Parameter]
        public List<CustomTextElement> CustomTexts { get; set; } = new();

        public List<CustomTextElement> TempCustomTexts { get; set; } = new();

        private readonly List<CustomTextElement> predefinedTexts = new()
    {
        new("- Köpt biljett återlöses ej -", 120, 265, 8, null),
        new("Serviceavgift", 250, 185, 8, null),
        new("Sektion", 398, 185, 9, "#7a7979"),
        new("Plats", 640, 185, 9, "#7a7979"),
        new("Rad", 580, 185, 9, "#7a7979"),
        new("Ingång", 788, 185, 9, "#7a7979")
    };

        // UI Event Handlers
        private void AddOrSaveText(CustomTextElement customText)
        {
            if (!string.IsNullOrEmpty(customText.Text))
            {
                var existingText = CustomTexts.FirstOrDefault(ct => ct.CustomTextId == customText.CustomTextId);
                if (existingText != null)
                {
                    existingText.Text = customText.Text;
                    existingText.PositionX = customText.PositionX;
                    existingText.PositionY = customText.PositionY;
                    existingText.FontSize = customText.FontSize;
                    existingText.Color = customText.Color;
                }
                else
                {
                    CustomTexts.Add(new CustomTextElement(customText.Text, customText.PositionX, customText.PositionY, customText.FontSize, customText.Color));
                }

                TempCustomTexts.RemoveAll(ct => ct.CustomTextId == customText.CustomTextId);
            }
        }

        private void AddAllPredefinedTexts()
        {
            predefinedTexts.ForEach(predef =>
            {
                if (!CustomTexts.Any(ct => ct.Text == predef.Text))
                {
                    CustomTexts.Add(new CustomTextElement(predef.Text, predef.PositionX, predef.PositionY, predef.FontSize, predef.Color));
                }
            });
        }

        private void AddNextPredefinedText()
        {
            var nextPredefinedText = NextUnaddedPredefined();
            var openTempText = TempCustomTexts.FirstOrDefault(t => t.IsInEditMode);
            if (openTempText != null)
            {
                UpdateTextElement(openTempText, nextPredefinedText);
            }
            else
            {
                TempCustomTexts.Add(PredefinedToTempText(nextPredefinedText));
            }
        }

        private void CancelCustomText(CustomTextElement customText) => TempCustomTexts.RemoveAll(ct => ct.CustomTextId == customText.CustomTextId);

        private void ClearTexts() => CustomTexts.Clear();

        private void InitEditNewText()
        {
            if (!TempCustomTexts.Any(t => t.IsInEditMode))
            {
                TempCustomTexts.Add(new CustomTextElement { IsInEditMode = true });
            }
        }

        private void RemoveText(CustomTextElement customText)
        {
            CustomTexts.RemoveAll(ct => ct.CustomTextId == customText.CustomTextId);
            TempCustomTexts.RemoveAll(ct => ct.CustomTextId == customText.CustomTextId);
        }

        private void ToggleTextEditing(CustomTextElement customText, bool isInEditMode)
        {
            if (!isInEditMode) return;

            var existingTemp = TempCustomTexts.FirstOrDefault(t => t.CustomTextId == customText.CustomTextId);
            if (existingTemp == null)
            {
                existingTemp = new CustomTextElement
                {
                    CustomTextId = customText.CustomTextId,
                    Text = customText.Text,
                    PositionX = customText.PositionX,
                    PositionY = customText.PositionY,
                    FontSize = customText.FontSize,
                    Color = customText.Color,
                    IsInEditMode = true
                };
                TempCustomTexts.Add(existingTemp);
            }
            else
            {
                existingTemp.IsInEditMode = true;
            }
        }

        // Utility Methods
        private bool AreAllPredefinedTextsAdded() => predefinedTexts.All(predef => CustomTexts.Any(ct => ct.Text == predef.Text));

        private CustomTextElement? NextUnaddedPredefined() =>
            predefinedTexts.FirstOrDefault(predef => CustomTexts.All(ct => ct.Text != predef.Text));

        private CustomTextElement PredefinedToTempText(CustomTextElement source) =>
            new CustomTextElement(source.Text, source.PositionX, source.PositionY, source.FontSize, source.Color) { IsInEditMode = true };

        private void UpdateTextElement(CustomTextElement target, CustomTextElement source) =>
            (target.Text, target.PositionX, target.PositionY, target.FontSize, target.Color) =
            (source.Text, source.PositionX, source.PositionY, source.FontSize, source.Color);
    }
}