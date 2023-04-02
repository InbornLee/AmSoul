using System.Reflection;

namespace AmSoul.Core.Utilis;

public class TypeNameHelper
{
    private static readonly Dictionary<Type, string> _builtInTypeNames = new()
    {
        { typeof(bool), "bool" },
        { typeof(byte), "byte" },
        { typeof(char), "char" },
        { typeof(decimal), "decimal" },
        { typeof(double), "double" },
        { typeof(float), "float" },
        { typeof(int), "int" },
        { typeof(long), "long" },
        { typeof(object), "object" },
        { typeof(sbyte), "sbyte" },
        { typeof(short), "short" },
        { typeof(string), "string" },
        { typeof(uint), "uint" },
        { typeof(ulong), "ulong" },
        { typeof(ushort), "ushort" }
    };

    public static string GetTypeDisplayName(Type type)
    {
        if (type.GetTypeInfo().IsGenericType)
        {
            var fullName = type.GetGenericTypeDefinition().FullName;

            var parts = fullName!.Split('+');

            for (var i = 0; i < parts.Length; i++)
            {
                var partName = parts[i];

                var backTickIndex = partName.IndexOf('`');
                if (backTickIndex >= 0)
                {
                    partName = partName[..backTickIndex];
                }

                parts[i] = partName;
            }
            return string.Join(".", parts);
        }
        else if (_builtInTypeNames.ContainsKey(type))
        {
            return _builtInTypeNames[type];
        }
        else
        {
            var fullName = type.FullName;

            if (type.IsNested)
            {
                fullName = fullName!.Replace('+', '.');
            }

            return fullName!;
        }
    }
}
