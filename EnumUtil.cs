using System.Collections.Concurrent;
using System.ComponentModel;

public static class EnumUtil
{
    // 缓存：枚举类型 -> (枚举名 -> 描述)
    private static readonly ConcurrentDictionary<Type, Dictionary<string, string>> _descCache = new();

    public static string GetDescription(this Enum value)
    {
        var type = value.GetType();
        var name = value.ToString();

        // 获取或添加缓存
        var map = _descCache.GetOrAdd(type, t =>
        {
            var dict = new Dictionary<string, string>();
            foreach (var field in t.GetFields())
            {
                if (!field.IsStatic) continue;
                var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                dict[field.Name] = attr?.Description ?? field.Name;
            }
            return dict;
        });

        return map.TryGetValue(name, out var desc) ? desc : name;
    }
}