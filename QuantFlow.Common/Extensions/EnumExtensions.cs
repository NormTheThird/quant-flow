namespace QuantFlow.Common.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attr = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                         .FirstOrDefault() as DescriptionAttribute;
        return attr?.Description ?? value.ToString();
    }
}