
public class TranslateService
{
    private readonly ILogger<TranslateService> _logger;
    public TranslateService(ILogger<TranslateService> logger)
    {
        _logger = logger;
    }

    public TranslateContext<T> BuildCollect<T>(IEnumerable<T> items) where T : class
    {
        return new TranslateContext<T>(this, items);
    }

    /// <summary>
    /// 逻辑代码，模拟调用第三方翻译接口,返回对应的翻译
    /// </summary>
    /// <param name="pendingTranslations">待翻译字符串列表</param>
    /// <returns></returns>
    public async Task<Dictionary<string, string>> GetTranslationDic(IEnumerable<string> pendingTranslations)
    {
        if (pendingTranslations == null || !pendingTranslations.Any())
        {
            return new Dictionary<string, string>();
        }
        pendingTranslations = pendingTranslations.Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim()).Distinct();

        var dic = new Dictionary<string, string>();
        foreach (var text in pendingTranslations)
        {
            dic[text] = text switch
            {
                "启用" => "Enable",
                "禁用" => "Disable",
                _ => text
            };
        }
        return await Task.FromResult(dic);
    }
}

