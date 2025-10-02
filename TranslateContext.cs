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