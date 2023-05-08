using MongoDB.Bson;
using System.ComponentModel;
using System.Globalization;

namespace AmSoul.MongoDB;

public class ObjectIdConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        return value is string id
            ? ObjectId.TryParse(id, out var objectId) ? objectId : (object?)default
            : base.ConvertFrom(context, culture, value);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        return destinationType == typeof(string) && value is ObjectId objectId
            ? objectId.ToString()
            : base.ConvertTo(context, culture, value, destinationType);
    }
}
