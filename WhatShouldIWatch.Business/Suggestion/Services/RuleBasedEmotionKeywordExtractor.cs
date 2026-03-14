namespace WhatShouldIWatch.Business.Suggestion.Services;

public class RuleBasedEmotionKeywordExtractor : IEmotionKeywordExtractor
{
    private static readonly IReadOnlyList<(string[] Triggers, string[] Keywords)> EmotionRules = new[]
    {
        (new[] { "kavga", "kavga ettim", "sinir", "sinirli", "öfke", "öfkeli", "kızgın" }, new[] { "öfke", "üzgün", "drama" }),
        (new[] { "aile", "ailemle", "ailem", "annem", "babam", "kardeş" }, new[] { "aile" }),
        (new[] { "aç", "açım", "karnım aç", "acıktım" }, new[] { "komedi", "rahatlama", "eğlence" }),
        (new[] { "yalnız", "yalnızım", "tek başıma" }, new[] { "yalnız", "drama", "romantik" }),
        (new[] { "mutlu", "mutluyum", "keyif", "keyifli" }, new[] { "mutluluk", "komedi" }),
        (new[] { "üzgün", "üzüldüm", "üzüntü", "mutsuz", "depresif" }, new[] { "üzgün", "üzüntü", "drama" }),
        (new[] { "stres", "stresli", "yorgun", "yoruldum", "bıktım" }, new[] { "stres", "rahatlama", "komedi" }),
        (new[] { "arkadaş", "arkadaşlarımla", "arkadaşlarla" }, new[] { "arkadaş", "komedi", "eğlence" }),
        (new[] { "gülmek", "gülelim", "güldüm", "komik" }, new[] { "komedi", "eğlence" }),
        (new[] { "ağlamak", "ağladım", "duygusal" }, new[] { "drama", "duygusal" }),
        (new[] { "korku", "korkunç", "korktum" }, new[] { "korku" }),
        (new[] { "aşk", "romantik", "sevgili", "flört" }, new[] { "romantik", "aşk" }),
        (new[] { "rahatla", "rahatlamak", "sakin", "huzur" }, new[] { "rahatlama", "huzur" }),
        (new[] { "aksiyon", "macera", "adrenalin" }, new[] { "aksiyon", "macera" }),
        (new[] { "eğlence", "eğlenmek", "parti" }, new[] { "eğlence", "komedi" }),
        (new[] { "üzüntü", "keder" }, new[] { "üzüntü", "drama" }),
        (new[] { "neşe", "neşeli", "şen" }, new[] { "mutluluk", "komedi" }),
        (new[] { "kızgın", "öfkeliyim" }, new[] { "öfke" }),
        (new[] { "sıkıldım", "sıkıcı" }, new[] { "komedi", "aksiyon", "eğlence" }),
        (new[] { "sevgi", "sevdim", "seviyorum" }, new[] { "romantik", "aşk", "drama" }),
    };

    public Task<IReadOnlyList<string>> ExtractKeywordsAsync(string userText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userText))
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());

        var text = userText.Trim().ToLowerInvariant();
        var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (triggers, keys) in EmotionRules)
        {
            foreach (var trigger in triggers)
            {
                if (text.Contains(trigger, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var k in keys)
                        keywords.Add(k);
                    break;
                }
            }
        }

        return Task.FromResult<IReadOnlyList<string>>(keywords.ToList());
    }
}
