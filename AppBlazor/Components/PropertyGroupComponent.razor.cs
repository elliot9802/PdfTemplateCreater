using Microsoft.AspNetCore.Components;
using Models;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AppBlazor.Components
{
    public partial class PropertyGroupComponent
    {
        [Parameter]
        public TicketHandling? TicketHandling { get; set; }

        public List<PropertyGroup>? PropertyGroups { get; set; } = new List<PropertyGroup>();

        protected override void OnParametersSet()
        {
            PropertyGroups = GetPropertyGroups();
        }

        private List<PropertyGroup> GetPropertyGroups()
        {
            var groups = new List<PropertyGroup>();
            var includeProperties = typeof(TicketHandling).GetProperties()
                .Where(p => p.PropertyType == typeof(bool) && p.Name.StartsWith("Include"));

            foreach (var includeProperty in includeProperties)
            {
                var suffix = includeProperty.Name.Substring("Include".Length);
                List<PropertyInfo> positionProperties;
                if (suffix == "EventName" || suffix == "SubEventName")
                {
                    positionProperties = typeof(TicketHandling).GetProperties()
                        .Where(p => p.Name.StartsWith(suffix) && (p.Name.EndsWith("PositionX") || p.Name.EndsWith("PositionY")))
                        .ToList();
                }
                else if (suffix == "BookingNr" || suffix == "WebBookingNr")
                {
                    positionProperties = typeof(TicketHandling).GetProperties()
                        .Where(p => p.Name.StartsWith(suffix) && (p.Name.EndsWith("PositionX") || p.Name.EndsWith("PositionY")))
                        .ToList();
                }
                else
                {
                    positionProperties = typeof(TicketHandling).GetProperties()
                        .Where(p => p.Name.EndsWith(suffix + "PositionX") || p.Name.EndsWith(suffix + "PositionY"))
                        .ToList();
                }

                groups.Add(new PropertyGroup
                {
                    IncludeProperty = includeProperty,
                    PositionProperties = positionProperties
                });
            }

            return groups;
        }

        private void HandleCheckboxChange(ChangeEventArgs e, PropertyInfo? propertyInfo)
        {
            if (propertyInfo is null) return;

            if (bool.TryParse(e.Value?.ToString(), out var isChecked))
            {
                propertyInfo.SetValue(TicketHandling, isChecked);
            }
        }

        private void HandlePositionChange(ChangeEventArgs e, PropertyInfo propertyInfo)
        {
            var inputValue = e.Value?.ToString();
            if (string.IsNullOrEmpty(inputValue))
            {
                propertyInfo.SetValue(TicketHandling, null);
            }
            else
            {
                if (propertyInfo.PropertyType == typeof(bool))
                {
                    if (bool.TryParse(inputValue, out var boolValue))
                    {
                        propertyInfo.SetValue(TicketHandling, boolValue);
                    }
                }
                else if (propertyInfo.PropertyType == typeof(float?))
                {
                    if (float.TryParse(inputValue, out var floatValue))
                    {
                        propertyInfo.SetValue(TicketHandling, floatValue);
                    }
                }
            }
        }

        private string GetDisplayName(string propertyName) => Regex.Replace(propertyName, "(\\B[A-Z])", " $1");
    }
}