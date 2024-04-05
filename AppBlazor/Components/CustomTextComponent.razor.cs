using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Models;

namespace AppBlazor.Components
{
    public partial class CustomTextComponent
    {
        // Parameters and Properties
        [Parameter]
        public List<CustomTextElement> CustomTexts { get; set; } = new();

        [Inject]
        private IJSRuntime? JSRuntime { get; set; }

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

        private bool RequiresScrollAndFocus { get; set; }
        private string? ElementToFocus { get; set; }

        // UI Event Handlers
        private void AddOrSaveText(CustomTextElement customText)
        {
            if (!string.IsNullOrEmpty(customText.Text))
            {
                var existingText = CustomTexts.Find(ct => ct.CustomTextId == customText.CustomTextId);
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
                if (!CustomTexts.Exists(ct => ct.Text == predef.Text))
                {
                    CustomTexts.Add(new CustomTextElement(predef.Text, predef.PositionX, predef.PositionY, predef.FontSize, predef.Color));
                }
            });
        }

        private void AddNextPredefinedText()
        {
            var nextPredefinedText = NextUnaddedPredefined();
            if (nextPredefinedText == null) return;

            var openTempText = TempCustomTexts.Find(t => t.IsInEditMode);
            if (openTempText != null)
            {
                UpdateTextElement(openTempText, nextPredefinedText);
            }
            else
            {
                var tempText = PredefinedToTempText(nextPredefinedText);
                if (tempText != null)
                {
                    TempCustomTexts.Add(tempText);
                }
            }
        }

        private void CancelCustomText(CustomTextElement customText) => TempCustomTexts.RemoveAll(ct => ct.CustomTextId == customText.CustomTextId);

        private void ClearTexts() => CustomTexts.Clear();

        private void InitEditNewText()
        {
            if (!TempCustomTexts.Exists(t => t.IsInEditMode))
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

            var existingTemp = TempCustomTexts.Find(t => t.CustomTextId == customText.CustomTextId);
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
                TempCustomTexts.Insert(0, existingTemp);
            }
            else
            {
                existingTemp.IsInEditMode = true;
            }
            RequiresScrollAndFocus = true;
            ElementToFocus = $"#tempText-{existingTemp.CustomTextId}";
        }

        // Utility Methods
        private bool AreAllPredefinedTextsAdded() => predefinedTexts.TrueForAll(predef => CustomTexts.Exists(ct => ct.Text == predef.Text));

        private CustomTextElement? NextUnaddedPredefined() =>
            predefinedTexts.Find(predef => CustomTexts.TrueForAll(ct => ct.Text != predef.Text));

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (JSRuntime != null && RequiresScrollAndFocus)
            {
                RequiresScrollAndFocus = false;
                try
                {
                    await JSRuntime.InvokeVoidAsync("scrollToElementAndFocus", ElementToFocus);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error invoking JS for scroll focus: {ex.Message}");
                }
            }
        }

        private static CustomTextElement? PredefinedToTempText(CustomTextElement? source)
        {
            if (source == null) return null;
            return new(source.Text, source.PositionX, source.PositionY, source.FontSize, source.Color) { IsInEditMode = true };
        }

        private static void UpdateTextElement(CustomTextElement target, CustomTextElement source) =>
            (target.Text, target.PositionX, target.PositionY, target.FontSize, target.Color) =
            (source.Text, source.PositionX, source.PositionY, source.FontSize, source.Color);
    }
}