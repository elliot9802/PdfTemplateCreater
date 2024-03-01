using System.Reflection;

namespace Models
{
    public class PropertyGroup
    {
        public PropertyInfo? IncludeProperty { get; set; }
        public List<PropertyInfo>? PositionProperties { get; set; }
    }
}