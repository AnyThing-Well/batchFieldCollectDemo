using System.Linq.Expressions;
using System.Reflection;

public class TranslateContext<T> where T : class
{
    private readonly TranslateService _service;
    private readonly IEnumerable<T> _items;
    private readonly List<(T Item, string SourceText, Action<T, string> Action)> _translations;


    public TranslateContext(TranslateService service, IEnumerable<T> items)
    {
        _service = service;
        _items = items;
        _translations = new List<(T, string, Action<T, string>)>();
    }

    /// <summary>
    /// 收集并且映射翻译及赋值
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="selector"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public TranslateContext<T> MapTranslation(Func<T, string> selector, Action<T, string> action)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(action);
        foreach (var item in _items)
        {
            var result = selector(item);
            _translations.Add((item!, result, action));
        }
        return this;
    }

    public TranslateContext<T> MapTranslation(
        Func<T, string> sourceExpr,
        Expression<Func<T, string>> targetExpr)
    {
        if (sourceExpr == null || targetExpr == null) throw new ArgumentNullException();

        // var getter = sourceExpr.Compile();

        // 解析目标属性
        if (targetExpr.Body is not MemberExpression member || member.Member is not PropertyInfo prop)
            throw new InvalidOperationException("目标表达式必须是可写属性");
        if (!prop.CanWrite) throw new InvalidOperationException("属性不可写");

        // 构造 setter 委托： (T e, string v) => e.Prop = v;
        var pEntity = Expression.Parameter(typeof(T), "e"); //创建一个类型为 T 的参数，名字叫 "e"
        var pValue = Expression.Parameter(typeof(string), "v"); //创建一个类型为 string 的参数，名字叫 "v"
        var assign = Expression.Assign(Expression.Property(pEntity, prop), pValue); // 创建赋值表达式 e.Prop = v
        var setter = Expression.Lambda<Action<T, string>>(assign, pEntity, pValue).Compile(); // 创建 Lambda 表达式并编译成委托

        foreach (var item in _items)
        {
            var source = sourceExpr(item);
            _translations.Add((item, source, setter));
        }
        return this;
    }

    public async Task<IEnumerable<T>> ExecuteAsync()
    {
        var toTranslate = _translations
            .Select(x => x.SourceText)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();
        Console.WriteLine($"待翻译文本：{string.Join(", ", toTranslate)}");
        var dic = await _service.GetTranslationDic(toTranslate);

        foreach (var (item, sourceText, action) in _translations)
        {
            if (dic.TryGetValue(sourceText, out var translated))
            {
                action(item, translated);
            }
            else
            {
                action(item, sourceText);
            }
        }
        return _items;
    }
}