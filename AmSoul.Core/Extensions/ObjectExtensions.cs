namespace AmSoul.Core;

public static class ObjectExtensions
{
    public static object? GetPropertyValue(this object info, string field)
    {
        if (info == null) return null;
        Type t = info.GetType();
        IEnumerable<System.Reflection.PropertyInfo> property = from pi in t.GetProperties() where pi.Name.ToLower() == field.ToLower() select pi;
        return property.First().GetValue(info, null);
    }
}
