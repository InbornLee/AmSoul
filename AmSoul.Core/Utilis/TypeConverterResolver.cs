using System.ComponentModel;

namespace AmSoul.Core;

public static class TypeConverterResolver
{
    public static void RegisterTypeConverter<T, TC>() where TC : TypeConverter
    {
        TypeDescriptor.AddAttributes(typeof(T), new Attribute[1] { new TypeConverterAttribute(typeof(TC)) });
    }
}
