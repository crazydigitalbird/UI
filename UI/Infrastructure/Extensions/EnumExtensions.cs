using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace UI
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            var displayName = value.GetType()
                .GetMember(value.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DisplayAttribute>()?.Name ?? "Unknow";
            return displayName;
        }
    }
}
