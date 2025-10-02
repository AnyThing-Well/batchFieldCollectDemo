public static class ExpressionBuilder
{
    public static TranslatedContext<T> BuildCollect<T>(IEnumerable<T> items, TranslateService? translateService) where T : class
    {
        return new TranslatedContext<T>(items, translateService);
    }
}

public class TranslatedContext<T> where T : class
{
    private readonly TranslateService _service;
    private readonly IEnumerable<T> _items;
    private readonly List<(T Item, string SourceText, Action<T, string> Action)> _translations;

    public TranslatedContext(IEnumerable<T> source, TranslateService translateService)
    {
        ArgumentNullException.ThrowIfNull(source);
        _items = source;
        _service = translateService;
        _translations = new List<(T, string, Action<T, string>)>();
    }

    // 收集需要翻译的枚举项，action 延后执行
    public IEnumerable<T> MapTranslation(Func<T, string> selector, Action<T, string> action)
    {
        ArgumentNullException.ThrowIfNull(_items);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in _items)
        {
            var result = selector(item);
            // 收集 item、枚举值、action
            lock (_translations)
            {
                _translations.Add((item!, result!, action));
            }
            yield return item;
        }
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


